using ProjectM;
using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using System;
using System.Collections.Generic;
using Unity.Entities;

namespace RPGMods.Commands
{
    [Command("siege", Usage = "siege [<on>|<off>]", Description = "Display all players currently in siege mode, or engage siege mode.")]
    public static class Siege
    {
        private static Dictionary<ulong, DateTime> SiegeConfirm = new();
        public static void Initialize(Context ctx)
        {
            if (PvPSystem.isHonorSystemEnabled == false || PvPSystem.isHonorBenefitEnabled == false)
            {
                Output.CustomErrorMessage(ctx, "Honor system is not enabled.");
                return;
            }

            var user = ctx.Event.User;
            var userEntity = ctx.Event.SenderUserEntity;
            var charEntity = ctx.Event.SenderCharacterEntity;
            var CharName = user.CharacterName.ToString();
            var SteamID = user.PlatformId;

            Database.PvPStats.TryGetValue(SteamID, out var PvPStats);
            Database.SiegeState.TryGetValue(SteamID, out var siegeState);

            if (ctx.Args.Length == 0)
            {
                if (siegeState.IsSiegeOn)
                {
                    if (PvPStats.Reputation <= -20000)
                    {
                        Output.CustomErrorMessage(ctx, $"You're [{PvPSystem.GetHonorTitle(PvPStats.Reputation).Title}], siege mode is enforced.");
                    }
                    TimeSpan TimeLeft = siegeState.SiegeEndTime - DateTime.Now;
                    double tLeft = Math.Round(TimeLeft.TotalHours, 2);

                    Output.SendSystemMessage(ctx, $"Siege mode will end in {Color.White(tLeft.ToString())} hour(s)");
                }
                else
                {
                    Output.SendSystemMessage(ctx, $"You're currently in defensive mode.");
                }

                _ = PvPSystem.SiegeList(ctx);
                return;
            }

            if (ctx.Args.Length == 1 && ctx.Args[0].ToLower().Equals("on"))
            {
                bool doConfirm = SiegeConfirm.TryGetValue(SteamID, out DateTime TimeStamp);
                if (doConfirm)
                {
                    TimeSpan span = DateTime.Now - TimeStamp;
                    if (span.TotalSeconds > 60)
                    {
                        doConfirm = false;
                    }
                }

                if (!doConfirm)
                {
                    if (Database.SiegeState.TryGetValue(SteamID, out var siegeData))
                    {
                        if (siegeData.IsSiegeOn)
                        {
                            Output.CustomErrorMessage(ctx, "You're already in active siege mode.");
                            return;
                        }
                    }

                    Output.SendSystemMessage(ctx, "Are you sure you want to enter castle siege mode?");
                    TimeSpan TimeLeft = DateTime.Now.AddMinutes(PvPSystem.SiegeDuration) - DateTime.Now;
                    double calcHours = Math.Round(TimeLeft.TotalHours, 2);
                    Output.SendSystemMessage(ctx, "You and your allies will not be able to exit siege mode for (" + calcHours + ") hours once you start.");
                    Output.SendSystemMessage(ctx, "Type \"" + CommandHandler.Prefix + "siege on\" again to confirm.");
                    SiegeConfirm.Add(SteamID, DateTime.Now);
                    return;
                }
                else
                {
                    PvPSystem.SiegeON(SteamID, charEntity, userEntity);
                    SiegeConfirm.Remove(SteamID);
                    Output.SendSystemMessage(ctx, "Active siege mode engaged.");
                    return;
                }
            }
            else if (ctx.Args.Length == 1 && ctx.Args[0].ToLower().Equals("off"))
            {
                Helper.GetAllies(charEntity, out var allies);
                if (allies.AllyCount > 0)
                {
                    allies.Allies.Add(userEntity, charEntity);
                    foreach(var ally in allies.Allies)
                    {
                        Cache.HostilityState.TryGetValue(ally.Value, out var hostilityState);
                        Database.PvPStats.TryGetValue(hostilityState.SteamID, out var stats);
                        if (stats.Reputation <= -20000)
                        {
                            PvPStats.Reputation = -20000;
                            break;
                        }
                    }
                }

                if (PvPStats.Reputation <= -20000)
                {
                    Output.CustomErrorMessage(ctx, $"You or your allies are [{PvPSystem.GetHonorTitle(PvPStats.Reputation).Title}], siege mode is enforced.");
                    return;
                }
                TimeSpan TimeLeft = siegeState.SiegeEndTime - DateTime.Now;
                
                if (TimeLeft.TotalSeconds <= 0)
                {
                    PvPSystem.SiegeOFF(SteamID, charEntity);
                    Output.SendSystemMessage(ctx, "Defensive siege mode engaged.");
                    return;
                }
                else
                {
                    double tLeft = Math.Round(TimeLeft.TotalHours, 2);
                    Output.SendSystemMessage(ctx, $"Siege mode cannot be ended until {Color.White(tLeft.ToString())} more hour(s)");
                    return;
                }
            }
            else if (ctx.Args.Length == 1 && ctx.Args[0].ToLower().Equals("global") && (ctx.Event.User.IsAdmin || PermissionSystem.PermissionCheck(ctx.Event.User.PlatformId, "siege_args")))
            {
                if (PvPSystem.Interlocked.isSiegeOn)
                {
                    PvPSystem.Interlocked.isSiegeOn = false;
                    ServerChatUtils.SendSystemMessageToAllClients(Plugin.Server.EntityManager, "Server wide siege mode has been deactivated!");
                }
                else
                {
                    PvPSystem.Interlocked.isSiegeOn = true;
                    ServerChatUtils.SendSystemMessageToAllClients(Plugin.Server.EntityManager, "Server wide siege mode is now active!");
                }
            }
            else
            {
                Output.InvalidArguments(ctx);
            }
        }
    }
}
