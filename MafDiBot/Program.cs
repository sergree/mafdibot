using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;

namespace MafDiBot
{
    class Program
    {
        public static string assemblyName;
        public static System.Version assemblyVersion;
        public static DiscordClient discord;
        public static CommandsNextModule commands;

        static void Main(string[] args)
        {
            assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Game.Load();
            View.Load();
            Config.Load();
            if (Config.Loaded)
            {
                GameMessages.Load();
                MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            else
            {
                View.Print("Не могу найти config.json!");
                View.Wait();
            }
        }

        static async Task MainAsync(string[] args)
        {
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = Config.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            });

            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = "!",
                CaseSensitive = false,
                EnableDefaultHelp = false
            });

            commands.RegisterCommands<Commands>();

            discord.Ready += Handlers.OnReady;
            discord.Ready += HandlersAsync.OnReady;

            discord.GuildAvailable += Handlers.OnServerAvailable;
            discord.GuildAvailable += HandlersAsync.OnServerAvailable;

            discord.ClientErrored += Handlers.OnError;

            discord.MessageCreated += Handlers.OnMessage;
            discord.MessageCreated += HandlersAsync.OnMessage;
            discord.GuildMemberUpdated += Handlers.OnMemberUpdate;
            discord.GuildMemberUpdated += HandlersAsync.OnMemberUpdate;
            discord.GuildMemberRemoved += Handlers.OnMemberRemove;
            discord.GuildMemberRemoved += HandlersAsync.OnMemberRemove;

            commands.CommandExecuted += Handlers.CommandExecuted;
            commands.CommandErrored += Handlers.CommandErrored;

            await discord.ConnectAsync();

            await Task.Delay(-1);
        }
    }
}
