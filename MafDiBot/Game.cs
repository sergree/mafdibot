using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using System.Linq;
using System.Diagnostics;
using System;

namespace MafDiBot
{
    static class Game
    {
        // Текущий статус игры
        public static GameStatus Status { get; private set; } = GameStatus.Idle;
        // Идёт игра или нет
        public static bool On
        {
            get
            {
                return Status != GameStatus.Idle && Status != GameStatus.Registration && Status != GameStatus.Ending && Status != GameStatus.Pause;
            }
        }
        // Текущие игровые сутки
        public static int Day { get; private set; } = 0;
        // Счетчик времени игры
        public static Stopwatch timer = new Stopwatch();
        // Счетчик для линча
        public static Stopwatch timerLynch = new Stopwatch();
        // Сколько раз игроки могут пропустить ход
        public static int NumSkip = 0;
        // Словарь играющих пользователей
        public static Dictionary<DiscordMember, PlayerProperties> Players = new Dictionary<DiscordMember, PlayerProperties>();
        public static Dictionary<DiscordMember, PlayerProperties> PlayersDump = new Dictionary<DiscordMember, PlayerProperties>();
        public static Dictionary<DiscordMember, PlayerProperties> DeadPlayers = new Dictionary<DiscordMember, PlayerProperties>();
        public static List<DiscordMember> AllPlayers = new List<DiscordMember>();
        public static List<DiscordMember> MafiaPlayers = new List<DiscordMember>();
        // Активные роли
        public static DiscordMember ActiveMafioso { get; private set; } = null;
        public static List<DiscordMember> Active = new List<DiscordMember>();
        // Ночные заказы
        public static Dictionary<Role, NightOrder> NightOrders = new Dictionary<Role, NightOrder>();
        // Дневные действия
        public static List<DayAction> DayActions = new List<DayAction>();

        // Голосование на выбор цели
        public static Dictionary<DiscordMember, DiscordMember> VotingFirst = new Dictionary<DiscordMember, DiscordMember>();
        public static Dictionary<DiscordMember, int> VotingFirstResult
        {
            get
            {
                Dictionary<DiscordMember, int> counts = new Dictionary<DiscordMember, int>();

                foreach (KeyValuePair<DiscordMember, DiscordMember> entry in VotingFirst)
                {
                    if (counts.ContainsKey(entry.Value))
                    {
                        counts[entry.Value]++;
                    } else
                    {
                        counts.Add(entry.Value, 1);
                    }
                }

                Dictionary<DiscordMember, int> result = (from entry in counts orderby entry.Value descending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);

                int max = result.Max(x => x.Value);
                if (result.Count(x=> x.Value == max) == 1)
                {
                    VotingFirstTarget = result.First().Key;
                } else
                {
                    VotingFirstTarget = null;
                }
                
                return result;
            }
        }
        public static bool VotingFirstLynch = false;
        public static bool VotingFirstEnds = false;
        public static DiscordMember VotingFirstTarget = null;

        // Голосование вешать или нет
        public static Dictionary<DiscordMember, Vote> VotingSecond = new Dictionary<DiscordMember, Vote>();
        public static Dictionary<Vote, int> VotingSecondResult
        {
            get
            {
                Dictionary<Vote, int> counts = new Dictionary<Vote, int>
                {
                    [Vote.Yes] = 0,
                    [Vote.No] = 0,
                    [Vote.IDK] = 0
                };

                foreach (KeyValuePair<DiscordMember, Vote> entry in VotingSecond)
                {
                    counts[entry.Value]++;
                }

                if (counts[Vote.No] >= counts[Vote.Yes])
                {
                    VotingSecondTarget = Vote.No;
                } else if (counts[Vote.No] + counts[Vote.Yes] <= (Players.Count - 1) / 2) {
                    VotingSecondTarget = Vote.IDK;
                } else
                {
                    VotingSecondTarget = Vote.Yes;
                }

                return counts;
            }
        }
        public static bool VotingSecondEnds = false;
        public static Vote VotingSecondTarget = Vote.Yes;

        // Очередь изгоняемых игроков, которые покинули сервер, написали killme или попали в Тролли
        public static List<DiscordMember> PlayersToKick = new List<DiscordMember>();

        // -- -- -- -- -- -- --

        // Очистка игровой сессии

        public static async Task Reset()
        {
            Status = GameStatus.Pause;
            timer.Reset();
            timerLynch.Reset();
            NumSkip = 0;
            await View.Wipe();
            PlayersDump.Clear();
            AllPlayers.Clear();
            DeadPlayers.Clear();
            MafiaPlayers.Clear();
            ActiveMafioso = null;
            Active.Clear();
            PlayersToKick.Clear();
            NightOrders.Clear();
            DayActions.Clear();
            VotingFirst.Clear();
            VotingFirstLynch = false;
            VotingFirstEnds = false;
            VotingFirstTarget = null;
            VotingSecond.Clear();
            VotingSecondEnds = false;
            VotingSecondTarget = Vote.Yes;
            await Task.Delay(GameRules.intervalPause);
            if (Day > 0)
            {
                await View.DisableMod();
                await View.ReconnectNotPlayers();
                await View.RemoveLogovo();
            }
            Players.Clear();
            Day = 0;
            Status = GameStatus.Idle;
            await View.SendMain("Для запуска регистрации на следующую игру введите команду **!старт**.");
        }

        // Игровая логика

        public static async Task MoveStatus()
        {
            switch (Status)
            {
                case GameStatus.Idle:
                    await StartRegistration();
                    break;
                case GameStatus.Registration:
                    if (Players.Count >= GameRules.minNumPlayers)
                    {
                        await StartCasting();
                    }
                    else {
                        await NotEnoughPlayers();
                    }
                    break;
                case GameStatus.Casting:
                    await StartNight();
                    break;
                case GameStatus.Night:
                    await StartDay();
                    break;
                case GameStatus.Day:
                    // Проверка на конец игры
                    if (CheckWinner() == Role.NoWinner)
                    {
                        await StartVotingFirst();
                    }
                    else {
                        await EndGame();
                    }
                    break;
                case GameStatus.VotingFirst:
                    timerLynch.Stop();
                    if (VotingFirstLynch || VotingFirstTarget == null || VotingFirstTarget == Config.Guild.CurrentMember)
                    {
                        await StartVotingResult();
                    } else
                    {
                        await StartLastWordFirst();
                    }
                    break;
                case GameStatus.LastWordFirst:
                    await StartVotingSecond();
                    break;
                case GameStatus.VotingSecond:
                    if (VotingSecondTarget != Vote.Yes)
                    {
                        await StartVotingResult();
                    } else
                    {
                        await StartLastWordSecond();
                    }
                    break;
                case GameStatus.LastWordSecond:
                    await StartVotingResult();
                    break;
                case GameStatus.VotingResult:
                    // Проверка на конец игры
                    if (CheckWinner() == Role.NoWinner)
                    {
                        await StartNight();
                    }
                    else
                    {
                        await EndGame();
                    }
                    break;
                case GameStatus.Ending:
                    await Reset();
                    break;
                case GameStatus.Pause:
                    break;
                case GameStatus.Kicking:
                    break;
            }
        }

        public static async Task StartRegistration()
        {
            Status = GameStatus.Registration;

            await View.SendMain(GameMessages.ShowBeginReg());
            await Task.Delay(GameRules.intervalRegistration);
            if (Status == GameStatus.Registration)
            {
                await View.SendMain(GameMessages.ShowRegInterval1());
                await Task.Delay(GameRules.intervalRegistration);
                if (Status == GameStatus.Registration)
                {
                    await View.SendMain(GameMessages.ShowRegInterval2());
                    await Task.Delay(GameRules.intervalRegistration);
                    if (Status == GameStatus.Registration)
                    {
                        await View.SendMain(GameMessages.ShowAddReg());
                        await Task.Delay(GameRules.intervalRegistrationAdditional);
                        if (Status == GameStatus.Registration)
                        {
                            await MoveStatus();
                        }
                    }
                }
            }
        }

        public static async Task NotEnoughPlayers()
        {
            Status = GameStatus.Ending;
            await Task.Delay(GameRules.minInterval);

            await View.SendMain(GameMessages.ShowNotEnoughPlayers());
            await MoveStatus();
        }

        public static async Task StartCasting()
        {
            Status = GameStatus.Casting;
            
            if (Players.Count >= GameRules.maxNumPlayers)
            {
                await View.SendMain(GameMessages.ShowMaxPlayers());
            }
            await Task.Delay(GameRules.minInterval);

            await View.EnableMod();
            await View.SendMain(GameMessages.ShowBeginRoles());
            await View.ReconnectNotPlayers();
            await View.CreateLogovo();
            NumSkip = GameRules.GetNumSkip(Players.Count);
            AllPlayers = Players.Keys.ToList();

            List<DiscordMember> Civilians = new List<DiscordMember>(Players.Keys);

            DiscordMember commissar = null;
            // Выдаем комиссара
            commissar = Utilities.RandomChoiceAndRemove(ref Civilians);
            Players[commissar].Role = Role.Commissar;
            // Считаем и выдаем мафов
            int MafiaCount = GameRules.GetMafiaCount(Players.Count);
            DiscordMember tempMafioso = null;
            for (int i = 0; i < MafiaCount; i++)
            {
                tempMafioso = Utilities.RandomChoiceAndRemove(ref Civilians);
                Players[tempMafioso].Role = Role.Mafia;
                MafiaPlayers.Add(tempMafioso);
            }
            // Проверяем количество и выдаем маньяка
            DiscordMember maniac = null;
            if (Players.Count >= GameRules.inGameManiac)
            {
                maniac = Utilities.RandomChoiceAndRemove(ref Civilians);
                Players[maniac].Role = Role.Maniac;
            }
            // Проверяем количество и выдаем доктора
            DiscordMember doctor = null;
            if (Players.Count >= GameRules.inGameDoctor)
            {
                doctor = Utilities.RandomChoiceAndRemove(ref Civilians);
                Players[doctor].Role = Role.Doctor;
            }

            // + Пригласить соответствующие роли на скрытые каналы
            foreach (DiscordMember mafioso in MafiaPlayers)
            {
                await View.AddToLogovo(mafioso);
                await Task.Delay(1000);
            }

            await Task.Delay(GameRules.minInterval);

            // Пишем каждому не мирному о его роли
            await View.SendDM(commissar, GameMessages.ShowRoleMessage(Role.Commissar));
            await Task.Delay(1000);
            await View.SendLogovo(GameMessages.ShowRoleMessage(Role.Mafia));
            await Task.Delay(1000);
            if (maniac != null)
            {
                await View.SendDM(maniac, GameMessages.ShowRoleMessage(Role.Maniac));
                await Task.Delay(1000);
            }
            if (doctor != null)
            {
                await View.SendDM(doctor, GameMessages.ShowRoleMessage(Role.Doctor));
                await Task.Delay(1000);
            }
            // ---

            await View.SendMain(GameMessages.ShowPlayersList());
            await Task.Delay(GameRules.minInterval);
            await View.SendMain(GameMessages.ShowEndRoles());
            timer.Start();
            await MoveStatus();
        }

        public static async Task StartNight()
        {
            Status = GameStatus.Kicking;
            await ProcessPlayersToKick();
            Status = GameStatus.Night;
            Day++;
            int CurrentDay = Day;
            NextActiveMafioso();
            NextActive();
            NightOrders.Clear();
            DayActions.Clear();
            VotingFirst.Clear();
            VotingSecond.Clear();
            await Task.Delay(GameRules.minInterval);

            await View.SendMain(GameMessages.ShowBeginNight());

            bool success;
            foreach (DiscordMember activePlayer in Active) {
                success = true;
                switch (Players[activePlayer].Role)
                {
                    case Role.Mafia:
                        await View.SendLogovo(GameMessages.ShowNightQuery(activePlayer));
                        break;
                    case Role.Commissar:
                        success = await View.SendDM(activePlayer, GameMessages.ShowNightQuery(activePlayer));
                        break;
                    case Role.Maniac:
                        success = await View.SendDM(activePlayer, GameMessages.ShowNightQuery(activePlayer));
                        break;
                    case Role.Doctor:
                        success = await View.SendDM(activePlayer, GameMessages.ShowNightQuery(activePlayer));
                        break;
                }
                if (!success)
                {
                    await MakeRandomOrder(activePlayer);
                    await View.SendMain(GameMessages.ShowRandomTurn(activePlayer));
                }
                await Task.Delay(1000);
            }

            await Task.Delay(GameRules.intervalNight);
            if (Status == GameStatus.Night && Day == CurrentDay)
            {
                await View.SendMain(GameMessages.ShowNightInterval1());
                await Task.Delay(GameRules.intervalNight);
                if (Status == GameStatus.Night && Day == CurrentDay)
                {
                    await View.SendMain(GameMessages.ShowNightInterval2());
                    await Task.Delay(GameRules.intervalNight);
                    if (Status == GameStatus.Night && Day == CurrentDay)
                    {
                        //await View.SendMain("Ночь закончилась - не все сделали ход!");
                        await MoveStatus();
                    }
                }
            }
        }

        public static async Task StartDay()
        {
            Status = GameStatus.Day;
            PlayersDump = new Dictionary<DiscordMember, PlayerProperties>(Players);
            ConvertNightOrdersToDayActions();
            await Task.Delay(GameRules.minInterval);

            await View.SendMain(GameMessages.ShowBeginDay());
            await Task.Delay(GameRules.minInterval);
            /*
            for (int i = 1; i < 6; i++)
            {
                await View.SendMain($"Кто-то что-то сделал ночью. ({i})");
                await Task.Delay(GameRules.intervalDay);
            }
            */
            foreach (DayAction action in DayActions)
            {
                await action.action(action.args);
                await Task.Delay(GameRules.intervalDay);
            }
            //await View.SendMain("День закончился!");
            await MoveStatus();
        }

        public static async Task StartVotingFirst()
        {
            Status = GameStatus.Kicking;
            await ProcessPlayersToKick();
            Status = GameStatus.VotingFirst;
            timerLynch.Reset();
            VotingFirstLynch = false;
            VotingFirstEnds = false;
            VotingFirstTarget = null;
            int CurrentDay = Day;
            //await Task.Delay(GameRules.minInterval);
            timerLynch.Start();

            await View.SendMain(GameMessages.ShowGolo1());
            await Task.Delay(GameRules.intervalVotingFirst);
            if (Status == GameStatus.VotingFirst && Day == CurrentDay && !VotingFirstEnds)
            {
                await View.SendMain(GameMessages.ShowGolo1Interval1());
                await Task.Delay(GameRules.intervalVotingFirst);
                if (Status == GameStatus.VotingFirst && Day == CurrentDay && !VotingFirstEnds)
                {
                    await View.SendMain(GameMessages.ShowGolo1Interval2());
                    await Task.Delay(GameRules.intervalVotingFirst);
                    if (Status == GameStatus.VotingFirst && Day == CurrentDay && !VotingFirstEnds)
                    {
                        //await View.SendMain("Голосование закончилось - не все проголосовали!");
                        //await View.SendMain($"Цель выбраная при голосовании: {VotingFirstTarget?.Mention ?? "не определена"}");
                        await MoveStatus();
                    }
                }
            }
        }

        public static async Task StartLastWordFirst()
        {
            Status = GameStatus.LastWordFirst;
            int CurrentDay = Day;

            await View.SendMain(GameMessages.ShowGolo2());
            await Task.Delay(GameRules.intervalLastWordFirst);
            if (Status == GameStatus.LastWordFirst && Day == CurrentDay)
            {
                //await View.SendMain("Вы не написали **!все**, продолжаем!");
                await MoveStatus();
            }
        }

        public static async Task StartVotingSecond()
        {
            Status = GameStatus.VotingSecond;
            VotingSecondEnds = false;
            VotingSecondTarget = Vote.Yes;
            int CurrentDay = Day;

            await View.SendMain(GameMessages.ShowGolo3());
            await Task.Delay(GameRules.intervalVotingSecond);
            if (Status == GameStatus.VotingSecond && Day == CurrentDay && !VotingSecondEnds)
            {
                await View.SendMain(GameMessages.ShowGolo2Interval());
                await Task.Delay(GameRules.intervalVotingSecond);
                if (Status == GameStatus.VotingSecond && Day == CurrentDay && !VotingSecondEnds)
                {
                    //await View.SendMain("Голосование закончилось - не все проголосовали!");
                    await MoveStatus();
                }
            }
        }

        public static async Task StartLastWordSecond()
        {
            Status = GameStatus.LastWordSecond;
            int CurrentDay = Day;

            await View.SendMain(GameMessages.ShowGolo4());
            await Task.Delay(GameRules.intervalLastWordSecond);
            if (Status == GameStatus.LastWordSecond && Day == CurrentDay)
            {
                //await View.SendMain("Вы не написали **!все**!");
                await MoveStatus();
            }
        }

        public static async Task StartVotingResult()
        {
            Status = GameStatus.VotingResult;

            bool needToKill = false; // Убиваем кого-то в результате голосования

            if (VotingFirstTarget == Config.Guild.CurrentMember)
            {
                // Ночь
                await View.SendMain(GameMessages.ShowGoloNight());
            } else if (VotingFirstTarget == null || VotingSecondTarget == Vote.IDK)
            {
                await View.SendMain(GameMessages.ShowGoloFailed());
            } else
            {
                if (VotingFirstLynch)
                {
                    await View.SendMain(GameMessages.ShowLynch());
                    needToKill = true;
                    await Task.Delay(GameRules.minInterval);
                } else
                {
                    if (VotingSecondTarget == Vote.Yes)
                    {
                        // Обычная казнь
                        needToKill = true;
                    } else
                    {
                        // Оправдание
                        await View.SendMain(GameMessages.ShowGoloNoKill());
                    }
                }
            }

            if (needToKill)
            {
                await View.SendMain(GameMessages.ShowGoloKill());
                await Death(VotingFirstTarget);
            }

            await Task.Delay(GameRules.minInterval);
            await MoveStatus();
        }

        public static async Task EndGame()
        {
            Status = GameStatus.Ending;
            timer.Stop();
            await Task.Delay(GameRules.minInterval);

            Role winner = CheckWinner();
            await View.SendMain(GameMessages.ShowWinner(winner));
            await Task.Delay(GameRules.minInterval);
            await View.SendMain(GameMessages.ShowEndMes(winner));
            await Task.Delay(GameRules.minInterval);
            await View.SendMain(GameMessages.ShowGameStat());
            await MoveStatus();
        }

        // Нахождение следующего активного мафа
        public static void NextActiveMafioso()
        {
            if (ActiveMafioso != null)
            {
                // Следующий живой, или сначала
                int index = MafiaPlayers.IndexOf(ActiveMafioso);
                ActiveMafioso = null;
                for (int i = index + 1; i < MafiaPlayers.Count; i++)
                {
                    if (Players.ContainsKey(MafiaPlayers[i]))
                    {
                        ActiveMafioso = MafiaPlayers[i];
                        break;
                    }
                }
            }
            if (ActiveMafioso == null)
            {
                // Первый живой
                foreach (DiscordMember mafioso in MafiaPlayers)
                {
                    if (Players.ContainsKey(mafioso))
                    {
                        ActiveMafioso = mafioso;
                        break;
                    }
                }
            }
        }

        // Нахождение списка текущих активных ролей
        public static void NextActive()
        {
            Active.Clear();
            if (ActiveMafioso != null)
            {
                Active.Add(ActiveMafioso);
            }
            DiscordMember comissar = FindPlayerByRole(Role.Commissar);
            DiscordMember maniac = FindPlayerByRole(Role.Maniac);
            DiscordMember doctor = FindPlayerByRole(Role.Doctor);
            if (comissar != null)
            {
                Active.Add(comissar);
            }
            if (maniac != null)
            {
                Active.Add(maniac);
            }
            if (doctor != null)
            {
                Active.Add(doctor);
            }
        }

        // Функция убийства
        // Удачного
        public static async Task<bool> Death(DiscordMember member)
        {
            if (Players.ContainsKey(member))
            {
                DeadPlayers[member] = Players[member];
                Players.Remove(member);
                if (DeadPlayers[member].Role == Role.Mafia)
                {
                    await View.RemoveFromLogovo(member);
                    await Task.Delay(1000);
                }
                await View.RemoveMainRole(member);
                if (!DeadPlayers[member].Kicked)
                {
                    await Task.Delay(500);
                    await View.ReconnectToVoice(member);
                }
                return true;
            }
            return false;
        }
        // Неудачного
        public static async Task<bool> DeathFailed(DiscordMember member)
        {
            if (Players.ContainsKey(member))
            {
                await View.RemoveMainRole(member);
                return true;
            }
            return false;
        }
        // Воскрешение после неудачного убийства
        public static async Task<bool> DeathFailedRevive(DiscordMember member)
        {
            if (Players.ContainsKey(member))
            {
                await View.AddMainRole(member);
                return true;
            }
            return false;
        }

        // Проверка на конец игры
        public static Role CheckWinner()
        {
            // Переменные
            int numComissar = RoleCount(Role.Commissar);
            int numDoctor = RoleCount(Role.Doctor);
            int numManiac = RoleCount(Role.Maniac);
            int numMafia = RoleCount(Role.Mafia);
            int numCivilians = RoleCount(Role.Civilian);
            
            // Победа комиссара
            if (Players.Count == 1 && numComissar == 1)
            {
                return Role.Commissar;
            }

            // Победа доктора
            if (Players.Count == 1 && numDoctor == 1)
            {
                return Role.Doctor;
            }

            // Победа мирных
            if (Players.Count > 0 && numMafia + numManiac == 0)
            {
                return Role.Civilian;
            }

            // Победа мафии
            if (numMafia > 0 && numComissar + numManiac == 0 && Players.Count - numMafia <= 1)
            {
                return Role.Mafia;
            }
            // Победа маньяка
            if (numManiac > 0 && numComissar + numMafia == 0 && Players.Count - numManiac <= 1)
            {
                return Role.Maniac;
            }

            // Ничья
            if (Players.Count == 2)
            {
                return Role.Standoff;
            }

            // Все умерли
            if (Players.Count == 0)
            {
                return Role.Nobody;
            }

            return Role.NoWinner;
        }

        // Фундаментальные классы

        public class PlayerProperties
        {
            // Роль игрока
            public Role Role { get; set; } = Role.Civilian;
            // Сколько ночей подряд лечится доктором
            public int Heals { get; set; } = 0;
            // Сколько ходов пропустил игрок
            public int Skips { get; set; } = 0;
            // Был ли кикнут
            public bool Kicked { get; set; } = false;
        }

        public enum GameStatus
        {
            Idle,
            Registration,
            Casting,
            Night,
            Day,
            VotingFirst,
            LastWordFirst,
            VotingSecond,
            LastWordSecond,
            VotingResult,
            Ending,
            Pause,
            Kicking
        }

        public enum Role
        {
            // Основные игровые роли
            Civilian,
            Mafia,
            Commissar,
            Maniac,
            Doctor,
            // Вспомогательные роли для определения победителя
            NoWinner = 100, // Никто не победил
            Standoff, // Ничья
            Nobody, // Все проиграли
        }

        /*
        public static List<Role> Allies(Role role)
        {
            List<Role> list = new List<Role>();

            switch (role)
            {
                case Role.Civilian:
                    list.Add(Role.Commissar);
                    list.Add(Role.Doctor);
                    break;
                case Role.Mafia:
                    break;
                case Role.Commissar:
                    list.Add(Role.Civilian);
                    list.Add(Role.Doctor);
                    break;
                case Role.Maniac:
                    break;
                case Role.Doctor:
                    list.Add(Role.Civilian);
                    list.Add(Role.Commissar);
                    break;
            }

            return list;
        }
        */

        public static Dictionary<Role, List<Role>> allies = new Dictionary<Role, List<Role>>();

        public static void Load()
        {
            List<Role> TeamCivilian = new List<Role> { Role.Civilian, Role.Commissar, Role.Doctor };
            List<Role> TeamMafia    = new List<Role> { Role.Mafia };
            List<Role> Maniac       = new List<Role> { Role.Maniac };
            List<Role> Empty        = new List<Role> {  };

            allies[Role.Civilian]   = TeamCivilian;
            allies[Role.Mafia]      = TeamMafia;
            allies[Role.Commissar]  = TeamCivilian;
            allies[Role.Maniac]     = Maniac;
            allies[Role.Doctor]     = TeamCivilian;

            allies[Role.Standoff]   = Empty;
            allies[Role.Nobody]     = Empty;
        }

        public enum Action
        {
            Kill,
            Check,
            Heal
        }

        public class NightOrder
        {
            public Action Action { get; set; }
            public DiscordMember Doer { get; set; }
            public DiscordMember Target { get; set; }
            public string Message { get; set; }
        }

        // Добавление ночного заказа
        public static async Task AddNightOrder(DiscordChannel channel, Action action, DiscordMember doer, DiscordMember target, string message)
        {
            NightOrders.Add(Players[doer].Role, new NightOrder { Action = action, Doer = doer, Target = target, Message = message });
            
            // Написать об успешном заказе в ЛС или специальный канал
            if (channel != null) // Заглушка для дебага
            {
                if (channel.IsPrivate)
                {
                    await View.Send(channel, GameMessages.ShowMade(action, target));
                    if (Players[doer].Role == Role.Mafia)
                    {
                        await View.SendLogovo(GameMessages.ShowMadeChannel(action, doer, target));
                    }
                }
                else
                {
                    await View.Send(channel, GameMessages.ShowMadeChannel(action, doer, target));
                }
            }

            // Вывести завуалированное сообщение в главный канал
            await View.SendMain(GameMessages.ShowNightMessage(Players[doer].Role));
            //await View.SendMain($"Параметры: {doer.Mention}, {action}, {target.Mention}, {message}");

            // День, если все сделали ход
            if (AllOrdersDone && /* Условие для закрытия бага */ Status == GameStatus.Night)
            {
                await MoveStatus();
            }
        }

        // Отмена ночного заказа
        public static async Task CancelNightOrder(DiscordChannel channel, DiscordMember doer)
        {
            NightOrders.Remove(Players[doer].Role);

            // Написать об успешной отмене в ЛС или специальный канал
            if (channel.IsPrivate)
            {
                await View.Send(channel, GameMessages.ShowCancelTurn());
                if (Players[doer].Role == Role.Mafia)
                {
                    await View.SendLogovo(GameMessages.ShowCancelTurnChannel(doer));
                }
            }
            else
            {
                await View.Send(channel, GameMessages.ShowCancelTurnChannel(doer));
            }

            // Вывести завуалированное сообщение в главный канал
            await View.SendMain(GameMessages.ShowCancelMessage(Players[doer].Role));
        }

        // -- ДНЕВНЫЕ ДЕЙСТВИЯ --

        // Перевод ночных заказов в дневные действия
        public static void ConvertNightOrdersToDayActions()
        {
            DayActions.Clear(); // На всякий случай

            // Прибавляем 1 к Skips у игроков не сделавших ход
            foreach (DiscordMember player in UnDoneOrders)
            {
                Players[player].Skips++;
            }

            bool healUsed = false; // Лечение было использовано

            // Получение вылечиваемого игрока
            DiscordMember healedPlayer = null;
            if (NightOrders.ContainsKey(Role.Doctor))
            {
                healedPlayer = NightOrders[Role.Doctor].Target;
            }
            // Сбрасываем количество лечений у всех игроков, кроме вылечиваемого, у него делаем +1
            int healsCount = 0;
            foreach (KeyValuePair<DiscordMember, PlayerProperties> entry in Players)
            {
                if (entry.Key == healedPlayer)
                {
                    entry.Value.Heals++;
                    healsCount = entry.Value.Heals;
                } else
                {
                    entry.Value.Heals = 0;
                }
            }

            // УПРОЩАЕМ КОД: внутренняя лямбда проверки пропуска хода
            Action<DiscordMember> CheckSkip = (player) => 
            {
                if (player != null)
                {
                    if (Players[player].Skips <= NumSkip)
                    {
                        DayActions.Add(new DayAction(Sleep, new DayActionArgs { doer = player }));
                    }
                    else
                    {
                        DayActions.Add(new DayAction(BotKill, new DayActionArgs { target = player }));
                    }
                }
            }; 

            // Проверка комиссара
            DiscordMember comissar = FindPlayerByRole(Role.Commissar);
            if (NightOrders.ContainsKey(Role.Commissar))
            {
                // Когда действий будет много, тут (и в остальных) будет switch по действиям
                DayActions.Add(new DayAction(Check, new DayActionArgs(NightOrders[Role.Commissar])));
            }
            else CheckSkip(comissar);

            // УПРОЩАЕМ КОД: внутренняя лямбда использования лечения доктора
            System.Action UseHeal = () =>
            {
                healUsed = true;
                if (healsCount <= GameRules.maxHealNum)
                {
                    DayActions.Add(new DayAction(Heal, new DayActionArgs(NightOrders[Role.Doctor])));
                }
                else
                {
                    DayActions.Add(new DayAction(Kill, new DayActionArgs(NightOrders[Role.Doctor])));
                }
            };

            // Убийство мафии
            DiscordMember mafioso = ActiveMafioso;
            if (NightOrders.ContainsKey(Role.Mafia))
            {
                if (!healUsed && NightOrders[Role.Mafia].Target == healedPlayer)
                {
                    DayActions.Add(new DayAction(KillFailed, new DayActionArgs(NightOrders[Role.Mafia])));
                    UseHeal();
                }
                else
                {
                    DayActions.Add(new DayAction(Kill, new DayActionArgs(NightOrders[Role.Mafia])));
                }
            }
            else CheckSkip(mafioso);

            // Убийство маньяка
            DiscordMember maniac = FindPlayerByRole(Role.Maniac);
            if (NightOrders.ContainsKey(Role.Maniac))
            {
                if (!healUsed && NightOrders[Role.Maniac].Target == healedPlayer)
                {
                    DayActions.Add(new DayAction(KillFailed, new DayActionArgs(NightOrders[Role.Maniac])));
                    UseHeal();
                }
                else
                {
                    DayActions.Add(new DayAction(Kill, new DayActionArgs(NightOrders[Role.Maniac])));
                }
            }
            else CheckSkip(maniac);

            // Неудачное лечение доктора
            DiscordMember doctor = FindPlayerByRole(Role.Doctor);
            if (NightOrders.ContainsKey(Role.Doctor))
            {
                if (!healUsed)
                {
                    if (healsCount <= GameRules.maxHealNum)
                    {
                        DayActions.Add(new DayAction(HealFailed, new DayActionArgs(NightOrders[Role.Doctor])));
                    }
                    else
                    {
                        DayActions.Add(new DayAction(Kill, new DayActionArgs(NightOrders[Role.Doctor])));
                    }
                }
            }
            else CheckSkip(doctor);
        }
        
        // Аргумент дневного действия
        public class DayActionArgs
        {
            public DiscordMember doer = null;
            public DiscordMember target = null;
            public string message = String.Empty;

            public DayActionArgs()
            {

            }

            public DayActionArgs (NightOrder order)
            {
                doer = order.Doer;
                target = order.Target;
                message = Utilities.SafeMessage(order.Message);
            }
        }

        // Проверка
        public static async Task Check(DayActionArgs e)
        {
            await View.SendMain(GameMessages.ShowCheck(e.doer, e.target, e.message));
            await View.SendDM(e.doer, GameMessages.ShowCheckResult(e.target));
        }
        // Удачное убийство
        public static async Task Kill(DayActionArgs e)
        {
            await View.SendMain(GameMessages.ShowKill(e.doer, e.target, e.message));
            await Death(e.target);
        }
        // Неудачное убийство
        public static async Task KillFailed(DayActionArgs e)
        {
            await View.SendMain(GameMessages.ShowKillFailed(e.doer, e.target));
            await DeathFailed(e.target);
        }
        // Удачное лечение
        public static async Task Heal(DayActionArgs e)
        {
            await View.SendMain(GameMessages.ShowHeal(e.target, e.message));
            await DeathFailedRevive(e.target);
        }
        // Неудачное лечение
        public static async Task HealFailed(DayActionArgs e)
        {
            await View.SendMain(GameMessages.ShowHealFailed(e.target, e.message));
        }
        // Пропуск хода
        public static async Task Sleep(DayActionArgs e)
        {
            await View.SendMain(GameMessages.ShowSkip(e.doer));
        }
        // Убийство ботом
        public static async Task BotKill(DayActionArgs e)
        {
            await View.SendMain(GameMessages.ShowBotKill(e.target));
            await Death(e.target);
        }

        // Дневное действие
        public class DayAction
        {
            public Func<DayActionArgs, Task> action;
            public DayActionArgs args;
            public DayAction (Func<DayActionArgs, Task> action_, DayActionArgs args_)
            {
                action = action_;
                args = args_;
            }
        }

        public static async Task ProcessVoteFirst(DiscordMember doer, DiscordMember target)
        {
            Dictionary<DiscordMember, int> votingFirstResult = null;

            // Упрощаем код: функция добавления голоса
            Func<Task> AddVote = async () =>
            {
                if (!VotingFirstLynch || (VotingFirstLynch && VotingFirstTarget == target))
                {
                    VotingFirst[doer] = target;
                    votingFirstResult = VotingFirstResult;
                    await View.SendMain(GameMessages.ShowGolo1Stat(votingFirstResult));
                } else
                {
                    await View.SendDM(doer, GameMessages.ShowCannotAntiLynch());
                }
            };

            if (VotingFirst.ContainsKey(doer))
            {
                // Если уже голосовал и поменял цель
                if (VotingFirst[doer] != target)
                {
                    await AddVote();
                }
            } else
            {
                // Если еще не голосовал
                await AddVote();
            }

            // Выполняем проверки, только если были изменения и уже не идет завершение голосования
            if (votingFirstResult != null && !VotingFirstEnds)
            {
                // Проверка на окончание голосования
                if (Players.Count == VotingFirst.Count)
                {
                    VotingFirstEnds = true;
                } else
                {
                    if (VotingFirstTarget != null)
                    {
                        Dictionary<DiscordMember, int> temp = new Dictionary<DiscordMember, int>(votingFirstResult);
                        temp.Remove(VotingFirstTarget);
                        int maxNext = 0;
                        if (temp.Count > 0)
                        {
                            maxNext = temp.Max(x => x.Value);
                        }
                        if (maxNext + Players.Count - VotingFirst.Count < votingFirstResult[VotingFirstTarget])
                        {
                            VotingFirstEnds = true;
                        }
                    }
                }

                // Проверка на линч
                if (VotingFirstEnds && !VotingFirstLynch && VotingFirstTarget != null)
                {
                    if (Players.Count * GameRules.coefLynch > timerLynch.Elapsed.TotalSeconds && votingFirstResult[VotingFirstTarget] >= VotingFirst.Count - 1 && VotingFirstTarget != Config.Guild.CurrentMember)
                    {
                        VotingFirstLynch = true;
                        await View.SendMain(GameMessages.ShowDoneLynch());
                    }
                }

                // Запускаем завершение голосования, если VotingFirstEnds стало true
                if (VotingFirstEnds)
                {
                    await Task.Delay(GameRules.intervalVotingAdditional);
                    // Если голосование еще не закончилось, то меняем статус
                    if (Status == GameStatus.VotingFirst)
                    {
                        await MoveStatus();
                    }
                }
            }
        }

        public static async Task ProcessVoteSecond(DiscordMember doer, Vote vote)
        {
            if (VotingFirstTarget != null) // Проверка на всякий случай, если каким то образом цель не выбралась
            {
                Dictionary<Vote, int> votingSecondResult = null;

                // Упрощаем код: функция добавления голоса
                Func<Task> AddVote = async () =>
                {
                    VotingSecond[doer] = vote;
                    votingSecondResult = VotingSecondResult;
                    await View.SendMain(GameMessages.ShowGolo2Stat(votingSecondResult, doer, vote));
                };

                if (VotingSecond.ContainsKey(doer))
                {
                    // Если уже голосовал и поменял цель
                    if (VotingSecond[doer] != vote)
                    {
                        if (vote == Vote.IDK)
                        {
                            await View.SendDM(doer, GameMessages.ShowCannotHZ());
                        } else
                        {
                            await AddVote();
                        }
                    }
                }
                else
                {
                    // Если еще не голосовал
                    await AddVote();
                }

                // Выполняем проверки, только если были изменения и уже не идет завершение голосования
                if (votingSecondResult != null && !VotingSecondEnds)
                {
                    if (votingSecondResult[Vote.Yes] > (Players.Count - 1) / 2)
                    {
                        VotingSecondEnds = true;
                    }

                    if (votingSecondResult[Vote.No] > (Players.Count - 1) / 2)
                    {
                        VotingSecondEnds = true;
                    }

                    if (votingSecondResult[Vote.Yes] + votingSecondResult[Vote.No] + votingSecondResult[Vote.IDK] == Players.Count - 1)
                    {
                        VotingSecondEnds = true;
                    }

                    // Запускаем завершение голосования, если VotingSecondEnds стало true
                    if (VotingSecondEnds)
                    {
                        await Task.Delay(GameRules.intervalVotingAdditional);
                        // Если голосование еще не закончилось, то меняем статус
                        if (Status == GameStatus.VotingSecond)
                        {
                            await MoveStatus();
                        }
                    }
                }
            }
        }

        public enum Vote
        {
            Yes,
            No,
            IDK
        }

        public static async Task AddToPlayersToKick(DiscordMember player)
        {
            if (On)
            {
                await View.SendMain(GameMessages.ShowUserKicked(player));
                PlayersToKick.Add(player);
                Players[player].Kicked = true;
                if (Players[player].Role == Role.Mafia)
                {
                    await View.RemoveFromLogovo(player);
                    await Task.Delay(1000);
                }
                await View.RemoveMainRole(player);
                await Task.Delay(500);
                await View.ReconnectToVoice(player);
            } else
            {
                await View.SendMain(GameMessages.ShowUnreg(player));
                Players.Remove(player);
                await View.RemoveMainRole(player);
            }
        }

        public static async Task ProcessPlayersToKick()
        {
            List<DiscordMember> playersToKick = new List<DiscordMember>(PlayersToKick);
            PlayersToKick.Clear();
            foreach (DiscordMember player in playersToKick)
            {
                await BotKill(new DayActionArgs { target = player });
                await Task.Delay(GameRules.minInterval);
            }
        }

        // Вспомогательные функции

        // Показывает количество игроков с определенной ролью
        public static int RoleCount(Role role)
        {
            return Players.Where(t => t.Value.Role == role).Count();
        }

        // Показывает сделал ли пользователь ход или нет
        public static bool MadeAnOrder(DiscordMember player)
        {
            if (NightOrders.ContainsKey(Players[player].Role))
            {
                return true;
            }
            return false;
        }

        // Показывает все ли сделали ход
        public static bool AllOrdersDone
        {
            get
            {
                return NightOrders.Count >= Active.Count;
            }
        }

        // Выдает список тех, кто не походил ночью
        public static List<DiscordMember> UnDoneOrders
        {
            get
            {
                List<DiscordMember> unDone = new List<DiscordMember>();

                foreach (DiscordMember doer in Active)
                {
                    if (!MadeAnOrder(doer))
                    {
                        unDone.Add(doer);
                    }
                }

                return unDone;
            }
        }

        // Показывает проголосовал ли пользователь в первом голосовании или нет
        public static bool MadeAVoteFirst(DiscordMember player)
        {
            if (VotingFirst.ContainsKey(player))
            {
                return true;
            }
            return false;
        }

        // Показывает проголосовал ли пользователь во втором голосовании или нет
        public static bool MadeAVoteSecond(DiscordMember player)
        {
            if (VotingSecond.ContainsKey(player))
            {
                return true;
            }
            return false;
        }

        // Находит игрока по пользователю
        public static DiscordMember FindPlayerByUser(DiscordUser user)
        {
            foreach (DiscordMember player in Players.Keys)
            {
                if (player.Id == user.Id)
                {
                    return player;
                }
            }
            return null;
        }

        // Находит игрока по роли
        public static DiscordMember FindPlayerByRole(Role role)
        {
            foreach (KeyValuePair<DiscordMember, PlayerProperties> entry in Players)
            {
                if (entry.Value.Role == role)
                {
                    return entry.Key;
                }
            }
            return null;
        }

        public static async Task MakeRandomOrder(DiscordMember doer)
        {
            Action action = Action.Check;
            List<DiscordMember> targetList = Players.Keys.ToList();
            switch (Players[doer].Role)
            {
                case Role.Mafia:
                    action = Action.Kill;
                    foreach (DiscordMember mafioso in MafiaPlayers)
                    {
                        if (targetList.Contains(mafioso) && mafioso != doer)
                        {
                            targetList.Remove(mafioso);
                        }
                    }
                    break;
                case Role.Commissar:
                    action = Action.Check;
                    break;
                case Role.Maniac:
                    action = Action.Kill;
                    break;
                case Role.Doctor:
                    action = Action.Heal;
                    break;
            }
            await AddNightOrder(null, action, doer, Utilities.RandomChoice(targetList), String.Empty);
        }

        public static async Task Pause()
        {
            if (Status == GameStatus.Idle)
            {
                Status = GameStatus.Pause;
                await View.SendMain("Бот остановлен на техническую паузу.");
            } else if (Status == GameStatus.Pause)
            {
                Status = GameStatus.Idle;
                await View.SendMain("Бот снят с технической паузы.");
            }
        }

        // Дебажные функции

        // Делает рандомные заказы
        public static async Task DEBUG_MakeRandomOrders()
        {
            foreach (DiscordMember doer in new List<DiscordMember>(Active))
            {
                if (!MadeAnOrder(doer) && Status == GameStatus.Night)
                {
                    await MakeRandomOrder(doer);
                    await Task.Delay(3000);
                }
            }
        }

        // Делает рандомные голоса
        public static async Task DEBUG_MakeRandomVotes(bool mode = false, DiscordMember target = null, string args = "")
        {
            // mode =
            //     false - голоса для выбора цели
            //     true  - голоса для выбора результата
            // target - цель при mode = false
            // args   - yes/no/hz
            int currentDay = Day;
            if (!mode)
            {
                List<DiscordMember> tempList = new List<DiscordMember>(Players.Keys)
                {
                    Config.Guild.CurrentMember
                };
                foreach (DiscordMember player in new List<DiscordMember>(Players.Keys))
                {
                    if (!MadeAVoteFirst(player) && Status == GameStatus.VotingFirst && currentDay == Day)
                    {
                        if (target == null)
                        {
                            await ProcessVoteFirst(player, Utilities.RandomChoice(tempList));
                        } else
                        {
                            await ProcessVoteFirst(player, target);
                        }
                        await Task.Delay(1000);
                    }
                }
            } else
            {
                List<Vote> tempList = new List<Vote>()
                {
                    Vote.Yes, Vote.No, Vote.IDK
                };
                foreach (DiscordMember player in new List<DiscordMember>(Players.Keys))
                {
                    if (!MadeAVoteSecond(player) && Status == GameStatus.VotingSecond && currentDay == Day && player != VotingFirstTarget)
                    {
                        switch (args)
                        {
                            case "yes":
                                await ProcessVoteSecond(player, Vote.Yes);
                                break;
                            case "no":
                                await ProcessVoteSecond(player, Vote.No);
                                break;
                            case "hz":
                                await ProcessVoteSecond(player, Vote.IDK);
                                break;
                            default:
                                await ProcessVoteSecond(player, Utilities.RandomChoice(tempList));
                                break;
                        }
                        await Task.Delay(1000);
                    }
                }
            }
        }
    }
}
