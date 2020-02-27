using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Threading.Tasks;
using System.Threading;
using MySql.Data.MySqlClient;

namespace _2DWaifus
{
    class Program
    {
        public DiscordClient bot;
        public CommandsNextExtension Commands { get; set; }

        public static Program instance = new Program();
        static void Main(string[] args)
        {
            GlobalVars.initVars();
            Thread bgThread = new Thread(new ParameterizedThreadStart(bgThreadVoid)) { IsBackground = true };
            bgThread.Start();
            Console.WriteLine("[2DWaifus][Thread] Background thread started.");
            instance.startBotAsync().GetAwaiter().GetResult();
        }

        internal static void bgThreadVoid(object info)
        {
            GlobalVars.connection.Open();
            MySqlCommand allCmd = new MySqlCommand("SELECT * FROM waifus", GlobalVars.connection);
            MySqlDataReader allReader = allCmd.ExecuteReader();
            while (allReader.Read())
            {
                GlobalVars.allList.Add(allReader.GetString(1));
            }
            allReader.Close();
            MySqlCommand unownedCommand = new MySqlCommand("SELECT * FROM waifus WHERE owned = '0'", GlobalVars.connection);
            MySqlDataReader unownedReader = unownedCommand.ExecuteReader();
            while (unownedReader.Read())
            {
                GlobalVars.unownedList.Add(unownedReader.GetInt16(0).ToString()); // IDs
            }
            unownedReader.Close();
            GlobalVars.connection.Close();
            Console.WriteLine("[2DWaifus][Thread] Background thread has completed.");
        }       
        private Task botReady(ReadyEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "2DWaifus", "Bot is ready", DateTime.Now);

            return Task.CompletedTask;
        }

        private Task botGuildAvailable(GuildCreateEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "2DWaifus", $"Guild Available: {e.Guild.Name}", DateTime.Now);

            return Task.CompletedTask;
        }

        private Task botErrorHandler(ClientErrorEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "2DWaifus", $"Exception occurred: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);

            return Task.CompletedTask;
        }

        private Task commandsExecuted(CommandExecutionEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "2DWaifus", $"{e.Context.User.Username} issued the command '{e.Command.QualifiedName}'", DateTime.Now);

            return Task.CompletedTask;
        }

        private async Task commandsError(CommandErrorEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "2DWaifus", $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

            if (e.Exception is ChecksFailedException ex)
            {
                DiscordEmoji emoji = DiscordEmoji.FromName(e.Context.Client, ":x:");

                var embed = new DiscordEmbedBuilder
                {
                    Title = "",
                    Description = $"{emoji} You do not have permission to use this command.",
                    Color = new DiscordColor(0xff0000)
                };

                await e.Context.RespondAsync("", embed: embed);
            }
        }

        public async Task botStatusAsync(ReadyEventArgs e)
        {
            DiscordActivity activity = new DiscordActivity
            {
                ActivityType = ActivityType.Playing,
                Name = "/2d helpme",
            };
            await this.bot.UpdateStatusAsync(activity);
        }

        public async Task startBotAsync()
        {
            //load the JASOOON!
            var config = new DiscordConfiguration
            {
                Token = GlobalVars.tokenJson.token,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                LogLevel = LogLevel.Info,
                UseInternalLogHandler = true
            };

            this.bot = new DiscordClient(config);

            this.bot.Ready += this.botReady;
            this.bot.Ready += this.botStatusAsync;
            this.bot.GuildAvailable += this.botGuildAvailable;
            this.bot.ClientErrored += this.botErrorHandler;

            var commandConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new[] { GlobalVars.confJson.prefix },

                EnableMentionPrefix = true
            };

            this.Commands = this.bot.UseCommandsNext(commandConfig);

            this.Commands.CommandExecuted += this.commandsExecuted;
            this.Commands.CommandErrored += this.commandsError;

            this.Commands.RegisterCommands<_2DWaifusAdmin>();
            this.Commands.RegisterCommands<_2DWaifusRolls>();
            this.Commands.RegisterCommands<_2DWaifusBase>();

            await this.bot.ConnectAsync();

            //prevent bot from stopping
            await Task.Delay(-1);
        }
    }

    class JConf
    {
        public string prefix = "";
    }
    class JToken
    {
        public string token = "";
    }
    class JConnection
    {
        public string connection = "";
    }
}
