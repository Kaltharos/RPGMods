using ProjectM;
using ProjectM.Gameplay.Scripting;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using UnhollowerRuntimeLib;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Wetstone.API;
using System.Reflection;
using static RPGMods.Utils.Database;

namespace RPGMods.Utils
{
    public static class CommandHelper
    {
        private static Entity empty_entity = new Entity();

        public static void ApplyBuff(Entity User, Entity Char, PrefabGUID GUID)
        {
            var des = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
            var fromCharacter = new FromCharacter()
            {
                User = User,
                Character = Char
            };
            var buffEvent = new ApplyBuffDebugEvent()
            {
                BuffPrefabGUID = GUID
            };
            des.ApplyBuff(fromCharacter, buffEvent);
        }

        public static void RemoveBuff(Entity Char, PrefabGUID GUID)
        {
            if (BuffUtility.HasBuff(VWorld.Server.EntityManager, Char, GUID))
            {
                BuffUtility.TryGetBuff(VWorld.Server.EntityManager, Char, GUID, out var BuffEntity_);
                VWorld.Server.EntityManager.AddComponent<DestroyTag>(BuffEntity_);
                return;
            }
        }

        public static string GetNameFromSteamID(ulong SteamID)
        {
            var UserEntities = VWorld.Server.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<User>()).ToEntityArray(Allocator.Temp);
            foreach (var Entity in UserEntities)
            {
                var EntityData = VWorld.Server.EntityManager.GetComponentData<User>(Entity);
                if (EntityData.PlatformId == SteamID) return EntityData.CharacterName.ToString();
            }
            return null;
        }

        public static PrefabGUID GetGUIDFromName(string name)
        {
            var gameDataSystem = VWorld.Server.GetExistingSystem<GameDataSystem>();
            var managed = gameDataSystem.ManagedDataRegistry;

            foreach (var entry in gameDataSystem.ItemHashLookupMap)
            {
                try
                {
                    var item = managed.GetOrDefault<ManagedItemData>(entry.Key);
                    if (item.PrefabName.StartsWith("Item_VBloodSource") || item.PrefabName.StartsWith("GM_Unit_Creature_Base") || item.PrefabName == "Item_Cloak_ShadowPriest") continue;
                    if (item.Name.ToString().ToLower() == name.ToLower())
                    {
                        return entry.Key;
                    }
                }
                catch { }
            }

            return new PrefabGUID(0);
        }

        public static Entity AddItemToInventory(Context ctx, PrefabGUID guid, int amount)
        {
            unsafe
            {
                var gameData = VWorld.Server.GetExistingSystem<GameDataSystem>();
                var bytes = stackalloc byte[Marshal.SizeOf<FakeNull>()];
                var bytePtr = new IntPtr(bytes);
                Marshal.StructureToPtr<FakeNull>(new()
                {
                    value = 7,
                    has_value = true
                }, bytePtr, false);
                var boxedBytePtr = IntPtr.Subtract(bytePtr, 0x10);
                var hack = new Il2CppSystem.Nullable<int>(boxedBytePtr);
                var hasAdded = InventoryUtilitiesServer.TryAddItem(ctx.EntityManager, gameData.ItemHashLookupMap, ctx.Event.SenderCharacterEntity, guid, amount, out _, out Entity e, default, hack);
                return e;
            }
        }

        public static BloodType GetBloodTypeFromName(string name)
        {
            BloodType type = BloodType.Frailed;
            if (Enum.IsDefined(typeof(BloodType), CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name)))
                Enum.TryParse(name, true, out type);
            return type;
        }

        public static PrefabGUID GetSourceTypeFromName(string name)
        {
            PrefabGUID type;
            name = name.ToLower();
            if (name.Equals("brute")) type = new PrefabGUID(-1464869978);
            else if (name.Equals("warrior")) type = new PrefabGUID(-1128238456);
            else if (name.Equals("rogue")) type = new PrefabGUID(-1030822544);
            else if (name.Equals("scholar")) type = new PrefabGUID(-700632469);
            else if (name.Equals("creature")) type = new PrefabGUID(1897056612);
            else if (name.Equals("worker")) type = new PrefabGUID(-1342764880);
            else type = new PrefabGUID();
            return type;
        }

        public static bool FindPlayer(string name, bool mustOnline, out Entity playerEntity, out Entity userEntity)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            foreach (var UsersEntity in entityManager.CreateEntityQuery(ComponentType.ReadOnly<User>()).ToEntityArray(Allocator.Temp))
            {
                var target_component = entityManager.GetComponentData<User>(UsersEntity);
                if (mustOnline)
                {
                    if (!target_component.IsConnected) continue;
                }
                

                string CharName = target_component.CharacterName.ToString();
                if (CharName.Equals(name))
                {
                    userEntity = UsersEntity;
                    playerEntity = target_component.LocalCharacter._Entity;
                    return true;
                }
            }
            playerEntity = empty_entity;
            userEntity = empty_entity;
            return false;
        }

        public static bool IsPlayerInCombat(Entity player)
        {
            return BuffUtility.HasBuff(VWorld.Server.EntityManager, player, buff.InCombat) || BuffUtility.HasBuff(VWorld.Server.EntityManager, player, buff.InCombat_PvP);
        }

        public static bool IsPlayerHasBuff(Entity player, PrefabGUID BuffGUID)
        {
            return BuffUtility.HasBuff(VWorld.Server.EntityManager, player, BuffGUID);
        }

        public static void SetPvPShield(Entity character, bool value)
        {
            var em = VWorld.Server.EntityManager;
            var cUnitStats = em.GetComponentData<UnitStats>(character);
            var cBuffer = em.GetBuffer<BoolModificationBuffer>(character);
            cUnitStats.PvPProtected.Set(value, cBuffer);
            em.SetComponentData(character, cUnitStats);
        }

        public static bool SpawnAtPosition(Entity user, string name, int count, float2 position, float minRange = 1, float maxRange = 2, float duration = -1)
        {
            var isFound = database_units.TryGetValue(name, out var unit);
            if (!isFound) return false;

            var translation = VWorld.Server.EntityManager.GetComponentData<Translation>(user);
            var f3pos = new float3(position.x, translation.Value.y, position.y);
            VWorld.Server.GetExistingSystem<UnitSpawnerUpdateSystem>().SpawnUnit(empty_entity, unit, f3pos, count, minRange, maxRange, duration);
            return true;
        }

        public static bool SpawnAtPosition(Entity user, int GUID, int count, float2 position, float minRange = 1, float maxRange = 2, float duration = -1)
        {
            var unit = new PrefabGUID(GUID);

            var translation = VWorld.Server.EntityManager.GetComponentData<Translation>(user);
            var f3pos = new float3(position.x, translation.Value.y, position.y);
            try
            {
                VWorld.Server.GetExistingSystem<UnitSpawnerUpdateSystem>().SpawnUnit(empty_entity, unit, f3pos, count, minRange, maxRange, duration);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static PrefabGUID GetPrefabGUID(Entity entity)
        {
            var entityManager = VWorld.Server.EntityManager;
            PrefabGUID guid;
            try
            {
                guid = entityManager.GetComponentData<PrefabGUID>(entity);
            }
            catch
            {
                guid.GuidHash = 0;
            }
            return guid;
        }

        public static string GetPrefabName(PrefabGUID hashCode)
        {
            var s = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>();
            string name = "Nonexistent";
            if (hashCode.GuidHash == 0)
            {
                return name;
            }
            try
            {
                name = s.PrefabNameLookupMap[hashCode].ToString();
            }
            catch
            {
                name = "NoPrefabName";
            }
            return name;
        }

        public static void TeleportTo(Context ctx, Float2 position)
        {
            var entity = ctx.EntityManager.CreateEntity(
                    ComponentType.ReadWrite<FromCharacter>(),
                    ComponentType.ReadWrite<PlayerTeleportDebugEvent>()
                );

            ctx.EntityManager.SetComponentData<FromCharacter>(entity, new()
            {
                User = ctx.Event.SenderUserEntity,
                Character = ctx.Event.SenderCharacterEntity
            });

            ctx.EntityManager.SetComponentData<PlayerTeleportDebugEvent>(entity, new()
            {
                Position = new float2(position.x, position.y),
                Target = PlayerTeleportDebugEvent.TeleportTarget.Self
            });
        }

        struct FakeNull
        {
            public int value;
            public bool has_value;
        }

        public enum BloodType
        {
            Frailed = -899826404,
            Creature = -77658840,
            Warrior = -1094467405,
            Rogue = 793735874,
            Brute = 581377887,
            Scholar = -586506765,
            Worker = -540707191
        }
    }
}
