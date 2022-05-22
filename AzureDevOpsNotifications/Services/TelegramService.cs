using AzureDevOpsNotifications.Models;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;
using Telegram.Bot.Types.ReplyMarkups;

namespace AzureDevOpsNotifications.Services
{
    public class TelegramService
    {
        private readonly TelegramBotClient botClient;
        private readonly IConfiguration configuration;
        private readonly ILogger<TelegramService> logger;
        private readonly IServiceProvider serviceProvider;

        private readonly Dictionary<string, Func<Message, Task>> commands;

        
        public TelegramService(IConfiguration configuration, ILogger<TelegramService> logger, IServiceProvider serviceProvider)
        {
            this.configuration = configuration;

            var token = configuration["TelegramToken"];
            botClient = new TelegramBotClient(token);
            this.logger = logger;
            this.commands = new Dictionary<string, Func<Message, Task>>();
            this.serviceProvider = serviceProvider;
            
            commands.Add("chat", async (m) => await ChatCommand(m));
        }
        
        public async Task<List<(int, ChatId)>> SendAzureMessageAsync(string text)
        {
            var ids = new List<(int, ChatId)>();
            var chatIds = configuration.GetSection("TelegramChatIds").Get<long[]>();

            foreach (var chatId in chatIds)
            {
                logger.LogInformation($"Send telegram message:\n '{text}'\n To {chatId}...");
                var msg = await botClient.SendTextMessageAsync(chatId, text, Telegram.Bot.Types.Enums.ParseMode.Markdown);
                ids.Add((msg.MessageId, msg.Chat.Id));
            }

            return ids;
        }

        public async Task EditAzureMessageAsync(List<(int MsgId, ChatId ChatId)> messages, string text)
        {
            foreach (var message in messages)
            {
                logger.LogInformation($"Edit telegram message:\n '{text}'\n...");

                await botClient.EditMessageTextAsync(message.ChatId, message.MsgId, text, Telegram.Bot.Types.Enums.ParseMode.Markdown);
            }
        }

        public async Task ExecuteMessage(Message msg)
        {
            
            if (msg.Text == null)
            {
                msg.Text = msg.Caption;
            }

            if(msg.Text != null)
            {
                var command = msg.Text.Split(" ")[0].Replace("@namebot", "").Replace("/", "");

                try
                {
                    await commands[command.ToLower()].Invoke(msg);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                    await botClient.SendTextMessageAsync(msg.Chat.Id, $"❌ *Exception*: \n {ex.Message}", ParseMode.Markdown);
                }
            }
            
           
        }
        
        private async Task ExecuteUpdates(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == UpdateType.Message) await ExecuteMessage(update.Message);

            }catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        private  Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            logger.LogError(ErrorMessage);
            return Task.CompletedTask;
        }

        public async Task GetUpdates(CancellationToken cancellationToken)
        {

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[]{ UpdateType.Message } // receive all update types
            };
            botClient.StartReceiving(ExecuteUpdates, HandleErrorAsync, receiverOptions, cancellationToken);

            await Task.Delay(1000);
        }

        public async Task SendDebug()
        {
            //await botClient.SendTextMessageAsync(221647307, "Bot started.", ParseMode.Markdown);
        }


        private async Task ChatCommand(Message msg)
        {
            var text = $"✅ ChatId: `{msg.Chat.Id}`";

            await botClient.SendTextMessageAsync(msg.Chat.Id, text, ParseMode.Markdown);
        }
    }
}