using HarmonyLib;
using Unity.Entities;
using Unity.Collections;
using ProjectM.Network;
using ProjectM;
using RPGMods.Utils;
using RPGMods.Systems;

namespace RPGMods.Hooks;
[HarmonyPatch(typeof(ModifyUnitStatBuffSystem_Spawn), nameof(ModifyUnitStatBuffSystem_Spawn.OnUpdate))]
public class ModifyUnitStatBuffSystem_Spawn_Patch
{
    #region GodMode & Other Buff
    private static ModifyUnitStatBuff_DOTS Cooldown = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.CooldownModifier,
        Value = 0,
        ModificationType = ModificationType.Set,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS SunCharge = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.SunChargeTime,
        Value = 50000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS Hazard = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.ImmuneToHazards,
        Value = 1,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS SunResist = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.SunResistance,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS Speed = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.MovementSpeed,
        Value = 15,
        ModificationType = ModificationType.Set,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS PResist = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.PhysicalResistance,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS FResist = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.FireResistance,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS HResist = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.HolyResistance,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS SResist = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.SilverResistance,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS GResist = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.GarlicResistance,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS SPResist = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.SpellResistance,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS PPower = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.PhysicalPower,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS RPower = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.ResourcePower,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS SPPower = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.SpellPower,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS PHRegen = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.PassiveHealthRegen,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS HRecovery = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.HealthRecovery,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS MaxHP = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.MaxHealth,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS MaxYield = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.ResourceYield,
        Value = 10,
        ModificationType = ModificationType.Multiply,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS DurabilityLoss = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.ReducedResourceDurabilityLoss,
        Value = -10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };
    #endregion

    private static void Prefix(ModifyUnitStatBuffSystem_Spawn __instance)
    {
        if (__instance.__OnUpdate_LambdaJob0_entityQuery == null) return;

        EntityManager entityManager = __instance.EntityManager;
        NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);

        foreach(var entity in entities)
        {
            PrefabGUID GUID = entityManager.GetComponentData<PrefabGUID>(entity);
            if (GUID.Equals(Database.Buff.Buff_VBlood_Perk_Moose))
            {
                Entity Owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
                if (!entityManager.HasComponent<PlayerCharacter>(Owner)) continue;

                PlayerCharacter playerCharacter = entityManager.GetComponentData<PlayerCharacter>(Owner);
                Entity User = playerCharacter.UserEntity._Entity;
                User Data = entityManager.GetComponentData<User>(User);

                var Buffer = entityManager.GetBuffer<ModifyUnitStatBuff_DOTS>(entity);

                Buffer.Clear();

                if (Database.PowerUpList.TryGetValue(Data.PlatformId, out var powerUpData))
                {
                    Buffer.Add(new ModifyUnitStatBuff_DOTS()
                    {
                        StatType = UnitStatType.MaxHealth,
                        Value = powerUpData.MaxHP,
                        ModificationType = ModificationType.Add,
                        Id = ModificationId.NewId(0)
                    });

                    Buffer.Add(new ModifyUnitStatBuff_DOTS()
                    {
                        StatType = UnitStatType.PhysicalPower,
                        Value = powerUpData.PATK,
                        ModificationType = ModificationType.Add,
                        Id = ModificationId.NewId(0)
                    });

                    Buffer.Add(new ModifyUnitStatBuff_DOTS()
                    {
                        StatType = UnitStatType.SpellPower,
                        Value = powerUpData.SATK,
                        ModificationType = ModificationType.Add,
                        Id = ModificationId.NewId(0)
                    });

                    Buffer.Add(new ModifyUnitStatBuff_DOTS()
                    {
                        StatType = UnitStatType.PhysicalResistance,
                        Value = powerUpData.PDEF,
                        ModificationType = ModificationType.Add,
                        Id = ModificationId.NewId(0)
                    });

                    Buffer.Add(new ModifyUnitStatBuff_DOTS()
                    {
                        StatType = UnitStatType.SpellResistance,
                        Value = powerUpData.SDEF,
                        ModificationType = ModificationType.Add,
                        Id = ModificationId.NewId(0)
                    });
                }

                if (WeaponMasterSystem.isMasteryEnabled) WeaponMasterSystem.BuffReceiver(Buffer, Owner, Data.PlatformId);

                if (Database.nocooldownlist.ContainsKey(Data.PlatformId))
                {
                    Buffer.Add(Cooldown);
                }

                if (Database.sunimmunity.ContainsKey(Data.PlatformId))
                {
                    Buffer.Add(SunCharge);
                    Buffer.Add(Hazard);
                    Buffer.Add(SunResist);
                }

                if (Database.speeding.ContainsKey(Data.PlatformId))
                {
                    Buffer.Add(Speed);
                }

                if (Database.godmode.ContainsKey(Data.PlatformId))
                {
                    Buffer.Add(PResist);
                    Buffer.Add(FResist);
                    Buffer.Add(HResist);
                    Buffer.Add(SResist);
                    Buffer.Add(SunResist);
                    Buffer.Add(GResist);
                    Buffer.Add(SPResist);
                    Buffer.Add(PPower);
                    Buffer.Add(RPower);
                    Buffer.Add(SPPower);
                    Buffer.Add(MaxYield);
                    Buffer.Add(MaxHP);
                    Buffer.Add(Hazard);
                    Buffer.Add(SunCharge);
                    Buffer.Add(DurabilityLoss);
                }
            }
        }
    }
}

[HarmonyPatch(typeof(BuffSystem_Spawn_Server), nameof(BuffSystem_Spawn_Server.OnUpdate))]
public class BuffSystem_Spawn_Server_Patch
{
    private static void Prefix(BuffSystem_Spawn_Server __instance)
    {
        if (__instance.__OnUpdate_LambdaJob0_entityQuery == null) return;

        if (PvPSystem.isPunishEnabled || SiegeSystem.isSiegeBuff || PermissionSystem.isVIPSystem || PvPSystem.isHonorSystemEnabled)
        {
            NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
            foreach(var entity in entities)
            {
                PrefabGUID GUID = __instance.EntityManager.GetComponentData<PrefabGUID>(entity);
                //if (WeaponMasterSystem.isMasteryEnabled) WeaponMasterSystem.BuffReceiver(entities[i], GUID);
                if (PvPSystem.isHonorSystemEnabled) PvPSystem.HonorBuffReceiver(entity, GUID);
                if (PermissionSystem.isVIPSystem) PermissionSystem.BuffReceiver(entity, GUID);
                if (PvPSystem.isPunishEnabled) PvPSystem.BuffReceiver(entity, GUID);
                if (SiegeSystem.isSiegeBuff) SiegeSystem.BuffReceiver(entity, GUID);
            }
        }
    }

    private static void Postfix(BuffSystem_Spawn_Server __instance)
    {
        if (__instance.__OnUpdate_LambdaJob0_entityQuery == null) return;

        if (PvPSystem.isPunishEnabled || HunterHuntedSystem.isActive || WeaponMasterSystem.isMasteryEnabled)
        {
            NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                if (!__instance.EntityManager.HasComponent<InCombatBuff>(entity)) continue;
                Entity e_Owner = __instance.EntityManager.GetComponentData<EntityOwner>(entity).Owner;
                if (!__instance.EntityManager.HasComponent<PlayerCharacter>(e_Owner)) continue;
                Entity e_User = __instance.EntityManager.GetComponentData<PlayerCharacter>(e_Owner).UserEntity._Entity;

                if (HunterHuntedSystem.isActive)
                {
                    HunterHuntedSystem.HeatManager(e_User);
                    HunterHuntedSystem.HumanAmbusher(e_User, e_Owner, true);
                    HunterHuntedSystem.BanditAmbusher(e_User, e_Owner, true);
                }
                if (WeaponMasterSystem.isMasteryEnabled) WeaponMasterSystem.LoopMastery(e_User, e_Owner);
                if (PvPSystem.isPunishEnabled && !ExperienceSystem.isEXPActive) PvPSystem.OnCombatEngaged(entity, e_Owner);
            }
        }
    }
}

[HarmonyPatch(typeof(ModifyBloodDrainSystem_Spawn), nameof(ModifyBloodDrainSystem_Spawn.OnUpdate))]
public class ModifyBloodDrainSystem_Spawn_Patch
{
    private static void Prefix(ModifyBloodDrainSystem_Spawn __instance)
    {
        if (__instance.__OnUpdate_LambdaJob0_entityQuery == null) return;

        if (PermissionSystem.isVIPSystem || PvPSystem.isHonorSystemEnabled)
        {
            NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                PrefabGUID GUID = __instance.EntityManager.GetComponentData<PrefabGUID>(entity);
                //if (WeaponMasterSystem.isMasteryEnabled) WeaponMasterSystem.BuffReceiver(entities[i], GUID);
                if (PermissionSystem.isVIPSystem) PermissionSystem.BuffReceiver(entity, GUID);
                if (PvPSystem.isHonorSystemEnabled) PvPSystem.HonorBuffReceiver(entity, GUID);
            }
        }
    }
}

[HarmonyPatch(typeof(Destroy_TravelBuffSystem), nameof(Destroy_TravelBuffSystem.OnUpdate))]
public class Destroy_TravelBuffSystem_Patch
{
    private static void Postfix(Destroy_TravelBuffSystem __instance)
    {
        if (__instance.__OnUpdate_LambdaJob0_entityQuery != null)
        {
            var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                PrefabGUID GUID = __instance.EntityManager.GetComponentData<PrefabGUID>(entity);
                //-- Most likely it's a new player!
                if (GUID.Equals(Database.Buff.AB_Interact_TombCoffinSpawn_Travel))
                {
                    var Owner = __instance.EntityManager.GetComponentData<EntityOwner>(entity).Owner;
                    if (!__instance.EntityManager.HasComponent<PlayerCharacter>(Owner)) return;

                    var userEntity = __instance.EntityManager.GetComponentData<PlayerCharacter>(Owner).UserEntity._Entity;
                    var playerName = __instance.EntityManager.GetComponentData<User>(userEntity).CharacterName.ToString();

                    if (PvPSystem.isHonorSystemEnabled) PvPSystem.NewPlayerReceiver(userEntity, Owner, playerName);
                    else Helper.UpdatePlayerCache(userEntity, playerName, playerName);
                }
            }
        }
    }
}