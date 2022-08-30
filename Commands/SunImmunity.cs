using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using RPGMods.Utils;

namespace RPGMods.Commands
{
    [Command("sunimmunity, sun", Usage = "sunimmunity", Description = "Toggles sun immunity.")]
    public static class SunImmunity
    {
        public static void Initialize(Context ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isSunImmune = Database.sunimmunity.ContainsKey(SteamID);
            if (isSunImmune) isSunImmune = false;
            else isSunImmune = true;
            UpdateImmunity(ctx, isSunImmune);
            string s = isSunImmune ? "Activated" : "Deactivated";
            Output.SendSystemMessage(ctx, $"Sun Immunity <color=#ffff00>{s}</color>");
            Helper.ApplyBuff(ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, Database.Buff.Buff_VBlood_Perk_Moose);
        }

        public static bool UpdateImmunity(Context ctx, bool isSunImmune)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isExist = Database.sunimmunity.ContainsKey(SteamID);
            if (isExist || !isSunImmune) RemoveImmunity(ctx);
            else Database.sunimmunity.Add(SteamID, isSunImmune);
            return true;
        }

        public static void SaveImmunity()
        {
            File.WriteAllText("BepInEx/config/RPGMods/Saves/sunimmunity.json", JsonSerializer.Serialize(Database.sunimmunity, Database.JSON_options));
        }

        public static bool RemoveImmunity(Context ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            if (Database.sunimmunity.ContainsKey(SteamID))
            {
                Database.sunimmunity.Remove(SteamID);
                return true;
            }
            return false;
        }

        public static void LoadSunImmunity()
        {
            if (!File.Exists("BepInEx/config/RPGMods/Saves/sunimmunity.json"))
            {
                var stream = File.Create("BepInEx/config/RPGMods/Saves/sunimmunity.json");
                stream.Dispose();
            }

            string json = File.ReadAllText("BepInEx/config/RPGMods/Saves/sunimmunity.json");
            try
            {
                Database.sunimmunity = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                Plugin.Logger.LogWarning("SunImmunity DB Populated.");
            }
            catch
            {
                Database.sunimmunity = new Dictionary<ulong, bool>();
                Plugin.Logger.LogWarning("SunImmunity DB Created.");
            }
        }
    }
}
