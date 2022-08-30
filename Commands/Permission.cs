using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using System.Linq;

namespace RPGMods.Commands
{
    [Command("permission, perm", Usage = "permission <list>|<save>|<reload>|<set> <0-100> <playername>|<steamid>", Description = "Manage commands and user permissions level.")]
    public static class Permission
    {
        public static void Initialize(Context ctx)
        {
            var args = ctx.Args;

            if (args.Length == 1)
            {
                if (args[0].ToLower().Equals("list"))
                {
                    _ = PermissionSystem.PermissionList(ctx);
                }
                else if (args[0].ToLower().Equals("save"))
                {
                    PermissionSystem.SaveUserPermission();
                    Output.SendSystemMessage(ctx, "Saved user permission to JSON file.");
                }
                else if (args[0].ToLower().Equals("reload"))
                {
                    PermissionSystem.LoadPermissions();
                    Output.SendSystemMessage(ctx, "Reloaded permission from JSON file.");
                }
                else
                {
                    Output.MissingArguments(ctx);
                }
                return;
            }

            if (args.Length < 3)
            {
                Output.MissingArguments(ctx);
                return;
            }

            if (args[0].ToLower().Equals("set")) {
                var tryParse = int.TryParse(args[1], out var level);
                if (!tryParse)
                {
                    Output.InvalidArguments(ctx);
                    return;
                }
                else
                {
                    if (level < 0 || level > 100)
                    {
                        Output.InvalidArguments(ctx);
                        return;
                    }
                }

                var tryParse_2 = ulong.TryParse(args[2], out var SteamID);
                string playerName = null;
                if (!tryParse_2)
                {
                    bool tryFind = Helper.FindPlayer(args[2], false, out _, out var target_userEntity);
                    if (!tryFind)
                    {
                        Output.CustomErrorMessage(ctx, $"Could not find specified player \"{args[2]}\".");
                        return;
                    }
                    playerName = args[2];
                    SteamID = Plugin.Server.EntityManager.GetComponentData<User>(target_userEntity).PlatformId;
                }
                else
                {
                    playerName = Helper.GetNameFromSteamID(SteamID);
                    if (playerName == null)
                    {
                        Output.CustomErrorMessage(ctx, $"Could not find specified player steam id \"{args[2]}\".");
                        return;
                    }
                }

                if (level == 0) Database.user_permission.Remove(SteamID);
                else Database.user_permission[SteamID] = level;

                Output.SendSystemMessage(ctx, $"Player \"{playerName}\" permission is now set to <color=#fffffffe>{level}</color>.");
                return;
            }
            else
            {
                Output.InvalidArguments(ctx);
                return;
            }
        }
    }
}
