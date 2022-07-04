using ProjectM;
using RPGMods.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("speed", Usage = "speed", Description = "Toggles increased movement speed.")]

    public static class Speed
    {
        public static void Initialize(Context ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isSpeeding = Database.speeding.TryGetValue(SteamID, out bool isSpeeding_);
            if (isSpeeding) isSpeeding = false;
            else isSpeeding = true;
            UpdateSpeed(ctx, isSpeeding);
            string s = isSpeeding ? "Activated" : "Deactivated";
            ctx.Event.User.SendSystemMessage($"Speed buff <color=#ffff00ff>{s}</color>");
            Helper.ApplyBuff(ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, Database.buff.Buff_VBlood_Perk_Moose);
        }

        public static bool UpdateSpeed(Context ctx, bool isGodMode)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isExist = Database.speeding.TryGetValue(SteamID, out bool isSpeeding_);
            if (isExist || !isGodMode) RemoveSpeed(ctx);
            else Database.speeding.Add(SteamID, isGodMode);
            return true;
        }

        public static void SaveSpeed()
        {
            File.WriteAllText("BepInEx/config/RPGMods/Saves/speeding.json", JsonSerializer.Serialize(Database.speeding, Database.JSON_options));
        }

        public static bool RemoveSpeed(Context ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId; ;
            if (Database.speeding.TryGetValue(SteamID, out bool isGodMode_))
            {
                Database.speeding.Remove(SteamID);
                return true;
            }
            return false;
        }

        public static void LoadSpeed()
        {
            if (!File.Exists("BepInEx/config/RPGMods/Saves/speeding.json"))
            {
                var stream = File.Create("BepInEx/config/RPGMods/Saves/speeding.json");
                stream.Dispose();
            }
            string json = File.ReadAllText("BepInEx/config/RPGMods/Saves/speeding.json");
            try
            {
                Database.speeding = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                Plugin.Logger.LogWarning("Speed DB Populated.");
            }
            catch
            {
                Database.speeding = new Dictionary<ulong, bool>();
                Plugin.Logger.LogWarning("Speed DB Created.");
            }
        }
    }
}
