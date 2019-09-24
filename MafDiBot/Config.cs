using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DSharpPlus.Entities;

namespace MafDiBot
{
    class Config
    {
        [JsonProperty("Token")]
        public static string Token { get; private set; }
        [JsonProperty("Language")]
        public static string Language { get; private set; }
        [JsonProperty("GameString")]
        public static string GameString { get; private set; }
        [JsonProperty("GuildId")]
        public static ulong GuildId { get; private set; }
        [JsonProperty("ChannelMainId")]
        public static ulong ChannelMainId { get; private set; }
        [JsonProperty("ChannelMainVoiceId")]
        public static ulong ChannelMainVoiceId { get; private set; }
        [JsonProperty("CategoryId")]
        public static ulong CategoryId { get; private set; }
        [JsonProperty("ChannelDonorId")]
        public static ulong ChannelDonorId { get; private set; }
        [JsonProperty("ChannelAFKVoiceId")]
        public static ulong ChannelAFKVoiceId { get; private set; }
        [JsonProperty("RoleMainId")]
        public static ulong RoleMainId { get; private set; }
        [JsonProperty("RoleBadId")]
        public static ulong RoleBadId { get; private set; }

        public static bool Loaded { get; private set; } = false;

        public static DiscordGame ClientGame { get; private set; }
        public static DiscordGuild Guild { get; private set; }
        public static DiscordChannel ChannelMain { get; private set; }
        public static DiscordChannel ChannelMainVoice { get; private set; }
        public static DiscordChannel Category { get; private set; }
        public static DiscordChannel ChannelDonor { get; private set; }
        public static DiscordChannel ChannelAFKVoice { get; private set; }
        public static DiscordRole RoleEveryone { get; private set; }
        public static DiscordRole RoleMain { get; private set; }
        public static DiscordRole RoleBad { get; private set; }

        public static void Load()
        {
            try
            {
                using (var fs = File.OpenRead("config.json"))
                using (StreamReader sr = new StreamReader(fs))
                {
                    string json = sr.ReadToEnd();
                    Config obj = JsonConvert.DeserializeObject<Config>(json);
                    Loaded = true;
                }
            }
            catch (Exception) { }
        }

        public static async Task LoadEntities()
        {
            ClientGame = new DiscordGame()
            {
                Name = GameString
            };

            Guild = await Program.discord.GetGuildAsync(GuildId);
            ChannelMain = Guild.GetChannel(ChannelMainId);
            ChannelMainVoice = Guild.GetChannel(ChannelMainVoiceId);
            Category = Guild.GetChannel(CategoryId);
            ChannelDonor = Guild.GetChannel(ChannelDonorId);
            ChannelAFKVoice = Guild.GetChannel(ChannelAFKVoiceId);
            RoleEveryone = Guild.GetRole(GuildId);
            RoleMain = Guild.GetRole(RoleMainId);
            RoleBad = Guild.GetRole(RoleBadId);
        }
    }
}
