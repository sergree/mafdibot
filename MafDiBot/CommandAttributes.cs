using System;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Attributes
{
    // Только приваты
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class OnlyDM : CheckBaseAttribute
    {
        public override Task<bool> CanExecute(CommandContext ctx, bool help)
            => Task.FromResult(ctx.Guild == null);
    }
    // Только сервер
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class OnlyGuild1 : CheckBaseAttribute
    {
        public override Task<bool> CanExecute(CommandContext ctx, bool help)
            => Task.FromResult(ctx.Guild != null);
    }

    // Только главный канал игры
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class OnlyMainChannel : CheckBaseAttribute
    {
        public override Task<bool> CanExecute(CommandContext ctx, bool help)
            => Task.FromResult(ctx.Channel.Id == MafDiBot.Config.ChannelMainId);
    }
    // Только приваты или закрытые игровые каналы (логово, офис, кладбище)
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class OnlyDMOrClosedChannels : CheckBaseAttribute
    {
        public override Task<bool> CanExecute(CommandContext ctx, bool help)
            => Task.FromResult(ctx.Guild == null || ctx.Channel.Id == MafDiBot.View.LogovoId);
    }
    // Только приваты и игровые каналы
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class OnlyGameChannels : CheckBaseAttribute
    {
        public override Task<bool> CanExecute(CommandContext ctx, bool help)
            => Task.FromResult(ctx.Guild == null || ctx.Channel.Id == MafDiBot.View.LogovoId || ctx.Channel.Id == MafDiBot.Config.ChannelMainId);
    }
}
