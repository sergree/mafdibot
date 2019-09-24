using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace MafDiBot
{
    static class View
    {
        public static DiscordChannel Logovo { get; private set; } = null;
        public static ulong LogovoId { get; private set; } = 123456;

        // Консоль
        public static void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }

        public static void Wait()
        {
            Console.ReadLine();
        }

        public static Task Print(string input)
        {
            Program.discord.DebugLogger.LogMessage(LogLevel.Info, Program.assemblyName, input, DateTime.Now);
            return Task.CompletedTask;
        }

        public static Task PrintError(string input)
        {
            Program.discord.DebugLogger.LogMessage(LogLevel.Error, Program.assemblyName, input, DateTime.Now);
            return Task.CompletedTask;
        }

        public static Task WriteToErrorsLog(string input)
        {
            using (StreamWriter sw = File.AppendText("errors.log"))
            {
                sw.WriteLine($"{ DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss")}: {input}");
            }
            return Task.CompletedTask;
        }

        // Отправка сообщений
        public static async Task<bool> SendMain(string input)
        {
            try
            {
                await Config.ChannelMain.SendMessageAsync(input);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<bool> SendLogovo(string input)
        {
            try
            {
                await Logovo.SendMessageAsync(input);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<bool> SendDM(DiscordMember member, string input)
        {
            try
            {
                await member.SendMessageAsync(input);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<bool> Send(DiscordChannel channel, string input)
        {
            try
            {
                await channel.SendMessageAsync(input);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<bool> AddMainRole(DiscordMember member)
        {
            try
            {
                await member.GrantRoleAsync(Config.RoleMain);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<bool> RemoveMainRole(DiscordMember member)
        {
            try
            {
                await member.RevokeRoleAsync(Config.RoleMain);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Включить модерацию на главном канале
        public static async Task<bool> EnableMod()
        {
            try
            {
                // Закрытый режим
                //await Config.ChannelMain.AddOverwriteAsync(Config.RoleEveryone, Permissions.None, Permissions.AccessChannels | Permissions.SendMessages);
                //await Config.ChannelMainVoice.AddOverwriteAsync(Config.RoleEveryone, Permissions.None, Permissions.AccessChannels | Permissions.UseVoice | Permissions.Speak);

                // Открытый режим
                await Config.ChannelMain.AddOverwriteAsync(Config.RoleEveryone, Permissions.None, Permissions.SendMessages);
                await Config.ChannelMainVoice.AddOverwriteAsync(Config.RoleEveryone, Permissions.None, Permissions.Speak);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Отключить модерацию на главном канале
        public static async Task<bool> DisableMod()
        {
            try
            {
                // Закрытый режим
                //await Config.ChannelMain.AddOverwriteAsync(Config.RoleEveryone, Permissions.None, Permissions.AccessChannels);
                //await Config.ChannelMainVoice.AddOverwriteAsync(Config.RoleEveryone, Permissions.None, Permissions.AccessChannels | Permissions.UseVoice);

                // Открытый режим
                await Config.ChannelMain.AddOverwriteAsync(Config.RoleEveryone, Permissions.None, Permissions.None);
                await Config.ChannelMainVoice.AddOverwriteAsync(Config.RoleEveryone, Permissions.None, Permissions.None);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Впилить в скрытый канал
        public static async Task<bool> AddToLogovo(DiscordMember member)
        {
            try
            {
                await Logovo.AddOverwriteAsync(member, Permissions.AccessChannels, Permissions.None);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        // Выпилить из скрытого канала
        public static async Task<bool> RemoveFromLogovo(DiscordMember member)
        {
            try
            {
                Logovo = Config.Guild.GetChannel(LogovoId);
                foreach (DiscordOverwrite overwrite in Logovo.PermissionOverwrites)
                {
                    if (overwrite.Id == member.Id)
                    {
                        await Logovo.DeleteOverwriteAsync(overwrite);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Переподключить участника к голосовому каналу
        public static async Task<bool> ReconnectToVoice(DiscordMember member)
        {
            try
            {
                if (member.VoiceState.Channel == Config.ChannelMainVoice)
                {
                    await member.PlaceInAsync(Config.ChannelAFKVoice);
                    await Task.Delay(1000);
                    await member.PlaceInAsync(Config.ChannelMainVoice);
                    await Task.Delay(1000);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Переподключить всех, кроме игроков
        public static async Task ReconnectNotPlayers()
        {
            List<DiscordVoiceState> voiceStates = new List<DiscordVoiceState>(Config.Guild.VoiceStates);
            foreach (DiscordVoiceState voiceState in voiceStates)
            {
                if (voiceState.Channel == Config.ChannelMainVoice)
                {
                    DiscordMember member = await Config.Guild.GetMemberAsync(voiceState.User.Id);
                    if (member != null)
                    {
                        if (!Game.Players.Keys.Contains(member))
                        {
                            await ReconnectToVoice(member);
                        }
                    }
                }
            }
        }

        // Забираем у всего сервера роль игры, выпиливаем всех из скрытого канала
        public static async Task<bool> Wipe(bool fullWipe = false)
        {
            try
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                await Print("Wipe(): ПРОЦЕДУРА ЗАПУЩЕНА...");

                if (Logovo != null)
                {
                    await Print("Wipe(): Запущено удаление канала логова...");
                    await RemoveLogovo();
                    await Print($"Wipe(): Выполнено за {timer.Elapsed.TotalSeconds} сек.");
                    timer.Restart();
                }


                IReadOnlyList<DiscordMember> members = null;
                if (fullWipe)
                {
                    await Print("Wipe(): Получаю список пользователей сервера...");
                    members = await Config.Guild.GetAllMembersAsync();
                    await Print($"Wipe(): Выполнено за {timer.Elapsed.TotalSeconds} сек.");
                    timer.Restart();

                    await Print("Wipe(): Открываю доступ в главный канал...");
                    await DisableMod();
                    await Print($"Wipe(): Выполнено за {timer.Elapsed.TotalSeconds} сек.");
                    timer.Restart();
                }
                else
                {
                    await Print("Wipe(): Получаю список игроков...");
                    members = Game.Players.Keys.ToList();
                    await Print($"Wipe(): Выполнено за {timer.Elapsed.TotalSeconds} сек.");
                    timer.Restart();
                }

                await Print("Wipe(): Удаляю роли у всех пользователей с ролью мафии...");
                foreach (DiscordMember member in members)
                {
                    if (member.Roles.ToList().Contains(Config.RoleMain))
                    {
                        await member.RevokeRoleAsync(Config.RoleMain);
                        await Task.Delay(2000);
                    }
                }
                await Print($"Wipe(): Выполнено за {timer.Elapsed.TotalSeconds} сек.");
                timer.Reset();

                await Print("Wipe(): ПРОЦЕДУРА ЗАВЕРШЕНА!");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Создание канала мафии
        public static async Task<bool> CreateLogovo()
        {
            try
            {
                Logovo = await Config.Guild.CreateChannelAsync("logovo", ChannelType.Text, Config.Category, null, null, Config.ChannelDonor.PermissionOverwrites);
                LogovoId = Logovo.Id;
                //await Logovo.AddOverwriteAsync(Config.RoleEveryone, Permissions.None, Permissions.AccessChannels);
                await Logovo.ModifyAsync(null, null, "Логово мафии");
                /*DiscordChannel logovo = await Config.Guild.CreateChannelAsync("logovo", ChannelType.Text, );*/
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Удаление канала мафии
        public static async Task<bool> RemoveLogovo()
        {
            try
            {
                await Logovo.DeleteAsync();
                Logovo = null;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
