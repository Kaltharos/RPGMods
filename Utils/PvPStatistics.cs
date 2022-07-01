using ProjectM;
using ProjectM.Network;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Unity.Entities;
using Wetstone.API;

namespace RPGMods.Utils
{
    public class PvPStatistics
    {
        public static bool announce_kills = true;
        private static EntityManager entityManager = VWorld.Server.EntityManager;
        public static void Monitor(Entity KillerEntity, Entity VictimEntity)
        {
            var killer = entityManager.GetComponentData<PlayerCharacter>(KillerEntity);
            var killer_userEntity = killer.UserEntity._Entity;
            var killer_user = entityManager.GetComponentData<User>(killer_userEntity);
            var killer_name = killer_user.CharacterName.ToString();
            var killer_id = killer_user.PlatformId;

            var victim = entityManager.GetComponentData<PlayerCharacter>(VictimEntity);
            var victim_userEntity = victim.UserEntity._Entity;
            var victim_user = entityManager.GetComponentData<User>(victim_userEntity);
            var victim_name = victim_user.CharacterName.ToString();
            var victim_id = victim_user.PlatformId;

            victim_user.SendSystemMessage($"<color=#c90e21ff>You've been killed by \"{killer_name}\"</color>");

            Database.pvpkills.TryGetValue(killer_id, out var KillerKills);
            Database.pvpdeath.TryGetValue(victim_id, out var VictimDeath);

            Database.pvpkills[killer_id] = KillerKills + 1;
            Database.pvpdeath[victim_id] = VictimDeath + 1;

            //-- Update K/D
            UpdateKD(killer_id, victim_id);

            //-- Announce Kills
            if (announce_kills) ServerChatUtils.SendSystemMessageToAllClients(entityManager, $"Vampire \"{killer_name}\" has killed \"{victim_name}\"!");
        }

        public static void UpdateKD(ulong killer_id, ulong victim_id)
        {
            var isExist = Database.pvpdeath.TryGetValue(killer_id, out _);
            if (!isExist) Database.pvpdeath[killer_id] = 0;

            isExist = Database.pvpkills.TryGetValue(victim_id, out _);
            if (!isExist) Database.pvpkills[victim_id] = 0;

            if (Database.pvpdeath[killer_id] != 0) Database.pvpkd[killer_id] = (double) Database.pvpkills[killer_id] / Database.pvpdeath[killer_id];
            else Database.pvpkd[killer_id] = Database.pvpkills[killer_id];

            if (Database.pvpkills[victim_id] != 0) Database.pvpkd[victim_id] = (double) Database.pvpkills[victim_id] / Database.pvpdeath[victim_id];
            else Database.pvpkd[victim_id] = 0;
        }

        public static void SavePvPStat()
        {
            File.WriteAllText("BepInEx/config/RPGMods/Saves/pvpkills.json", JsonSerializer.Serialize(Database.pvpkills, Database.JSON_options));
            File.WriteAllText("BepInEx/config/RPGMods/Saves/pvpdeath.json", JsonSerializer.Serialize(Database.pvpdeath, Database.JSON_options));
            File.WriteAllText("BepInEx/config/RPGMods/Saves/pvpkd.json", JsonSerializer.Serialize(Database.pvpkd, Database.JSON_options));
        }

        public static void LoadPvPStat()
        {
            if (!File.Exists("BepInEx/config/RPGMods/Saves/pvpkills.json"))
            {
                var stream = File.Create("BepInEx/config/RPGMods/Saves/pvpkills.json");
                stream.Dispose();
            }
            string json = File.ReadAllText("BepInEx/config/RPGMods/Saves/pvpkills.json");
            try
            {
                Database.pvpkills = JsonSerializer.Deserialize<Dictionary<ulong, int>>(json);
                Plugin.Logger.LogWarning("PvP Kills List Populated.");
            }
            catch
            {
                Database.pvpkills = new Dictionary<ulong, int>();
                Plugin.Logger.LogWarning("PvP Kills List Created.");
            }

            if (!File.Exists("BepInEx/config/RPGMods/Saves/pvpdeath.json"))
            {
                var stream = File.Create("BepInEx/config/RPGMods/Saves/pvpdeath.json");
                stream.Dispose();
            }
            json = File.ReadAllText("BepInEx/config/RPGMods/Saves/pvpdeath.json");
            try
            {
                Database.pvpdeath = JsonSerializer.Deserialize<Dictionary<ulong, int>>(json);
                Plugin.Logger.LogWarning("PvP Death List Populated.");
            }
            catch
            {
                Database.pvpdeath = new Dictionary<ulong, int>();
                Plugin.Logger.LogWarning("PvP Death List Created.");
            }

            if (!File.Exists("BepInEx/config/RPGMods/Saves/pvpkd.json"))
            {
                var stream = File.Create("BepInEx/config/RPGMods/Saves/pvpkd.json");
                stream.Dispose();
            }
            json = File.ReadAllText("BepInEx/config/RPGMods/Saves/pvpkd.json");
            try
            {
                Database.pvpkd = JsonSerializer.Deserialize<Dictionary<ulong, double>>(json);
                Plugin.Logger.LogWarning("PvP K/D List Populated.");
            }
            catch
            {
                Database.pvpkd = new Dictionary<ulong, double>();
                Plugin.Logger.LogWarning("PvP K/D List Created.");
            }
        }
    }
}
