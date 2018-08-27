// 
// This file is licensed under the terms of the Simple Non Code License (SNCL) 2.3.0.
// The full license text can be found in the file named LICENSE.txt.
// Written originally by Alexandre Quoniou in 2018.
//

using System;
using System.Collections.Generic;
using System.DrawingCore;
using System.DrawingCore.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

namespace Nepgya.Challenges
{
    [Group("puzzle_challenge")]
    [Description("Solve this puzzle.")]
    class PuzzleChallenge
    {
        private static readonly FileInfo[] Pictures = new DirectoryInfo("Puzzle").GetFiles("*.*");

        [Command("start")]
        [Description("Starts the challenge.")]
        public async Task Start(CommandContext ctx)
        {
            if (ctx.Channel.IsPrivate)
            {
                await ctx.TriggerTypingAsync();
                var interactivity = ctx.Client.GetInteractivityModule();

                string challenge;

                const int horizontalBlocks = 4;
                const int verticalBlocks = 4;

                using (var picture = Pictures[StaticRandom.Next(Pictures.Length)].OpenRead())
                using (var bitmap = new Bitmap(picture))
                using (var puzzle = new Bitmap(bitmap.Width - bitmap.Width % horizontalBlocks, bitmap.Height - bitmap.Height % verticalBlocks, PixelFormat.Format24bppRgb))
                {
                    var rectangle = new Rectangle(0, 0, puzzle.Width, puzzle.Height);
                    var bytesPerPixel = Image.GetPixelFormatSize(puzzle.PixelFormat) / 8;

                    var blockWidth = puzzle.Width / horizontalBlocks;
                    var blockHeight = puzzle.Height / verticalBlocks;

                    var bitmapData = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, puzzle.PixelFormat);
                    var bitmapBytes = new byte[bitmapData.Stride * rectangle.Height];
                    Marshal.Copy(bitmapData.Scan0, bitmapBytes, 0, bitmapBytes.Length);
                    bitmap.UnlockBits(bitmapData);

                    var availableBlocks = Enumerable.Range(0, horizontalBlocks * verticalBlocks).ToList();
                    var blocks = new List<int>();
                    while (availableBlocks.Count > 0)
                    {
                        var blockIndex = StaticRandom.Next(availableBlocks.Count);
                        blocks.Add(availableBlocks[blockIndex]);
                        availableBlocks.RemoveAt(blockIndex);
                    }
                    challenge = string.Join('-', blocks);
                    
                    var puzzleBytes = new byte[bitmapBytes.Length];
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        var originX = blockWidth * (i % horizontalBlocks);
                        var originY = blockHeight * (i / verticalBlocks);
                    
                        var targetX = blockWidth * (blocks[i] % horizontalBlocks);
                        var targetY = blockHeight * (blocks[i] / verticalBlocks);

                        for (int y = 0; y < blockHeight; y++)
                        {
                            Array.Copy(bitmapBytes, (originX + (originY + y) * rectangle.Width) * bytesPerPixel, puzzleBytes, (targetX + (targetY + y) * rectangle.Width) * bytesPerPixel, blockWidth * bytesPerPixel);
                        }
                    }

                    var puzzleData = puzzle.LockBits(rectangle, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                    Marshal.Copy(puzzleBytes, 0, puzzleData.Scan0, puzzleBytes.Length);
                    puzzle.UnlockBits(puzzleData);

                    using (var stream = new MemoryStream())
                    {
                        puzzle.Save(stream, ImageFormat.Jpeg);

                        stream.Seek(0, SeekOrigin.Begin);
                        await ctx.RespondWithFileAsync(stream, "Puzzle.jpg");
                    }
                }
                
                var response = await interactivity.WaitForMessageAsync(message => message.Author.Id == ctx.User.Id &&
                                                                                      message.Channel.IsPrivate, TimeSpan.FromSeconds(5.0));
                await ctx.TriggerTypingAsync();

                if (response == null)
                {
                    await (await ctx.RespondAsync("Too late!")).CreateReactionAsync(DiscordEmoji.FromUnicode("⌛"));
                    return;
                }

                if (challenge.Equals(response.Message.Content, StringComparison.Ordinal))
                {
                    await response.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🎉"));
                    await ctx.RespondAsync("You did it! The flag is Pycon_{{35c4p3_7h3_L4byr1n7h}}.");
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
                await ctx.RespondAsync($"This challenge might be a bit harder than the others, so hang tight!{Environment.NewLine}" +
                                       $"You'll receive a 4x4 puzzle everytime you ask me for a challenge (it'll never be the same), you'll have to solve it.{Environment.NewLine}" +
                                       $"Since the pieces are shuffled randomly, you'll have to tell me where each piece is.{Environment.NewLine}" +
                                       $"{Environment.NewLine}" +
                                       $"For example, if the picture wasn't shuffled, you'd have to tell me \"{string.Join('-', Enumerable.Range(0, 16))}\".{Environment.NewLine}" +
                                       $"However, since the picture will be shuffled, the order will be different, for example \"1-14-12-0-...-3\".{Environment.NewLine}" +
                                       $"{Environment.NewLine}" +
                                       $"You have 5 seconds to solve this challenge.");
            }
        }
    }
}