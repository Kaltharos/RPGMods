using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using RPGMods.Utils;

namespace RPGMods.Commands
{
    [Command("godmode, god", Usage = "godmode", Description = "Toggles god mode.")]
    public static class GodMode
    {
        public static void Initialize(Context ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isGodMode = Database.godmode.TryGetValue(SteamID, out bool isGodMode_);
            if (isGodMode) isGodMode = false;
            else isGodMode = true;
            UpdateGodMode(ctx, isGodMode);
            string s = isGodMode ? "Activated" : "Deactivated";
            Output.SendSystemMessage(ctx, $"God mode <color=#ffff00ff>{s}</color>");
            Helper.ApplyBuff(ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, Database.buff.Buff_VBlood_Perk_Moose);
        }

        public static bool UpdateGodMode(Context ctx, bool isGodMode)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isExist = Database.godmode.TryGetValue(SteamID, out bool isGodMode_);
            if (isExist || !isGodMode) RemoveGodMode(ctx);
            else Database.godmode.Add(SteamID, isGodMode);
            return true;
        }

        public static void SaveGodMode()
        {
            File.WriteAllText("BepInEx/config/RPGMods/Saves/godmode.json", JsonSerializer.Serialize(Database.godmode, Database.JSON_options));
        }

        public static bool RemoveGodMode(Context ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            if (Database.godmode.TryGetValue(SteamID, out bool isGodMode_))
            {
                Database.godmode.Remove(SteamID);
                return true;
            }
            return false;
        }

        public static void LoadGodMode()
        {
            if (!File.Exists("BepInEx/config/RPGMods/Saves/godmode.json"))
            {
                var stream = File.Create("BepInEx/config/RPGMods/Saves/godmode.json");
                stream.Dispose();
            }
            string json = File.ReadAllText("BepInEx/config/RPGMods/Saves/godmode.json");
            try
            {
                Database.godmode = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                Plugin.Logger.LogWarning("GodMode DB Populated.");
            }
            catch
            {
                Database.godmode = new Dictionary<ulong, bool>();
                Plugin.Logger.LogWarning("GodMode DB Created.");
            }
        }
    }
}
