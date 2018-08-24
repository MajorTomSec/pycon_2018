// 
// This file is licensed under the terms of the Simple Non Code License (SNCL) 2.3.0.
// The full license text can be found in the file named LICENSE.txt.
// Written originally by Alexandre Quoniou in 2018.
//

using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

namespace Nepgya
{
    [Group("tictactoe_challenge")]
    [Description("Play tic tac toe against the bot.")]
    class TicTacToeChallenge
    {
        enum GameValue
        {
            Player = 'P',
            Bot = 'B',
            Draw = 'D',
            Timeout = 'T',
            IncorrectInput = 'I',
            Empty = '\0'
        }

        [Command("start")]
        [Description("Starts the challenge.")]
        public async Task Start(CommandContext ctx)
        {
            if (ctx.Channel.IsPrivate)
            {
                await ctx.TriggerTypingAsync();
                var interactivity = ctx.Client.GetInteractivityModule();

                for (int i = 0; i < 3; i++)
                {
                    var result = await PlayGame(ctx, interactivity);

                    await ctx.TriggerTypingAsync();
                    switch (result)
                    {
                        case GameValue.Player:
                            await (await ctx.RespondAsync("You won!")).CreateReactionAsync(DiscordEmoji.FromUnicode("🙌"));
                            break;
                        case GameValue.Bot:
                            await (await ctx.RespondAsync("You lost!")).CreateReactionAsync(DiscordEmoji.FromUnicode("😏"));
                            return;
                        case GameValue.Draw:
                            await (await ctx.RespondAsync("It's a draw! You lost!")).CreateReactionAsync(DiscordEmoji.FromUnicode("🎲"));
                            return;
                        case GameValue.Timeout:
                            await (await ctx.RespondAsync("Too late! You lost!")).CreateReactionAsync(DiscordEmoji.FromUnicode("⌛"));
                            return;
                        case GameValue.IncorrectInput:
                            await (await ctx.RespondAsync("That last input was incorrect! You lost! If you need help, read the information on this challenge.")).CreateReactionAsync(DiscordEmoji.FromUnicode("❓"));
                            return;
                    }
                }

                await ctx.TriggerTypingAsync();
                await (await ctx.RespondAsync("You did it! The flag is Pycon_{{T1cT4cT03_0wn3r}}.")).CreateReactionAsync(DiscordEmoji.FromUnicode("🎉"));
            }
        }

        private async Task<GameValue> PlayGame(CommandContext ctx, InteractivityModule interactivity)
        {
            var board = new GameValue[9];
            var emptySlots = Enumerable.Range(0, 9).ToList();
            var myTurn = StaticRandom.Next(2) == 1;
            GameValue winner = GameValue.Empty;

            if (!myTurn)
                await ctx.RespondAsync($"You are the one starting!");

            while ((winner = GetWinner(board)) == GameValue.Empty)
            {
                if (myTurn)
                {
                    await ctx.TriggerTypingAsync();

                    var nextSlot = StaticRandom.Next(0, emptySlots.Count);
                    board[emptySlots[nextSlot]] = GameValue.Bot;
                    await ctx.RespondAsync($"{emptySlots[nextSlot] % 3 + 1}:{emptySlots[nextSlot] / 3 + 1}");

                    emptySlots.RemoveAt(nextSlot);
                }
                else
                {
                    var response = await interactivity.WaitForMessageAsync(message => message.Author.Id == ctx.User.Id &&
                                                                                      message.Channel.IsPrivate, TimeSpan.FromSeconds(1.0));
                    if (response == null)
                        return GameValue.Timeout;

                    var messageParts = response.Message.Content.Split(':');
                    if (messageParts.Length == 2 
                        && int.TryParse(messageParts[0], out var x)
                        && int.TryParse(messageParts[1], out var y)
                        && x >= 1 && x <= 3
                        && y >= 1 && y <= 3)
                    {
                        var nextSlot = (y - 1) * 3 + (x - 1);
                        if (emptySlots.Contains(nextSlot))
                        {
                            board[nextSlot] = GameValue.Player;
                            emptySlots.Remove(nextSlot);
                        }
                        else
                        {
                            return GameValue.IncorrectInput;
                        }
                    }
                    else
                    {
                        return GameValue.IncorrectInput;
                    }
                }

                myTurn = !myTurn;
            }

            return winner;
        }

        private GameValue GetWinner(GameValue[] board)
        {
            if (board[0] != GameValue.Empty && board[0] == board[1] && board[1] == board[2]) return board[0];
            if (board[3] != GameValue.Empty && board[3] == board[4] && board[4] == board[5]) return board[3];
            if (board[6] != GameValue.Empty && board[6] == board[7] && board[7] == board[8]) return board[6];

            if (board[0] != GameValue.Empty && board[0] == board[3] && board[3] == board[6]) return board[0];
            if (board[1] != GameValue.Empty && board[1] == board[4] && board[4] == board[7]) return board[1];
            if (board[2] != GameValue.Empty && board[2] == board[5] && board[5] == board[8]) return board[2];

            if (board[0] != GameValue.Empty && board[0] == board[4] && board[4] == board[8]) return board[0];
            if (board[6] != GameValue.Empty && board[6] == board[4] && board[4] == board[2]) return board[6];

            if (board.All(cell => cell != GameValue.Empty)) return GameValue.Draw;

            return GameValue.Empty;
        }
        

        [Command("info")]
        [Description("Get detailed information on the challenge.")]
        public async Task GetInformation(CommandContext ctx)
        {
            if (ctx.Channel.IsPrivate)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"The goal of this challenge is simple.{Environment.NewLine}" +
                                       $"You will play tic tac toe against me 3 times.{Environment.NewLine}" +
                                       $"If you lose a single time, you're out.{Environment.NewLine}" +
                                       $"If you never lose, I'll give you the flag.{Environment.NewLine}" +
                                       $"{Environment.NewLine}" +
                                       $"The bot will tell you if you are the one starting at the beginning of each game.{Environment.NewLine}" +
                                       $"Moves will have to be written this way: X:Y.{Environment.NewLine}" +
                                       $"For example, if I want to play in the middle cell of the board, I'll say 2:2.{Environment.NewLine}" +
                                       $"If you want to play on the top-right cell of the board, you'll have to say 3:1.{Environment.NewLine}" +
                                       $"{Environment.NewLine}" +
                                       $"You only have 1 second to give me your move at each turn.");
            }
        }
    }
}