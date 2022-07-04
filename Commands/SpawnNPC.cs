using RPGMods.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Transforms;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("spawnnpc, spn", "spawnnpc <Prefab Name/GUID> [<Amount>] [<Waypoint>]", "Spawns a NPC to a previously created waypoint.")]
    public static class SpawnNPC
    {
        public static void Initialize(Context ctx)
        {
            if (ctx.Args.Length != 0)
            {
                string name = "";
                string waypoint = "";
                int count = 1;

                int n;
                bool isParsable = false;

                bool isUsingGUID = int.TryParse(ctx.Args[0], out var GUID);

                if (ctx.Args.Length >= 2)
                {
                    isParsable = int.TryParse(ctx.Args[1], out n);
                    if (isParsable) count = Convert.ToInt32(ctx.Args[1]);
                    else count = 1;
                }

                if (ctx.Args.Length == 1 || ctx.Args.Length == 2 && isParsable)
                {
                    name = ctx.Args[0];
                    var pos = ctx.EntityManager.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;
                    if (isUsingGUID)
                    {
                        if (!Helper.SpawnAtPosition(ctx.Event.SenderUserEntity, GUID, count, new(pos.x, pos.z), 1, 2, 1800))
                        {
                            Output.CustomErrorMessage(ctx, $"Failed to spawn: {name}");
                            return;
                        }
                    }
                    else
                    {
                        if (!Helper.SpawnAtPosition(ctx.Event.SenderUserEntity, name, count, new(pos.x, pos.z), 1, 2, 1800))
                        {
                            Output.CustomErrorMessage(ctx, $"Could not find specified unit: {name}");
                            return;
                        }
                    }
                    ctx.Event.User.SendSystemMessage($"Spawning {count} {name} at <{pos.x}, {pos.z}>");
                }
                else if (ctx.Args.Length >= 2 && !isParsable)
                {
                    name = ctx.Args[0];
                    waypoint = ctx.Args.Last().ToLower();
                    ulong SteamID = ctx.Event.User.PlatformId;

                    if (Database.globalWaypoint.TryGetValue(waypoint, out var WPData))
                    {
                        Float2 wp = WPData.Location;
                        if (!Helper.SpawnAtPosition(ctx.Event.SenderUserEntity, name, count, new(wp.x, wp.y), 1, 2, 1800))
                        {
                            Output.CustomErrorMessage(ctx, $"Could not find specified unit: {name}");
                            return;
                        }
                        ctx.Event.User.SendSystemMessage($"Spawning {count} {name} at <{wp.x}, {wp.y}>");
                        return;
                    }

                    if (Database.waypoints.TryGetValue(waypoint+"_"+SteamID, out var WPData_))
                    {
                        Float2 wp = WPData_.Location;
                        if (!Helper.SpawnAtPosition(ctx.Event.SenderUserEntity, name, count, new(wp.x, wp.y), 1, 2, 1800))
                        {
                            Output.CustomErrorMessage(ctx, $"Could not find specified unit: {name}");
                            return;
                        }
                        ctx.Event.User.SendSystemMessage($"Spawning {count} {name} at <{wp.x}, {wp.y}>");
                        return;
                    }
                    Output.CustomErrorMessage(ctx, "This waypoint doesn't exist.");
                }
            }
            else
            {
                Output.MissingArguments(ctx);
            }
        }
    }
}