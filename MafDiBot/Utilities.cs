using System;
using System.Text;
using System.Collections.Generic;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;

namespace MafDiBot
{
    static class Utilities
    {
        static Random random = new Random();

        public static T RandomChoice<T>(IReadOnlyList<T> source)
        {
            return source[random.Next(source.Count)];
        }

        public static T RandomChoiceAndRemove<T>(ref List<T> source)
        {
            T element = source[random.Next(source.Count)];
            source.Remove(element);
            return element;
        }

        public static async Task<DiscordMember> GetMemberByMention(string mention)
        {
            StringBuilder temp = new StringBuilder(mention);
            temp.Replace("<", "").Replace(">", "").Replace("@", "").Replace("!", "");
            ulong id = Convert.ToUInt64(temp.ToString());
            return await Config.Guild.GetMemberAsync(id);
        }

        public static DiscordMember GetPlayerByMention(string mention)
        {
            StringBuilder temp = new StringBuilder(mention);
            temp.Replace("<", "").Replace(">", "").Replace("@", "").Replace("!", "");
            ulong id = Convert.ToUInt64(temp.ToString());
            foreach (KeyValuePair<DiscordMember, Game.PlayerProperties> entry in Game.Players)
            {
                if (entry.Key.Id == id)
                {
                    return entry.Key;
                }
            }
            return null;
        }

        public static string FirstCharToUpper(string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} не может быть пустым", nameof(input));
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }

        static List<string> badWords = new List<string>() { "http://", "https://", ".com", ".ru", ".net", ".org", ".gg", ".me", ".ly", ".io" };

        public static string SafeMessage(string input)
        {
            StringBuilder temp = new StringBuilder(input);

            temp.Replace("@everyone", "everyone");
            temp.Replace("@here", "here");
            temp.Replace("\n", " ");
            temp.Replace("*", "");
            temp.Replace("_", "");
            temp.Replace("~", "");
            temp.Replace("`", "");

            List<string> words = new List<string>();
            bool bad;
            foreach (string word in temp.ToString().Split(" "))
            {
                bad = false;
                foreach (string badWord in badWords)
                {
                    if (word.ToLower().Contains(badWord))
                    {
                        bad = true;
                        break;
                    }
                }
                if (bad)
                {
                    words.Add("[URL]");
                } else
                {
                    words.Add(word);
                }
            }

            string result = Regex.Replace(String.Join(" ", words), @"\s+", " ").Trim();
            if (result.Length > 255)
            {
                return result.Substring(0, 255);
            }
            if (result.Length == 0)
            {
                return String.Empty;
            }
            return result;
        }

        public static bool Administrator(DiscordMember member)
        {
            foreach (DiscordRole role in member.Roles)
            {
                if (role.Permissions.HasFlag(DSharpPlus.Permissions.Administrator))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
