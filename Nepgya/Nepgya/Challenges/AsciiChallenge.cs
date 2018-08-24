// 
// This file is licensed under the terms of the Simple Non Code License (SNCL) 2.3.0.
// The full license text can be found in the file named LICENSE.txt.
// Written originally by Alexandre Quoniou in 2018.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using WenceyWang.FIGlet;

namespace Nepgya
{
    [Group("ascii_challenge")]
    [Description("Solve an ascii captcha.")]
    class AsciiChallenge
    {
        private static readonly List<byte[]> Fonts = new DirectoryInfo("Figlet").GetFiles("*.flf").Select(file => File.ReadAllBytes(file.FullName)).ToList();

        [Command("start")]
        [Description("Starts the challenge.")]
        public async Task Start(CommandContext ctx)
        {
            if (ctx.Channel.IsPrivate)
            {
                await ctx.TriggerTypingAsync();
                var interactivity = ctx.Client.GetInteractivityModule();

                var text = StaticRandom.NextString(StaticRandom.Next(6, 8), "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPRQSTUVWXYZ0123456789");
                
                var fontIndex = StaticRandom.Next(-1, Fonts.Count);
                using (var font = fontIndex == -1 ? null : new MemoryStream(Fonts[fontIndex]))
                {
                    var asciiText = new AsciiArt(text, font == null ? null : new FIGletFont(font));
                    await ctx.RespondAsync($"```{asciiText}```");
                }
                
                var response = await interactivity.WaitForMessageAsync(message => message.Author.Id == ctx.User.Id &&
                                                                                  message.Channel.IsPrivate, TimeSpan.FromSeconds(1.0));
                await ctx.TriggerTypingAsync();

                if (response == null)
                {
                    await (await ctx.RespondAsync("Too late!")).CreateReactionAsync(DiscordEmoji.FromUnicode("⌛"));
                    return;
                }

                if (text.Equals(response.Message.Content, StringComparison.Ordinal))
                {
                    await response.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🎉"));
                    await ctx.RespondAsync("You did it! The flag is Pycon_{{45c11_M45t3r}}.");
                }
                else
                {
                    await (await ctx.RespondAsync("You lost!")).CreateReactionAsync(DiscordEmoji.FromUnicode("😏"));
                }
            }
        }

        [Command("info")]
        [Description("Get detailed information on the challenge.")]
        public async Task GetInformation(CommandContext ctx)
        {
            if (ctx.Channel.IsPrivate)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"Do you like ASCII art? I do.{Environment.NewLine}" +
                                       $"You have one second to solve the challenge.");
            }
        }
    }
}