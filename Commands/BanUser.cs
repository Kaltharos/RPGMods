using ProjectM.Network;
using RPGMods.Utils;
using RPGMods.Systems;
using System.Linq;
using System;

namespace RPGMods.Commands
{
    [Command("ban", Usage = "ban <playername> <days> <reason>", Description = "Check the status of specified player, or ban them. 0 is permanent.")]
    public static class BanUser
    {
        public static void Initialize(Context ctx)
        {
            var args = ctx.Args;

            if (args.Length == 1)
            {
                if (Helper.FindPlayer(args[0], false, out _, out var targetUserEntity_))
                {
                    var targetData_ = Plugin.Server.EntityManager.GetComponentData<User>(targetUserEntity_);
                    if (BanSystem.IsUserBanned(targetData_.PlatformId, out var banData_))
                    {
                        TimeSpan duration = banData_.BanUntil - DateTime.Now;
                        Output.SendSystemMessage(ctx, $"Player:<color=#fffffffe> {args[0]}</color>");
                        Output.SendSystemMessage(ctx, $"Status:<color=#fffffffe> Banned</color> | By:<color=#fffffffe> {banData_.BannedBy}</color>");
                        Output.SendSystemMessage(ctx, $"Duration:<color=#fffffffe> {Math.Round(duration.TotalDays)}</color> day(s) [<color=#fffffffe>{banData_.BanUntil}</color>]");
                        Output.SendSystemMessage(ctx, $"Reason:<color=#fffffffe> {banData_.Reason}</color>");
                        return;
                    }
                    else
                    {
                        Output.CustomErrorMessage(ctx, "Specified user is not banned.");
                        return;
                    }
                }
                else
                {
                    Output.CustomErrorMessage(ctx, "Unable to find the specified player.");
                    return;
                }
            }

            if (args.Length < 3)
            {
                Output.MissingArguments(ctx);
                return;
            }

            if (!int.TryParse(args[1], out var days))
            {
                Output.InvalidArguments(ctx);
                return;
            }

            var name = args[0];
            var reason = string.Join(' ', args.Skip(2));
            if (reason.Length > 150)
            {
                Output.CustomErrorMessage(ctx, "Keep the reason short will ya?!");
                return;
            }

            if (Helper.FindPlayer(name, false, out _, out var targetUserEntity))
            {
                if(BanSystem.BanUser(ctx.Event.SenderUserEntity, targetUserEntity, days, reason, out var banData))
                {
                    var user = ctx.Event.User;
                    Helper.KickPlayer(targetUserEntity);
                    Output.SendSystemMessage(ctx, $"Player \"{name}\" is now banned.");
                    Output.SendSystemMessage(ctx, $"Banned Until:<color=#fffffffe> {banData.BanUntil}</color>");
                    Output.SendSystemMessage(ctx, $"Reason:<color=#fffffffe> {reason}</color>");
                    return;
                }
                else
                {
                    Output.CustomErrorMessage(ctx, $"Failed to ban \"{name}\".");
                    return;
                }
            }
            else
            {
                Output.CustomErrorMessage(ctx, "Specified player not found.");
                return;
            }
        }
    }

    [Command("unban", Usage = "unban <playername>", Description = "Unban the specified player.")]
    public static class UnbanUser
    {
        public static void Initialize(Context ctx)
        {
            var args = ctx.Args;
            if (args.Length < 1)
            {
                Output.MissingArguments(ctx);
                return;
            }

            if (Helper.FindPlayer(args[0], false, out _, out var targetUserEntity))
            {
                if (BanSystem.UnbanUser(targetUserEntity))
                {
                    Output.SendSystemMessage(ctx, $"Player \"{args[0]}\" is no longer banned.");
                    return;
                }
                else
                {
                    Output.CustomErrorMessage(ctx, $"Specified player does not exist in the ban database.");
                    return;
                }
            }
            else
            {
                Output.CustomErrorMessage(ctx, "Specified player not found.");
                return;
            }
        }
    }
}
