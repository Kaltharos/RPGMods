using ProjectM.Network;
using RPGMods.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace RPGMods.Commands
{
    [Command("powerup, pu", Usage = "pu <player_name> <add>|<remove> <max hp> <p.atk> <s.atk> <p.def> <s.def>", Description = "Buff specified player with the specified value.")]
    public static class PowerUp
    {
        public static void Initialize(Context ctx)
        {
            if (ctx.Args.Length < 3)
            {
                Output.MissingArguments(ctx);
                return;
            }

            string PlayerName = ctx.Args[0].ToLower();
            if (!Helper.FindPlayer(PlayerName, false, out var playerEntity, out var userEntity))
            {
                Output.CustomErrorMessage(ctx, "Specified player not found.");
                return;
            }
            ulong SteamID = Plugin.Server.EntityManager.GetComponentData<User>(userEntity).PlatformId;

            if (ctx.Args[1].ToLower().Equals("remove"))
            {
                Database.PowerUpList.Remove(SteamID);
                Helper.ApplyBuff(userEntity, playerEntity, Database.Buff.Buff_VBlood_Perk_Moose);
                Output.SendSystemMessage(ctx, "PowerUp removed from specified player.");
                return;
            }

            if (ctx.Args.Length < 7)
            {
                Output.MissingArguments(ctx);
                return;
            }

            if (ctx.Args[1].ToLower().Equals("add"))
            {
                bool maxHPOK = float.TryParse(ctx.Args[2], out var MaxHP);
                bool patkOK = float.TryParse(ctx.Args[3], out var PATK);
                bool satkOK = float.TryParse(ctx.Args[4], out var SATK);
                bool pdefOK = float.TryParse(ctx.Args[5], out var PDEF);
                bool sdefOK = float.TryParse(ctx.Args[6], out var SDEF);

                if (!maxHPOK || !patkOK || !pdefOK || !satkOK || !sdefOK)
                {
                    Output.InvalidArguments(ctx);
                    return;
                }

                var PowerUpData = new PowerUpData()
                {
                    Name = PlayerName,
                    MaxHP = MaxHP,
                    PATK = PATK,
                    PDEF = PDEF,
                    SATK = SATK,
                    SDEF = SDEF
                };

                Database.PowerUpList[SteamID] = PowerUpData;
                Helper.ApplyBuff(userEntity, playerEntity, Database.Buff.Buff_VBlood_Perk_Moose);
                Output.SendSystemMessage(ctx, "PowerUp added to specified player.");
                return;
            }

            Output.InvalidArguments(ctx);
            return;
        }

        public static void SavePowerUp()
        {
            File.WriteAllText("BepInEx/config/RPGMods/Saves/powerup.json", JsonSerializer.Serialize(Database.PowerUpList, Database.JSON_options));
        }

        public static void LoadPowerUp()
        {
            if (!File.Exists("BepInEx/config/RPGMods/Saves/powerup.json"))
            {
                var stream = File.Create("BepInEx/config/RPGMods/Saves/powerup.json");
                stream.Dispose();
            }
            string content = File.ReadAllText("BepInEx/config/RPGMods/Saves/powerup.json");
            try
            {
                Database.PowerUpList = JsonSerializer.Deserialize<Dictionary<ulong, PowerUpData>>(content);
                Plugin.Logger.LogWarning("PowerUp DB Populated.");
            }
            catch
            {
                Database.PowerUpList = new ();
                Plugin.Logger.LogWarning("PowerUp DB Created.");
            }
        }
    }
}
