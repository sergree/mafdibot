using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MafDiBot
{
    static class GameMessages
    {
        static string mainPath;

        // Переменные сообщений
        static string[] Hello { get; set; }
        static string[] Help { get; set; }

        static string[] BeginReg { get; set; }
        static string[] RegInterval { get; set; }
        static string[] AddReg { get; set; }
        static string[] NotEnoughPlayers { get; set; }
        static string[] MaxPlayers { get; set; }
        static string[] Reg { get; set; }
        static string[] Unreg { get; set; }
        static string[] UserKicked { get; set; }

        static string[] BeginRoles { get; set; }
        static string[] EndRoles { get; set; }
        static string[] Players { get; set; }
        static string[] PlayersList { get; set; }

        static string[] AllNightDone { get; set; }
        static string[] TurnsList { get; set; }

        static string[] PreList { get; set; }
        static string[] NoPlayers { get; set; }
        static string[] CurGameStat { get; set; }

        static Dictionary<Game.Role, string[]> RoleMessage = new Dictionary<Game.Role, string[]>();
        static Dictionary<Noun, string[]> MafiaRoleMessage = new Dictionary<Noun, string[]>();

        static Dictionary<Game.Role, string[]> Winner = new Dictionary<Game.Role, string[]>();

        static Dictionary<Game.Role, string[]> NightMessage = new Dictionary<Game.Role, string[]>();
        static Dictionary<Game.Role, string[]> CancelMessage = new Dictionary<Game.Role, string[]>();
        static Dictionary<Game.Role, string[]> NightQuery = new Dictionary<Game.Role, string[]>();

        static Dictionary<Game.Action, string[]> Made = new Dictionary<Game.Action, string[]>();
        static Dictionary<Game.Action, string[]> MadeChannel = new Dictionary<Game.Action, string[]>();
        static string[] NoPlayer { get; set; }
        static string[] CancelTurn { get; set; }
        static string[] CancelTurnChannel { get; set; }

        static string[] GameStat { get; set; }
        static string[] BegEndMes { get; set; }

        static string[,,] RoleCase = new string[Enum.GetNames(typeof(Game.Role)).Length, Enum.GetNames(typeof(Noun)).Length, Enum.GetNames(typeof(Case)).Length];

        static string[] BeginNight { get; set; }
        static string[] NightInterval { get; set; }
        static string[] CannotKillLogovoMember { get; set; }
        static string[] BeginDay { get; set; }

        static string[] Golo1 { get; set; }
        static string[] Golo2 { get; set; }
        static string[] Golo3 { get; set; }
        static string[] Golo4 { get; set; }
        static string[] GoloInterval { get; set; }
        static string[] DoneLynch { get; set; }
        static string[] CannotAntiLynch { get; set; }
        static string[] GoloFailed { get; set; }
        static string[] GoloNight { get; set; }
        static string[] Lynch { get; set; }
        static string[] GoloNoKill { get; set; }
        static string[] CannotHZ { get; set; }
        static Dictionary<Game.Role, string[]> GoloKill = new Dictionary<Game.Role, string[]>();

        // Дневные сообщения
        static Dictionary<Game.Role, string[]> BotKill = new Dictionary<Game.Role, string[]>();
        static Dictionary<Game.Role, string[]> Skip = new Dictionary<Game.Role, string[]>();
        static Dictionary<Game.Role, string[]> Phrase = new Dictionary<Game.Role, string[]>();
        static Dictionary<Game.Role, string[]> Check = new Dictionary<Game.Role, string[]>();
        static string[] Heal { get; set; }
        static string[] HealFailed { get; set; }
        static string[] CheckResult { get; set; }
        static Dictionary<Game.Role, string[]> KillFailed = new Dictionary<Game.Role, string[]>();
        static Dictionary<Game.Role, Dictionary<Game.Role, string[]>> Kill = new Dictionary<Game.Role, Dictionary<Game.Role, string[]>>();

        // Методы вывода сообщений
        public static string ShowHello()
        {
            return Utilities.RandomChoice(Hello).Replace("{CurrentUserMention}", $"{Program.discord.CurrentUser.Mention}").Replace("{ServerName}", $"{Config.Guild.Name}");
        }

        public static string ShowHelp()
        {
            return String.Join("\n", Help).Replace("{assemblyVersion}", Program.assemblyVersion.ToString());
        }

        public static string ShowRole(DiscordMember player)
        {
            return $"Вы - **{ShowRoleCaseRU(Game.Players[player].Role, Noun.Singular, Case.Nominative)}**.";
        }

        public static string ShowBeginReg()
        {
            StringBuilder temp = new StringBuilder(Utilities.RandomChoice(BeginReg).Replace("{TimeString}", $"{SecondsToTimeStringRU((3 * GameRules.intervalRegistration + GameRules.intervalRegistrationAdditional) / 1000)}"));
            if (PingMe.PlayersToPing.Count > 0)
            {
                temp.Append("\n\n");
                foreach (DiscordMember member in PingMe.PlayersToPing)
                {
                    temp.Append($"{member.Mention}, ");
                }
                temp.Append("а не сыграть ли нам в **Мафию**?");
            }
            return temp.ToString();
        }
        public static string ShowRegInterval1()
        {
            return Utilities.RandomChoice(RegInterval).Replace("{TimeString}", $"{SecondsToTimeStringRU((2 * GameRules.intervalRegistration + GameRules.intervalRegistrationAdditional) / 1000)}");
        }
        public static string ShowRegInterval2()
        {
            return Utilities.RandomChoice(RegInterval).Replace("{TimeString}", $"{SecondsToTimeStringRU((GameRules.intervalRegistration + GameRules.intervalRegistrationAdditional) / 1000)}");
        }
        public static string ShowAddReg()
        {
            return Utilities.RandomChoice(AddReg);
        }
        public static string ShowNotEnoughPlayers()
        {
            return Utilities.RandomChoice(NotEnoughPlayers).Replace("{minPlayers}", $"{PlayersToStringRU(GameRules.minNumPlayers)}");
        }
        public static string ShowMaxPlayers()
        {
            return Utilities.RandomChoice(MaxPlayers);
        }
        public static string ShowReg(DiscordMember member)
        {
            return Utilities.RandomChoice(Reg).Replace("{MemberMention}", $"{member.Mention}").Replace("{PlayersCount}", $"{Game.Players.Count}");
        }
        public static string ShowReg_(DiscordMember member)
        {
            return Utilities.RandomChoice(Reg).Replace("{PlayersCount}", $"{Game.Players.Count + 1}");
        }
        public static string ShowUnreg(DiscordMember member)
        {
            return Utilities.RandomChoice(Unreg).Replace("{MemberMention}", $"{member.Mention}");
        }
        public static string ShowUserKicked(DiscordMember member)
        {
            return Utilities.RandomChoice(UserKicked).Replace("{Mention}", $"{member.Mention}");
        }

        public static string ShowBeginRoles()
        {
            return Utilities.RandomChoice(BeginRoles);
        }
        public static string ShowEndRoles()
        {
            return Utilities.RandomChoice(EndRoles);
        }

        public static string ShowRandomTurn(DiscordMember player)
        {
            return $"*Так как **{ShowRoleCaseRU(Game.Players[player].Role, Noun.Plural, Case.Nominative)}** отключил личные сообщения от участников сервера, его цель выбрана случайно!*";
        }
        public static string ShowCantPlay(DiscordMember member)
        {
            return $"*{member.Mention}, так как вы отключили личные сообщения от участников сервера, вы не можете участвовать в игре!*";
        }

        public static string ShowPlayers()
        {
            return Utilities.RandomChoice(Players).Replace("{PlayersCount}", $"{PlayersToStringRU(Game.Players.Count)}");
        }
        public static string ShowPlayersList()
        {
            if (Game.Players.Count > 0)
            {
                StringBuilder temp = new StringBuilder(Utilities.RandomChoice(PlayersList));
                temp.Replace("{PlayersCount}", $"{PlayersToStringRU(Game.Players.Count)}");
                List<string> list = new List<string>();
                int count;
                foreach (Game.Role role in Enum.GetValues(typeof(Game.Role)))
                {
                    if ((int)role < 100)
                    {
                        count = Game.RoleCount(role);
                        if (count > 0)
                        {
                            if ((int)role < 2)
                            {
                                list.Add($"**{ShowRoleCaseRU(role, Noun.Plural, Case.Nominative)}** - **{PlayersToStringRU(count)}**");
                            }
                            else
                            {
                                list.Add($"**{ShowRoleCaseRU(role, Noun.Singular, Case.Nominative)}**");
                            }
                        }
                    }
                }
                temp.Append(String.Join(", ", list));
                temp.Append(".");
                return temp.ToString();
            } else
            {
                return Utilities.RandomChoice(NoPlayers);
            }
        }
        public static string ShowCurGameStat()
        {
            return Utilities.RandomChoice(CurGameStat).Replace("{GameDay}", $"{NightsToStringRU(Game.Day)}").Replace("{GameTime}", $"{SecondsToTimeStringRU((int)Game.timer.Elapsed.TotalSeconds)}");
        }

        public static string ShowList(bool Cheat = false, bool Plain = false, DiscordMember caller = null)
        {
            if (Game.Players.Count > 0)
            {
                StringBuilder temp = new StringBuilder(Utilities.RandomChoice(PreList));
                int counter = 1;
                foreach (KeyValuePair<DiscordMember, Game.PlayerProperties> entry in Game.Players)
                {
                    if (Cheat)
                    {
                        temp.Append($" {counter}. {entry.Key.Mention} - **{ShowRoleCaseRU(entry.Value.Role, Noun.Singular, Case.Nominative)}**");
                    }
                    else {
                        if (Plain)
                        {
                            if (entry.Key == caller)
                            {
                                temp.Append($" {counter}. **`{entry.Key.DisplayName}`**");
                            } else
                            {
                                temp.Append($" {counter}. `{entry.Key.DisplayName}`");
                            }
                        } else
                        {
                            temp.Append($" {counter}. {entry.Key.Mention}");
                        }

                    }
                    counter++;
                }
                return temp.ToString();
            }
            else {
                return Utilities.RandomChoice(NoPlayers);
            }
        }
        public static string ShowListExceptMafia(DiscordMember caller)
        {
            if (Game.Players.Count > 0)
            {
                StringBuilder temp = new StringBuilder(Utilities.RandomChoice(PreList));
                int counter = 1;
                foreach (KeyValuePair<DiscordMember, Game.PlayerProperties> entry in Game.Players)
                {
                    if (Game.MafiaPlayers.Contains(entry.Key))
                    {
                        if (entry.Key == caller)
                        {
                            temp.Append($" {counter}. **`{entry.Key.DisplayName}`**");
                        }
                        else
                        {
                            temp.Append($" {counter}. `{entry.Key.DisplayName}`");
                        }
                    }
                    else
                    {
                        temp.Append($" {counter}. {entry.Key.Mention}");
                    }
                    counter++;
                }
                return temp.ToString();
            }
            else
            {
                return Utilities.RandomChoice(NoPlayers);
            }
        }

        public static string ShowTurns()
        {
            if (Game.AllOrdersDone)
            {
                return Utilities.RandomChoice(AllNightDone);
            }
            else {
                StringBuilder temp = new StringBuilder(Utilities.RandomChoice(TurnsList));
                List<DiscordMember> unDone = Game.UnDoneOrders;
                temp.Append(String.Join(", ", unDone.Select(m => Bold(ShowRoleCaseRU(Game.Players[m].Role, Noun.Singular, Case.Nominative))).ToArray()));
                temp.Append(".");
                return temp.ToString();
            }
        }

        public static string ShowRoleMessage(Game.Role role)
        {
            if (role == Game.Role.Mafia)
            {
                if (Game.MafiaPlayers.Count > 1)
                {
                    List<string> mentions = new List<string>();
                    foreach (DiscordMember mafioso in Game.MafiaPlayers)
                    {
                        mentions.Add(mafioso.Mention);
                    }
                    return RoleReplace(Utilities.RandomChoice(MafiaRoleMessage[Noun.Plural])).Replace("{Mention}", String.Join(", ", mentions));
                }
                else {
                    return RoleReplace(Utilities.RandomChoice(MafiaRoleMessage[Noun.Singular])).Replace("{Mention}", Game.FindPlayerByRole(Game.Role.Mafia).Mention);
                }
            }
            else {
                return RoleReplace(Utilities.RandomChoice(RoleMessage[role]));
            }
        }

        public static string ShowCannotKillLogovoMember()
        {
            return Utilities.RandomChoice(CannotKillLogovoMember);
        }

        public static string ShowNightQuery(DiscordMember activePlayer)
        {
            StringBuilder temp = new StringBuilder(Utilities.RandomChoice(NightQuery[Game.Players[activePlayer].Role]));
            temp.Replace("{Mention}", activePlayer.Mention);
            temp.Append("\n\n");
            if (new List<Game.Role> { Game.Role.Mafia }.Contains(Game.Players[activePlayer].Role))
            {
                temp.Append(ShowListExceptMafia(activePlayer));
            } else
            {
                temp.Append(ShowList(false, true));
            }
            return temp.ToString();
        }
        public static string ShowNoPlayer()
        {
            return Utilities.RandomChoice(NoPlayer);
        }
        public static string ShowMade(Game.Action action, DiscordMember target)
        {
            return Utilities.RandomChoice(Made[action]).Replace("{Target}", Code(target.DisplayName));
        }
        public static string ShowMadeChannel(Game.Action action, DiscordMember doer, DiscordMember target)
        {
            return Utilities.RandomChoice(MadeChannel[action]).Replace("{Doer}", doer.Mention).Replace("{Target}", target.Mention);
        }

        public static string ShowCancelTurn()
        {
            return Utilities.RandomChoice(CancelTurn);
        }
        public static string ShowCancelTurnChannel(DiscordMember doer)
        {
            return Utilities.RandomChoice(CancelTurnChannel).Replace("{Doer}", doer.Mention);
        }

        public static string ShowWinner(Game.Role winner)
        {
            if ((int)winner < 2)
            {
                return Utilities.RandomChoice(Winner[winner]).Replace("{Role}", ShowRoleCaseRU(winner, Noun.Plural, Case.Genitive));
            }
            if ((int)winner >= 100)
            {
                return Utilities.RandomChoice(Winner[winner]);
            }
            return Utilities.RandomChoice(Winner[winner]).Replace("{Role}", ShowRoleCaseRU(winner, Noun.Singular, Case.Genitive));
        }
        public static string ShowGameStat()
        {
            return Utilities.RandomChoice(GameStat).Replace("{GameDay}", $"{NightsToStringRU(Game.Day)}").Replace("{GameTime}", $"{SecondsToTimeStringRU((int)Game.timer.Elapsed.TotalSeconds)}");
        }
        public static string ShowEndMes(Game.Role winner)
        {
            StringBuilder temp = new StringBuilder(Utilities.RandomChoice(BegEndMes));
            temp.Append("\n");
            List<string> civilians = new List<string>();
            List<string> mafia = new List<string>();
            // Живые
            foreach (KeyValuePair<DiscordMember, Game.PlayerProperties> entry in Game.Players)
            {
                if (entry.Value.Role != Game.Role.Civilian && entry.Value.Role != Game.Role.Mafia)
                {
                    if (Game.allies[winner].Contains(entry.Value.Role))
                    {
                        temp.Append(UnderlineBold(Utilities.FirstCharToUpper(ShowRoleCaseRU(entry.Value.Role, Noun.Singular, Case.Nominative))));
                    }
                    else {
                        temp.Append(Bold(Utilities.FirstCharToUpper(ShowRoleCaseRU(entry.Value.Role, Noun.Singular, Case.Nominative))));
                    }
                    temp.Append(": ");
                    temp.Append(entry.Key.Mention);
                    temp.Append("\n");
                }
                if (entry.Value.Role == Game.Role.Mafia)
                {
                    mafia.Add(entry.Key.Mention);
                }
                if (entry.Value.Role == Game.Role.Civilian) {
                    civilians.Add(entry.Key.Mention);
                }
            }
            // Мертвые
            foreach (KeyValuePair<DiscordMember, Game.PlayerProperties> entry in Game.DeadPlayers)
            {
                if (entry.Value.Role != Game.Role.Civilian && entry.Value.Role != Game.Role.Mafia)
                {
                    if (Game.allies[winner].Contains(entry.Value.Role))
                    {
                        temp.Append(UnderlineBold(Utilities.FirstCharToUpper(ShowRoleCaseRU(entry.Value.Role, Noun.Singular, Case.Nominative))));
                    }
                    else
                    {
                        temp.Append(Bold(Utilities.FirstCharToUpper(ShowRoleCaseRU(entry.Value.Role, Noun.Singular, Case.Nominative))));
                    }
                    temp.Append(": ");
                    temp.Append(Strikeout(entry.Key.Mention));
                    temp.Append("\n");
                }
                if (entry.Value.Role == Game.Role.Mafia)
                {
                    mafia.Add(Strikeout(entry.Key.Mention));
                }
                if (entry.Value.Role == Game.Role.Civilian)
                {
                    civilians.Add(Strikeout(entry.Key.Mention));
                }
            }
            // Мафия
            if (Game.allies[winner].Contains(Game.Role.Mafia))
            {
                temp.Append(UnderlineBold(Utilities.FirstCharToUpper(ShowRoleCaseRU(Game.Role.Mafia, Noun.Plural, Case.Nominative))));
            }
            else
            {
                temp.Append(Bold(Utilities.FirstCharToUpper(ShowRoleCaseRU(Game.Role.Mafia, Noun.Plural, Case.Nominative))));
            }
            temp.Append(": ");
            temp.Append(String.Join(", ", mafia));
            temp.Append("\n");
            // Мирные
            if (Game.allies[winner].Contains(Game.Role.Civilian))
            {
                temp.Append(UnderlineBold(Utilities.FirstCharToUpper(ShowRoleCaseRU(Game.Role.Civilian, Noun.Plural, Case.Nominative))));
            }
            else {
                temp.Append(Bold(Utilities.FirstCharToUpper(ShowRoleCaseRU(Game.Role.Civilian, Noun.Plural, Case.Nominative))));
            }
            temp.Append(": ");
            temp.Append(String.Join(", ", civilians));
            return temp.ToString();
        }

        public static string ShowBeginNight()
        {
            return Utilities.RandomChoice(BeginNight);
        }
        public static string ShowNightInterval1()
        {
            return Utilities.RandomChoice(NightInterval).Replace("{TimeString}", $"{SecondsToTimeStringRU((2 * GameRules.intervalNight) / 1000)}");
        }
        public static string ShowNightInterval2()
        {
            return Utilities.RandomChoice(NightInterval).Replace("{TimeString}", $"{SecondsToTimeStringRU((GameRules.intervalNight) / 1000)}");
        }
        public static string ShowNightMessage(Game.Role role)
        {
            return RoleReplace(Utilities.RandomChoice(NightMessage[role]));
        }
        public static string ShowCancelMessage(Game.Role role)
        {
            return RoleReplace(Utilities.RandomChoice(CancelMessage[role]));
        }

        public static string ShowBeginDay()
        {
            return Utilities.RandomChoice(BeginDay);
        }

        // Отображение дневных сообщений

        public static string ShowBotKill(DiscordMember target)
        {
            Game.Role role = Game.Role.Nobody;
            if (Game.PlayersDump.ContainsKey(target))
            {
                role = Game.PlayersDump[target].Role;
            }
            if (Game.Players.ContainsKey(target))
            {
                role = Game.Players[target].Role;
            }
            if (Game.DeadPlayers.ContainsKey(target))
            {
                role = Game.DeadPlayers[target].Role;
            }
            return RoleReplace(Utilities.RandomChoice(BotKill[role])).Replace("{Mention}", target.Mention);
        }

        public static string ShowSkip(DiscordMember doer)
        {
            return RoleReplace(Utilities.RandomChoice(Skip[Game.PlayersDump[doer].Role]));
        }

        public static string ShowPhrase(Game.Role role, string message)
        {
            if (message != String.Empty)
            {
                return RoleReplace(Utilities.RandomChoice(Phrase[role])).Replace("{Message}", message);
            } else
            {
                return String.Empty;
            }
        }

        public static string ShowKillFailed(DiscordMember doer, DiscordMember target)
        {
            return RoleReplace(Utilities.RandomChoice(KillFailed[Game.PlayersDump[doer].Role])).Replace("{Mention}", target.Mention);
        }

        public static string ShowKill(DiscordMember doer, DiscordMember target, string message)
        {
            return $"{RoleReplace(Utilities.RandomChoice(Kill[Game.PlayersDump[doer].Role][Game.PlayersDump[target].Role])).Replace("{Mention}", target.Mention)}{ShowPhrase(Game.PlayersDump[doer].Role, message)}";
        }

        public static string ShowCheck(DiscordMember doer, DiscordMember target, string message)
        {
            return $"{RoleReplace(Utilities.RandomChoice(Check[Game.PlayersDump[doer].Role])).Replace("{Mention}", target.Mention)}{ShowPhrase(Game.PlayersDump[doer].Role, message)}";
        }

        public static string ShowCheckResult(DiscordMember target)
        {
            return Utilities.RandomChoice(CheckResult).Replace("{Mention}", Code(target.DisplayName)).Replace("{Role}", ShowRoleCaseRU(Game.PlayersDump[target].Role, Noun.Singular, Case.Nominative));
        }

        public static string ShowHeal(DiscordMember target, string message)
        {
            return $"{RoleReplace(Utilities.RandomChoice(Heal)).Replace("{Mention}", target.Mention)}{ShowPhrase(Game.Role.Doctor, message)}";
        }

        public static string ShowHealFailed(DiscordMember target, string message)
        {
            return $"{RoleReplace(Utilities.RandomChoice(HealFailed)).Replace("{Mention}", target.Mention)}{ShowPhrase(Game.Role.Doctor, message)}";
        }

        // Отображение сообщений во время голосования

        public static string ShowGolo1()
        {
            return $"{Utilities.RandomChoice(Golo1)}\n\n{ShowList()}";
        }

        public static string ShowGolo2()
        {
            return Utilities.RandomChoice(Golo2).Replace("{Mention}", Game.VotingFirstTarget.Mention).Replace("{TimeString}", $"{SecondsToTimeStringRU(GameRules.intervalLastWordFirst / 1000)}");
        }

        public static string ShowGolo3()
        {
            return Utilities.RandomChoice(Golo3).Replace("{Mention}", Game.VotingFirstTarget.Mention);
        }

        public static string ShowGolo4()
        {
            return Utilities.RandomChoice(Golo4).Replace("{Mention}", Game.VotingFirstTarget.Mention).Replace("{TimeString}", $"{SecondsToTimeStringRU(GameRules.intervalLastWordSecond / 1000)}");
        }

        public static string ShowGolo1Interval1()
        {
            return Utilities.RandomChoice(GoloInterval).Replace("{TimeString}", $"{SecondsToTimeStringRU((2 * GameRules.intervalVotingFirst) / 1000)}");
        }
        public static string ShowGolo1Interval2()
        {
            return Utilities.RandomChoice(GoloInterval).Replace("{TimeString}", $"{SecondsToTimeStringRU(GameRules.intervalVotingFirst / 1000)}");
        }
        public static string ShowGolo2Interval()
        {
            return Utilities.RandomChoice(GoloInterval).Replace("{TimeString}", $"{SecondsToTimeStringRU(GameRules.intervalVotingSecond / 1000)}");
        }
        public static string ShowGolo1Stat(Dictionary<DiscordMember, int> votingFirstResult)
        {
            StringBuilder temp = new StringBuilder($"Статистика голосов **[{Game.VotingFirst.Count}:{Game.Players.Count}]**:");

            string mention;
            foreach (KeyValuePair<DiscordMember, int> entry in votingFirstResult)
            {
                if (entry.Key == Config.Guild.CurrentMember)
                {
                    mention = "**ночь**";
                } else
                {
                    mention = $"`{entry.Key.DisplayName}`";
                }
                temp.Append($" {mention} - **{entry.Value}**");
            }
            temp.Append(".");

            return temp.ToString();
        }

        public static string ShowGolo2Stat(Dictionary<Game.Vote, int> votingSecondResult, DiscordMember doer, Game.Vote vote)
        {
            StringBuilder temp = new StringBuilder($"**[{votingSecondResult[Game.Vote.Yes]}:{votingSecondResult[Game.Vote.No]}:{votingSecondResult[Game.Vote.IDK]}][{Game.VotingSecond.Count}:{votingSecondResult[Game.Vote.Yes]+votingSecondResult[Game.Vote.No]}:{Game.Players.Count-1}]**:");

            temp.Append($" Товарищ {doer.Mention} ");

            switch (vote)
            {
                case Game.Vote.Yes:
                    temp.Append("проголосовал **за** казнь");
                    break;
                case Game.Vote.No:
                    temp.Append("проголосовал **против** казни");
                    break;
                case Game.Vote.IDK:
                    temp.Append("**воздержался** от голосования за казнь");
                    break;
            }

            temp.Append($" подсудимого `{Game.VotingFirstTarget.DisplayName}`!");

            return temp.ToString();
        }

        public static string ShowDoneLynch()
        {
            return Utilities.RandomChoice(DoneLynch).Replace("{Mention}", Game.VotingFirstTarget.Mention);
        }

        public static string ShowCannotAntiLynch()
        {
            return Utilities.RandomChoice(CannotAntiLynch);
        }

        public static string ShowCannotHZ()
        {
            return Utilities.RandomChoice(CannotHZ);
        }

        public static string ShowGoloFailed()
        {
            return Utilities.RandomChoice(GoloFailed);
        }

        public static string ShowGoloNight()
        {
            return Utilities.RandomChoice(GoloNight);
        }

        public static string ShowLynch()
        {
            return Utilities.RandomChoice(Lynch).Replace("{Mention}", Game.VotingFirstTarget.Mention);
        }

        public static string ShowGoloNoKill()
        {
            return Utilities.RandomChoice(GoloNoKill).Replace("{Mention}", Game.VotingFirstTarget.Mention);
        }

        public static string ShowGoloKill()
        {
            return RoleReplace(Utilities.RandomChoice(GoloKill[Game.Players[Game.VotingFirstTarget].Role])).Replace("{Mention}", Game.VotingFirstTarget.Mention);
        }

        // Первоначальная загрузка сообщений
        public static void Load()
        {
            mainPath = $"messages/{Config.Language}/";

            Hello = SafeReadAllLines("others/hello.txt");
            Help = SafeReadAllLines("help.txt");

            BeginReg = SafeReadAllLines("reg/beginreg.txt");
            RegInterval = SafeReadAllLines("reg/reginterval.txt");
            AddReg = SafeReadAllLines("reg/addreg.txt");
            NotEnoughPlayers = SafeReadAllLines("reg/notenoughplayers.txt");
            MaxPlayers = SafeReadAllLines("reg/maxplayers.txt");
            Reg = SafeReadAllLines("reg/reg.txt");
            Unreg = SafeReadAllLines("reg/unreg.txt");
            UserKicked = SafeReadAllLines("others/userkicked.txt");

            BeginRoles = SafeReadAllLines("role/beginroles.txt");
            EndRoles = SafeReadAllLines("role/endroles.txt");
            Players = SafeReadAllLines("role/players.txt");
            PlayersList = SafeReadAllLines("role/playerslist.txt");

            AllNightDone = SafeReadAllLines("others/allnightdone.txt");
            TurnsList = SafeReadAllLines("others/turnslist.txt");

            PreList = SafeReadAllLines("others/prelist.txt");
            NoPlayers = SafeReadAllLines("others/noplayers.txt");
            CurGameStat = SafeReadAllLines("others/curgamestat.txt");

            foreach (Game.Role role in Enum.GetValues(typeof(Game.Role)))
            {
                Winner.Add(role, SafeReadAllLines($"endgame/{role}.txt"));
                if ((int)role < 100)
                {
                    RoleMessage.Add(role, SafeReadAllLines($"role/{role}.txt"));
                    NightMessage.Add(role, SafeReadAllLines($"{role}/night.txt"));
                    CancelMessage.Add(role, SafeReadAllLines($"{role}/cancel.txt"));
                    NightQuery.Add(role, SafeReadAllLines($"nightqueries/{role}.txt"));
                    GoloKill.Add(role, SafeReadAllLines($"golo/kill{role}.txt"));
                }
            }

            MafiaRoleMessage.Add(Noun.Singular, SafeReadAllLines($"role/Mafia0.txt"));
            MafiaRoleMessage.Add(Noun.Plural, SafeReadAllLines($"role/Mafia1.txt"));

            GameStat = SafeReadAllLines("endgame/gamestat.txt");
            BegEndMes = SafeReadAllLines("endgame/begendmes.txt");

            foreach (Game.Role role in Enum.GetValues(typeof(Game.Role)))
            {
                if ((int)role < 100)
                {
                    foreach (Noun noun in Enum.GetValues(typeof(Noun)))
                    {
                        foreach (Case case_ in Enum.GetValues(typeof(Case)))
                        {
                            RoleCase[(int)role, (int)noun, (int)case_] = SafeRead($"skin/{role}{(int)noun}{(int)case_}.txt");
                        }
                    }
                }
            }

            BeginNight = SafeReadAllLines("beginnight.txt");
            NightInterval = SafeReadAllLines("nightqueries/nightinterval.txt");

            NoPlayer = SafeReadAllLines("nightqueries/noplayer.txt");
            CannotKillLogovoMember = SafeReadAllLines("nightqueries/cannotkilllogovomember.txt");
            foreach (Game.Action action in Enum.GetValues(typeof(Game.Action)))
            {
                Made.Add(action, SafeReadAllLines($"nightqueries/made{action}.txt"));
                MadeChannel.Add(action, SafeReadAllLines($"nightqueries/madechannel{action}.txt"));
            }

            CancelTurn = SafeReadAllLines("nightqueries/cancelturn.txt");
            CancelTurnChannel = SafeReadAllLines("nightqueries/cancelturnchannel.txt");

            BeginDay = SafeReadAllLines("beginday.txt");

            // Дневные сообщения
            foreach (Game.Role role in Enum.GetValues(typeof(Game.Role)))
            {
                if ((int)role < 100)
                {
                    BotKill.Add(role, SafeReadAllLines($"bot/kill{role}.txt"));
                    Skip.Add(role, SafeReadAllLines($"{role}/skip.txt"));
                    Phrase.Add(role, SafeReadAllLines($"{role}/phrase.txt"));

                    if (role == Game.Role.Commissar)
                    {
                        Check.Add(role, SafeReadAllLines($"{role}/Check.txt"));
                    }
                    if (new List<Game.Role>() { Game.Role.Mafia, Game.Role.Maniac, Game.Role.Doctor }.Contains(role))
                    {
                        KillFailed.Add(role, SafeReadAllLines($"{role}/killFailed.txt"));
                        Kill[role] = new Dictionary<Game.Role, string[]>();
                        foreach (Game.Role role_ in Enum.GetValues(typeof(Game.Role)))
                        {
                            if((int)role_ < 100)
                            {
                                Kill[role][role_] = SafeReadAllLines($"{role}/kill{role_}.txt");
                            }
                        }
                    }
                }
            }
            Heal = SafeReadAllLines($"Doctor/Heal.txt");
            HealFailed = SafeReadAllLines($"Doctor/HealFailed.txt");
            CheckResult = SafeReadAllLines($"others/checkresult.txt");

            Golo1 = SafeReadAllLines("golo/golo1.txt");
            Golo2 = SafeReadAllLines("golo/golo2.txt");
            Golo3 = SafeReadAllLines("golo/golo3.txt");
            Golo4 = SafeReadAllLines("golo/golo4.txt");
            GoloInterval = SafeReadAllLines("golo/golointerval.txt");
            DoneLynch = SafeReadAllLines("golo/donelynch.txt");
            CannotAntiLynch = SafeReadAllLines("golo/cannotantilynch.txt");
            GoloNight = SafeReadAllLines("golo/golonight.txt");
            GoloFailed = SafeReadAllLines("golo/golofailed.txt");
            Lynch = SafeReadAllLines("golo/lynch.txt");
            GoloNoKill = SafeReadAllLines("golo/nokill.txt");
            CannotHZ = SafeReadAllLines("golo/cannothz.txt");
        }

        public static string[] SafeReadAllLines(string path, bool addMainPath = true)
        {
            string fullPath = mainPath + path;
            if (!addMainPath)
            {
                fullPath = path;
            }
            if (File.Exists(fullPath))
            {
                return File.ReadAllLines(fullPath);
            }
            return new string[] { $"FILE NOT FOUND: {fullPath}" };
        }

        static string SafeRead(string path)
        {
            string fullPath = mainPath + path;
            if (File.Exists(fullPath))
            {
                return File.ReadLines(fullPath).First();
            }
            return $"FILE NOT FOUND: {fullPath}";
        }

        // Форматирование

        public static string Bold(string input)
        {
            return $"**{input}**";
        }
        public static string Italics(string input)
        {
            return $"*{input}*";
        }
        public static string BoldItalics(string input)
        {
            return $"***{input}***";
        }
        public static string Underline(string input)
        {
            return $"__{input}__";
        }
        public static string UnderlineBold(string input)
        {
            return $"__**{input}**__";
        }
        public static string Strikeout(string input)
        {
            return $"~~{input}~~";
        }
        public static string Code(string input)
        {
            return $"`{input}`";
        }

        // РУССКИЙ ЯЗЫК

        // Склоненное название роли
        public static string ShowRoleCaseRU(Game.Role role, Noun noun, Case case_)
        {
            return RoleCase[(int) role, (int) noun, (int) case_];
        }

        // Строка количества дней, часов, минут, секунд по секундам
        public static string SecondsToTimeStringRU(int seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            StringBuilder result = new StringBuilder();
            String Temp = String.Empty;
            if (time.Days > 0)
            {
                if (time.Days % 10 == 1)
                {
                    Temp = " день";
                }
                if (new[] { 2, 3, 4 }.Contains(time.Days % 10))
                {
                    Temp = " дня";
                }
                if (new[] { 0, 5, 6, 7, 8, 9 }.Contains(time.Days % 10) || new[] { 11, 12, 13, 14 }.Contains(time.Days % 100))
                {
                    Temp = " дней";
                }
                /*
                if (result.Length > 0)
                {
                    result.Append(" ");
                }
                */
                result.Append(time.Days);
                result.Append(Temp);
            }
            if (time.Hours > 0)
            {
                if (time.Hours % 10 == 1)
                {
                    Temp = " час";
                }
                if (new[] { 2, 3, 4 }.Contains(time.Hours % 10))
                {
                    Temp = " часа";
                }
                if (new[] { 0, 5, 6, 7, 8, 9 }.Contains(time.Hours % 10) || new[] { 11, 12, 13, 14 }.Contains(time.Hours))
                {
                    Temp = " часов";
                }
                if (result.Length > 0)
                {
                    result.Append(" ");
                }
                result.Append(time.Hours);
                result.Append(Temp);
            }
            if (time.Minutes > 0)
            {
                if (time.Minutes % 10 == 1)
                {
                    Temp = " минута";
                }
                if (new[] { 2, 3, 4 }.Contains(time.Minutes % 10))
                {
                    Temp = " минуты";
                }
                if (new[] { 0, 5, 6, 7, 8, 9 }.Contains(time.Minutes % 10) || new[] { 11, 12, 13, 14 }.Contains(time.Minutes))
                {
                    Temp = " минут";
                }
                if (result.Length > 0)
                {
                    result.Append(" ");
                }
                result.Append(time.Minutes);
                result.Append(Temp);
            }
            if (time.Seconds > 0)
            {
                if (time.Seconds % 10 == 1)
                {
                    Temp = " секунда";
                }
                if (new[] { 2, 3, 4 }.Contains(time.Seconds % 10))
                {
                    Temp = " секунды";
                }
                if (new[] { 0, 5, 6, 7, 8, 9 }.Contains(time.Seconds % 10) || new[] { 11, 12, 13, 14 }.Contains(time.Seconds))
                {
                    Temp = " секунд";
                }
                if (result.Length > 0)
                {
                    result.Append(" ");
                }
                result.Append(time.Seconds);
                result.Append(Temp);
            }
            return result.ToString();
        }

        // Строка количества человек
        public static string PlayersToStringRU(int players)
        {
            String Temp = String.Empty;
            if (players % 10 == 1)
            {
                Temp = " человек";
            }
            if (new[] { 2, 3, 4 }.Contains(players % 10))
            {
                Temp = " человека";
            }
            if (new[] { 0, 5, 6, 7, 8, 9 }.Contains(players % 10) || new[] { 11, 12, 13, 14 }.Contains(players % 100))
            {
                Temp = " человек";
            }
            return players + Temp;
        }

        // Строка количество ночей
        public static string NightsToStringRU(int nights)
        {
            String Temp = String.Empty;
            if (nights % 10 == 1)
            {
                Temp = " ночь";
            }
            if (new[] { 2, 3, 4 }.Contains(nights % 10))
            {
                Temp = " ночи";
            }
            if (new[] { 0, 5, 6, 7, 8, 9 }.Contains(nights % 10) || new[] { 11, 12, 13, 14 }.Contains(nights % 100))
            {
                Temp = " ночей";
            }
            return nights + Temp;
        }

        // Фундаментальные классы

        // Множественное число
        public enum Noun
        {
            Singular,     // Единственное
            Plural        // Множественное
        }

        // Падежи
        public enum Case
        {
            Nominative,   // Именительный
            Genitive,     // Родительный
            Dative,       // Дательный
            Accusative,   // Винительный
            Ablative,     // Творительный
            Prepositional // Предложный
        }

        // Функция подстановки ролей по числу и падежам
        public static string RoleReplace(string input)
        {
            StringBuilder inputSB = new StringBuilder(input); 
            foreach(string capital in new List<string> {"", "c"})
            {
                foreach (Game.Role role in Enum.GetValues(typeof(Game.Role)))
                {
                    if ((int)role < 100)
                    {
                        foreach (Noun noun in Enum.GetValues(typeof(Noun)))
                        {
                            foreach (Case case_ in Enum.GetValues(typeof(Case)))
                            {
                                if (capital == "c")
                                {
                                    inputSB.Replace($"{{c{role}{(int)noun}{(int)case_}}}", Utilities.FirstCharToUpper(ShowRoleCaseRU(role, noun, case_)));
                                }
                                else {
                                    inputSB.Replace($"{{{role}{(int)noun}{(int)case_}}}", ShowRoleCaseRU(role, noun, case_));
                                }
                            }
                        }
                    }
                }
            }

            return inputSB.ToString();
        }
    }
}
