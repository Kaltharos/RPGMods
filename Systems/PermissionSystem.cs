using ProjectM;
using ProjectM.Network;
using RPGMods;
using RPGMods.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Unity.Entities;
using Wetstone.API;
using Wetstone.Hooks;

namespace RPGMods.Systems
{
    public static class PermissionSystem
    {
        public static bool isVIPSystem = true;
        public static bool isVIPWhitelist = true;
        public static int VIP_Permission = 10;
        public static string commandPrefix = ".";

        public static double VIP_OutCombat_ResYield = -1.0;
        public static double VIP_OutCombat_DurabilityLoss = -1.0;
        public static double VIP_OutCombat_MoveSpeed = -1.0;
        public static double VIP_OutCombat_GarlicResistance = -1.0;
        public static double VIP_OutCombat_SilverResistance = -1.0;

        public static double VIP_InCombat_ResYield = -1.0;
        public static double VIP_InCombat_DurabilityLoss = -1.0;
        public static double VIP_InCombat_MoveSpeed = -1.0;
        public static double VIP_InCombat_GarlicResistance = -1.0;
        public static double VIP_InCombat_SilverResistance = -1.0;

        private static EntityManager em = VWorld.Server.EntityManager;

        public static void VIPChat(VChatEvent ev)
        {
            if (ev.Message.StartsWith(commandPrefix)) return;
            if (!IsUserVIP(ev.User.PlatformId)) return;
        }
        public static bool IsUserVIP(ulong steamID)
        {
            bool isVIP = GetUserPermission(steamID) >= VIP_Permission;
            return isVIP;
        }

        public static int GetUserPermission(ulong steamID)
        {
            bool isExist = Database.user_permission.TryGetValue(steamID, out var permission);
            if (isExist) return permission;
            return 0;
        }

        public static int GetCommandPermission(string command)
        {
            var isExist = Database.command_permission.TryGetValue(command, out int requirement);
            if (isExist) return requirement;
            return 100;
        }

        public static bool PermissionCheck(ulong steamID, string command)
        {
            bool isAllowed = GetUserPermission(steamID) >= GetCommandPermission(command);
            return isAllowed;
        }

        public static void BuffReceiver(Entity buffEntity, PrefabGUID GUID)
        {
            if (!GUID.Equals(Database.buff.OutofCombat) && !em.HasComponent<InCombatBuff>(buffEntity)) return;
            var Owner = em.GetComponentData<EntityOwner>(buffEntity).Owner;
            if (!em.HasComponent<PlayerCharacter>(Owner)) return;

            var userEntity = em.GetComponentData<PlayerCharacter>(Owner).UserEntity._Entity;
            var SteamID = em.GetComponentData<User>(userEntity).PlatformId;

            if (IsUserVIP(SteamID))
            {
                var Buffer = em.AddBuffer<ModifyUnitStatBuff_DOTS>(buffEntity);
                //-- Out of Combat Buff
                if (GUID.Equals(Database.buff.OutofCombat))
                {
                    if (VIP_OutCombat_ResYield > 0)
                    {
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.ResourceYield,
                            Value = (float)VIP_OutCombat_ResYield,
                            ModificationType = ModificationType.Multiply,
                            Id = ModificationId.NewId(0)
                        });
                    }
                    if (VIP_OutCombat_DurabilityLoss > 0)
                    {
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.ReducedResourceDurabilityLoss,
                            Value = (float)VIP_OutCombat_DurabilityLoss,
                            ModificationType = ModificationType.Multiply,
                            Id = ModificationId.NewId(0)
                        });
                    }
                    if (VIP_OutCombat_MoveSpeed > 0)
                    {
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.MovementSpeed,
                            Value = (float)VIP_OutCombat_MoveSpeed,
                            ModificationType = ModificationType.Multiply,
                            Id = ModificationId.NewId(0)
                        });
                    }
                    if (VIP_OutCombat_GarlicResistance > 0)
                    {
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.GarlicResistance,
                            Value = (float)VIP_OutCombat_GarlicResistance,
                            ModificationType = ModificationType.Multiply,
                            Id = ModificationId.NewId(0)
                        });
                    }
                    if (VIP_OutCombat_SilverResistance > 0)
                    {
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.SilverResistance,
                            Value = (float)VIP_OutCombat_SilverResistance,
                            ModificationType = ModificationType.Multiply,
                            Id = ModificationId.NewId(0)
                        });
                    }
                }
                //-- In Combat Buff
                else if (em.HasComponent<InCombatBuff>(buffEntity))
                {
                    if (VIP_InCombat_ResYield > 0)
                    {
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.ResourceYield,
                            Value = (float)VIP_InCombat_ResYield,
                            ModificationType = ModificationType.Multiply,
                            Id = ModificationId.NewId(0)
                        });
                    }
                    if (VIP_InCombat_DurabilityLoss > 0)
                    {
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.ReducedResourceDurabilityLoss,
                            Value = (float)VIP_InCombat_DurabilityLoss,
                            ModificationType = ModificationType.Multiply,
                            Id = ModificationId.NewId(0)
                        });
                    }
                    if (VIP_InCombat_MoveSpeed > 0)
                    {
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.MovementSpeed,
                            Value = (float)VIP_InCombat_MoveSpeed,
                            ModificationType = ModificationType.Multiply,
                            Id = ModificationId.NewId(0)
                        });
                    }
                    if (VIP_InCombat_GarlicResistance > 0)
                    {
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.GarlicResistance,
                            Value = (float)VIP_InCombat_GarlicResistance,
                            ModificationType = ModificationType.Multiply,
                            Id = ModificationId.NewId(0)
                        });
                    }
                    if (VIP_InCombat_SilverResistance > 0)
                    {
                        Buffer.Add(new ModifyUnitStatBuff_DOTS()
                        {
                            StatType = UnitStatType.SilverResistance,
                            Value = (float)VIP_InCombat_SilverResistance,
                            ModificationType = ModificationType.Multiply,
                            Id = ModificationId.NewId(0)
                        });
                    }
                }
            }
        }

        public static void SavePermissions()
        {
            File.WriteAllText("BepInEx/config/RPGMods/command_permission.json", JsonSerializer.Serialize(Database.command_permission, Database.Pretty_JSON_options));
        }

        public static void SaveUserPermission()
        {
            File.WriteAllText("BepInEx/config/RPGMods/user_permission.json", JsonSerializer.Serialize(Database.user_permission, Database.Pretty_JSON_options));
        }

        public static void LoadPermissions()
        {
            if (!File.Exists("BepInEx/config/RPGMods/user_permission.json"))
            {
                FileStream stream = File.Create("BepInEx/config/RPGMods/user_permission.json");
                stream.Dispose();
            }
            string json = File.ReadAllText("BepInEx/config/RPGMods/user_permission.json");
            try
            {
                Database.user_permission = JsonSerializer.Deserialize<Dictionary<ulong, int>>(json);
                Plugin.Logger.LogWarning("UserPermissions DB Populated");
            }
            catch
            {
                Database.user_permission = new Dictionary<ulong, int>();
                Plugin.Logger.LogWarning("UserPermission DB Created.");
            }

            if (!File.Exists("BepInEx/config/RPGMods/command_permission.json"))
            {
                FileStream stream = File.Create("BepInEx/config/RPGMods/command_permission.json");
                stream.Dispose();
            }
            json = File.ReadAllText("BepInEx/config/RPGMods/command_permission.json");
            try
            {
                Database.command_permission = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
                Plugin.Logger.LogWarning("CommandPermissions DB Populated");
            }
            catch
            {
                Database.command_permission = new Dictionary<string, int>();
                Plugin.Logger.LogWarning("CommandPermissions DB Created.");
            }
        }
    }
}
