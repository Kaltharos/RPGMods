using RPGMods.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace RPGMods.Systems
{
    public static class PermissionSystem
    {
        public static bool isVIPSystem = true;
        public static int min_PermissionBypass_Login = 10;

        public static int GetUserPermission(ulong steamID)
        {
            bool isExist = Database.user_permission.TryGetValue(steamID, out var permission);
            if (isExist) return permission;
            return 0;
        }

        public static void SavePermissions()
        {
            File.WriteAllText("BepInEx/config/RPGMods/command_permission.json", JsonSerializer.Serialize(Database.command_permission, Database.Pretty_JSON_options));
        }

        public static void SaveUserPermission()
        {
            File.WriteAllText("BepInEx/config/RPGMods/user_permission.json", JsonSerializer.Serialize(Database.user_permission, Database.Pretty_JSON_options));
        }

        public static void LoadPermissions()
        {
            if (!File.Exists("BepInEx/config/RPGMods/user_permission.json"))
            {
                FileStream stream = File.Create("BepInEx/config/RPGMods/user_permission.json");
                stream.Dispose();
            }
            string json = File.ReadAllText("BepInEx/config/RPGMods/user_permission.json");
            try
            {
                Database.user_permission = JsonSerializer.Deserialize<Dictionary<ulong, int>>(json);
                Plugin.Logger.LogWarning("UserPermissions DB Populated");
            }
            catch
            {
                Database.user_permission = new Dictionary<ulong, int>();
                Plugin.Logger.LogWarning("UserPermission DB Created.");
            }

            if (!File.Exists("BepInEx/config/RPGMods/command_permission.json"))
            {
                FileStream stream = File.Create("BepInEx/config/RPGMods/command_permission.json");
                stream.Dispose();
            }
            json = File.ReadAllText("BepInEx/config/RPGMods/command_permission.json");
            try
            {
                Database.command_permission = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
                Plugin.Logger.LogWarning("CommandPermissions DB Populated");
            }
            catch
            {
                Database.command_permission = new Dictionary<string, int>();
                Plugin.Logger.LogWarning("CommandPermissions DB Created.");
            }
        }
    }
}
