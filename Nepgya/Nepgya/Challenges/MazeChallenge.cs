// 
// This file is licensed under the terms of the Simple Non Code License (SNCL) 2.3.0.
// The full license text can be found in the file named LICENSE.txt.
// Written originally by Alexandre Quoniou in 2018.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DeveMazeGenerator.Generators;
using DeveMazeGenerator.Generators.Helpers;
using DeveMazeGenerator.Imageification;
using DeveMazeGenerator.InnerMaps;
using DeveMazeGenerator.PathFinders;
using DeveMazeGenerator.Structures;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

namespace Nepgya
{
    [Group("maze_challenge")]
    [Description("Find the shortest path to the destination in this maze.")]
    class MazeChallenge
    {
        [Command("start")]
        [Description("Starts the challenge.")]
        public async Task Start(CommandContext ctx)
        {
            if (ctx.Channel.IsPrivate)
            {
                await ctx.TriggerTypingAsync();
                var interactivity = ctx.Client.GetInteractivityModule();

                var generator = new AlgorithmBacktrack();
                var map = generator.Generate<BitArreintjeFastInnerMap, NetRandom>(StaticRandom.Next(64, 128), StaticRandom.Next(64, 128), null);
                int steps;
                while ((steps = PathFinderDepthFirstSmartWithPos.GoFind(map, null).Count) == 0)
                {
                    map = generator.Generate<BitArreintjeFastInnerMap, NetRandom>(StaticRandom.Next(64, 128), StaticRandom.Next(64, 128), null);
                }
                
                using (var stream = new MemoryStream())
                {
                    WithPath.SaveMazeAsImageDeluxePng(map, new List<MazePointPos>(), stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    await ctx.RespondWithFileAsync(stream, "Maze.png");
                }

                var response = await interactivity.WaitForMessageAsync(message => message.Author.Id == ctx.User.Id &&
                                                                                  message.Channel.IsPrivate, TimeSpan.FromSeconds(3.0));
                await ctx.TriggerTypingAsync();

                if (response == null)
                {
                    await (await ctx.RespondAsync("Too late!")).CreateReactionAsync(DiscordEmoji.FromUnicode("⌛"));
                    return;
                }

                if (steps.ToString().Equals(response.Message.Content, StringComparison.Ordinal))
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
                await ctx.RespondAsync($"To solve this challenge, you will have to tell me how many steps are required to go from the top left-hand corner to the bottom right-hand corner of a maze." +
                                       $"The number of steps has to be the lowest possible.{Environment.NewLine}" +
                                       $"The mazes will be different each time you try the challenge.{Environment.NewLine}" +
                                       $"You have three seconds to solve the challenge.");
            }
        }
    }
}