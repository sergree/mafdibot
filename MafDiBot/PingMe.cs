using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MafDiBot
{
    class PingMe
    {
        public static List<DiscordMember> PlayersToPing = new List<DiscordMember>();

        public static async Task Load()
        {
            PlayersToPing.Clear(); // Фикс реконнекта бота
            string[] input = GameMessages.SafeReadAllLines($"ping.txt", false);
            DiscordMember tempMember = null;
            foreach (string idStr in input)
            {
                ulong id = 0;
                try
                {
                    id = Convert.ToUInt64(idStr);
                }
                catch (Exception e) { await View.PrintError($"Ошибка модуля PingMe: {e.GetType()}: {e.Message}"); }
                tempMember = await Config.Guild.GetMemberAsync(id);
                if (tempMember != null)
                {
                    PlayersToPing.Add(tempMember);
                }
            }
        }

        public static void Save()
        {
            string[] lines = PlayersToPing.Select(u => u.Id.ToString()).ToArray();
            System.IO.File.WriteAllLines("ping.txt", lines);
        }

        public static async Task Process(DiscordMember member)
        {
            if (!PlayersToPing.Contains(member))
            {
                PlayersToPing.Add(member);
                Save();
                await View.SendDM(member, "Вам __**будут**__ поступать упоминания при начале регистрации на игру!");
            } else
            {
                PlayersToPing.Remove(member);
                Save();
                await View.SendDM(member, "Вам __**больше не будут**__ поступать упоминания при начале регистрации на игру!");
            }
        }
    }
}
