// 
// This file is licensed under the terms of the Simple Non Code License (SNCL) 2.3.0.
// The full license text can be found in the file named LICENSE.txt.
// Written originally by Alexandre Quoniou in 2018.
//

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using Nepgya.Challenges;
using Newtonsoft.Json;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Nepgya
{
    class Program
    {
        private static Configuration _configuration;

        static async Task Main(string[] args)
        {
            var cancelKeyPress = new TaskCompletionSource<object>();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cancelKeyPress.SetResult(null);
            };

            _configuration = JsonConvert.DeserializeObject<Configuration>(await File.ReadAllTextAsync("config.json"));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(_configuration.MinimumLogLevel)
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.RollingFile(_configuration.Logs)
                .CreateLogger();

            Log.Logger.Information("Nepgya is starting.");

            var discordConfiguration = new DiscordConfiguration()
            {
                Token = _configuration.DiscordToken,
                TokenType = _configuration.DiscordTokenType,
                LogLevel = LogLevel.Debug,
            };

            var discordClient = new DiscordClient(discordConfiguration);
            discordClient.DebugLogger.LogMessageReceived += DebugLogger_LogMessageReceived;

            discordClient.UseInteractivity(new InteractivityConfiguration());
            var commands = discordClient.UseCommandsNext(new CommandsNextConfiguration()
            {
                CaseSensitive = false,
                StringPrefix = "."
            });
            commands.CommandExecuted += Commands_CommandExecuted;
            commands.RegisterCommands<CaesarChallenge>();
            commands.RegisterCommands<TicTacToeChallenge>();
            commands.RegisterCommands<SticksChallenge>();
            commands.RegisterCommands<CaptchaChallenge>();
            commands.RegisterCommands<AsciiChallenge>();
            commands.RegisterCommands<MazeChallenge>();
            commands.RegisterCommands<PuzzleChallenge>();

            await discordClient.ConnectAsync();

            await Task.WhenAny(Task.Delay(Timeout.InfiniteTimeSpan), cancelKeyPress.Task);
            await discordClient.DisconnectAsync();
        }

        private static void DebugLogger_LogMessageReceived(object sender, DSharpPlus.EventArgs.DebugLogMessageEventArgs e)
        {
            Log
                .Logger
                .ForContext(nameof(e.Application), e.Application)
                .Write(e.Level.ToLogEventLevel(), e.Message);
        }

        private static Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            Log
                .Logger
                .ForContext(e.Command.Name, e.Command)
                .Information("{User} ran {Command}.", e.Context.User, e.Command);
            return Task.CompletedTask;
        }
    }
}
