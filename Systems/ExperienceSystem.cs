using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using RPGMods.Utils;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using UnityEngine;
using Cache = RPGMods.Utils.Cache;

namespace RPGMods.Systems
{
    public class ExperienceSystem
    {
        private static EntityManager entityManager = Plugin.Server.EntityManager;

        public static bool isEXPActive = true;
        public static float EXPMultiplier = 1;
        public static float VBloodMultiplier = 15;
        public static float EXPConstant = 0.1f;
        public static int EXPPower = 2;
        public static int MaxLevel = 80;
        public static double GroupModifier = 0.75;
        public static float GroupMaxDistance = 50;

        public static double EXPLostOnDeath = 0.10;

        private static readonly PrefabGUID vBloodType = new PrefabGUID(1557174542);
        private static readonly int MaxCacheAge = 300;

        public static void EXPMonitor(Entity killerEntity, Entity victimEntity)
        {
            //-- Check victim is not a summon
            if (entityManager.HasComponent<Minion>(victimEntity)) return;

            //-- Check victim has a level
            if (!entityManager.HasComponent<UnitLevel>(victimEntity)) return;

            //-- Must be executed from main thread
            if (Cache.PlayerAllies.TryGetValue(killerEntity, out var PlayerGroup))
            {
                TimeSpan CacheAge = DateTime.Now - PlayerGroup.TimeStamp;
                if (CacheAge.TotalSeconds > MaxCacheAge) goto UpdateCache;
                goto StartTask;
            }

            UpdateCache:
            int allyCount = Helper.GetAllies(killerEntity, out var Group);
            PlayerGroup = new PlayerGroup()
            {
                AllyCount = allyCount,
                Allies = Group,
                TimeStamp = DateTime.Now
            };
            Cache.PlayerAllies[killerEntity] = PlayerGroup;
            //-- ---------------------------------

            StartTask:
            UpdateEXP(killerEntity, victimEntity, PlayerGroup);
        }

        public static void UpdateEXP(Entity killerEntity, Entity victimEntity, PlayerGroup PlayerGroup)
        {
            PlayerCharacter player = entityManager.GetComponentData<PlayerCharacter>(killerEntity);
            Entity userEntity = player.UserEntity._Entity;
            User user = entityManager.GetComponentData<User>(userEntity);
            ulong SteamID = user.PlatformId;

            int player_level = 0;
            if (Database.player_experience.TryGetValue(SteamID, out int exp))
            {
                player_level = convertXpToLevel(exp);
                if (exp >= convertLevelToXp(MaxLevel)) return;
            }

            UnitLevel UnitLevel = entityManager.GetComponentData<UnitLevel>(victimEntity);

            bool isVBlood;
            if (entityManager.HasComponent<BloodConsumeSource>(victimEntity))
            {
                BloodConsumeSource BloodSource = entityManager.GetComponentData<BloodConsumeSource>(victimEntity);
                isVBlood = BloodSource.UnitBloodType.Equals(vBloodType);
            }
            else
            {
                isVBlood = false;
            }

            int EXPGained;
            if (isVBlood) EXPGained = (int)(UnitLevel.Level * VBloodMultiplier);
            else EXPGained = UnitLevel.Level;

            int level_diff = UnitLevel.Level - player_level;
            if (level_diff > 10) level_diff = 10;

            if (level_diff > 0) EXPGained = (int)(EXPGained * (1 + level_diff * 0.1) * EXPMultiplier);
            else if (level_diff <= -20) EXPGained = (int) Math.Ceiling(EXPGained * 0.10 * EXPMultiplier);
            else if (level_diff <= -15) EXPGained = (int) Math.Ceiling(EXPGained * 0.25 * EXPMultiplier);
            else if (level_diff <= -10) EXPGained = (int) Math.Ceiling(EXPGained * 0.50 * EXPMultiplier);
            else if (level_diff <= -5) EXPGained = (int) Math.Ceiling(EXPGained * 0.75 * EXPMultiplier);
            else EXPGained = (int)(EXPGained * EXPMultiplier);

            if (PlayerGroup.AllyCount > 0)
            {
                List<Entity> CloseAllies = new();
                LocalToWorld playerPos = Cache.PlayerLocations[killerEntity];
                foreach (var ally in PlayerGroup.Allies)
                {
                    LocalToWorld allyPos = Cache.PlayerLocations[ally.Value];
                    var Distance = math.distance(playerPos.Position.xz, allyPos.Position.xz);
                    if (Distance <= GroupMaxDistance)
                    {
                        EXPGained = (int)(EXPGained * GroupModifier);
                        CloseAllies.Add(ally.Key);
                    }
                }
                
                foreach (var teammate in CloseAllies)
                {
                    ShareEXP(teammate, EXPGained);
                }
            }

            if (exp <= 0) Database.player_experience[SteamID] = EXPGained;
            else Database.player_experience[SteamID] = exp + EXPGained;

            SetLevel(killerEntity, userEntity, SteamID);
            if (Database.player_log_exp.TryGetValue(SteamID, out bool isLogging))
            {
                if (isLogging)
                {
                    Output.SendLore(userEntity, $"<color=#ffdd00ff>You gain {EXPGained} experience points by slaying a Lv.{UnitLevel.Level} enemy.</color>");
                }
            }
        }

        public static void ShareEXP(Entity user, int EXPGain)
        {
            var user_component = entityManager.GetComponentData<User>(user);
            if (EXPGain > 0)
            {
                Database.player_experience.TryGetValue(user_component.PlatformId, out var exp);
                Database.player_experience[user_component.PlatformId] = exp + EXPGain;
                SetLevel(user_component.LocalCharacter._Entity, user, user_component.PlatformId);
            }
        }

        public static void LoseEXP(Entity playerEntity)
        {
            PlayerCharacter player = entityManager.GetComponentData<PlayerCharacter>(playerEntity);
            Entity userEntity = player.UserEntity._Entity;
            User user = entityManager.GetComponentData<User>(userEntity);
            ulong SteamID = user.PlatformId;

            int EXPLost;
            Database.player_experience.TryGetValue(SteamID, out int exp);
            if (exp <= 0) EXPLost = 0;
            else
            {
                int variableEXP = convertLevelToXp(convertXpToLevel(exp) + 1) - convertLevelToXp(convertXpToLevel(exp));
                EXPLost = (int)(variableEXP * EXPLostOnDeath);
            }

            Database.player_experience[SteamID] = exp - EXPLost;

            SetLevel(playerEntity, userEntity, SteamID);
            Output.SendLore(userEntity, $"You've been defeated,<color=#ffffffff> {EXPLostOnDeath * 100}%</color> experience is lost.");
        }

        public static void BuffReceiver(Entity buffEntity)
        {
            PrefabGUID GUID = entityManager.GetComponentData<PrefabGUID>(buffEntity);
            if (GUID.Equals(Database.Buff.LevelUp_Buff)) {
                Entity Owner = entityManager.GetComponentData<EntityOwner>(buffEntity).Owner;
                if (entityManager.HasComponent<PlayerCharacter>(Owner))
                {
                    LifeTime lifetime = entityManager.GetComponentData<LifeTime>(buffEntity);
                    lifetime.Duration = 0.0001f;
                    entityManager.SetComponentData(buffEntity, lifetime);
                }
            }
        }

        public static void SetLevel(Entity entity, Entity user, ulong SteamID)
        {
            if (!Database.player_experience.ContainsKey(SteamID)) Database.player_experience[SteamID] = 0;
            float level = convertXpToLevel(Database.player_experience[SteamID]);
            if (level < 0) return;
            if (level > MaxLevel)
            {
                level = MaxLevel;
                Database.player_experience[SteamID] = convertLevelToXp(MaxLevel);
            }

            bool isLastLevel = Cache.player_level.TryGetValue(SteamID, out var level_);
            if (isLastLevel)
            {
                if (level_ < level) 
                {
                    Cache.player_level[SteamID] = level;
                    Helper.ApplyBuff(user, entity, Database.Buff.LevelUp_Buff);
                    if (Database.player_log_exp.TryGetValue(SteamID, out bool isLogging))
                    {
                        if (isLogging) 
                        {
                            var userData = entityManager.GetComponentData<User>(user);
                            Output.SendLore(user, $"<color=#ffdd00ff>Level up! You're now level</color><color=#ffffffff> {level}</color><color=#ffdd00ff>!</color>");
                        }
                    }
                    
                }
            }
            else
            {
                Cache.player_level[SteamID] = level;
            }
            Equipment eq_comp = entityManager.GetComponentData<Equipment>(entity);
            level = level - eq_comp.WeaponLevel._Value - eq_comp.ArmorLevel._Value;
            eq_comp.SpellLevel._Value = level;

            entityManager.SetComponentData(entity, eq_comp);
        }

        public static int convertXpToLevel(int xp)
        {
            // Level = 0.05 * sqrt(xp)
            return (int)Math.Floor(EXPConstant * Math.Sqrt(xp));
        }

        public static int convertLevelToXp(int level)
        {
            // XP = (Level / 0.05) ^ 2
            return (int)Math.Pow(level / EXPConstant, EXPPower);
        }

        public static int getXp(ulong SteamID)
        {
            if (Database.player_experience.TryGetValue(SteamID, out int exp)) return exp;
            return 0;
        }

        public static int getLevel(ulong SteamID)
        {
            return convertXpToLevel(getXp(SteamID));
        }

        public static int getLevelProgress(ulong SteamID)
        {
            int currentXP = getXp(SteamID);
            int currentLevelXP = convertLevelToXp(getLevel(SteamID));
            int nextLevelXP = convertLevelToXp(getLevel(SteamID) + 1);

            double neededXP = nextLevelXP - currentLevelXP;
            double earnedXP = nextLevelXP - currentXP;

            return 100 - (int)Math.Ceiling(earnedXP / neededXP * 100);
        }

        public static void SaveEXPData()
        {
            File.WriteAllText("BepInEx/config/RPGMods/Saves/player_experience.json", JsonSerializer.Serialize(Database.player_experience, Database.JSON_options));
            File.WriteAllText("BepInEx/config/RPGMods/Saves/player_log_exp.json", JsonSerializer.Serialize(Database.player_log_exp, Database.JSON_options));
        }

        public static void LoadEXPData()
        {
            if (!File.Exists("BepInEx/config/RPGMods/Saves/player_experience.json"))
            {
                FileStream stream = File.Create("BepInEx/config/RPGMods/Saves/player_experience.json");
                stream.Dispose();
            }
            string json = File.ReadAllText("BepInEx/config/RPGMods/Saves/player_experience.json");
            try
            {
                Database.player_experience = JsonSerializer.Deserialize<Dictionary<ulong, int>>(json);
                Plugin.Logger.LogWarning("PlayerEXP DB Populated.");
            }
            catch
            {
                Database.player_experience = new Dictionary<ulong, int>();
                Plugin.Logger.LogWarning("PlayerEXP DB Created.");
            }

            if (!File.Exists("BepInEx/config/RPGMods/Saves/player_log_exp.json"))
            {
                FileStream stream = File.Create("BepInEx/config/RPGMods/Saves/player_log_exp.json");
                stream.Dispose();
            }
            json = File.ReadAllText("BepInEx/config/RPGMods/Saves/player_log_exp.json");
            try
            {
                Database.player_log_exp = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                Plugin.Logger.LogWarning("PlayerEXP_Log_Switch DB Populated.");
            }
            catch
            {
                Database.player_log_exp = new Dictionary<ulong, bool>();
                Plugin.Logger.LogWarning("PlayerEXP_Log_Switch DB Created.");
            }
        }
    }
}