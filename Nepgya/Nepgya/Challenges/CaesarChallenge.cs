// 
// This file is licensed under the terms of the Simple Non Code License (SNCL) 2.3.0.
// The full license text can be found in the file named LICENSE.txt.
// Written originally by Alexandre Quoniou in 2018.
//

using System;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

namespace Nepgya
{
    [Group("caesar_challenge")]
    [Description("Basically an introduction to communication with the bot.")]
    class CaesarChallenge
    {
        private readonly char[] _charset = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

        [Command("start")]
        [Description("Starts the challenge.")]
        public async Task Start(CommandContext ctx)
        {
            if (ctx.Channel.IsPrivate)
            {
                await ctx.TriggerTypingAsync();
                var interactivity = ctx.Client.GetInteractivityModule();

                var challengeSolutionBuilder = new StringBuilder();
                for (int i = 0; i < 32; i++) challengeSolutionBuilder.Append(_charset[StaticRandom.Next(0, _charset.Length)]);
                var challengeSolution = challengeSolutionBuilder.ToString();

                await ctx.RespondAsync($"Decode this, you have 1 second: {Rot13(challengeSolution)}");
                var response = await interactivity.WaitForMessageAsync(message => message.Author.Id == ctx.User.Id &&
                                                                                  message.Channel.IsPrivate &&
                                                                                  challengeSolution.Equals(message.Content, StringComparison.Ordinal), TimeSpan.FromSeconds(1.0));
                await ctx.TriggerTypingAsync();
                if (response != null)
                {
                    await response.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🎉"));
                    await ctx.RespondAsync($"You did it! The flag is Pycon_{{T00_34sy_4_u!}}.");
                }
                else
                {
                    await (await ctx.RespondAsync("Too late!")).CreateReactionAsync(DiscordEmoji.FromUnicode("⌛"));
                }
            }
        }

        private string Rot13(string value)
        {
            var encoded = new StringBuilder();
            foreach (var c in value)
                encoded.Append((char)(97 + (c - 'a' + 13) % 26));
            return encoded.ToString();
        }
    }
}