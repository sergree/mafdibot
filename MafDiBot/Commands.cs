using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Linq;

namespace MafDiBot
{
    public class Commands
    {
        [Command("help")]
        [Aliases("помощь", "gjvjom", "рудз")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [OnlyGameChannels]
        public async Task Help(CommandContext ctx)
        {
            await ctx.RespondAsync(GameMessages.ShowHelp());
        }

        [Command("start")]
        [Aliases("старт", "cnfhn", "ыефке")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [OnlyMainChannel]
        public async Task Start(CommandContext ctx)
        {
            if (Game.Status == Game.GameStatus.Idle)
            {
                await Game.MoveStatus();
            }
        }

        [Command("reg")]
        [Aliases("рег", "htu", "куп")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [OnlyMainChannel]
        public async Task Reg(CommandContext ctx)
        {
            if (Game.Status == Game.GameStatus.Registration || Game.Status == Game.GameStatus.Idle)
            {
                if (!Utilities.Administrator(ctx.Member))
                {
                    bool success = true;
                    if (Game.Players.Count < GameRules.maxNumPlayers) /* Условие для закрытия бага */
                    {
                        if (!Game.Players.ContainsKey(ctx.Member))
                        {
                            
                            success = await View.SendDM(ctx.Member, GameMessages.ShowReg_(ctx.Member));
                            if (success)
                            {
                                Game.Players.Add(ctx.Member, new Game.PlayerProperties());
                                await View.AddMainRole(ctx.Member);
                            } else
                            {
                                await View.SendMain(GameMessages.ShowCantPlay(ctx.Member));
                            }
                            //await ctx.RespondAsync(GameMessages.ShowReg(ctx.Member));
                        }
                        // !!!v--- Убрать ---v!!!
                        //else { DiscordMember temp = Utilities.RandomChoice(Config.Guild.Members); Game.Players.Add(temp, new Game.PlayerProperties()); await View.AddMainRole(temp); await ctx.RespondAsync(GameMessages.ShowReg(temp)); }
                        // !!!^--- Убрать ---^!!!
                    }
                    if (success)
                    {
                        if ((Game.Players.Count >= GameRules.maxNumPlayers && /* Условие для закрытия бага */ Game.Status == Game.GameStatus.Registration) || /* Запускаем регистрацию, если она еще не запущена */ Game.Status == Game.GameStatus.Idle)
                        {
                            await Game.MoveStatus();
                        }
                    }
                } else
                {
                    await View.SendMain($"{ctx.Member.Mention}, пользователи с привилегией **Администратор** не могут участвовать в игре!");
                }
            }
        }

        [Command("unreg")]
        [Aliases("анрег", "fyhtu", "гткуп")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [OnlyMainChannel]
        public async Task Unreg(CommandContext ctx)
        {
            if (Game.Status == Game.GameStatus.Registration)
            {
                if (Game.Players.ContainsKey(ctx.Member))
                {
                    Game.Players.Remove(ctx.Member);
                    await View.RemoveMainRole(ctx.Member);
                    await View.SendMain(GameMessages.ShowUnreg(ctx.Member));
                }
            }
        }

        [Command("pingme")]
        [Aliases("позовименя", "gjpjdbvtyz", "зштпьу")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [OnlyGameChannels]
        public async Task ProcessPingMe(CommandContext ctx)
        {
            if (!ctx.Channel.IsPrivate)
            {
                await PingMe.Process(ctx.Member);
            } else
            {
                DiscordMember member = await Config.Guild.GetMemberAsync(ctx.User.Id);
                if (member != null)
                {
                    await PingMe.Process(member);
                }
            }
        }

        [Command("killme")]
        [Aliases("смерть", "cvthnm", "лшддьу")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [OnlyGameChannels]
        public async Task KillMe(CommandContext ctx)
        {
            if (Game.On)
            {
                DiscordMember doer = Game.FindPlayerByUser(ctx.User);
                if (doer != null)
                {
                    await Game.AddToPlayersToKick(doer);
                }
            }
        }

        [Command("all")]
        [Aliases("все", "dct", "фдд")]
        [Cooldown(1, 3, CooldownBucketType.User)]
        [OnlyMainChannel]
        public async Task All(CommandContext ctx)
        {
            if (Game.Status == Game.GameStatus.LastWordFirst || Game.Status == Game.GameStatus.LastWordSecond)
            {
                if (ctx.Member == Game.VotingFirstTarget)
                {
                    await Game.MoveStatus();
                }
            }
        }

        [Command("list")]
        [Aliases("лист", "kbcn", "дшые", "players", "игроки", "buhjrb", "здфнукы")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [OnlyGameChannels]
        public async Task List(CommandContext ctx)
        {
            await ctx.RespondAsync(GameMessages.ShowList(false, true));
        }

        [Command("q")]
        [Aliases("кол", "rjk", "й")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [OnlyGameChannels]
        public async Task Quantity(CommandContext ctx)
        {
            await ctx.RespondAsync(GameMessages.ShowPlayers());
        }

        [Command("stat")]
        [Aliases("стат", "cnfn", "ыефе", "roles", "роли", "hjkb", "кщдуы")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [OnlyGameChannels]
        public async Task Stat(CommandContext ctx)
        {
            if (Game.On)
            {
                await ctx.RespondAsync(GameMessages.ShowPlayersList());
            }
        }

        [Command("game")]
        [Aliases("игра", "buhf", "пфьу")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [OnlyGameChannels]
        public async Task CurGameStat(CommandContext ctx)
        {
            if (Game.On)
            {
                await ctx.RespondAsync(GameMessages.ShowCurGameStat());
            }
        }

        [Command("turns")]
        [Aliases("ходы", "[jls", "егкты")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [OnlyGameChannels]
        public async Task Turns(CommandContext ctx)
        {
            if (Game.Status == Game.GameStatus.Night)
            {
                await ctx.RespondAsync(GameMessages.ShowTurns());
            }
        }

        // Дневное голосование за цель и ночь

        [Command("vote")]
        [Aliases("голос", "ujkjc", "мщеу", "v", "г", "u", "м")]
        [Cooldown(1, 4, CooldownBucketType.User)]
        [OnlyMainChannel]
        public async Task Vote(CommandContext ctx, [RemainingText] string args = "")
        {
            if (Game.Status == Game.GameStatus.VotingFirst)
            {
                if (Game.Players.ContainsKey(ctx.Member))
                {
                    TargetCommandArgs targetCommandArgs = ParseTargetCommand(args);
                    if (targetCommandArgs.Member != null)
                    {
                        await Game.ProcessVoteFirst(ctx.Member, targetCommandArgs.Member);
                    }
                }
            }
        }

        [Command("yes")]
        [Aliases("да", "lf", "нуы", "y", "д")]
        [Cooldown(1, 3, CooldownBucketType.User)]
        [OnlyMainChannel]
        public async Task Yes(CommandContext ctx)
        {
            if (Game.Status == Game.GameStatus.VotingSecond)
            {
                if (Game.Players.ContainsKey(ctx.Member) && ctx.Member != Game.VotingFirstTarget)
                {
                    await Game.ProcessVoteSecond(ctx.Member, Game.Vote.Yes);
                }
            }
        }

        [Command("no")]
        [Aliases("нет", "ytn", "тщ", "n", "н")]
        [Cooldown(1, 3, CooldownBucketType.User)]
        [OnlyMainChannel]
        public async Task No(CommandContext ctx)
        {
            if (Game.Status == Game.GameStatus.VotingSecond)
            {
                if (Game.Players.ContainsKey(ctx.Member) && ctx.Member != Game.VotingFirstTarget)
                {
                    await Game.ProcessVoteSecond(ctx.Member, Game.Vote.No);
                }
            }
        }

        [Command("hz")]
        [Aliases("хз", "[p", "ря")]
        [Cooldown(1, 3, CooldownBucketType.User)]
        [OnlyMainChannel]
        public async Task IDK(CommandContext ctx)
        {
            if (Game.Status == Game.GameStatus.VotingSecond)
            {
                if (Game.Players.ContainsKey(ctx.Member) && ctx.Member != Game.VotingFirstTarget)
                {
                    await Game.ProcessVoteSecond(ctx.Member, Game.Vote.IDK);
                }
            }
        }

        [Command("night")]
        [Aliases("ночь", "yjxm", "тшпре")]
        [Cooldown(1, 4, CooldownBucketType.User)]
        [OnlyMainChannel]
        public async Task VoteNight(CommandContext ctx)
        {
            if (Game.Status == Game.GameStatus.VotingFirst)
            {
                if (Game.Players.ContainsKey(ctx.Member))
                {
                    await Game.ProcessVoteFirst(ctx.Member, Config.Guild.CurrentMember);
                }
            }
        }

        // Ночные действия

        [Command("role")]
        [Aliases("роль", "hjkm", "кщду")]
        [Cooldown(1, 3, CooldownBucketType.User)]
        [OnlyGameChannels]
        public async Task Role(CommandContext ctx)
        {
            if (Game.On)
            {
                DiscordMember doer = Game.FindPlayerByUser(ctx.User);
                if (doer != null)
                {
                    await View.SendDM(doer, GameMessages.ShowRole(doer));
                }
            }
        }

        [Command("kill")]
        [Aliases("убить", "e,bnm", "лшдд")]
        [Cooldown(1, 3, CooldownBucketType.User)]
        [OnlyDMOrClosedChannels]
        public async Task Kill(CommandContext ctx, [RemainingText] string args = "")
        {
            await ProcessTargetCommand(ctx.Channel, Game.Action.Kill, ctx.User, ParseTargetCommand(args));
        }

        [Command("check")]
        [Aliases("проверить", "ghjdthbnm", "срусл")]
        [Cooldown(1, 3, CooldownBucketType.User)]
        [OnlyDMOrClosedChannels]
        public async Task Check(CommandContext ctx, [RemainingText] string args = "")
        {
            await ProcessTargetCommand(ctx.Channel, Game.Action.Check, ctx.User, ParseTargetCommand(args));
        }

        [Command("heal")]
        [Aliases("лечить", "ktxbnm", "руфд")]
        [Cooldown(1, 3, CooldownBucketType.User)]
        [OnlyDMOrClosedChannels]
        public async Task Heal(CommandContext ctx, [RemainingText] string args = "")
        {
            await ProcessTargetCommand(ctx.Channel, Game.Action.Heal, ctx.User, ParseTargetCommand(args));
        }

        async Task ProcessTargetCommand(DiscordChannel channel, Game.Action action, DiscordUser user, TargetCommandArgs targetCommandArgs)
        {
            // Проверяем ночь ли сейчас
            if (Game.Status == Game.GameStatus.Night)
            {
                DiscordMember doer = Game.FindPlayerByUser(user);
                // Проверяем является ли пользователь участником игры
                if (doer != null)
                {
                    bool authorized = true;

                    // Проверяем является ли игрок активной ролью (или ходящим мафом) или сделал ли участик ход
                    if (!Game.Active.Contains(doer) || Game.MadeAnOrder(doer))
                    {
                        authorized = false;
                    }
                    
                    // Проверяем совпадает ли действие с ролью
                    if (authorized)
                    {
                        switch (action)
                        {
                            case Game.Action.Kill:
                                //if (!(Game.Players[doer].Role == Game.Role.Commissar || Game.Players[doer].Role == Game.Role.Mafia || Game.Players[doer].Role == Game.Role.Maniac)) ДОБАВИТЬ: возможность убийства у комиссара после введения денежной системы
                                if (!(Game.Players[doer].Role == Game.Role.Mafia || Game.Players[doer].Role == Game.Role.Maniac))
                                {
                                    authorized = false;
                                }
                                break;
                            case Game.Action.Check:
                                if (!(Game.Players[doer].Role == Game.Role.Commissar))
                                {
                                    authorized = false;
                                }
                                break;
                            case Game.Action.Heal:
                                if (!(Game.Players[doer].Role == Game.Role.Doctor))
                                {
                                    authorized = false;
                                }
                                break;
                        }

                        // Если все ок, то вносим проверяем цель и вносим
                        if (authorized)
                        {
                            if (targetCommandArgs.Member != null)
                            {
                                // Проверка чтобы мафиози не убивал мафиози
                                if (action == Game.Action.Kill && Game.Players[doer].Role == Game.Role.Mafia && Game.Players[targetCommandArgs.Member].Role == Game.Role.Mafia && doer != targetCommandArgs.Member)
                                {
                                    await View.Send(channel, GameMessages.ShowCannotKillLogovoMember());
                                } else
                                {
                                    await Game.AddNightOrder(channel, action, doer, targetCommandArgs.Member, targetCommandArgs.Message);
                                }
                            }
                            else {
                                await View.Send(channel, GameMessages.ShowNoPlayer());
                            }
                        }
                    }
                }
            }
        }

        [Command("cancel")]
        [Aliases("отмена", "jnvtyf", "сфтсуд")]
        [Cooldown(1, 3, CooldownBucketType.User)]
        [OnlyDMOrClosedChannels]
        public async Task Cancel(CommandContext ctx)
        {
            if (Game.Status == Game.GameStatus.Night)
            {
                DiscordMember doer = Game.FindPlayerByUser(ctx.User);
                if (Game.MadeAnOrder(doer))
                {
                    await Game.CancelNightOrder(ctx.Channel, doer);
                }
            }
        }

        // -- ПАРСИНГ КОМАНД --
        TargetCommandArgs ParseTargetCommand (string input)
        {
            TargetCommandArgs parsed = new TargetCommandArgs();

            // Сплит по пробелу
            string[] inputSplitted = input.Split(' ');

            // Проверяем число ли в нулевом и присваиваем
            if (int.TryParse(inputSplitted[0], out int value))
            {
                try
                {
                    parsed.Member = Game.Players.Keys.ElementAt(value - 1);
                }
                catch (Exception) { }
            }

            // Проверяем упоминание ли в нулевом и присваиваем
            if (parsed.Member == null)
            {
                try
                {
                    DiscordMember target = Utilities.GetPlayerByMention(inputSplitted[0]);
                    if (Game.Players.ContainsKey(target))
                    {
                        parsed.Member = target;
                    }
                }
                catch (Exception) { }
            }

            // Джойним остальные
            if (inputSplitted.Length > 1)
            {
                string[] messageSplitted = inputSplitted.Skip(1).ToArray();
                parsed.Message = String.Join(" ", messageSplitted);
            }

            return parsed;
        }

        class TargetCommandArgs
        {
            public DiscordMember Member { get; set; } = null;
            public string Message { get; set; } = String.Empty;
        }
        // -- -- -- -- -- -- --

        // vvvvvvvvvvvvvvvvvvvvvvvv
        // vvvАДМИНСКИЕ  КОМАНДЫvvv
        // vvvvvvvvvvvvvvvvvvvvvvvv
        /*
                [Command("ikill")]
                [RequirePermissions(DSharpPlus.Permissions.Administrator)]
                public async Task InstantKill(CommandContext ctx, [RemainingText] string args = "")
                {
                    TargetCommandArgs targetCommandArgs = ParseTargetCommand(args);

                    if (targetCommandArgs.Member != null)
                    {
                        await Game.Death(targetCommandArgs.Member);
                        if (targetCommandArgs.Message != String.Empty)
                        {
                            await ctx.RespondAsync($"{ctx.Member.Mention} убил {targetCommandArgs.Member.Mention}: *{targetCommandArgs.Message}*");
                        }
                        else {
                            await ctx.RespondAsync($"{ctx.Member.Mention} убил {targetCommandArgs.Member.Mention}.");
                        }
                    }
                    else {
                        await ctx.RespondAsync($"Неверная цель!");
                    }
                }

                [Command("rlist")]
                [RequirePermissions(DSharpPlus.Permissions.Administrator)]
                public async Task RolesList(CommandContext ctx)
                {
                    await ctx.RespondAsync(GameMessages.ShowList(true));
                }

                [Command("mro")]
                [RequirePermissions(DSharpPlus.Permissions.Administrator)]
                public async Task MakeRandomOrders(CommandContext ctx)
                {
                    if(Game.Status == Game.GameStatus.Night)
                    {
                        await Game.DEBUG_MakeRandomOrders();
                    }
                }

                [Command("mrv")]
                [RequirePermissions(DSharpPlus.Permissions.Administrator)]
                public async Task MakeRandomVotes(CommandContext ctx, [RemainingText] string args = "")
                {
                    if (Game.Status == Game.GameStatus.VotingFirst)
                    {
                        TargetCommandArgs targetCommandArgs = ParseTargetCommand(args);
                        await Game.DEBUG_MakeRandomVotes(false, targetCommandArgs.Member, args);
                    }
                    if (Game.Status == Game.GameStatus.VotingSecond)
                    {
                        await Game.DEBUG_MakeRandomVotes(true, null, args);
                    }
                }

                [Command("stop")]
                [RequirePermissions(DSharpPlus.Permissions.Administrator)]
                public async Task Stop(CommandContext ctx)
                {
                    await Game.Reset();
                }

                [Command("pause")]
                [RequirePermissions(DSharpPlus.Permissions.Administrator)]
                public async Task Pause(CommandContext ctx)
                {
                    await Game.Pause();
                }
        */
        [Command("wipe")]
        [OnlyGameChannels]
        public async Task Wipe(CommandContext ctx)
        {
            DiscordMember member = ctx.Member;
            if (ctx.Member == null)
            {
                member = await Config.Guild.GetMemberAsync(ctx.User.Id);
            }
            if (Utilities.Administrator(member))
            {
                await ctx.RespondAsync("`Процедура Wipe() запущена...`");
                await View.Wipe(true);
                await ctx.RespondAsync("`Выполнение процедуры Wipe() завершено!`");
            }
        }
    }
}
