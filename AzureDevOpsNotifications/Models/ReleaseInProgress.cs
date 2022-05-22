using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace AzureDevOpsNotifications.Models
{
    public class ReleaseInProgress
    {
        public int Id { get; set; }
        public Guid Project { get; set; }
        public List<(int MsgId, ChatId ChatId)> TelegramMessages { get; set; }
        public DeploymentStatus LastStatus { get; set; }
        public ReleaseView ReleaseView { get; set; }

    }
}
