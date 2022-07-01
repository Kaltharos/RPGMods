using ProjectM.Network;
using RPGMods.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Unity.Entities;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("autorespawn", Usage = "autorespawn [<PlayerName>]", Description = "Toggle auto respawn on the same position on death.")]
    public static class AutoRespawn
    {
        public static void Initialize(Context ctx)
        {
            var entityManager = ctx.EntityManager;
            ulong SteamID = ctx.Event.User.PlatformId;
            string PlayerName = ctx.Event.User.CharacterName.ToString();
            bool isServerWide = false;
            if (ctx.Args.Length > 0 && ctx.Event.User.IsAdmin)
            {
                string TargetName = string.Join(' ', ctx.Args);
                if (TargetName.ToLower().Equals("all"))
                {
                    SteamID = 1;
                    isServerWide = true;
                }
                else
                {
                    if (CommandHelper.FindPlayer(TargetName, false, out Entity targetEntity, out Entity targetUserEntity))
                    {
                        var user_component = entityManager.GetComponentData<User>(targetUserEntity);
                        SteamID = user_component.PlatformId;
                        PlayerName = TargetName;
                    }
                    else
                    {
                        Utils.CommandOutput.CustomErrorMessage(ctx, $"Player \"{TargetName}\" not found!");
                        return;
                    }
                }
            }
            bool isAutoRespawn = Database.autoRespawn.ContainsKey(SteamID);
            if (isAutoRespawn) isAutoRespawn = false;
            else isAutoRespawn = true;
            UpdateAutoRespawn(SteamID, isAutoRespawn);
            string s = isAutoRespawn ? "Activated" : "Deactivated";
            if (isServerWide)
            {
                ctx.Event.User.SendSystemMessage($"Server wide Auto Respawn <color=#ffff00ff>{s}</color>");
            }
            else
            {
                ctx.Event.User.SendSystemMessage($"Player \"{PlayerName}\" Auto Respawn <color=#ffff00ff>{s}</color>");
            }
        }

        public static bool UpdateAutoRespawn(ulong SteamID, bool isAutoRespawn)
        {
            bool isExist = Database.autoRespawn.ContainsKey(SteamID);
            if (isExist || !isAutoRespawn) RemoveAutoRespawn(SteamID);
            else Database.autoRespawn.Add(SteamID, isAutoRespawn);
            return true;
        }

        public static void SaveAutoRespawn()
        {
            File.WriteAllText("BepInEx/config/RPGMods/Saves/autorespawn.json", JsonSerializer.Serialize(Database.autoRespawn, Database.JSON_options));
        }

        public static bool RemoveAutoRespawn(ulong SteamID)
        {
            if (Database.autoRespawn.ContainsKey(SteamID))
            {
                Database.autoRespawn.Remove(SteamID);
                return true;
            }
            return false;
        }

        public static void LoadAutoRespawn()
        {
            if (!File.Exists("BepInEx/config/RPGMods/Saves/autorespawn.json"))
            {
                var stream = File.Create("BepInEx/config/RPGMods/Saves/autorespawn.json");
                stream.Dispose();
            }
            string json = File.ReadAllText("BepInEx/config/RPGMods/Saves/autorespawn.json");
            try
            {
                Database.autoRespawn = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                Plugin.Logger.LogWarning("AutoRespawn List Populated.");
            }
            catch
            {
                Database.autoRespawn = new Dictionary<ulong, bool>();
                Plugin.Logger.LogWarning("New AutoRespawn List Created.");
            }
        }
    }
}
