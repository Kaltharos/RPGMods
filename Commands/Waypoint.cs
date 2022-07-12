using RPGMods.Systems;
using RPGMods.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Unity.Entities;
using Unity.Transforms;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("waypoint, wp", "waypoint <Name|Set|Remove|List> [<Name>] [global]", "Teleports you to previously created waypoints.")]
    public static class Waypoint
    {
        public static int WaypointLimit = 3;
        private static EntityManager entityManager = VWorld.Server.EntityManager;
        public static void Initialize(Context ctx)
        {
            var PlayerEntity = ctx.Event.SenderCharacterEntity;
            var SteamID = ctx.Event.User.PlatformId;
            if (Helper.IsPlayerInCombat(PlayerEntity))
            {
                Output.CustomErrorMessage(ctx, "Unable to use waypoint! You're in combat!");
                return;
            }
            if (ctx.Args.Length < 1)
            {
                ctx.Event.User.SendSystemMessage("Missing parameters.");
                return;
            }

            if (ctx.Args.Length > 1)
            {
                string wp_name = ctx.Args[1].ToLower();
                string wp_true_name = ctx.Args[1].ToLower();
                bool global = false;
                if (ctx.Args.Length > 2)
                {
                    var args_2nd = ctx.Args[2].ToLower();
                    bool isAllowed = ctx.Event.User.IsAdmin || PermissionSystem.PermissionCheck(ctx.Event.User.PlatformId, "waypoint_args");
                    if ((args_2nd.Equals("true") || args_2nd.Equals("global")) && isAllowed) global = true;
                    else
                    {
                        Output.CustomErrorMessage(ctx, "You do not have permission to edit a global waypoint.");
                        return;
                    }
                }
                if (ctx.Args[0].ToLower().Equals("set"))
                {
                    if (Database.globalWaypoint.TryGetValue(wp_name, out _))
                    {
                        Output.CustomErrorMessage(ctx, $"A global waypoint with the \"{wp_name}\" name existed. Please rename your waypoint.");
                        return;
                    }
                    if (!global)
                    {
                        if (Database.waypoints_owned.TryGetValue(SteamID, out var total) && !ctx.Event.User.IsAdmin)
                        {
                            if (total >= WaypointLimit)
                            {
                                Output.CustomErrorMessage(ctx, "You already have reached your total waypoint limit.");
                                return;
                            }
                        }
                        wp_name = wp_name + "_" +SteamID;
                        if (Database.waypoints.TryGetValue(wp_name, out _))
                        {
                            Output.CustomErrorMessage(ctx, $"You already have a waypoint with the same name.");
                            return;
                        }
                    }
                    var location = ctx.EntityManager.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;
                    var f2_location = new Float2(location.x, location.z);
                    AddWaypoint(SteamID, f2_location, wp_name, wp_true_name, global);
                    ctx.Event.User.SendSystemMessage("Successfully added Waypoint.");
                    return;
                }
                if (ctx.Args[0].ToLower().Equals("remove"))
                {
                    if (!Database.globalWaypoint.TryGetValue(wp_name, out _) && global)
                    {
                        Output.CustomErrorMessage(ctx, $"Global \"{wp_name}\" waypoint not found.");
                        return;
                    }
                    if (!global)
                    {
                        wp_name = wp_name + "_" + SteamID;
                        if (!Database.waypoints.TryGetValue(wp_name, out _))
                        {
                            Output.CustomErrorMessage(ctx, $"You do not have any waypoint with this name.");
                            return;
                        }
                    }
                    ctx.Event.User.SendSystemMessage("Successfully removed Waypoint.");
                    RemoveWaypoint(SteamID, wp_name, global);
                    return;
                }
            }

            if (ctx.Args[0].ToLower().Equals("list"))
            {
                int total_wp = 0;
                foreach (var global_wp in Database.globalWaypoint)
                {
                    ctx.Event.User.SendSystemMessage($" - <color=#ffff00ff>{global_wp.Key}</color> [<color=#00dd00ff>Global</color>]");
                    total_wp++;
                }
                foreach (var wp in Database.waypoints)
                {
                    ctx.Event.User.SendSystemMessage($" - <color=#ffff00ff>{wp.Value.Name}</color>");
                    total_wp++;
                }
                if (total_wp == 0) Output.CustomErrorMessage(ctx, "No waypoint available.");
                return;
            }

            string waypoint = ctx.Args[0].ToLower();
            if (Database.globalWaypoint.TryGetValue(waypoint, out var WPData))
            {
                Helper.TeleportTo(ctx, WPData.Location);
                return;
            }

            if (Database.waypoints.TryGetValue(waypoint + "_" + SteamID, out var WPData_))
            {
                Helper.TeleportTo(ctx, WPData_.Location);
                return;
            }
            Output.CustomErrorMessage(ctx, "Waypoint not found.");
        }

        public static void AddWaypoint(ulong owner, Float2 location, string name, string true_name, bool isGlobal)
        {
            var WaypointData = new WaypointData(true_name, owner, location);
            if (isGlobal) Database.globalWaypoint[name] = WaypointData;
            else Database.waypoints[name] = WaypointData;
            if (!isGlobal && Database.waypoints_owned.TryGetValue(owner, out var total))
            {
                Database.waypoints_owned[owner] = total + 1;
            }
            else Database.waypoints_owned[owner] = 0;
        }

        public static void RemoveWaypoint(ulong owner, string name, bool global)
        {
            if(global)
            {
                Database.globalWaypoint.Remove(name);
            }
            else
            {
                Database.waypoints_owned[owner] -= 1;
                if (Database.waypoints_owned[owner] < 0) Database.waypoints_owned[owner] = 0;
                Database.waypoints.Remove(name);
            }
        }

        public static void LoadWaypoints()
        {
            if (!File.Exists("BepInEx/config/RPGMods/Saves/waypoints.json"))
            {
                var stream = File.Create("BepInEx/config/RPGMods/Saves/waypoints.json");
                stream.Dispose();
            }

            string json = File.ReadAllText("BepInEx/config/RPGMods/Saves/waypoints.json");
            try
            {
                Database.waypoints = JsonSerializer.Deserialize<Dictionary<string, WaypointData>>(json);
                Plugin.Logger.LogWarning("Waypoints DB Populated");
            }
            catch
            {
                Database.waypoints = new Dictionary<string, WaypointData>();
                Plugin.Logger.LogWarning("Waypoints DB Created");
            }

            if (!File.Exists("BepInEx/config/RPGMods/Saves/global_waypoints.json"))
            {
                var stream = File.Create("BepInEx/config/RPGMods/Saves/global_waypoints.json");
                stream.Dispose();
            }

            json = File.ReadAllText("BepInEx/config/RPGMods/Saves/global_waypoints.json");
            try
            {
                Database.globalWaypoint = JsonSerializer.Deserialize<Dictionary<string, WaypointData>>(json);
                Plugin.Logger.LogWarning("GlobalWaypoints DB Populated");
            }
            catch
            {
                Database.globalWaypoint = new Dictionary<string, WaypointData>();
                Plugin.Logger.LogWarning("GlobalWaypoints DB Created");
            }

            if (!File.Exists("BepInEx/config/RPGMods/Saves/total_waypoints.json"))
            {
                var stream = File.Create("BepInEx/config/RPGMods/Saves/total_waypoints.json");
                stream.Dispose();
            }

            json = File.ReadAllText("BepInEx/config/RPGMods/Saves/total_waypoints.json");
            try
            {
                Database.waypoints_owned = JsonSerializer.Deserialize<Dictionary<ulong, int>>(json);
                Plugin.Logger.LogWarning("TotalWaypoints DB Populated");
            }
            catch
            {
                Database.waypoints_owned = new Dictionary<ulong, int>();
                Plugin.Logger.LogWarning("TotalWaypoints DB Created");
            }
        }

        public static void SaveWaypoints()
        {
            File.WriteAllText("BepInEx/config/RPGMods/Saves/waypoints.json", JsonSerializer.Serialize(Database.waypoints, Database.JSON_options));
            File.WriteAllText("BepInEx/config/RPGMods/Saves/global_waypoints.json", JsonSerializer.Serialize(Database.globalWaypoint, Database.JSON_options));
            File.WriteAllText("BepInEx/config/RPGMods/Saves/total_waypoints.json", JsonSerializer.Serialize(Database.waypoints_owned, Database.JSON_options));
        }
    }
}
