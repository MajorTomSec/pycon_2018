// 
// This file is licensed under the terms of the Simple Non Code License (SNCL) 2.3.0.
// The full license text can be found in the file named LICENSE.txt.
// Written originally by Alexandre Quoniou in 2018.
//

using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

namespace Nepgya.Challenges
{
    [Group("sticks_challenge")]
    [Description("Play pick-up sticks against the bot.")]
    class SticksChallenge
    {
        [Command("start")]
        [Description("Starts the challenge.")]
        public async Task Start(CommandContext ctx)
        {
            if (ctx.Channel.IsPrivate)
            {
                await ctx.TriggerTypingAsync();
                var interactivity = ctx.Client.GetInteractivityModule();

                for (int i = 0; i < 2; i++)
                {
                    var sticks = StaticRandom.Next(10, 20);
                    var isPlayerTurn = StaticRandom.Next(2) == 1;

                    await ctx.RespondAsync($"There are {sticks} sticks for this game and {(isPlayerTurn ? "you begin!" : "I begin!")}");

                    while (sticks > 0)
                    {
                        if (isPlayerTurn)
                        {
                            var response = await interactivity.WaitForMessageAsync(message => message.Author.Id == ctx.User.Id &&
                                                                                              message.Channel.IsPrivate, TimeSpan.FromSeconds(1.0));

                            if (response == null)
                            {
                                await (await ctx.RespondAsync("Too late! You lost!")).CreateReactionAsync(DiscordEmoji.FromUnicode("⌛"));
                                return;
                            }

                            if (int.TryParse(response.Message.Content, out var sticksTaken) && sticksTaken > 0 && sticksTaken < 4)
                            {
                                sticks -= sticksTaken;
                            }
                            else
                            {
                                await (await ctx.RespondAsync("That last input was incorrect! You lost! If you need help, read the information on this challenge.")).CreateReactionAsync(DiscordEmoji.FromUnicode("❓"));
                            }
                        }
                        else
                        {
                            await ctx.TriggerTypingAsync();

                            if (sticks <= 3)
                            {
                                await ctx.RespondAsync($"{sticks}");
                                sticks = 0;
                            }
                            else
                            {
                                var sticksTaken = StaticRandom.Next(1, 4);
                                await ctx.RespondAsync($"{sticksTaken}");
                                sticks -= sticksTaken;
                            }
                        }

                        isPlayerTurn = !isPlayerTurn;
                    }

                    if (!isPlayerTurn)
                    {
                        await (await ctx.RespondAsync("You won!")).CreateReactionAsync(DiscordEmoji.FromUnicode("🙌"));
                    }
                    else
                    {
                        await (await ctx.RespondAsync("You lost!")).CreateReactionAsync(DiscordEmoji.FromUnicode("😏"));
                        return;
                    }
                }

                await ctx.TriggerTypingAsync();
                await (await ctx.RespondAsync("You did it! The flag is Pycon_{{G1mm3_7h3_s71ck}}.")).CreateReactionAsync(DiscordEmoji.FromUnicode("🎉"));
            }
        }

        [Command("info")]
        [Description("Get detailed information on the challenge.")]
        public async Task GetInformation(CommandContext ctx)
        {
            if (ctx.Channel.IsPrivate)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"The goal of this challenge is simple.{Environment.NewLine}" +
                                       $"You will play tic tac toe against me twice.{Environment.NewLine}" +
                                       $"If you lose a single time, you're out.{Environment.NewLine}" +
                                       $"If you never lose, I'll give you the flag.{Environment.NewLine}" +
                                       $"{Environment.NewLine}" +
                                       $"I will tell you if you are the one starting at the beginning of each game as well as the amount of sticks (between 10 and 20).{Environment.NewLine}" +
                                       $"You only have to tell me the amount of sticks you want to take (between 1 and 3).{Environment.NewLine}" +
                                       $"{Environment.NewLine}" +
                                       $"You only have 1 second to give me your move at each turn.");
            }
        }
    }
}