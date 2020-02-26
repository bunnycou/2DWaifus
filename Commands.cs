using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace _2DWaifus
{
    class _2DWaifusCommands : BaseCommandModule
    {
        [Command("ping")] // let's define this method as a command
        [Description("Example ping command")] // this will be displayed to tell users what this command does when they invoke help
        [Aliases("pong")] // alternative names for the command
        public async Task Ping(CommandContext ctx) // this command takes no arguments
        {
            //trigger a typing indicator to let users know 
            await ctx.TriggerTypingAsync();

            var emoji = DiscordEmoji.FromName(ctx.Client, ":ping_pong:");

            // respond with ping
            await ctx.RespondAsync($"{emoji} Pong! Ping: {ctx.Client.Ping}ms");
        }
    }

    class _2DWaifusRolls : BaseCommandModule
    {
        public static DiscordColor blue = new DiscordColor("00aeef");
        public static DiscordColor purple = new DiscordColor("7349AC");
        public static DiscordColor wished = new DiscordColor("00FF00");

        public static int waifuCount = 5;

        public static List<string> waifuIDList = new List<string>();
        public static DiscordMember ownerMember { get; set; }

        [Command("listall")]
        public async Task all(CommandContext ctx)
        {
            string messageToSend = "";
            Program.allList.ForEach(x => messageToSend += $"{x}\n");
            await ctx.RespondAsync(messageToSend);
        }

        [Command("listunowned")]
        public async Task owned(CommandContext ctx)
        {
            string messageToSend = "";
            Program.unownedList.ForEach(x => messageToSend += $"{x}\n");
            await ctx.RespondAsync(messageToSend);
        }

        [Command("waifu"), Description("Spawns a waifu."), Aliases("w")]
        public async Task waifuRoll(CommandContext ctx)
        {
            string testID = "1"; //testID will be changed with array of random later
            MySqlConnection conn = new MySqlConnection(Program.connectionJson.connection); //connect
            conn.Open();
            MySqlCommand cmd = new MySqlCommand($"SELECT * FROM waifus WHERE id = '{testID}'", conn); //get the stuff from the ID
            //these are vars that are used later
            string name = "";
            string anime = "";
            bool owned = false;
            //use the command
            MySqlDataReader reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                name = $"{reader.GetString(1)}";
                anime = $"{reader.GetString(2)}";
                if (reader.GetInt16(3).Equals(1)) //if owned
                {
                    owned = true;
                    ownerMember = ctx.Guild.GetMemberAsync(Convert.ToUInt64(reader.GetString(4))).Result;
                } else {
                    ownerMember = ctx.Member;
                    owned = false; 
                }
                
            }
            reader.Close();
            conn.Close();
          
            DiscordEmbedBuilder.EmbedFooter footer = new DiscordEmbedBuilder.EmbedFooter
            {
                IconUrl = ownerMember.AvatarUrl,
                Text = $"Owned by {ownerMember.DisplayName}"
            };

            DiscordEmbed em = new DiscordEmbedBuilder
            {
                Title = name,
                Description = anime,
                ImageUrl = $"https://raw.githubusercontent.com/noahcou/2DWaifusImages/master/images/{Regex.Replace(name, @"\s+", string.Empty).ToLower()}1.png",
                Color = owned ? purple : blue,
                Footer = owned ? footer : new DiscordEmbedBuilder.EmbedFooter(),
            };

            await ctx.RespondAsync(embed: em);
        }
    }

    //change the commands/alias etc if you want to, just put these here for remembering we need them lol
    class _2DWaifusBase : BaseCommandModule
    {
        [Command("wishlist"), Description("Shows your wishlist"), Aliases("wl")]
        public async Task wishListTask(CommandContext ctx)
        {

        }

        [Command("info"), Description("Shows info about a waifu"), Aliases("i")]
        public async Task infoTask(CommandContext ctx, string[] waifuName)
        {

        }

        [Command("list"), Description("Shows your owned waifus"), Aliases("l")]
        public async Task listTask(CommandContext ctx)
        {

        }
    }
}
