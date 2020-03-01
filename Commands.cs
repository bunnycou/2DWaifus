using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using DSharpPlus.Interactivity;
using System.Threading;

namespace _2DWaifus
{

    class _2DWaifusAdmin : BaseCommandModule
    {
        [Command("reset"), Description("Resets the targets harem")]
        public async Task resetAsync(CommandContext ctx)
        {

        }
    }

    class _2DWaifusRolls : BaseCommandModule
    {
        [Command("listall")]
        public async Task all(CommandContext ctx)
        {
            string messageToSend = "";
            GlobalVars.allList.ForEach(x => messageToSend += $"{x}\n");
            await ctx.RespondAsync(messageToSend);
        }

        [Command("listunowned")]
        public async Task owned(CommandContext ctx)
        {
            string messageToSend = "";
            GlobalVars.unownedList.ForEach(x => messageToSend += $"{x}\n");
            await ctx.RespondAsync(messageToSend);
        }

        [Command("waifu"), Description("Spawns a waifu."), Aliases("w")]
        public async Task waifuRoll(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            DiscordEmoji heart = DiscordEmoji.FromName(Program.instance.bot, ":heart:");
            string rollID = GlobalVars.unownedList[new Random().Next(0, GlobalVars.unownedList.Count)];
            MySqlConnection conn = new MySqlConnection(GlobalVars.connectionJson.connection); //connect
            conn.Open();
            MySqlCommand cmd = new MySqlCommand($"SELECT * FROM waifus WHERE id = '{rollID}'", conn); //get the stuff from the ID
            //these are vars that are used later
            string name = "";
            string anime = "";
            //use the command
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                name = $"{reader.GetString(1)}";
                anime = $"{reader.GetString(2)}";
            }
            reader.Close();
            conn.Close();

            DiscordEmbed em = new DiscordEmbedBuilder
            {
                Title = name,
                Description = anime,
                ImageUrl = $"https://raw.githubusercontent.com/noahcou/2DWaifusImages/master/images/{Regex.Replace(name, @"\s+", string.Empty).ToLower()}/{Regex.Replace(name, @"\s+", string.Empty).ToLower()}1.png",
                Color = GlobalVars.unclaimed
            };

            DiscordMessage msg = await ctx.RespondAsync(embed: em);
            await msg.CreateReactionAsync(heart);
            var result = await interactivity.WaitForReactionAsync(x => x.Emoji.Equals(heart) && !x.User.IsBot, msg, ctx.User, TimeSpan.FromSeconds(30));
            if (!result.TimedOut)
            {
                await ctx.RespondAsync($":sparkling_heart: {name} and {ctx.User.Username} are now married :sparkling_heart:");         
            }
        }
    }

    //change the commands/alias etc if you want to, just put these here for remembering we need them lol
    class _2DWaifusBase : BaseCommandModule
    {
        [Command("info"), Description("Shows info about a waifu"), Aliases("i")]
        public async Task infoTask(CommandContext ctx, params string[] waifuName)
        {
            string name = Regex.Replace(String.Join(" ", waifuName).ToLower(), @"(^\w)|(\s\w)", x => x.Value.ToUpper());
            string anime = "";
            bool owned = false;


            switch (GlobalVars.allList.Contains(name)) {
                case true:
                    break;
                case false:
                    await ctx.RespondAsync("That waifu does not exist.");
                    return;
            }


            GlobalVars.connection.Open();
            MySqlCommand cmd = new MySqlCommand($"SELECT * FROM waifus WHERE name = '{name}'", GlobalVars.connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                anime = reader.GetString(2);
                if (reader.GetInt16(3).Equals(1)) {
                    owned = true;
                    GlobalVars.ownerMember = ctx.Guild.GetMemberAsync(Convert.ToUInt64(reader.GetString(4))).Result;
                } else {
                    GlobalVars.ownerMember = ctx.Member;
                    owned = false;
                }
            }
            reader.Close();
            GlobalVars.connection.Close();

            DiscordEmbedBuilder.EmbedFooter footer = new DiscordEmbedBuilder.EmbedFooter
            {
                IconUrl = GlobalVars.ownerMember.AvatarUrl,
                Text = $"Owned by {GlobalVars.ownerMember.DisplayName}"
            };

            DiscordEmbed em = new DiscordEmbedBuilder
            {
                Title = name,
                Description = anime,
                ImageUrl = $"https://raw.githubusercontent.com/noahcou/2DWaifusImages/master/images/{Regex.Replace(name, @"\s+", string.Empty).ToLower()}1.png",
                Color = owned ? GlobalVars.claimed : GlobalVars.unclaimed,
                Footer = owned ? footer : new DiscordEmbedBuilder.EmbedFooter(),
            };

            await ctx.RespondAsync(embed: em);
        }

        [Command("wishlist"), Description("Shows your wishlist"), Aliases("wl")]
        public async Task wishListTask(CommandContext ctx)
        {

        }

        [Command("list"), Description("Shows your owned waifus"), Aliases("l")]
        public async Task listTask(CommandContext ctx)
        {

        }

        [Command("divorce"), Description("Divorces a specified waifu"), Aliases("d")]
        public async Task divorceTask(CommandContext ctx, params string[] waifu)
        {

        }
    }
}
