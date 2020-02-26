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
        public static DiscordMember testMember { get; set; }

        [Command("test")]
        public async Task test(CommandContext ctx)
        {
            MySqlConnection conn = new MySqlConnection(@"server=play.explosionfish.net;database=test;uid=cear;pwd=verygoodpass;");
            conn.Open();
            string input = "admin";
            MySqlCommand cmd = new MySqlCommand($"SELECT * FROM users WHERE username = '{input}'", conn);
            string uname = string.Empty;
            string pword = string.Empty;
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                uname = $"{reader.GetString(0)}";
                pword = $"{reader.GetString(1)}";
            }
            reader.Close();
            conn.Close();
            DiscordEmbed em = new DiscordEmbedBuilder
            {
                Title = uname,
                Description = pword,
                Color = blue
            };

            await ctx.RespondAsync(embed: em);
        }

        [Command("waifu"), Description("Spawns a waifu."), Aliases("w")]
        public async Task waifuRoll(CommandContext ctx)
        {
            JConnection connectionJson;
            using (StreamReader r = new StreamReader("connection.json"))
            {
                connectionJson = JsonConvert.DeserializeObject<JConnection>(r.ReadToEnd());
            }
            string testID = "1";
            //grab waifu from ID
            MySqlConnection conn = new MySqlConnection(connectionJson.connection);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand($"SELECT * FROM waifus WHERE id = '{testID}'", conn);
            string name = "";
            string anime = "";
            //bool owned = false;
            string ownerID = "this should never happen";
            MySqlDataReader reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                name = $"{reader.GetString(1)}";
                anime = $"{reader.GetString(2)}";
                ownerID = reader.GetString(4);
            }
            reader.Close();
            conn.Close();
            DiscordMember owner = ctx.Guild.GetMemberAsync(Convert.ToUInt64(ownerID)).Result;
            DiscordEmbedBuilder.EmbedFooter footer = new DiscordEmbedBuilder.EmbedFooter
            {
                IconUrl = owner.AvatarUrl,
                Text = $"Owned by {owner.DisplayName}"
            };

            DiscordEmbed em = new DiscordEmbedBuilder
            {
                Title = name,
                Description = anime,
                ImageUrl = $"https://raw.githubusercontent.com/noahcou/2DWaifusImages/master/images/{Regex.Replace(name, @"\s+", string.Empty).ToLower()}1.png",
                Color = purple,
                Footer = footer,
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
