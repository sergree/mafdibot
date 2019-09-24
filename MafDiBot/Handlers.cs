using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;

namespace MafDiBot
{
    static class Handlers
    {
        public static Task OnReady(ReadyEventArgs e)
        {
            View.Print($"Успешно залогинился под пользователем {e.Client.CurrentUser.Username} ID {e.Client.CurrentUser.Id}!");
            return Task.CompletedTask;
        }

        public static Task OnServerAvailable(GuildCreateEventArgs e)
        {
            View.Print($"Сервер доступен: {e.Guild.Name}");
            return Task.CompletedTask;
        }

        public static Task OnError(ClientErrorEventArgs e)
        {
            View.PrintError($"Ошибка клиента: {e.Exception.GetType()}: {e.Exception.Message}");
            View.WriteToErrorsLog($"Ошибка клиента: {e.Exception.GetType()}: {e.Exception.Message}");
            return Task.CompletedTask;
        }

        public static Task OnMessage(MessageCreateEventArgs e)
        {
            if (e.Message.Author != e.Client.CurrentUser)
            {
                if (e.Author is DiscordMember member)
                {
                    // Сообщения на сервере
                    View.Print($"{member.DisplayName} -> #{e.Channel.Name} ({e.Guild.Name}): {e.Message.Content}");
                }
                else
                {
                    // Приватные сообщения
                    View.Print($"!{e.Author.Username} -> DM!: {e.Message.Content}");
                }
            }
            return Task.CompletedTask;
        }

        public static Task CommandExecuted(CommandExecutionEventArgs e)
        {
            View.Print($"Команда '{e.Command.Name}' выполнена");
            return Task.CompletedTask;
        }

        public static Task CommandErrored(CommandErrorEventArgs e)
        {
            if (e.Command != null)
            {
                View.PrintError($"Ошибка команды '{e.Command.Name}': {e.Exception.GetType()}: {e.Exception.Message}");
                if (!e.Exception.Message.Contains("One or more pre-execution checks failed")) // не логируем эту частую ошибку
                {
                    View.WriteToErrorsLog($"Ошибка команды '{e.Command.Name}': {e.Exception.GetType()}: {e.Exception.Message}");
                }
            }
            return Task.CompletedTask;
        }

        public static Task OnMemberUpdate(GuildMemberUpdateEventArgs e)
        {
            return Task.CompletedTask;
        }

        public static Task OnMemberRemove(GuildMemberRemoveEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
