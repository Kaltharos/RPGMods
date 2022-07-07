using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Unity.Entities;
using Wetstone.API;
using RPGMods.Utils;

namespace RPGMods.Systems
{
    public class WeaponMasterSystem
    {
        public static EntityManager em = VWorld.Server.EntityManager;
        public static bool isMasteryEnabled = true;
        public static bool isDecaySystemEnabled = true;
        public static int MasteryCombatTick = 5;
        public static int MaxCombatTick = 12;
        public static float MasteryMultiplier = 1;
        public static int DecayInterval = 60;
        public static int Online_DecayValue = 0;
        public static int Offline_DecayValue = 1;
        public static int MaxMastery = 100000;
        public static float VBloodMultiplier = 15;

        private static PrefabGUID vBloodType = new PrefabGUID(1557174542);

        private static Random rand = new Random();

        public static void UpdateMastery(Entity Killer, Entity Victim)
        {
            if (Killer == Victim) return;
            if (em.HasComponent<Minion>(Victim)) return;

            Entity userEntity = em.GetComponentData<PlayerCharacter>(Killer).UserEntity._Entity;
            User User = em.GetComponentData<User>(userEntity);
            ulong SteamID = User.PlatformId;
            WeaponType WeaponType = GetWeaponType(Killer);

            int MasteryValue;
            var VictimStats = em.GetComponentData<UnitStats>(Victim);
            if (WeaponType == WeaponType.None) MasteryValue = (int)VictimStats.SpellPower;
            else MasteryValue = (int)VictimStats.PhysicalPower;

            MasteryValue = (int)(MasteryValue * (rand.Next(10, 100) * 0.01));

            bool isVBlood;
            if (em.HasComponent<BloodConsumeSource>(Victim))
            {
                BloodConsumeSource BloodSource = em.GetComponentData<BloodConsumeSource>(Victim);
                isVBlood = BloodSource.UnitBloodType.Equals(vBloodType);
            }
            else
            {
                isVBlood = false;
            }

            if (isVBlood) MasteryValue = (int)(MasteryValue * VBloodMultiplier);

            if (em.HasComponent<PlayerCharacter>(Victim))
            {
                Equipment VictimGear = em.GetComponentData<Equipment>(Victim);
                var BonusMastery = VictimGear.ArmorLevel + VictimGear.WeaponLevel + VictimGear.SpellLevel;
                MasteryValue *= (int)(1 + (BonusMastery * 0.01));
            }

            MasteryValue = (int)(MasteryValue * MasteryMultiplier);
            SetMastery(SteamID, WeaponType, MasteryValue);

            bool isDatabaseEXPLog = Database.player_log_mastery.TryGetValue(SteamID, out bool isLogging);
            if (isDatabaseEXPLog)
            {
                if (!isLogging) return;
                Output.SendLore(userEntity, $"<color=#ffb700ff>Weapon mastery has increased by {MasteryValue * 0.001}%</color>");
            }
        }

        public static void LoopMastery(Entity User, Entity Player)
        {
            User userData = em.GetComponentData<User>(User);
            ulong SteamID = userData.PlatformId;

            Cache.player_last_combat.TryGetValue(SteamID, out var LastCombat);
            TimeSpan elapsed_time = DateTime.Now - LastCombat;
            if (elapsed_time.TotalSeconds >= 10) Cache.player_combat_ticks[SteamID] = 0;
            if (elapsed_time.TotalSeconds * 0.2 < 1) return;

            Cache.player_last_combat[SteamID] = DateTime.Now;

            if (Cache.player_combat_ticks[SteamID] > MaxCombatTick) return;
            WeaponType WeaponType = GetWeaponType(Player);

            int MasteryValue = (int)(MasteryCombatTick * MasteryMultiplier);
            Cache.player_combat_ticks[SteamID] += 1;
            
            SetMastery(SteamID, WeaponType, MasteryValue);
        }

        public static void DecayMastery(Entity userEntity)
        {
            User Data = em.GetComponentData<User>(userEntity);
            var SteamID = Data.PlatformId;
            if (Database.player_decaymastery_logout.TryGetValue(SteamID, out var LastDecay)) {
                TimeSpan elapsed_time = DateTime.Now - LastDecay;
                if (elapsed_time.TotalSeconds < DecayInterval) return;

                int DecayTicks = (int)Math.Floor(elapsed_time.TotalSeconds / DecayInterval);
                if (DecayTicks > 0)
                {
                    int DecayValue = Offline_DecayValue * DecayTicks *-1;
                    
                    Output.SendLore(userEntity, $"You've been sleeping for {(int)elapsed_time.TotalMinutes} minute(s). Your mastery has decayed by {DecayValue*0.001}%");

                    foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
                    {
                        SetMastery(SteamID, type, DecayValue);
                    }
                }
            }
        }

        public static void BuffReceiver(DynamicBuffer<ModifyUnitStatBuff_DOTS> Buffer, Entity Owner, ulong SteamID)
        {
            var WeaponType = GetWeaponType(Owner);
            var isMastered = ConvertMastery(SteamID, WeaponType, out var PMastery, out var SMastery);
            if (isMastered)
            {
                switch (WeaponType)
                {
                    case WeaponType.Sword:
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.SpellPower,
                            Value = (float)(PMastery * 0.125),
                            ModificationType = ModificationType.Add,
                            Id = ModificationId.NewId(0)
                        });
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.PhysicalPower,
                            Value = (float)(PMastery * 0.125),
                            ModificationType = ModificationType.Add,
                            Id = ModificationId.NewId(0)
                        });
                        break;
                    case WeaponType.Spear:
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.PhysicalPower,
                            Value = (float)(PMastery * 0.25),
                            ModificationType = ModificationType.Add,
                            Id = ModificationId.NewId(0)
                        });
                        break;
                    case WeaponType.Axes:
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.PhysicalPower,
                            Value = (float)(PMastery * 0.125),
                            ModificationType = ModificationType.Add,
                            Id = ModificationId.NewId(0)
                        });
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.MaxHealth,
                            Value = (float)(PMastery * 0.5),
                            ModificationType = ModificationType.Add,
                            Id = ModificationId.NewId(0)
                        });
                        break;
                    case WeaponType.Scythe:
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.PhysicalPower,
                            Value = (float)(PMastery * 0.125),
                            ModificationType = ModificationType.Add,
                            Id = ModificationId.NewId(0)
                        });
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.PhysicalCriticalStrikeChance,
                            Value = (float)(PMastery * 0.00125),
                            ModificationType = ModificationType.Add,
                            Id = ModificationId.NewId(0)
                        });
                        break;
                    case WeaponType.Slashers:
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.PhysicalCriticalStrikeChance,
                            Value = (float)(PMastery * 0.00125),
                            ModificationType = ModificationType.Add,
                            Id = ModificationId.NewId(0)
                        });
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.MovementSpeed,
                            Value = (float)(PMastery * 0.005),
                            ModificationType = ModificationType.Add,
                            Id = ModificationId.NewId(0)
                        });
                        break;
                    case WeaponType.Mace:
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.MaxHealth,
                            Value = (float)(PMastery),
                            ModificationType = ModificationType.Add,
                            Id = ModificationId.NewId(0)
                        });
                        break;
                    case WeaponType.Crossbow:
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.PhysicalCriticalStrikeChance,
                            Value = (float)(PMastery * 0.0025),
                            ModificationType = ModificationType.Add,
                            Id = ModificationId.NewId(0)
                        });
                        break;
                    case WeaponType.None:
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.MovementSpeed,
                            Value = (float)(PMastery * 0.01),
                            ModificationType = ModificationType.Add,
                            Id = ModificationId.NewId(0)
                        });
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.PhysicalPower,
                            Value = (float)(PMastery * 0.25),
                            ModificationType = ModificationType.Add,
                            Id = ModificationId.NewId(0)
                        });
                        if (SMastery > 0)
                        {
                            Buffer.Add(new ModifyUnitStatBuff_DOTS()
                            {
                                StatType = UnitStatType.CooldownModifier,
                                Value = (float)(1 - SMastery * 0.01 * 0.5),
                                ModificationType = ModificationType.Set,
                                Id = ModificationId.NewId(0)
                            });
                        }
                        break;
                    default:
                        break;
                    //-- Nothing for Fishing Pole
                }
            }
        }

        public static bool ConvertMastery(ulong SteamID, WeaponType weaponType, out float MasteryValue, out float MasterySpellValue)
        {
            MasteryValue = 0;
            MasterySpellValue = 0;

            bool isFound = Database.player_weaponmastery.TryGetValue(SteamID, out WeaponMasterData Mastery);
            if (!isFound) return false;

            switch (weaponType)
            {
                case WeaponType.Sword:
                    MasteryValue = Mastery.Sword; break;
                case WeaponType.Spear:
                    MasteryValue = Mastery.Spear; break;
                case WeaponType.None:
                    MasteryValue = Mastery.None;
                    MasterySpellValue = Mastery.Spell; break;
                case WeaponType.Scythe:
                    MasteryValue = Mastery.Scythe; break;
                case WeaponType.Axes:
                    MasteryValue = Mastery.Axes; break;
                case WeaponType.Mace:
                    MasteryValue = Mastery.Mace; break;
                case WeaponType.Crossbow:
                    MasteryValue = Mastery.Crossbow; break;
                case WeaponType.Slashers:
                    MasteryValue = Mastery.Slashers; break;
                case WeaponType.FishingPole:
                    MasteryValue = Mastery.FishingPole; break;
            }
            if (MasteryValue > 0) MasteryValue = (float)(MasteryValue * 0.001);
            if (MasterySpellValue > 0) MasterySpellValue = (float)(MasterySpellValue * 0.001);
            return true;
        }

        public static void SetMastery(ulong SteamID, WeaponType Type, int Value)
        {
            int NoneExpertise = 0;
            if (Type == WeaponType.None)
            {
                if (Value > 0) NoneExpertise = Value * 2;
                else NoneExpertise = Value;
            }
            bool isPlayerFound = Database.player_weaponmastery.TryGetValue(SteamID, out WeaponMasterData Mastery);
            if (isPlayerFound)
            {
                switch (Type)
                {
                    case WeaponType.Sword:
                        if (Mastery.Sword + Value > MaxMastery) Mastery.Sword = MaxMastery;
                        else if (Mastery.Sword + Value < 0) Mastery.Sword = 0;
                        else Mastery.Sword += Value;
                        break;
                    case WeaponType.Spear:
                        if (Mastery.Spear + Value >= MaxMastery) Mastery.Spear = MaxMastery;
                        else if (Mastery.Spear + Value < 0) Mastery.Spear = 0;
                        else Mastery.Spear += Value;
                        break;
                    case WeaponType.None:
                        if (Mastery.None + NoneExpertise > MaxMastery) Mastery.None = MaxMastery;
                        else if (Mastery.None + NoneExpertise < 0) Mastery.None = 0;
                        else Mastery.None += NoneExpertise;
                        if (Mastery.Spell + Value > MaxMastery) Mastery.Spell = MaxMastery;
                        else if (Mastery.Spell + Value < 0) Mastery.Spell = 0;
                        else Mastery.Spell += Value;
                        break;
                    case WeaponType.Scythe:
                        if (Mastery.Scythe + Value >= MaxMastery) Mastery.Scythe = MaxMastery;
                        else if (Mastery.Scythe + Value < 0) Mastery.Scythe = 0;
                        else Mastery.Scythe += Value;
                        break;
                    case WeaponType.Axes:
                        if (Mastery.Axes + Value >= MaxMastery) Mastery.Axes = MaxMastery;
                        else if (Mastery.Axes + Value < 0) Mastery.Axes = 0;
                        else Mastery.Axes += Value;
                        break;
                    case WeaponType.Mace:
                        if (Mastery.Mace + Value >= MaxMastery) Mastery.Mace = MaxMastery;
                        else if (Mastery.Mace + Value < 0) Mastery.Mace = 0;
                        else Mastery.Mace += Value;
                        break;
                    case WeaponType.Crossbow:
                        if (Mastery.Crossbow + Value >= MaxMastery) Mastery.Crossbow = MaxMastery;
                        else if (Mastery.Crossbow + Value < 0) Mastery.Crossbow = 0;
                        else Mastery.Crossbow += Value;
                        break;
                    case WeaponType.Slashers:
                        if (Mastery.Slashers + Value >= MaxMastery) Mastery.Slashers = MaxMastery;
                        else if (Mastery.Slashers + Value < 0) Mastery.Slashers = 0;
                        else Mastery.Slashers += Value;
                        break;
                    case WeaponType.FishingPole:
                        if (Mastery.FishingPole + Value >= MaxMastery) Mastery.FishingPole = MaxMastery;
                        else if (Mastery.FishingPole + Value < 0) Mastery.FishingPole = 0;
                        else Mastery.FishingPole += Value;
                        break;
                }
            }
            else
            {
                Mastery = new WeaponMasterData();

                if (NoneExpertise < 0) NoneExpertise = 0;
                if (Value < 0) Value = 0;

                switch (Type)
                {
                    case WeaponType.Sword:
                        Mastery.Sword += Value; break;
                    case WeaponType.Spear:
                        Mastery.Spear += Value; break;
                    case WeaponType.None:
                        Mastery.None += NoneExpertise;
                        Mastery.Spell += Value; break;
                    case WeaponType.Scythe:
                        Mastery.Scythe += Value; break;
                    case WeaponType.Axes:
                        Mastery.Axes += Value; break;
                    case WeaponType.Mace:
                        Mastery.Mace += Value; break;
                    case WeaponType.Crossbow:
                        Mastery.Crossbow += Value; break;
                    case WeaponType.Slashers:
                        Mastery.Slashers += Value; break;
                    case WeaponType.FishingPole:
                        Mastery.FishingPole += Value; break;
                }
            }
            Database.player_weaponmastery[SteamID] = Mastery;
            return;
        }

        public static WeaponType GetWeaponType(Entity Player)
        {
            Entity WeaponEntity = em.GetComponentData<Equipment>(Player).WeaponSlotEntity._Entity;
            WeaponType WeaponType = WeaponType.None;
            if (em.HasComponent<EquippableData>(WeaponEntity))
            {
                EquippableData WeaponData = em.GetComponentData<EquippableData>(WeaponEntity);
                WeaponType = WeaponData.WeaponType;
            }
            return WeaponType;
        }

        public static void SaveWeaponMastery()
        {
            File.WriteAllText("BepInEx/config/RPGMods/Saves/weapon_mastery.json", JsonSerializer.Serialize(Database.player_weaponmastery, Database.JSON_options));
            File.WriteAllText("BepInEx/config/RPGMods/Saves/mastery_decay.json", JsonSerializer.Serialize(Database.player_decaymastery_logout, Database.JSON_options));
            File.WriteAllText("BepInEx/config/RPGMods/Saves/player_log_mastery.json", JsonSerializer.Serialize(Database.player_log_mastery, Database.JSON_options));
        }

        public static void LoadWeaponMastery()
        {
            if (!File.Exists("BepInEx/config/RPGMods/Saves/weapon_mastery.json"))
            {
                FileStream stream = File.Create("BepInEx/config/RPGMods/Saves/weapon_mastery.json");
                stream.Dispose();
            }
            string json = File.ReadAllText("BepInEx/config/RPGMods/Saves/weapon_mastery.json");
            try
            {
                Database.player_weaponmastery = JsonSerializer.Deserialize<Dictionary<ulong, WeaponMasterData>>(json);
                Plugin.Logger.LogWarning("WeaponMastery DB Populated.");
            }
            catch
            {
                Database.player_weaponmastery = new Dictionary<ulong, WeaponMasterData>();
                Plugin.Logger.LogWarning("WeaponMastery DB Created.");
            }

            if (!File.Exists("BepInEx/config/RPGMods/Saves/mastery_decay.json"))
            {
                FileStream stream = File.Create("BepInEx/config/RPGMods/Saves/mastery_decay.json");
                stream.Dispose();
            }
            json = File.ReadAllText("BepInEx/config/RPGMods/Saves/mastery_decay.json");
            try
            {
                Database.player_decaymastery_logout = JsonSerializer.Deserialize<Dictionary<ulong, DateTime>>(json);
                Plugin.Logger.LogWarning("WeaponMasteryDecay DB Populated.");
            }
            catch
            {
                Database.player_decaymastery_logout = new Dictionary<ulong, DateTime>();
                Plugin.Logger.LogWarning("WeaponMasteryDecay DB Created.");
            }

            if (!File.Exists("BepInEx/config/RPGMods/Saves/player_log_mastery.json"))
            {
                FileStream stream = File.Create("BepInEx/config/RPGMods/Saves/player_log_mastery.json");
                stream.Dispose();
            }
            json = File.ReadAllText("BepInEx/config/RPGMods/Saves/player_log_mastery.json");
            try
            {
                Database.player_log_mastery = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                Plugin.Logger.LogWarning("Player_LogMastery_Switch DB Populated.");
            }
            catch
            {
                Database.player_log_mastery = new Dictionary<ulong, bool>();
                Plugin.Logger.LogWarning("Player_LogMastery_Switch DB Created.");
            }
        }
    }
}
