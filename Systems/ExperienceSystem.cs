using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Wetstone.API;
using RPGMods.Utils;

namespace RPGMods.Systems
{
    public class ExperienceSystem
    {
        private static EntityManager entityManager = VWorld.Server.EntityManager;

        public static bool isEXPActive = true;
        public static float EXPMultiplier = 1;
        public static float VBloodMultiplier = 15;
        public static float EXPConstant = 0.1f;
        public static int EXPPower = 2;
        public static int MaxLevel = 80;
        public static double GroupModifier = 0.75;
        public static float GroupMaxDistance = 50;

        public static double EXPLostOnDeath = 0.10;

        private static PrefabGUID vBloodType = new PrefabGUID(1557174542);

        public static void UpdateEXP(Entity killerEntity, Entity victimEntity)
        {
            bool isVictimNPC = entityManager.HasComponent<UnitLevel>(victimEntity);
            if (isVictimNPC)
            {
                if (entityManager.HasComponent<Minion>(victimEntity)) return;

                PlayerCharacter player = entityManager.GetComponentData<PlayerCharacter>(killerEntity);
                Entity userEntity = player.UserEntity._Entity;
                User user = entityManager.GetComponentData<User>(userEntity);
                ulong SteamID = user.PlatformId;

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

                Database.player_experience.TryGetValue(SteamID, out int exp);
                int level_diff = UnitLevel.Level - convertXpToLevel(exp);

                if (level_diff > 0) EXPGained = (int)(EXPGained * (1 + level_diff * 0.1) * EXPMultiplier);
                else if (level_diff <= -10) EXPGained = (int)(EXPGained * 0.5 * EXPMultiplier);
                else EXPGained = (int)(EXPGained * EXPMultiplier);

                bool HasAllies = GetAllies(killerEntity, out var Group);
                if (HasAllies)
                {
                    for (int i = 0; i < Group.Count; i++)
                    {
                        EXPGained = (int)(EXPGained * GroupModifier);
                    }
                    foreach (var teammate in Group)
                    {
                        ShareEXP(teammate.Key, EXPGained);
                    }
                }

                if (exp <= 0) Database.player_experience[SteamID] = EXPGained;
                else Database.player_experience[SteamID] = exp + EXPGained;

                SetLevel(killerEntity, userEntity, SteamID);
                bool isDatabaseEXPLog = Database.player_log_exp.TryGetValue(SteamID, out bool isLogging);
                if (isDatabaseEXPLog)
                {
                    if (!isLogging) return;
                    Output.SendLore(userEntity, $"<color=#ffdd00ff>You gain {EXPGained} experience points by slaying a Lv.{UnitLevel.Level} enemy.</color>");
                    //user.SendSystemMessage($"<color=#ffffffff>Total EXP: {player_experience[SteamID]} - Level: {level} ({getLevelProgress(SteamID)}%)</color>");
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

        public static bool GetAllies(Entity PlayerCharacter, out Dictionary<Entity, float> Group)
        {
            var sgm = VWorld.Server.GetExistingSystem<ServerScriptMapper>()?._ServerGameManager;
            Team team = entityManager.GetComponentData<Team>(PlayerCharacter);
            Group = new Dictionary<Entity, float>();
            if (sgm._TeamChecker.GetAlliedUsersCount(team) <= 1) return false;

            LocalToWorld playerPos = entityManager.GetComponentData<LocalToWorld>(PlayerCharacter);
            NativeList<Entity> allyBuffer = sgm._TeamChecker.GetTeamsChecked();
            sgm._TeamChecker.GetAlliedUsers(team, allyBuffer);
            int i = 0;
            try
            {
                foreach (var entity in allyBuffer)
                {
                    if (entityManager.HasComponent<User>(entity))
                    {
                        Entity allyEntity = entityManager.GetComponentData<User>(entity).LocalCharacter._Entity;
                        if (allyEntity.Equals(PlayerCharacter)) continue;
                        LocalToWorld allyPos = entityManager.GetComponentData<LocalToWorld>(allyEntity);
                        var Distance = math.distance(playerPos.Position.xz, allyPos.Position.xz);
                        if (Distance <= GroupMaxDistance)
                        {
                            Group[entity] = Distance;
                            i++;
                        }
                    }
                }
            }
            catch { };
            if (i == 0) return false;
            else return true;
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
            Output.SendLore(userEntity, $"You've died,<color=#ffffffff> {EXPLostOnDeath * 100}%</color> experience is lost.");
        }

        public static void BuffReceiver(Entity buffEntity)
        {
            PrefabGUID GUID = entityManager.GetComponentData<PrefabGUID>(buffEntity);
            if (GUID.Equals(Database.buff.LevelUp_Buff)) {
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
            if (!Database.player_experience.TryGetValue(SteamID, out int exp)) Database.player_experience[SteamID] = 0;
            float level = convertXpToLevel(Database.player_experience[SteamID]);
            if (level > MaxLevel) level = MaxLevel;

            bool isLastLevel = Cache.player_level.TryGetValue(SteamID, out var level_);
            if (isLastLevel)
            {
                if (level_ < level) 
                {
                    Cache.player_level[SteamID] = level;
                    Helper.ApplyBuff(user, entity, Database.buff.LevelUp_Buff);
                    bool isDatabaseEXPLog = Database.player_log_exp.TryGetValue(SteamID, out bool isLogging);
                    if (isDatabaseEXPLog)
                    {
                        if (!isLogging) return;
                        Output.SendLore(user, $"<color=#ffdd00ff>Level up! You're now level</color><color=#ffffffff> {level}</color><color=#ffdd00ff>!</color>");
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