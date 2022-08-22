using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Unity.Entities;

namespace RPGMods.Commands
{
    [Command("worlddynamics, wd", Usage = "wd [<faction>] [<stats>|<save>|<load>]", Description = "List all faction stats. Save them, or load from the json file.")]
    public static class WorldDynamics
    {
        public static void Initialize(Context ctx)
        {
            if (WorldDynamicsSystem.isFactionDynamic == false)
            {
                Output.CustomErrorMessage(ctx, "World dynamics system is not enabled.");
                return;
            }

            if (ctx.Args.Length < 2)
            {
                Output.MissingArguments(ctx);
                return;
            }

            if (ctx.Args[0].ToLower().Equals("faction"))
            {
                if (ctx.Args[1].ToLower().Equals("stats"))
                {
                    int i = 0;
                    foreach (var item in Database.FactionStats)
                    {
                        if (!item.Value.Active) continue;
                        i++;
                        Output.SendSystemMessage(ctx, $"Name: {Color.Green(item.Value.Name)} [Lv.{Color.Yellow(item.Value.Level.ToString())}]");
                        Output.SendSystemMessage(ctx, $"Active Pwr: [{Color.White(item.Value.ActivePower.ToString())}] Stored Pwr: [{Color.Yellow(item.Value.StoredPower.ToString())}]");
                        Output.SendSystemMessage(ctx, $"Daily Pwr: [{Color.Teal(item.Value.DailyPower.ToString())}] Req. Pwr: [{Color.SoftRed(item.Value.RequiredPower.ToString())}]");
                    }
                    if (i == 0) Output.SendSystemMessage(ctx, "No active facton.");
                    return;
                }
                if (ctx.Args[1].ToLower().Equals("save"))
                {
                    WorldDynamicsSystem.SaveFactionStats();
                    Output.SendSystemMessage(ctx, $"Factions data saved.");
                    return;
                }
                if (ctx.Args[1].ToLower().Equals("load"))
                {
                    WorldDynamicsSystem.LoadFactionStats();
                    Output.SendSystemMessage(ctx, $"Factions json data loaded.");
                    return;
                }
            }
        }
    }
}
