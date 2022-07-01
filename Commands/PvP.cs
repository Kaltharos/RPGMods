using ProjectM.Network;
using RPGMods.Utils;
using System.Linq;
using Unity.Entities;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("pvp", Usage = "pvp [<on|off>]", Description = "Toggles PvP Mode for you or display your PvP statistics & the current leaders in the ladder.")]
    public static class PvP
    {
        public static bool isLadderEnabled = false;
        public static void Initialize(Context ctx)
        {
            var user = ctx.Event.User;
            var userEntity = ctx.Event.SenderUserEntity;
            var charEntity = ctx.Event.SenderCharacterEntity;
            var CharName = user.CharacterName.ToString();
            var SteamID = user.PlatformId;

            if (ctx.Args.Length == 0)
            {
                Database.pvpkills.TryGetValue(SteamID, out var pvp_kills);
                Database.pvpdeath.TryGetValue(SteamID, out var pvp_deaths);
                Database.pvpkd.TryGetValue(SteamID, out var pvp_kd);


                user.SendSystemMessage($"-- <color=#ffffffff>{CharName}</color> --");
                user.SendSystemMessage($"K/D: <color=#ffffffff>{pvp_kd} [{pvp_kills}/{pvp_deaths}]</color>");

                if (isLadderEnabled)
                {
                    var SortedKD = Database.pvpkd.ToList();
                    SortedKD.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
                    var Top5Ladder = SortedKD.Take(5);
                    user.SendSystemMessage($"===================================");
                    int i = 0;
                    foreach (var result in SortedKD.Take(5))
                    {
                        i++;
                        user.SendSystemMessage($"{i}. <color=#ffffffff>{CommandHelper.GetNameFromSteamID(result.Key)} : {result.Value}</color>");
                    }
                    if (i == 0) user.SendSystemMessage($"<color=#ffffffff>No Result</color>");
                    user.SendSystemMessage($"===================================");
                }
            }
            
            if (ctx.Args.Length > 0)
            {
                var isVampirePvPOn = false;
                if (ctx.Args[0].ToLower().Equals("on")) isVampirePvPOn = true;
                else if (ctx.Args[0].ToLower().Equals("off")) isVampirePvPOn = false;
                else
                {
                    Utils.CommandOutput.InvalidArguments(ctx);
                    return;
                }

                if (ctx.Args.Length == 1)
                {
                    if (CommandHelper.IsPlayerInCombat(charEntity))
                    {
                        Utils.CommandOutput.CustomErrorMessage(ctx, $"Unable to change PvP Toggle, you are in combat!");
                        return;
                    }
                    CommandHelper.SetPvP(charEntity, isVampirePvPOn);
                    string s = isVampirePvPOn ? "ON" : "OFF";
                    user.SendSystemMessage($"PvP is now {isVampirePvPOn}");
                    return;
                }
                else if (ctx.Args.Length == 2 && ctx.Event.User.IsAdmin)
                {
                    try
                    {
                        string name = ctx.Args[2];
                        if (CommandHelper.FindPlayer(name,true,out Entity targetChar, out Entity targetUser))
                        {
                            CommandHelper.SetPvP(targetChar, isVampirePvPOn);
                            string s = isVampirePvPOn ? "ON" : "OFF";
                            user.SendSystemMessage($"Player \"{name}\" PvP is now {isVampirePvPOn}");
                        }
                        else
                        {
                            Utils.CommandOutput.CustomErrorMessage(ctx, $"Unable to find the specified player!");
                        }
                    }
                    catch
                    {
                        Utils.CommandOutput.InvalidArguments(ctx);
                    }
                }
            }
        }
    }
}
