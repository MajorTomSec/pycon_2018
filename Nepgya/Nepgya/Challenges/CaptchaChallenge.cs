// 
// This file is licensed under the terms of the Simple Non Code License (SNCL) 2.3.0.
// The full license text can be found in the file named LICENSE.txt.
// Written originally by Alexandre Quoniou in 2018.
//

using System;
using System.DrawingCore;
using System.DrawingCore.Imaging;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

namespace Nepgya.Challenges
{
    [Group("captcha_challenge")]
    [Description("Solve a captcha.")]
    class CaptchaChallenge
    {
        [Command("start")]
        [Description("Starts the challenge.")]
        public async Task Start(CommandContext ctx)
        {
            if (ctx.Channel.IsPrivate)
            {
                await ctx.TriggerTypingAsync();
                var interactivity = ctx.Client.GetInteractivityModule();

                var text = StaticRandom.NextString(StaticRandom.Next(10, 20), "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPRQSTUVWXYZ0123456789");
                var font = new Font(SystemFonts.DefaultFont.FontFamily, StaticRandom.Next(8, 48), (FontStyle)StaticRandom.Next(16));
                SizeF textSize;

                using (var image = new Bitmap(1, 1))
                using (var drawing = Graphics.FromImage(image))
                {
                    textSize = drawing.MeasureString(text, font);
                }

                using (var image = new Bitmap((int)textSize.Width, (int)textSize.Height))
                using (var drawing = Graphics.FromImage(image))
                using (var brush = new SolidBrush(Color.Black))
                using (var stream = new MemoryStream())
                {
                    drawing.Clear(Color.White);
                    drawing.DrawString(text, font, brush, 0, 0);

                    image.Save(stream, ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    await ctx.RespondWithFileAsync(stream, "Captcha.png");
                }
                
                var response = await interactivity.WaitForMessageAsync(message => message.Author.Id == ctx.User.Id &&
                                                                                  message.Channel.IsPrivate, TimeSpan.FromSeconds(2.0));
                await ctx.TriggerTypingAsync();

                if (response == null)
                {
                    await (await ctx.RespondAsync("Too late!")).CreateReactionAsync(DiscordEmoji.FromUnicode("⌛"));
                    return;
                }

                if (text.Equals(response.Message.Content, StringComparison.Ordinal))
                {
                    await response.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🎉"));
                    await ctx.RespondAsync("You did it! The flag is Pycon_{{345y_0cr}}.");
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
                await ctx.RespondAsync($"The goal of this challenge is to solve a captcha. That's it, easy. Oh and, you have two seconds.");
            }
        }
    }
}