using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using System.Linq;

namespace MafDiBot
{
    static class HandlersAsync
    {
        public static async Task OnReady(ReadyEventArgs e)
        {
            await Task.FromResult(0); // Пока типа пусто
        }

        public static async Task OnServerAvailable(GuildCreateEventArgs e)
        {
            if (e.Guild.Id == Config.GuildId)
            {
                await Config.LoadEntities();
                await Program.discord.UpdateStatusAsync(Config.ClientGame);
                await View.SendMain(GameMessages.ShowHello());
                await View.Wipe();
                await View.DisableMod();
                await PingMe.Load();
            }
        }

        public static async Task OnMessage(MessageCreateEventArgs e)
        {
            await Task.FromResult(0); // Пока типа пусто
        }

        public static async Task OnMemberUpdate(GuildMemberUpdateEventArgs e)
        {
            if (Game.Players.ContainsKey(e.Member) && !Game.PlayersToKick.Contains(e.Member))
            {
                if (e.RolesAfter.ToList().Contains(Config.RoleBad))
                {
                    await Game.AddToPlayersToKick(e.Member);
                }
            }
            await Task.FromResult(0); // Пока типа пусто
        }

        public static async Task OnMemberRemove(GuildMemberRemoveEventArgs e)
        {
            if (Game.Players.ContainsKey(e.Member))
            {
                await Game.AddToPlayersToKick(e.Member);
            }
        }
    }
}
