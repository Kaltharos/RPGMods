using ProjectM;
using ProjectM.Network;
using RPGMods.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

namespace RPGMods.Systems
{
    public class PvPSystem
    {
        public static bool isPvPToggleEnabled = true;

        public static bool isAnnounceKills = true;
        public static bool isLadderEnabled = true;
        public static int LadderLength = 10;

        //-- Punish System
        public static bool isPunishEnabled = true;
        public static bool isSortByHonor = true;
        public static int PunishLevelDiff = -10;
        public static float PunishDuration = 1800f;
        public static int OffenseLimit = 3;
        public static float Offense_Cooldown = 300f;

        //-- Honor System
        //-- The Only Potential Buff we can use for hostile mark
        //Buff_Cultist_BloodFrenzy_Buff - PrefabGuid(-106492795)
        public static PrefabGUID HostileBuff = new PrefabGUID(-106492795);
        public static bool isHonorSystemEnabled = true;
        public static bool isHonorTitleEnabled = true;
        public static int HonorGainSpanLimit = 60;
        public static int MaxHonorGainPerSpan = 250;
        public static bool isHonorBenefitEnabled = true;

        public static bool isEnableHostileGlow = true;
        public static bool isUseProximityGlow = true;
        #region HostileBuff Modification
        public static readonly LifeTime lifeTime_Permanent = new LifeTime()
        {
            Duration = 0,
            EndAction = LifeTimeEndAction.None
        };
        public static readonly ModifyMovementSpeedBuff MS_Zero = new ModifyMovementSpeedBuff()
        {
            MoveSpeed = 1,
            MultiplyAdd = false,
            Curve = default
        };
        #endregion

        //-- Custom Siege System
        //-- -- Global Siege
        public static class Interlocked
        {
            public static bool isSiegeOn = true;
        }
        public static DateTime SiegeStart = DateTime.Now;
        public static DateTime SiegeEnd = DateTime.Now;
        //-- -- Player Siege
        public static int SiegeDuration = 180;

        public static EntityManager em = Plugin.Server.EntityManager;

        #region PvP Punishment DOTS
        private static ModifyUnitStatBuff_DOTS PResist = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.PhysicalResistance,
            Value = -15,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        private static ModifyUnitStatBuff_DOTS FResist = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.FireResistance,
            Value = -15,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        private static ModifyUnitStatBuff_DOTS HResist = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.HolyResistance,
            Value = -15,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        private static ModifyUnitStatBuff_DOTS SPResist = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.SpellResistance,
            Value = -15,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        private static ModifyUnitStatBuff_DOTS SunResist = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.SunResistance,
            Value = -15,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        private static ModifyUnitStatBuff_DOTS PPower = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.PhysicalPower,
            Value = 0.75f,
            ModificationType = ModificationType.Multiply,
            Id = ModificationId.NewId(0)
        };

        private static ModifyUnitStatBuff_DOTS SPPower = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.SpellPower,
            Value = 0.75f,
            ModificationType = ModificationType.Multiply,
            Id = ModificationId.NewId(0)
        };
        #endregion

        public static void MobKillMonitor(Entity KillerEntity, Entity VictimEntity)
        {
            if (em.HasComponent<Minion>(VictimEntity)) return;

            var killer = em.GetComponentData<PlayerCharacter>(KillerEntity);
            var killer_userEntity = killer.UserEntity._Entity;
            var killer_user = em.GetComponentData<User>(killer_userEntity);
            var killer_name = killer_user.CharacterName.ToString();
            var killer_id = killer_user.PlatformId;

            Database.PvPStats.TryGetValue(killer_id, out var KillerStats);

            KillerStats.PlayerName = killer_name;

            bool renamePlayer = false;

            if (KillerStats.Reputation < 10000)
            {
                Cache.ReputationLog.TryGetValue(killer_id, out var RepLog);

                TimeSpan ReputationSpan = DateTime.Now - RepLog.TimeStamp;
                if (ReputationSpan.TotalMinutes > HonorGainSpanLimit)
                {
                    RepLog.TimeStamp = DateTime.Now;
                    RepLog.TotalGained = 0;
                }

                if (RepLog.TotalGained < MaxHonorGainPerSpan)
                {
                    if (KillerStats.Reputation + 1 > 10000) KillerStats.Reputation = 10000;
                    else KillerStats.Reputation += 1;

                    RepLog.TotalGained += 1;

                    var KillerHonorInfo = GetHonorTitle(KillerStats.Reputation);

                    if (KillerStats.Title != KillerHonorInfo.Title)
                    {
                        if (KillerStats.Reputation <= -1000) PvPSystem.HostileON(killer_id, KillerEntity, killer_userEntity);
                        KillerStats.Title = KillerHonorInfo.Title;
                        if (isHonorTitleEnabled)
                        {
                            var true_name = Helper.GetTrueName(killer_name);
                            killer_name = "[" + KillerHonorInfo.Title + "]" + true_name;
                            KillerStats.PlayerName = killer_name;
                            renamePlayer = true;
                        }
                    }
                }

                Cache.ReputationLog[killer_id] = RepLog;
            }

            Database.PvPStats[killer_id] = KillerStats;
            if (renamePlayer)
            {
                Helper.RenamePlayer(killer_userEntity, KillerEntity, killer_name);
            }
        }

        public static void Monitor(Entity KillerEntity, Entity VictimEntity)
        {
            var killer = em.GetComponentData<PlayerCharacter>(KillerEntity);
            var killer_userEntity = killer.UserEntity._Entity;
            var killer_user = em.GetComponentData<User>(killer_userEntity);
            var killer_name = killer_user.CharacterName.ToString();
            var killer_id = killer_user.PlatformId;

            var victim = em.GetComponentData<PlayerCharacter>(VictimEntity);
            var victim_userEntity = victim.UserEntity._Entity;
            var victim_user = em.GetComponentData<User>(victim_userEntity);
            var victim_name = victim_user.CharacterName.ToString();
            var victim_id = victim_user.PlatformId;

            Database.PvPStats.TryGetValue(killer_id, out var KillerStats);
            Database.PvPStats.TryGetValue(victim_id, out var VictimStats);

            KillerStats.PlayerName = killer_name;
            KillerStats.Kills += 1;

            VictimStats.PlayerName = victim_name;
            VictimStats.Deaths += 1;

            if (KillerStats.Deaths != 0) KillerStats.KD = Math.Round((double)KillerStats.Kills / KillerStats.Deaths, 2);
            else KillerStats.KD = KillerStats.Kills;

            if (VictimStats.Kills != 0) VictimStats.KD = Math.Round((double)VictimStats.Kills / VictimStats.Deaths, 2);
            else VictimStats.KD = 0;

            bool renamePlayer = false;
            if (isHonorSystemEnabled)
            {
                Cache.ReputationLog.TryGetValue(killer_id, out var RepLog);
                var VictimHonorInfo = GetHonorTitle(VictimStats.Reputation);

                TimeSpan ReputationSpan = DateTime.Now - RepLog.TimeStamp;
                if (ReputationSpan.TotalMinutes > HonorGainSpanLimit)
                {
                    RepLog.TimeStamp = DateTime.Now;
                    RepLog.TotalGained = 0;
                }

                if (RepLog.TotalGained < MaxHonorGainPerSpan)
                {
                    if (VictimHonorInfo.Rewards < 0)
                    {
                        Cache.HostilityState.TryGetValue(VictimEntity, out var state);
                        if (state.IsHostile) VictimHonorInfo.Rewards = 0;
                    }

                    if (VictimHonorInfo.Rewards > 0) RepLog.TotalGained += VictimHonorInfo.Rewards;

                    if (KillerStats.Reputation + VictimHonorInfo.Rewards > 10000) KillerStats.Reputation = 10000;
                    else KillerStats.Reputation += VictimHonorInfo.Rewards;

                    var KillerHonorInfo = GetHonorTitle(KillerStats.Reputation);

                    if (KillerStats.Title != KillerHonorInfo.Title)
                    {
                        if (KillerStats.Reputation <= -1000) PvPSystem.HostileON(killer_id, KillerEntity, killer_userEntity);
                        if (KillerStats.Reputation <= -20000) PvPSystem.SiegeON(killer_id, KillerEntity, killer_userEntity, true, true);

                        KillerStats.Title = KillerHonorInfo.Title;

                        if (isHonorTitleEnabled)
                        {
                            var true_name = Helper.GetTrueName(killer_name);
                            killer_name = "[" + KillerHonorInfo.Title + "]" + true_name;
                            KillerStats.PlayerName = killer_name;
                            renamePlayer = true;
                        }
                    }
                }

                Cache.ReputationLog[killer_id] = RepLog;
            }

            Database.PvPStats[killer_id] = KillerStats;
            Database.PvPStats[victim_id] = VictimStats;

            if (renamePlayer)
            {
                Helper.RenamePlayer(killer_userEntity, KillerEntity, killer_name);
            }

            ServerChatUtils.SendSystemMessageToClient(em, victim_user, Utils.Color.Red($"You've been defeated by \"{killer_name}\""));
            if (isAnnounceKills) ServerChatUtils.SendSystemMessageToAllClients(em, $"Vampire {Utils.Color.Red(killer_name)} has defeated {Utils.Color.Green(victim_name)}!");
        }

        public static bool HostileON(ulong SteamID, Entity playerEntity, Entity userEntity)
        {
            StateData stateData = new StateData(SteamID, true);
            Cache.HostilityState[playerEntity] = stateData;
            if (isEnableHostileGlow && !isUseProximityGlow) Helper.ApplyBuff(userEntity, playerEntity, PvPSystem.HostileBuff);
            return true;
        }

        public static bool HostileOFF(ulong SteamID, Entity playerEntity)
        {
            StateData stateData = new StateData(SteamID, false);
            Cache.HostilityState[playerEntity] = stateData;
            Helper.RemoveBuff(playerEntity, PvPSystem.HostileBuff);
            return true;
        }

        public static void SiegeON(ulong SteamID, Entity playerEntity, Entity userEntity, bool forceSiege = false, bool seekAlly = true)
        {
            if (seekAlly)
            {
                if (Helper.GetAllies(playerEntity, out var playerGroup) > 0)
                {
                    playerGroup.Allies.Add(userEntity, playerEntity);
                    if (forceSiege == false)
                    {
                        foreach (var ally in playerGroup.Allies)
                        {
                            Cache.HostilityState.TryGetValue(ally.Value, out var hostilityState);
                            Database.PvPStats.TryGetValue(hostilityState.SteamID, out var allyPvPStats);
                            if (allyPvPStats.Reputation <= -20000)
                            {
                                forceSiege = true;
                                break;
                            }
                        }
                    }
                    foreach (var ally in playerGroup.Allies)
                    {
                        Cache.HostilityState.TryGetValue(ally.Value, out var hostilityState);
                        SiegeONProc(hostilityState.SteamID, ally.Value, ally.Key, forceSiege);
                    }
                }
            }
            else
            {
                SiegeONProc(SteamID, playerEntity, userEntity, forceSiege);
            }
        }

        private static void SiegeONProc(ulong SteamID, Entity playerEntity, Entity userEntity, bool forceSiege)
        {
            PvPSystem.HostileON(SteamID, playerEntity, userEntity);
            Database.PvPStats.TryGetValue(SteamID, out var pvpStats);
            Database.SiegeState.TryGetValue(SteamID, out var siegeData);
            if (pvpStats.Reputation > -20000 && forceSiege == false)
            {
                if (siegeData.IsSiegeOn == false)
                {
                    Cache.SteamPlayerCache.TryGetValue(SteamID, out var playerData);
                    ServerChatUtils.SendSystemMessageToAllClients(em, $"{Utils.Color.Red(playerData.CharacterName.ToString())} has entered {Color.Red("Active Siege")}!");

                    siegeData.IsSiegeOn = true;
                    siegeData.SiegeEndTime = DateTime.Now.AddMinutes(PvPSystem.SiegeDuration);
                    siegeData.SiegeStartTime = DateTime.Now;
                    Database.SiegeState[SteamID] = siegeData;
                }
                TimeSpan span = siegeData.SiegeEndTime - DateTime.Now;
                TaskRunner.Start(taskWorld =>
                {
                    PvPSystem.SiegeOFF(SteamID, playerEntity);
                    return new object();
                }, false, false, false, span);
            }
            else
            {
                if (siegeData.IsSiegeOn == false)
                {
                    Cache.SteamPlayerCache.TryGetValue(SteamID, out var playerData);
                    ServerChatUtils.SendSystemMessageToAllClients(em, $"{Utils.Color.Red(playerData.CharacterName.ToString())} has entered {Color.Red("Active Siege")}!");
                }
                siegeData.IsSiegeOn = true;
                siegeData.SiegeEndTime = DateTime.MinValue;
                siegeData.SiegeStartTime = DateTime.Now;
                Database.SiegeState[SteamID] = siegeData;
            }
        }

        public static bool SiegeOFF(ulong SteamID, Entity playerEntity)
        {
            Database.PvPStats.TryGetValue(SteamID, out var pvpStats);
            if (pvpStats.Reputation <= -20000)
            {
                return false;
            }

            if (pvpStats.Reputation > -1000)
            {
                if (!Helper.IsPlayerInCombat(playerEntity))
                {
                    PvPSystem.HostileOFF(SteamID, playerEntity);
                }
            }
            Database.SiegeState[SteamID] = new SiegeData(false, default, default);
            return true;
        }

        public static async Task SiegeList(Context ctx)
        {
            await Task.Yield();

            List<string> messages = new List<string>();

            IEnumerable<KeyValuePair<ulong, SiegeData>> SortedList;

            SortedList = Database.SiegeState.Where(x => x.Value.IsSiegeOn == true).OrderByDescending(x => x.Value.SiegeEndTime - DateTime.Now);

            int page = 0;
            if (ctx.Args.Length >= 1 && int.TryParse(ctx.Args[0], out page))
            {
                page -= 1;
            }

            var recordsPerPage = 5;

            var maxPage = (int)Math.Ceiling(Database.SiegeState.Count / (double)recordsPerPage);
            page = Math.Min(maxPage - 1, page);

            var List = SortedList.Skip(page * recordsPerPage).Take(recordsPerPage);
            int order = (page * recordsPerPage);
            messages.Add($"============ Siege List [{page+1}/{maxPage}] ============");
            if (List.Count() == 0) messages.Add(Utils.Color.White("No Result"));
            else
            {
                foreach (var result in List)
                {
                    order++;
                    string PlayerName = Utils.Color.Teal(Cache.SteamPlayerCache[result.Key].CharacterName.ToString());
                    TimeSpan span = result.Value.SiegeEndTime - DateTime.Now;
                    var hSpan = Math.Round(span.TotalHours, 2);
                    string tempDisplay = "[Duration " + hSpan + " hour(s)]";
                    string DisplayStats = Utils.Color.White(tempDisplay);
                    messages.Add($"{order}. {PlayerName} : {DisplayStats}");
                }
            }
            messages.Add($"============ Siege List [{page+1}/{maxPage}] ============");

            TaskRunner.Start(taskWorld =>
            {
                foreach (var m in messages)
                {
                    Output.SendSystemMessage(ctx, m);
                }
                return new object();
            }, false);
        }

        public static void OnEquipChange(Entity player)
        {
            var PlayerLevel = em.GetComponentData<Equipment>(player).GetFullLevel();
            Cache.PlayerLevelCache.TryGetValue(player, out var levelData);

            if (PlayerLevel > levelData.Level)
            {
                levelData.Level = PlayerLevel;
                levelData.TimeStamp = DateTime.Now;
                Cache.PlayerLevelCache[player] = levelData;
            }
        }

        public static void OnCombatEngaged(Entity buffEntity, Entity player)
        {
            PrefabGUID GUID = em.GetComponentData<PrefabGUID>(buffEntity);
            if (GUID.Equals(Database.Buff.InCombat_PvP))
            {
                Cache.PlayerLevelCache.TryGetValue(player, out var levelData);
                if (DateTime.Now.Subtract(levelData.TimeStamp).TotalSeconds > 60)
                {
                    levelData.Level = em.GetComponentData<Equipment>(player).GetFullLevel();
                }
                levelData.TimeStamp = DateTime.Now;
                Cache.PlayerLevelCache[player] = levelData;
            }
        }

        public static void PunishCheck(Entity Killer, Entity Victim)
        {
            Entity KillerUser = em.GetComponentData<PlayerCharacter>(Killer).UserEntity._Entity;
            ulong KillerSteamID = em.GetComponentData<User>(KillerUser).PlatformId;

            float KillerLevel;
            if (Cache.PlayerLevelCache.TryGetValue(Killer, out var killerData)) KillerLevel = killerData.Level;
            else KillerLevel = em.GetComponentData<Equipment>(Killer).GetFullLevel();

            float VictimLevel;
            if (Cache.PlayerLevelCache.TryGetValue(Victim, out var victimData)) VictimLevel = victimData.Level;
            else VictimLevel = em.GetComponentData<Equipment>(Victim).GetFullLevel();

            if (VictimLevel - KillerLevel <= PunishLevelDiff)
            {
                Cache.OffenseLog.TryGetValue(KillerSteamID, out var OffenseData);

                TimeSpan timeSpan = DateTime.Now - OffenseData.LastOffense;
                if (timeSpan.TotalSeconds > Offense_Cooldown) OffenseData.Offense = 1;
                else OffenseData.Offense += 1;
                OffenseData.LastOffense = DateTime.Now;

                Cache.OffenseLog[KillerSteamID] = OffenseData;

                if (OffenseData.Offense >= OffenseLimit)
                {
                    Helper.ApplyBuff(KillerUser, Killer, Database.Buff.Severe_GarlicDebuff);
                }
            }
        }

        public static void BuffReceiver(Entity BuffEntity, PrefabGUID GUID)
        {
            if (GUID.Equals(Database.Buff.Severe_GarlicDebuff))
            {
                var lifeTime_component = em.GetComponentData<LifeTime>(BuffEntity);
                lifeTime_component.Duration = PvPSystem.PunishDuration;
                em.SetComponentData(BuffEntity, lifeTime_component);

                var Buffer = em.AddBuffer<ModifyUnitStatBuff_DOTS>(BuffEntity);
                Buffer.Add(PPower);
                Buffer.Add(SPPower);
                Buffer.Add(HResist);
                Buffer.Add(FResist);
                Buffer.Add(SPResist);
                Buffer.Add(PResist);
            }
        }

        public static void HonorBuffReceiver(Entity BuffEntity, PrefabGUID GUID)
        {
            if (isHonorBenefitEnabled == false) return;

            if (GUID.Equals(HostileBuff)) goto HostileBuff;
            if (!GUID.Equals(Database.Buff.OutofCombat) && !GUID.Equals(Database.Buff.InCombat) && !GUID.Equals(Database.Buff.InCombat_PvP)) return;

            var Owner = em.GetComponentData<EntityOwner>(BuffEntity).Owner;
            if (!em.HasComponent<PlayerCharacter>(Owner)) return;

            var userEntity = em.GetComponentData<PlayerCharacter>(Owner).UserEntity._Entity;
            var SteamID = em.GetComponentData<User>(userEntity).PlatformId;

            if (!Database.PvPStats.TryGetValue(SteamID, out var pvpData)) return;

            var Buffer = em.AddBuffer<ModifyUnitStatBuff_DOTS>(BuffEntity);
            if (pvpData.Reputation >= 1500)
            {
                Buffer.Add(new ModifyUnitStatBuff_DOTS()
                {
                    StatType = UnitStatType.ResourceYield,
                    Value = 1.15f,
                    ModificationType = ModificationType.Multiply,
                    Id = ModificationId.NewId(0)
                });
            }
            if (pvpData.Reputation >= 500)
            {
                Buffer.Add(new ModifyUnitStatBuff_DOTS()
                {
                    StatType = UnitStatType.ReducedResourceDurabilityLoss,
                    Value = 0.75f,
                    ModificationType = ModificationType.Multiply,
                    Id = ModificationId.NewId(0)
                });
            }
            return;

            HostileBuff:
            var HBOwner = em.GetComponentData<EntityOwner>(BuffEntity).Owner;
            if (!em.HasComponent<PlayerCharacter>(HBOwner)) return;
            if (Cache.HostilityState.TryGetValue(HBOwner, out var stateData))
            {
                if (stateData.IsHostile == false) return;

                em.AddComponent<Buff_Persists_Through_Death>(BuffEntity);
                em.SetComponentData(BuffEntity, lifeTime_Permanent);
                em.SetComponentData(BuffEntity, MS_Zero);
            }
            return;
        }

        public static void NewPlayerReceiver(Entity userEntity, Entity playerEntity, FixedString64 playerName)
        {
            if (isHonorTitleEnabled) Helper.RenamePlayer(userEntity, playerEntity, playerName);

            var steamID = Plugin.Server.EntityManager.GetComponentData<User>(userEntity).PlatformId;
            Cache.HostilityState[playerEntity] = new StateData(steamID, false);
        }

        public static HonorRankInfo GetHonorTitle(int honorPoint)
        {
            HonorRankInfo rankInfo = new HonorRankInfo();
            switch (honorPoint)
            {
                case >= 10000:
                    rankInfo.Title = "Glorious";
                    rankInfo.HonorRank = 10;
                    rankInfo.Rewards = -1000;
                    break;
                case >= 5000:
                    rankInfo.Title = "Noble";
                    rankInfo.HonorRank = 9;
                    rankInfo.Rewards = -500;
                    break;
                case >= 1500:
                    rankInfo.Title = "Virtuous";
                    rankInfo.HonorRank = 8;
                    rankInfo.Rewards = -150;
                    break;
                case >= 500:
                    rankInfo.Title = "Reputable";
                    rankInfo.HonorRank = 7;
                    rankInfo.Rewards = -50;
                    break;
                case >= 0:
                default:
                    rankInfo.Title = "Neutral";
                    rankInfo.HonorRank = 6;
                    rankInfo.Rewards = -25;
                    break;
                case <= -20000:
                    rankInfo.Title = "Dreaded";
                    rankInfo.HonorRank = 5;
                    rankInfo.Rewards = 10;
                    rankInfo.Rewards = 150;
                    break;
                case <= -10000:
                    rankInfo.Title = "Nefarious";
                    rankInfo.HonorRank = 4;
                    rankInfo.Rewards = 100;
                    break;
                case <= -3000:
                    rankInfo.Title = "Villainous";
                    rankInfo.HonorRank = 3;
                    rankInfo.Rewards = 50;
                    break;
                case <= -1000:
                    rankInfo.Title = "Infamous";
                    rankInfo.HonorRank = 2;
                    rankInfo.Rewards = 10;
                    break;
                case < 0:
                    rankInfo.Title = "Suspicious";
                    rankInfo.HonorRank = 1;
                    rankInfo.Rewards = 0;
                    break;
            }
            return rankInfo;
        }

        public static void UpdateAllNames()
        {
            if (Database.PvPStats.Count > 0)
            {
                var UserEntities = Plugin.Server.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<User>()).ToEntityArray(Allocator.Temp);
                foreach (var item in Database.PvPStats)
                {
                    if (item.Value.PlayerName == null)
                    {
                        PvPData data = item.Value;
                        foreach (var Entity in UserEntities)
                        {
                            var EntityData = Plugin.Server.EntityManager.GetComponentData<User>(Entity);
                            if (EntityData.PlatformId == item.Key) data.PlayerName = EntityData.CharacterName.ToString();
                        }
                        Database.PvPStats[item.Key] = data;
                    }
                }
            }
        }

        public static async Task TopRanks(Context ctx)
        {
            await Task.Yield();

            List<string> messages = new List<string>();

            IEnumerable<KeyValuePair<ulong, PvPData>> SortedList;

            if (isHonorSystemEnabled && isSortByHonor)
            {
                SortedList = Database.PvPStats.OrderByDescending(x => x.Value.Reputation).ThenByDescending(x => x.Value.KD).ThenBy(x => x.Value.Kills);
            }
            else
            {
                SortedList = Database.PvPStats.OrderByDescending(x => x.Value.KD).ThenByDescending(x => x.Value.Kills).ThenBy(x => x.Value.Deaths);
            }

            var List = SortedList.Take(LadderLength);
            int myRank = 0;
            foreach (var pair in SortedList)
            {
                myRank += 1;
                if (pair.Key == ctx.Event.User.PlatformId)
                {
                    messages.Add(Utils.Color.Green($"You're rank number #{myRank}!"));
                    break;
                }
            }

            messages.Add($"============ Leaderboard ============");
            if (List.Count() == 0) messages.Add(Utils.Color.White("No Result"));
            else
            {
                int i = 0;
                foreach (var result in List)
                {
                    i++;
                    string PlayerName = Utils.Color.Teal(result.Value.PlayerName);
                    string tempDisplay = "[K/D " + result.Value.KD.ToString() + "]";
                    if (isHonorSystemEnabled)
                    {
                        tempDisplay += " [REP " + result.Value.Reputation.ToString() + "]";
                    }
                    string DisplayStats = Utils.Color.White(tempDisplay);
                    messages.Add($"{i}. {PlayerName} : {DisplayStats}");
                }
            }
            messages.Add($"============ Leaderboard ============");

            TaskRunner.Start(taskWorld =>
            {
                foreach (var m in messages)
                {
                    Output.SendSystemMessage(ctx, m);
                }
                return new object();
            }, false);
        }

        public static void SavePvPStat()
        {
            //-- NEW
            File.WriteAllText("BepInEx/config/RPGMods/Saves/pvpstats.json", JsonSerializer.Serialize(Database.PvPStats, Database.JSON_options));
            File.WriteAllText("BepInEx/config/RPGMods/Saves/siegestates.json", JsonSerializer.Serialize(Database.SiegeState, Database.JSON_options));
        }

        public static void LoadPvPStat()
        {
            //-- NEW
            if (!File.Exists("BepInEx/config/RPGMods/Saves/pvpstats.json"))
            {
                var stream = File.Create("BepInEx/config/RPGMods/Saves/pvpstats.json");
                stream.Dispose();
            }
            string content = File.ReadAllText("BepInEx/config/RPGMods/Saves/pvpstats.json");
            try
            {
                Database.PvPStats = JsonSerializer.Deserialize<ConcurrentDictionary<ulong, PvPData>>(content);
                Plugin.Logger.LogWarning("PvPStats DB Populated.");
            }
            catch
            {
                Database.PvPStats = new ConcurrentDictionary<ulong, PvPData>();
                Plugin.Logger.LogWarning("PvPStats DB Created.");
            }

            //-- Siege Mechanic
            if (!File.Exists("BepInEx/config/RPGMods/Saves/siegestates.json"))
            {
                var stream = File.Create("BepInEx/config/RPGMods/Saves/siegestates.json");
                stream.Dispose();
            }
            content = File.ReadAllText("BepInEx/config/RPGMods/Saves/siegestates.json");
            try
            {
                Database.SiegeState = JsonSerializer.Deserialize<Dictionary<ulong, SiegeData>>(content);
                Plugin.Logger.LogWarning("SiegeStates DB Populated.");
            }
            catch
            {
                Database.SiegeState = new Dictionary<ulong, SiegeData>();
                Plugin.Logger.LogWarning("SiegeStates DB Created.");
            }

            //-- Transfer OLD Stats to new database.
            if (File.Exists("BepInEx/config/RPGMods/Saves/pvpkills.json"))
            {
                string json = File.ReadAllText("BepInEx/config/RPGMods/Saves/pvpkills.json");
                try
                {
                    Database.pvpkills = JsonSerializer.Deserialize<Dictionary<ulong, int>>(json);
                    foreach (var item in Database.pvpkills)
                    {
                        Database.PvPStats.TryGetValue(item.Key, out var data);
                        data.Kills = item.Value;
                        Database.PvPStats[item.Key] = data;
                    }
                    Plugin.Logger.LogWarning("PvPKills DB Transfered.");
                    File.Delete("BepInEx/config/RPGMods/Saves/pvpkills.json");
                }
                catch { }
            }

            if (File.Exists("BepInEx/config/RPGMods/Saves/pvpdeath.json"))
            {
                string json = File.ReadAllText("BepInEx/config/RPGMods/Saves/pvpdeath.json");
                try
                {
                    Database.pvpdeath = JsonSerializer.Deserialize<Dictionary<ulong, int>>(json);
                    foreach (var item in Database.pvpdeath)
                    {
                        Database.PvPStats.TryGetValue(item.Key, out var data);
                        data.Deaths = item.Value;
                        Database.PvPStats[item.Key] = data;
                    }
                    Plugin.Logger.LogWarning("PvPDeath DB Transfered.");
                    File.Delete("BepInEx/config/RPGMods/Saves/pvpdeath.json");
                }
                catch { }
            }
            

            if (File.Exists("BepInEx/config/RPGMods/Saves/pvpkd.json"))
            {
                string json = File.ReadAllText("BepInEx/config/RPGMods/Saves/pvpkd.json");
                try
                {
                    Database.pvpkd = JsonSerializer.Deserialize<Dictionary<ulong, double>>(json);
                    foreach (var item in Database.pvpkd)
                    {
                        Database.PvPStats.TryGetValue(item.Key, out var data);
                        data.KD = Math.Round(item.Value, 2);
                        Database.PvPStats[item.Key] = data;
                    }
                    Plugin.Logger.LogWarning("PvPKD DB Transfered.");
                    File.Delete("BepInEx/config/RPGMods/Saves/pvpkd.json");
                }
                catch { }
            }
        }
    }
}
