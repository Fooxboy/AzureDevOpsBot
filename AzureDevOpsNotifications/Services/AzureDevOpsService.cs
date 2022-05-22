using AzureDevOpsNotifications.Models;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevOpsNotifications.Services
{
    public class AzureDevOpsService
    {

        private readonly ReleaseHttpClient2 releaseClient;
        private readonly BuildHttpClient buildClient;
        private readonly WorkItemTrackingHttpClient wiclient;
        private readonly GitHttpClient gitClient;
        private readonly ProjectHttpClient projectClient;
        private readonly VssConnection connection;
        private readonly IConfiguration configuration;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<AzureDevOpsService> logger;
        private readonly TelegramService telegramService;

        private DateTime? LastScanTime = null;

        private DateTime? LastFailedBuildsTime = null;

        private readonly List<ReleaseInProgress> ReleasesInProgress;

        public AzureDevOpsService(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<AzureDevOpsService> logger)
        {
            this.configuration = configuration;
            this.serviceProvider = serviceProvider;
            var url = $"https://dev.azure.com/{configuration["AzureOrganization"]}";
            var pat = configuration["AzureToken"];
            connection = new VssConnection(new Uri(url), new VssBasicCredential(string.Empty, pat));

            releaseClient = connection.GetClient<ReleaseHttpClient2>();
            buildClient = connection.GetClient<BuildHttpClient>();
            wiclient = connection.GetClient<WorkItemTrackingHttpClient>();
            gitClient = connection.GetClient<GitHttpClient>();
            projectClient = connection.GetClient<ProjectHttpClient>();

            var scope = this.serviceProvider.CreateScope();

            this.telegramService = scope.ServiceProvider.GetRequiredService<TelegramService>();


            this.logger = logger;

            ReleasesInProgress = new List<ReleaseInProgress>();
        }
        
        /// <summary>
        /// Получение релизов, которые сейчас в сборке.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ExecuteReleases(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting execute releases...");
            await connection.ConnectAsync();


            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var releases = await this.GetNewReleasesAsync();
                    logger.LogInformation($"Found {releases.Count} releases");


                    foreach (var releaseItem in releases)
                    {
                        var project = releaseItem.ProjectReference.Id;
                        var release = await releaseClient.GetReleaseAsync(project: project, releaseId: releaseItem.Id);

                        var deploymentArtifact = release.Artifacts.FirstOrDefault();

                        var deployment = release.Environments.FirstOrDefault().DeploySteps.FirstOrDefault();
                        var buildRunId = Convert.ToInt32(deploymentArtifact.DefinitionReference["version"].Id);
                        var buildVersion = deploymentArtifact.DefinitionReference["version"].Name;

                        var buildInfo = await buildClient.GetBuildAsync(project: project, buildId: buildRunId).ConfigureAwait(false);

                        var commit = await gitClient.GetCommitAsync(commitId: buildInfo.SourceVersion, repositoryId: buildInfo.Repository.Id).ConfigureAwait(false);

                        var projectName = deploymentArtifact.DefinitionReference["project"].Name;
                        var repositoryName = deploymentArtifact.DefinitionReference["repository"].Name;
                        var branch = deploymentArtifact.DefinitionReference["branches"].Name;
                        var def = int.Parse(deploymentArtifact.DefinitionReference["definition"].Id);


                        var b = await releaseClient.GetDeploymentsAsync(project: project);


                        var workItems = await buildClient.GetBuildWorkItemsRefsAsync(project: project, buildRunId);


                        TimeSpan buildTime;


                        if (buildInfo.StartTime != null)
                        {
                            buildTime = (buildInfo.FinishTime - buildInfo.StartTime).Value;
                        } else
                        {
                            buildTime = TimeSpan.Zero;
                        }

                        var releaseView = new ReleaseView()
                        {
                            ReleaseName = $"[{release.Name}]({((ReferenceLink)release.Links.Links["web"]).Href})",
                            ReleaseState = $"{deployment.Status}",
                            ProjectName = $"{projectName}",
                            RepositoryName = $"{repositoryName}",
                            BranchName = $"{branch}",
                            UserDisplayName = $"{release.ModifiedBy.DisplayName}",
                            CommitName = $"[{commit.Comment.Split("\n")[0]}]({commit.RemoteUrl})",
                            BuildTime = $"0{buildTime.Minutes}:{buildTime.Seconds}",
                            WorkItems = String.Empty

                        };

                        

                        foreach (var workItem in workItems)
                        {
                            var wi = await wiclient.GetWorkItemAsync(id: Convert.ToInt32(workItem.Id));

                            var link = (ReferenceLink)wi.Links.Links["html"];
                            var type = wi.Fields["System.WorkItemType"];
                            var title = wi.Fields["System.Title"];

                            releaseView.WorkItems += $"\n ✅ [{type} {wi.Id} - {title}]({link.Href})";
                        }

                        if (workItems.Count == 0) releaseView.WorkItems += $"\n🥺 Нет привязанных задач";


                        var text =
                            $"\n🔔 *Новый релиз:* {releaseView.ReleaseName}" +
                            $"\n 🚩 _Состояние:_ {StatusToView(deployment.Status)}" +
                            $"\n ⚡ _Проект:_ {releaseView.ProjectName}" +
                            $"\n 📌 _Репозиторий:_ {releaseView.RepositoryName}" +
                            $"\n 🔀 _Ветка:_ {releaseView.BranchName}" +
                            $"\n 👤 _Вызвал:_ {releaseView.UserDisplayName}" +
                            $"\n 💭 _Коммит:_ {releaseView.CommitName}" +
                            $"\n 🕑 _Время сборки:_ {releaseView.BuildTime}" +

                            $"\n \n ⚒️ *Задачи:*" +
                            $"\n {releaseView.WorkItems}";


                       var ids = await telegramService.SendAzureMessageAsync(text);

                        if(deployment.Status != DeploymentStatus.Succeeded)
                        {
                            ReleasesInProgress.Add(new ReleaseInProgress { Id = release.Id, Project = project, ReleaseView = releaseView, TelegramMessages = ids, LastStatus = deployment.Status });
                        }

                    }

                    var idleTime = int.Parse(configuration["IdleTime"]);

                    await Task.Delay(idleTime * 1000);
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, ex.Message);

                    var idleTime = int.Parse(configuration["IdleTime"]);

                    await Task.Delay(idleTime * 1000);
                }
               
            }
        }

        /// <summary>
        /// Обновленние статуса релиза
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ExecuteReleasesInProgress(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting execute releases in progress...");
            var scope = this.serviceProvider.CreateScope();
            var telegramSerivce = scope.ServiceProvider.GetRequiredService<TelegramService>();


            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var deletedReleases = new List<ReleaseInProgress>();

                    if (ReleasesInProgress.Count > 0)
                    {
                        logger.LogInformation($"Found {ReleasesInProgress.Count} releases in progress");

                    }
                    foreach (var releaseItem in ReleasesInProgress)
                    {
                        var release = await releaseClient.GetReleaseAsync(project: releaseItem.Project, releaseId: releaseItem.Id);
                        var deployment = release.Environments.FirstOrDefault().DeploySteps.FirstOrDefault();

                        if (deployment.Status == releaseItem.LastStatus) continue;


                        var releaseView = releaseItem.ReleaseView;



                        var text =
                           $"\n🔔 *Новый релиз:* {releaseView.ReleaseName}" +
                           $"\n 🚩 _Состояние:_ {StatusToView(deployment.Status)}" +
                           $"\n ⚡ _Проект:_ {releaseView.ProjectName}" +
                           $"\n 📌 _Репозиторий:_ {releaseView.RepositoryName}" +
                           $"\n 🔀 _Ветка:_ {releaseView.BranchName}" +
                           $"\n 👤 _Вызвал:_ {releaseView.UserDisplayName}" +
                           $"\n 💭 _Коммит:_ {releaseView.CommitName}" +
                           $"\n 🕑 _Время сборки:_ {releaseView.BuildTime}" +

                           $"\n \n ⚒️ *Задачи:*" +
                           $"\n {releaseView.WorkItems}";

                        await telegramSerivce.EditAzureMessageAsync(releaseItem.TelegramMessages, text);


                        if (deployment.Status == DeploymentStatus.Succeeded || deployment.Status == DeploymentStatus.PartiallySucceeded || deployment.Status == DeploymentStatus.Failed)
                        {
                            deletedReleases.Add(releaseItem);
                        }

                        releaseItem.LastStatus = deployment.Status;

                    }

                    foreach (var removeItem in deletedReleases)
                    {
                        ReleasesInProgress.Remove(removeItem);
                    }

                    var idleTime = int.Parse(configuration["IdleInProgressTime"]);

                    await Task.Delay(idleTime * 1000);

                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);

                    var idleTime = int.Parse(configuration["IdleTime"]);

                    await Task.Delay(idleTime * 1000);
                }
            }
        }

        public async Task ExecuteTelegramMessagesUpdates(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting execute telegram messages...");

            var scope = this.serviceProvider.CreateScope();
            var telegramSerivce = scope.ServiceProvider.GetRequiredService<TelegramService>();

            await telegramSerivce.SendDebug().ConfigureAwait(false);

            await telegramSerivce.GetUpdates(cancellationToken);
        }
        private async Task<List<Release>> GetNewReleasesAsync()
        {
            if(LastScanTime == null) LastScanTime = DateTime.UtcNow;

           // LastScanTime = DateTime.Now - TimeSpan.FromHours(2);

            var releases = await releaseClient.GetReleasesAsync(minCreatedTime: LastScanTime);

            LastScanTime = DateTime.UtcNow;

            return releases;
        }


        public string StatusToView(DeploymentStatus status)
        {
            if (status == DeploymentStatus.Undefined) return "Undefined ❌";
            else if (status == DeploymentStatus.Succeeded) return "Succeeded ✅";
            else if (status == DeploymentStatus.NotDeployed) return "Not Deployed ⌛";
            else if (status == DeploymentStatus.Failed) return "Failed ❌";
            else if (status == DeploymentStatus.InProgress) return "In Progress ⏳";
            else if (status == DeploymentStatus.PartiallySucceeded) return "Partially Succeeded 🆗";
            else return "Undefined ❓";
        }

        /// <summary>
        /// Проверка неудачных билдов
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ExecuteFailedBuilds(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting execute failed builds...");
            await connection.ConnectAsync();
          
            while(!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var builds = await this.GetFailedBuildsAsync(cancellationToken);

                    if (builds.Count > 0)
                    {
                        logger.LogInformation($"Found {builds.Count} failed builds");
                    }
                    
                    foreach(var buildItem in builds)
                    {
                        var build = await buildClient.GetBuildAsync(project: buildItem.Project.Id, buildId: buildItem.Id).ConfigureAwait(false);
                        var commit = await gitClient.GetCommitAsync(commitId: build.SourceVersion, repositoryId: build.Repository.Id).ConfigureAwait(false);

                        TimeSpan buildTime;

                        if (build.StartTime != null)
                        {
                            buildTime = (build.FinishTime - build.StartTime).Value;
                        }
                        else
                        {
                            buildTime = TimeSpan.Zero;
                        }

                        var text = $"⛔ *Failed Build* №[{build.BuildNumber}]({((ReferenceLink)build.Links.Links["web"]).Href})" +
                            $"\n ⚡ _Проект:_ {build.Project.Name}" +
                            $"\n 📌 _Репозиторий:_ {build.Repository.Name}" +
                            $"\n 💭 _Коммит:_ [{commit.Comment.Split("\n")[0]}]({commit.RemoteUrl})" +
                            $"\n 👤 _Автор:_ {commit.Author.Name}" +
                            $"\n 🕑 Время сборки: 0{buildTime.Minutes}:{buildTime.Seconds}";

                        await telegramService.SendAzureMessageAsync(text);
                    }

                    var idleTime = int.Parse(configuration["IdleTime"]);

                    await Task.Delay(idleTime * 1000);

                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);

                    var idleTime = int.Parse(configuration["IdleTime"]);

                    await Task.Delay(idleTime * 1000);
                }
            }
        }

        private async Task<List<Build>> GetFailedBuildsAsync(CancellationToken cancellationToken)
        {
            if (LastFailedBuildsTime == null) LastFailedBuildsTime = DateTime.UtcNow;

            //LastFailedBuildsTime = DateTime.UtcNow - TimeSpan.FromDays(20);

            var buildsList = new List<Build>();

            var projects = await projectClient.GetProjects().ConfigureAwait(false);

            foreach (var project in projects)
            {
                var builds = await buildClient.GetBuildsAsync(minFinishTime: LastFailedBuildsTime, project: project.Id, resultFilter: BuildResult.Failed).ConfigureAwait(false);

                buildsList.AddRange(builds);
            }

            LastFailedBuildsTime = DateTime.UtcNow;


            return buildsList;
        }
    }
}
