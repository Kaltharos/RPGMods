using ProjectM;
using ProjectM.Network;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using RPGMods.Utils;
using RPGMods.Hooks;
using RPGMods.Systems;
using System.Text.RegularExpressions;
using ProjectM.Scripting;
using System.Collections.Generic;

namespace RPGMods.Utils
{
    public static class Helper
    {
        private static Entity empty_entity = new Entity();
        private static System.Random rand = new System.Random();

        public static Entity DNEntity = default;
        public static ServerGameSettings SGS = default;
        public static ServerGameManager SGM = default;

        public static Regex rxName = new Regex(@"(?<=\])[^\[].*");

        public static bool GetServerGameManager(out ServerGameManager sgm)
        {
            sgm = Plugin.Server.GetExistingSystem<ServerScriptMapper>()?._ServerGameManager;
            return true;
        }
        public static bool GetServerGameSettings(out ServerGameSettings settings)
        {
            settings = Plugin.Server.GetExistingSystem<ServerGameSettingsSystem>()?._Settings;
            return true;
        }

        public static bool GetDayNightCycle(out Entity dnEntity)
        {
            dnEntity = DNEntity;
            var entities = Plugin.Server.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<DayNightCycle>()).ToEntityArray(Allocator.Temp);
            if (entities.Length > 0)
            {
                dnEntity = entities[0];
                return true;
            }
            return false;
        }

        public static bool GetClanMember(Entity clanEntity, out Dictionary<Entity, Entity> Group)
        {
            Group = new();
            if (SGM._TeamChecker.HasClanTeam(clanEntity))
            {
                int AlliedUsersCount = SGM._TeamChecker.GetAlliedUsersCount(clanEntity);
                if (AlliedUsersCount <= 0) return false;

                NativeList<Entity> allyBuffer = SGM._TeamChecker.GetTeamsChecked();
                //SGM._TeamChecker.GetAlliedUsers(team, allyBuffer);
            }
            return false;
        }

        public static int GetAllies(Entity PlayerCharacter, out PlayerGroup playerGroup)
        {
            if (Cache.PlayerAllies.TryGetValue(PlayerCharacter, out playerGroup))
            {
                TimeSpan CacheAge = DateTime.Now - playerGroup.TimeStamp;
                if (CacheAge.TotalSeconds > 300) return playerGroup.AllyCount;
            }

            Team team = Helper.SGM._TeamChecker.GetTeam(PlayerCharacter);
            playerGroup.AllyCount = Helper.SGM._TeamChecker.GetAlliedUsersCount(team)-1;

            NativeList<Entity> allyBuffer = Helper.SGM._TeamChecker.GetTeamsChecked();
            Helper.SGM._TeamChecker.GetAlliedUsers(team, allyBuffer);

            Dictionary<Entity, Entity> Group = new();

            foreach (var entity in allyBuffer)
            {
                if (Plugin.Server.EntityManager.HasComponent<User>(entity))
                {
                    Entity playerEntity = Plugin.Server.EntityManager.GetComponentData<User>(entity).LocalCharacter._Entity;
                    if (playerEntity.Equals(PlayerCharacter)) continue;
                    Group[entity] = playerEntity;
                }
            }

            playerGroup.Allies = Group;
            playerGroup.TimeStamp = DateTime.Now;
            Cache.PlayerAllies[PlayerCharacter] = playerGroup;

            return playerGroup.AllyCount;
        }

        public static FixedString64 GetTrueName(string name)
        {
            MatchCollection match = rxName.Matches(name);
            if (match.Count > 0)
            {
                name = match[match.Count - 1].ToString();
            }
            return name;
        }

        public static void CreatePlayerCache()
        {
            if (PvPSystem.isHonorSystemEnabled)
            {
                if (Database.PvPStats == null)
                {
                    PvPSystem.LoadPvPStat();
                }
            }

            Cache.NamePlayerCache.Clear();
            Cache.SteamPlayerCache.Clear();
            EntityQuery query = Plugin.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<User>()
                    },
                Options = EntityQueryOptions.IncludeDisabled
            });
            var userEntities = query.ToEntityArray(Allocator.Temp);
            foreach (var entity in userEntities)
            {
                var userData = Plugin.Server.EntityManager.GetComponentData<User>(entity);
                PlayerData playerData = new PlayerData(userData.CharacterName, userData.PlatformId, userData.IsConnected, entity, userData.LocalCharacter._Entity);

                Cache.NamePlayerCache.TryAdd(GetTrueName(userData.CharacterName.ToString().ToLower()), playerData);
                Cache.SteamPlayerCache.TryAdd(userData.PlatformId, playerData);

                if (PvPSystem.isHonorSystemEnabled)
                {
                    Database.PvPStats.TryGetValue(userData.PlatformId, out var pvpStats);
                    Database.SiegeState.TryGetValue(userData.PlatformId, out var siegeData);

                    bool isHostile = HasBuff(userData.LocalCharacter._Entity, PvPSystem.HostileBuff);
                    if ((pvpStats.Reputation <= -1000 || siegeData.IsSiegeOn) && isHostile == false)
                    {
                        isHostile = true;
                        if (PvPSystem.isEnableHostileGlow && !PvPSystem.isUseProximityGlow) ApplyBuff(entity, userData.LocalCharacter._Entity, PvPSystem.HostileBuff);
                    }

                    Cache.HostilityState[userData.LocalCharacter._Entity] = new StateData(userData.PlatformId, isHostile);

                    if (siegeData.IsSiegeOn)
                    {
                        bool forceSiege = (siegeData.SiegeEndTime == DateTime.MinValue);
                        PvPSystem.SiegeON(userData.PlatformId, userData.LocalCharacter._Entity, entity, forceSiege, false);
                    }
                }
            }

            Plugin.Logger.LogWarning("Player Cache Created.");
        }

        public static void UpdatePlayerCache(Entity userEntity, string oldName, string newName, bool forceOffline = false)
        {
            var userData = Plugin.Server.EntityManager.GetComponentData<User>(userEntity);
            Cache.NamePlayerCache.Remove(GetTrueName(oldName.ToLower()));

            if (forceOffline) userData.IsConnected = false;
            PlayerData playerData = new PlayerData(newName, userData.PlatformId, userData.IsConnected, userEntity, userData.LocalCharacter._Entity);

            Cache.NamePlayerCache[GetTrueName(newName.ToLower())] = playerData;
            Cache.SteamPlayerCache[userData.PlatformId] = playerData;
        }

        public static bool RenamePlayer(Entity userEntity, Entity charEntity, FixedString64 newName)
        {
            //-- Max Char Length for FixedString64 is 61 bytes.
            //if (newName.utf8LengthInBytes > 61)
            //{
            //    return false;
            //}

            var userData = Plugin.Server.EntityManager.GetComponentData<User>(userEntity);
            if (PvPSystem.isHonorSystemEnabled)
            {
                var vampire_name = GetTrueName(newName.ToString());

                if (Database.PvPStats.TryGetValue(userData.PlatformId, out var PvPData))
                {
                    vampire_name = "[" + PvPSystem.GetHonorTitle(PvPData.Reputation).Title + "]" + vampire_name;
                }
                else
                {
                    vampire_name = "[" + PvPSystem.GetHonorTitle(0).Title + "]" + vampire_name;
                }
                newName = vampire_name;
            }
            UpdatePlayerCache(userEntity, userData.CharacterName.ToString(), newName.ToString());

            var des = Plugin.Server.GetExistingSystem<DebugEventsSystem>();
            var networkId = Plugin.Server.EntityManager.GetComponentData<NetworkId>(userEntity);
            var renameEvent = new RenameUserDebugEvent
            {
                NewName = newName,
                Target = networkId
            };
            var fromCharacter = new FromCharacter
            {
                User = userEntity,
                Character = charEntity
            };
            des.RenameUser(fromCharacter, renameEvent);
            return true;
        }

        public static bool ValidateName(string name, out CreateCharacterFailureReason invalidReason)
        {
            if (Regex.IsMatch(name, @"[^a-zA-Z0-9]"))
            {
                invalidReason = CreateCharacterFailureReason.InvalidName;
                return false;
            }

            //-- The game default max byte length is 20.
            //-- The max legth assignable is actually 61 bytes.
            FixedString64 charName = name;
            if (charName.utf8LengthInBytes > 20)
            {
                invalidReason = CreateCharacterFailureReason.InvalidName;
                return false;
            }

            if (Cache.NamePlayerCache.TryGetValue(name.ToLower(), out _))
            {
                invalidReason = CreateCharacterFailureReason.NameTaken;
                return false;
            }

            invalidReason = CreateCharacterFailureReason.None;
            return true;
        }

        public static void ApplyBuff(Entity User, Entity Char, PrefabGUID GUID)
        {
            var des = Plugin.Server.GetExistingSystem<DebugEventsSystem>();
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
            if (BuffUtility.HasBuff(Plugin.Server.EntityManager, Char, GUID))
            {
                BuffUtility.TryGetBuff(Plugin.Server.EntityManager, Char, GUID, out var BuffEntity_);
                Plugin.Server.EntityManager.AddComponent<DestroyTag>(BuffEntity_);
                return;
            }
        }

        public static string GetNameFromSteamID(ulong SteamID)
        {
            //var UserEntities = Plugin.Server.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<User>()).ToEntityArray(Allocator.Temp);
            //foreach (var Entity in UserEntities)
            //{
            //    var EntityData = Plugin.Server.EntityManager.GetComponentData<User>(Entity);
            //    if (EntityData.PlatformId == SteamID) return EntityData.CharacterName.ToString();
            //}
            //return null;
            if (Cache.SteamPlayerCache.TryGetValue(SteamID, out var data))
            {
                return data.CharacterName.ToString();
            }
            else
            {
                return null;
            }
        }

        public static PrefabGUID GetGUIDFromName(string name)
        {
            var gameDataSystem = Plugin.Server.GetExistingSystem<GameDataSystem>();
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

        public static void KickPlayer(Entity userEntity)
        {
            EntityManager em = Plugin.Server.EntityManager;
            var userData = em.GetComponentData<User>(userEntity);
            int index = userData.Index;
            NetworkId id = em.GetComponentData<NetworkId>(userEntity);

            var entity = em.CreateEntity(
                ComponentType.ReadOnly<NetworkEventType>(),
                ComponentType.ReadOnly<SendEventToUser>(),
                ComponentType.ReadOnly<KickEvent>()
            );

            var KickEvent = new KickEvent()
            {
                PlatformId = userData.PlatformId
            };

            em.SetComponentData<SendEventToUser>(entity, new()
            {
                UserIndex = index
            });
            em.SetComponentData<NetworkEventType>(entity, new()
            {
                EventId = NetworkEvents.EventId_KickEvent,
                IsAdminEvent = false,
                IsDebugEvent = false
            });

            em.SetComponentData(entity, KickEvent);
        }

        public static Entity AddItemToInventory(Context ctx, PrefabGUID guid, int amount)
        {
            unsafe
            {
                var gameData = Plugin.Server.GetExistingSystem<GameDataSystem>();
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
            EntityManager entityManager = Plugin.Server.EntityManager;

            //-- Way of the Cache
            if (Cache.NamePlayerCache.TryGetValue(name.ToLower(), out var data))
            {
                playerEntity = data.CharEntity;
                userEntity = data.UserEntity;
                if (mustOnline)
                {
                    var userComponent = entityManager.GetComponentData<User>(userEntity);
                    if (!userComponent.IsConnected)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                playerEntity = empty_entity;
                userEntity = empty_entity;
                return false;
            }

            //-- Way of the Query
            //foreach (var UsersEntity in entityManager.CreateEntityQuery(ComponentType.ReadOnly<User>()).ToEntityArray(Allocator.Temp))
            //{
            //    var target_component = entityManager.GetComponentData<User>(UsersEntity);
            //    if (mustOnline)
            //    {
            //        if (!target_component.IsConnected) continue;
            //    }


            //    string CharName = target_component.CharacterName.ToString();
            //    if (CharName.Equals(name))
            //    {
            //        userEntity = UsersEntity;
            //        playerEntity = target_component.LocalCharacter._Entity;
            //        return true;
            //    }
            //}
            //playerEntity = empty_entity;
            //userEntity = empty_entity;
            //return false;
        }
        public static bool FindPlayer(ulong steamid, bool mustOnline, out Entity playerEntity, out Entity userEntity)
        {
            EntityManager entityManager = Plugin.Server.EntityManager;

            //-- Way of the Cache
            if (Cache.SteamPlayerCache.TryGetValue(steamid, out var data))
            {
                playerEntity = data.CharEntity;
                userEntity = data.UserEntity;
                if (mustOnline)
                {
                    var userComponent = entityManager.GetComponentData<User>(userEntity);
                    if (!userComponent.IsConnected)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                playerEntity = empty_entity;
                userEntity = empty_entity;
                return false;
            }
        }

        public static bool IsPlayerInCombat(Entity player)
        {
            return BuffUtility.HasBuff(Plugin.Server.EntityManager, player, Database.Buff.InCombat) || BuffUtility.HasBuff(Plugin.Server.EntityManager, player, Database.Buff.InCombat_PvP);
        }

        public static bool HasBuff(Entity player, PrefabGUID BuffGUID)
        {
            return BuffUtility.HasBuff(Plugin.Server.EntityManager, player, BuffGUID);
        }

        public static void SetPvPShield(Entity character, bool value)
        {
            var em = Plugin.Server.EntityManager;
            var cUnitStats = em.GetComponentData<UnitStats>(character);
            var cBuffer = em.GetBuffer<BoolModificationBuffer>(character);
            cUnitStats.PvPProtected.Set(value, cBuffer);
            em.SetComponentData(character, cUnitStats);
        }

        public static bool SpawnNPCIdentify(out float identifier, string name, float3 position, float minRange = 1, float maxRange = 2, float duration = -1)
        {
            identifier = 0f;
            float default_duration = 5.0f;
            float duration_final;
            var isFound = Database.database_units.TryGetValue(name, out var unit);
            if (!isFound) return false;

            float UniqueID = (float)rand.NextDouble();
            if (UniqueID == 0.0) UniqueID += 0.00001f;
            else if (UniqueID == 1.0f) UniqueID -= 0.00001f;
            duration_final = default_duration + UniqueID;
            
            while (Cache.spawnNPC_Listen.ContainsKey(duration))
            {
                UniqueID = (float)rand.NextDouble();
                if (UniqueID == 0.0) UniqueID += 0.00001f;
                else if (UniqueID == 1.0f) UniqueID -= 0.00001f;
                duration_final = default_duration + UniqueID;
            }

            UnitSpawnerReactSystem_Patch.listen = true;
            identifier = duration_final;
            var Data = new SpawnNPCListen(duration, default, default, default, false);
            Cache.spawnNPC_Listen.Add(duration_final, Data);

            Plugin.Server.GetExistingSystem<UnitSpawnerUpdateSystem>().SpawnUnit(empty_entity, unit, position, 1, minRange, maxRange, duration_final);
            return true;
        }

        public static bool SpawnAtPosition(Entity user, string name, int count, float2 position, float minRange = 1, float maxRange = 2, float duration = -1)
        {
            var isFound = Database.database_units.TryGetValue(name, out var unit);
            if (!isFound) return false;

            var translation = Plugin.Server.EntityManager.GetComponentData<Translation>(user);
            var f3pos = new float3(position.x, translation.Value.y, position.y);
            Plugin.Server.GetExistingSystem<UnitSpawnerUpdateSystem>().SpawnUnit(empty_entity, unit, f3pos, count, minRange, maxRange, duration);
            return true;
        }

        public static bool SpawnAtPosition(Entity user, int GUID, int count, float2 position, float minRange = 1, float maxRange = 2, float duration = -1)
        {
            var unit = new PrefabGUID(GUID);

            var translation = Plugin.Server.EntityManager.GetComponentData<Translation>(user);
            var f3pos = new float3(position.x, translation.Value.y, position.y);
            try
            {
                Plugin.Server.GetExistingSystem<UnitSpawnerUpdateSystem>().SpawnUnit(empty_entity, unit, f3pos, count, minRange, maxRange, duration);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static PrefabGUID GetPrefabGUID(Entity entity)
        {
            var entityManager = Plugin.Server.EntityManager;
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
            var s = Plugin.Server.GetExistingSystem<PrefabCollectionSystem>();
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
