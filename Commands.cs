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
using Newtonsoft.Json;

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
            MySqlConnection conn = new MySqlConnection(GlobalVars.secretJson.connection); //connect
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
                string ownerid = ctx.Member.Id.ToString();
                GlobalVars.connection.Open();
                MySqlCommand haremcmd = new MySqlCommand($"SELECT waifus FROM users WHERE id = '{ownerid}'", GlobalVars.connection);
                MySqlDataReader haremreader = haremcmd.ExecuteReader();
                while (haremreader.Read())
                {
                    string ownerWaifus = haremreader.GetString(0);
                    GlobalVars.ownerInfo = JsonConvert.DeserializeObject<GlobalVars.Owaifulist>(ownerWaifus);
                }
                haremreader.Close();
                GlobalVars.connection.Close();

                string newHarem = $"{{\"waifus\": [\"{rollID}\"";
                foreach (string w in GlobalVars.ownerInfo.waifus)
                {
                    newHarem += $",\"{w}\"";
                }
                newHarem += "]}";
                GlobalVars.connection.Open();
                MySqlCommand updatecmd = new MySqlCommand($"update users set waifus = '{newHarem}' where id = '{ownerid}'", GlobalVars.connection);
                MySqlDataReader updatereader = updatecmd.ExecuteReader();
                while (updatereader.Read())
                {
                }
                updatereader.Close();
                GlobalVars.connection.Close();

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

        [Command("wish"), Description("Wishes a character"), Aliases("wi")]
        public async Task wishTask(CommandContext ctx, params string[] waifuName)
        {
            string name = Regex.Replace(String.Join(" ", waifuName).ToLower(), @"(^\w)|(\s\w)", x => x.Value.ToUpper());
            string ownerid = ctx.Member.Id.ToString();
            string waifuid = "";
            bool waifuowned = false;
            string waifuowner = "";

            GlobalVars.connection.Open();
            MySqlCommand waifuinfo = new MySqlCommand($"select * from waifus where name = '{name}'", GlobalVars.connection);
            MySqlDataReader waifuinfoRead = waifuinfo.ExecuteReader();
            while (waifuinfoRead.Read())
            {
                waifuid = waifuinfoRead.GetString(0);
                waifuowned = waifuinfoRead.GetBoolean(3);
                if (waifuowned)
                {
                    waifuowner = waifuinfoRead.GetString(4);
                }
            }
            waifuinfoRead.Close();
            GlobalVars.connection.Close();

            GlobalVars.connection.Open();
            MySqlCommand wishlistcmd = new MySqlCommand($"SELECT wishes FROM users WHERE id = '{ownerid}'", GlobalVars.connection);
            MySqlDataReader wishlistreader = wishlistcmd.ExecuteReader();
            while (wishlistreader.Read())
            {
                string ownerWishes = wishlistreader.GetString(0);
                GlobalVars.ownerInfo = JsonConvert.DeserializeObject<GlobalVars.Owaifulist>(ownerWishes);
            }
            wishlistreader.Close();
            GlobalVars.connection.Close();

            string newwishlist = $"{{\"wishes\": [\"{waifuid}\"";
            foreach (string wish in GlobalVars.ownerInfo.wishes)
            {
                newwishlist += $", \"{wish}\"";
            }
            newwishlist += "]}";

            GlobalVars.connection.Open();
            MySqlCommand wishlucmd = new MySqlCommand($"update users set wishes = '{newwishlist}' where id = '{ownerid}'", GlobalVars.connection);
            MySqlDataReader wishluRead = wishlucmd.ExecuteReader();
            while (wishluRead.Read())
            {
            }
            wishlistreader.Close();
            GlobalVars.connection.Close();

            if (!waifuowned)
            {
                await ctx.RespondAsync($"{name} has been added to your wishlist! ");
            } else
            {
                await ctx.RespondAsync($"{name} has been added to your wishlist! However, they are already owned by {waifuowner}");
            }
        }

        [Command("wishremove"), Description("Removes waifu from wishlist"), Aliases("wr")]
        public async Task wishRemoveTask(CommandContext ctx)
        {

        }

        [Command("wishlist"), Description("Shows your wishlist"), Aliases("wl")]
        public async Task wishListTask(CommandContext ctx)
        {
            string owner = ctx.Member.DisplayName;
            string ownerid = ctx.Member.Id.ToString();

            GlobalVars.connection.Open();
            MySqlCommand cmd = new MySqlCommand($"SELECT wishes FROM users WHERE id = '{ownerid}'", GlobalVars.connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string ownerWaifus = reader.GetString(0);
                GlobalVars.ownerInfo = JsonConvert.DeserializeObject<GlobalVars.Owaifulist>(ownerWaifus);
            }
            reader.Close();
            GlobalVars.connection.Close();

            string wishlist = "";
            foreach (string w in GlobalVars.ownerInfo.waifus)
            {
                string waifuname = "";
                GlobalVars.connection.Open();
                MySqlCommand namecmd = new MySqlCommand($"select name from waifus where id = {w}", GlobalVars.connection);
                MySqlDataReader namereader = namecmd.ExecuteReader();
                while (namereader.Read())
                {
                    waifuname = namereader.GetString(0);
                }
                namereader.Close();
                GlobalVars.connection.Close();
                wishlist += $"{waifuname}\n";
            }

            DiscordEmbedBuilder em = new DiscordEmbedBuilder
            {
                Title = $"{owner}'s Wishlist",
                Description = wishlist,
                Color = GlobalVars.list
            };

            await ctx.RespondAsync(embed: em);
        }

        [Command("list"), Description("Shows your owned waifus"), Aliases("l")]
        public async Task listTask(CommandContext ctx)
        {
            string owner = ctx.Member.DisplayName;
            string ownerid = ctx.Member.Id.ToString();

            GlobalVars.connection.Open();
            MySqlCommand cmd = new MySqlCommand($"SELECT waifus FROM users WHERE id = '{ownerid}'", GlobalVars.connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string ownerWaifus = reader.GetString(0);
                GlobalVars.ownerInfo = JsonConvert.DeserializeObject<GlobalVars.Owaifulist>(ownerWaifus);
            }
            reader.Close();
            GlobalVars.connection.Close();

            string waifulist = "";
            foreach (string w in GlobalVars.ownerInfo.waifus)
            {
                string waifuname = "";
                GlobalVars.connection.Open();
                MySqlCommand namecmd = new MySqlCommand($"select name from waifus where id = {w}", GlobalVars.connection);
                MySqlDataReader namereader = namecmd.ExecuteReader();
                while (namereader.Read())
                {
                    waifuname = namereader.GetString(0);
                }
                namereader.Close();
                GlobalVars.connection.Close();
                waifulist += $"{waifuname}\n";
            }

            DiscordEmbedBuilder em = new DiscordEmbedBuilder
            {
                Title = $"{owner}'s Waifus",
                Description = waifulist,
                Color = GlobalVars.list
            };

            await ctx.RespondAsync(embed: em);
        }

        [Command("divorce"), Description("Divorces a specified waifu"), Aliases("d")]
        public async Task divorceTask(CommandContext ctx, params string[] waifuName)
        {
            var interactivity = ctx.Client.GetInteractivity();
            DiscordEmoji confirm = DiscordEmoji.FromName(Program.instance.bot, ":white_check_mark:");
            DiscordEmoji deny = DiscordEmoji.FromName(Program.instance.bot, ":no_entry_sign:");

            string name = Regex.Replace(String.Join(" ", waifuName).ToLower(), @"(^\w)|(\s\w)", x => x.Value.ToUpper());
            string waifuid = "";
            bool owned = false;
            string owner = ctx.Member.Id.ToString();
            Console.WriteLine(name);
            Console.WriteLine(owner);

            GlobalVars.connection.Open();
            MySqlCommand divorceinfo = new MySqlCommand($"select * from waifus where name = '{name}'", GlobalVars.connection);
            MySqlDataReader dinforead = divorceinfo.ExecuteReader();
            while (dinforead.Read())
            {
                waifuid = dinforead.GetString(0);
                if (dinforead.GetString(3) == "True" && dinforead.GetString(4) == owner)
                {
                    owned = true;
                } else
                {
                    owned = false;
                }
            }
            dinforead.Close();
            GlobalVars.connection.Close();

            if (owned)
            {
                DiscordMessage confirmation = await ctx.RespondAsync($"Are you sure you want to divorce {name}?");
                await confirmation.CreateReactionAsync(confirm);
                await confirmation.CreateReactionAsync(deny);
            } else
            {
                await ctx.RespondAsync("You do not own that waifu!");
            }


            /*//mysql for removing from thing
            GlobalVars.connection.Open();
            MySqlCommand waifulist = new MySqlCommand($"SELECT waifus FROM users WHERE id = '{owner}'", GlobalVars.connection);
            MySqlDataReader waifulistreader = waifulist.ExecuteReader();
            while (waifulistreader.Read())
            {
                string ownerWaifus = waifulistreader.GetString(0);
                GlobalVars.ownerInfo = JsonConvert.DeserializeObject<GlobalVars.Owaifulist>(ownerWaifus);
            }
            waifulistreader.Close();
            GlobalVars.connection.Close();

            string divorcedHarem = "{\"waifus\": [";
            foreach (string w in GlobalVars.ownerInfo.waifus)
            {
                if (w != waifuid)
                {
                    divorcedHarem += $"\"{w}\"";
                }
            }
            divorcedHarem.Remove(divorcedHarem.Length - 1);
            divorcedHarem += "]}";

            GlobalVars.connection.Open();
            MySqlCommand updatelist = new MySqlCommand($"update users set waifus = '{divorcedHarem}' where id = '{owner}'");
            MySqlDataReader uplistread = updatelist.ExecuteReader();
            while(uplistread.Read())
            {
            }
            uplistread.Close();
            GlobalVars.connection.Close();

            await ctx.RespondAsync($"You have divorce {name}");*/
        }
    }
}
