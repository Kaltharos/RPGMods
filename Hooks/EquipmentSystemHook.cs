using HarmonyLib;
using ProjectM.Gameplay.Systems;
using Unity.Entities;
using Unity.Collections;
using ProjectM.Network;
using ProjectM;
using RPGMods.Systems;
using RPGMods.Utils;

namespace RPGMods.Hooks;

[HarmonyPatch(typeof(ArmorLevelSystem_Spawn), nameof(ArmorLevelSystem_Spawn.OnUpdate))]
public class ArmorLevelSystem_Spawn_Patch
{
    private static void Prefix(ArmorLevelSystem_Spawn __instance)
    {
        if (__instance.__OnUpdate_LambdaJob0_entityQuery == null) return;

        if (ExperienceSystem.isEXPActive)
        {
            EntityManager entityManager = __instance.EntityManager;
            NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                ArmorLevel level = entityManager.GetComponentData<ArmorLevel>(entity);
                level.Level = 0;
                entityManager.SetComponentData(entity, level);
            }
        }
    }
}

[HarmonyPatch(typeof(WeaponLevelSystem_Spawn), nameof(WeaponLevelSystem_Spawn.OnUpdate))]
public class WeaponLevelSystem_Spawn_Patch
{
    private static void Prefix(WeaponLevelSystem_Spawn __instance)
    {
        if (__instance.__OnUpdate_LambdaJob0_entityQuery == null) return;

        if (ExperienceSystem.isEXPActive || WeaponMasterSystem.isMasteryEnabled)
        {
            EntityManager entityManager = __instance.EntityManager;
            NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
            foreach(var entity in entities)
            {
                if (ExperienceSystem.isEXPActive)
                {
                    WeaponLevel level = entityManager.GetComponentData<WeaponLevel>(entity);
                    level.Level = 0;
                    entityManager.SetComponentData(entity, level);
                }
                if (WeaponMasterSystem.isMasteryEnabled)
                {
                    Entity Owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
                    if (!entityManager.HasComponent<PlayerCharacter>(Owner)) continue;

                    PlayerCharacter playerCharacter = entityManager.GetComponentData<PlayerCharacter>(Owner);
                    Entity User = playerCharacter.UserEntity._Entity;

                    Helper.ApplyBuff(User, Owner, Database.Buff.Buff_VBlood_Perk_Moose);
                }
            }
        }

    }
}

[HarmonyPatch(typeof(SpellLevelSystem_Spawn), nameof(SpellLevelSystem_Spawn.OnUpdate))]
public class SpellLevelSystem_Spawn_Patch
{
    private static void Prefix(SpellLevelSystem_Spawn __instance)
    {
        if (__instance.__OnUpdate_LambdaJob0_entityQuery == null) return;

        if (ExperienceSystem.isEXPActive)
        {
            EntityManager entityManager = __instance.EntityManager;
            NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                SpellLevel level = entityManager.GetComponentData<SpellLevel>(entity);
                level.Level = 0;
                entityManager.SetComponentData(entity, level);
            }
        }
    }

    private static void Postfix(SpellLevelSystem_Spawn __instance)
    {
        if (__instance.__OnUpdate_LambdaJob0_entityQuery == null) return;

        if (ExperienceSystem.isEXPActive)
        {
            EntityManager entityManager = __instance.EntityManager;
            NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                if (!entityManager.HasComponent<LastTranslation>(entity))
                {
                    Entity Owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
                    if (entityManager.HasComponent<PlayerCharacter>(Owner))
                    {
                        Entity User = entityManager.GetComponentData<PlayerCharacter>(Owner).UserEntity._Entity;
                        ulong SteamID = entityManager.GetComponentData<User>(User).PlatformId;
                        ExperienceSystem.SetLevel(Owner, User, SteamID);
                    }
                }
            }
        }
    }
}

[HarmonyPatch(typeof(SpellLevelSystem_Destroy), nameof(SpellLevelSystem_Destroy.OnUpdate))]
public class SpellLevelSystem_Destroy_Patch
{
    private static void Prefix(SpellLevelSystem_Destroy __instance)
    {
        if (__instance.__OnUpdate_LambdaJob0_entityQuery == null) return;

        if (ExperienceSystem.isEXPActive)
        {
            EntityManager entityManager = __instance.EntityManager;
            NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                SpellLevel level = entityManager.GetComponentData<SpellLevel>(entity);
                level.Level = 0;
                entityManager.SetComponentData(entity, level);
            }
        }
    }

    private static void Postfix(SpellLevelSystem_Destroy __instance)
    {
        if (__instance.__OnUpdate_LambdaJob0_entityQuery == null) return;

        if (ExperienceSystem.isEXPActive)
        {
            EntityManager entityManager = __instance.EntityManager;
            NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                if (!entityManager.HasComponent<LastTranslation>(entity))
                {
                    Entity Owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
                    if (entityManager.HasComponent<PlayerCharacter>(Owner))
                    {
                        Entity User = entityManager.GetComponentData<PlayerCharacter>(Owner).UserEntity._Entity;
                        ulong SteamID = entityManager.GetComponentData<User>(User).PlatformId;
                        ExperienceSystem.SetLevel(Owner, User, SteamID);
                    }
                }
            }
        }
    }
}