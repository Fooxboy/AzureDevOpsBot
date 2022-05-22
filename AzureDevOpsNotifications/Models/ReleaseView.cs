using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsNotifications.Models
{
    public class ReleaseView
    {
        public string ReleaseName { get; set; }
        public string ReleaseState { get; set; }
        public string ProjectName { get; set; }
        public string RepositoryName { get; set; }
        public string BranchName { get; set; }
        public string UserDisplayName { get; set; }
        public string CommitName { get; set; }
        public string BuildTime { get; set; }
        public string WorkItems { get; set; }
    }
}
