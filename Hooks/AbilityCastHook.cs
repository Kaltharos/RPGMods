//using HarmonyLib;
//using Unity.Entities;
//using Unity.Collections;
//using ProjectM.Network;
//using ProjectM;
//using RPGMods.Utils;

//namespace RPGMods.Hooks
//{
//    [HarmonyPatch(typeof(AbilityCastStarted_SpawnPrefabSystem_Server), nameof(AbilityCastStarted_SpawnPrefabSystem_Server.TrySpawnPrefabOnCast))]
//    public class AbilityCastStarted_SpawnPrefabSystem_Server_Patch
//    {
//        private static void Prefix(AbilityCastStarted_SpawnPrefabSystem_Server __instance, EntityManager entityManager, EntityCommandBufferSafe spawnCommandBuffer, EntityCommandBufferSafe destroyCommandBuffer, NativeHashMap<PrefabGUID, Entity> prefabLookupMap, BufferFromEntity<AbilitySpawnPrefabOnStartCast> getSpawnPrefabOnCast, BuffUtility.GetBuffComponentsFuncs getBuffComponentFuncs, Entity entity, bool ignoreInCombatBuff)
//        {
//            //-- Debugging Start
//            PrefabGUID GUID = entityManager.GetComponentData<PrefabGUID>(entity);
//            string PrefabName = Helper.GetPrefabName(GUID);
//            if (entityManager.HasComponent<EntityOwner>(entity))
//            {
//                Entity Owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
//                if (entityManager.HasComponent<PlayerCharacter>(Owner))
//                {
//                    var userEntity = entityManager.GetComponentData<PlayerCharacter>(Owner).UserEntity._Entity;
//                    var PlayerName = entityManager.GetComponentData<User>(userEntity).CharacterName;
//                    Plugin.Logger.LogWarning($"Spawning ID: {GUID} N: {PrefabName} O: {PlayerName}");

//                    try
//                    {
//                        var Content = entityManager.GetComponentData<EntityCreator>(entity).Creator._Entity;
//                        Plugin.Logger.LogWarning($"Creator ID: {Helper.GetPrefabGUID(Content)} N: {Helper.GetPrefabName(Helper.GetPrefabGUID(Content))}");
//                    }
//                    catch { }

//                    try
//                    {
//                        var Content = entityManager.GetComponentData<AbilityTarget>(entity);
//                        var Target = Content.Target._Entity;
//                        var Type = Content.GetTargetType;
//                        Plugin.Logger.LogWarning($"Target ID: {Helper.GetPrefabGUID(Target)} N: {Helper.GetPrefabName(Helper.GetPrefabGUID(Target))} [T:{Type}]");
//                        foreach (var t in entityManager.GetComponentTypes(Target))
//                        {
//                            Plugin.Logger.LogWarning(
//                            $"--{t}");
//                        }
//                        var BuffGUID = Content.Buff;
//                        var BuffCategory = Content.BuffCategory;
//                        Plugin.Logger.LogWarning($"Buff ID: {BuffGUID} N: {Helper.GetPrefabName(BuffGUID)} [T:{BuffCategory}]");
//                    }
//                    catch { }
//                }
//            }
//            //-- Debugging End
//        }
//    }
//}
