using System.Collections.Generic;
using DSharpPlus.Entities;
using MySql.Data.MySqlClient;
using System.IO;
using Newtonsoft.Json;

namespace _2DWaifus
{
    class GlobalVars
    {
        public static DiscordMember ownerMember { get; set; }
        public static DiscordColor unclaimed = new DiscordColor("00aeef");
        public static DiscordColor claimed = new DiscordColor("7349AC");
        public static DiscordColor wished = new DiscordColor("00FF00");
        public static DiscordColor list = new DiscordColor("00ffaa");
        public static JSecret secretJson;
        public static JConf confJson;
        public static Owaifulist ownerWaifuList;
        public static List<string> unownedList = new List<string>();
        public static List<string> allList = new List<string>();
        public static MySqlConnection connection { get; set; }

        internal static void initVars()
        {
            using (StreamReader r = new StreamReader("config.json"))
            {
                confJson = JsonConvert.DeserializeObject<JConf>(r.ReadToEnd());
            }
            using (StreamReader r = new StreamReader("secret.json"))
            {
                secretJson = JsonConvert.DeserializeObject<JSecret>(r.ReadToEnd());
            }
            connection = new MySqlConnection(secretJson.connection);
        }

        public class JConf
        {
            public string prefix = "";
        }
        public class JSecret
        {
            public string token = "";
            public string connection = "";
        }
        public class Owaifulist
        {
            public string[] waifus = new string[] { "" };
        }
    }
}
