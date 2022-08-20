using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using System;
using Unity.Transforms;

namespace RPGMods.Commands
{
    [Command("playerinfo, i", Usage = "playerinfo <Name>", Description = "Display the player information details.")]
    public static class PlayerInfo
    {
        public static void Initialize(Context ctx)
        {
            if (ctx.Args.Length < 1) 
            {
                Output.MissingArguments(ctx);
                return;
            }

            if (!Helper.FindPlayer(ctx.Args[0], false, out var playerEntity, out var userEntity))
            {
                Output.CustomErrorMessage(ctx, "Player not found."); 
                return;
            }

            var userData = ctx.EntityManager.GetComponentData<User>(userEntity);

            ulong SteamID = userData.PlatformId;
            string Name = userData.CharacterName.ToString();
            string CharacterEntity = playerEntity.Index.ToString() + ":" + playerEntity.Version.ToString();
            string UserEntity = userEntity.Index.ToString() + ":" + userEntity.Version.ToString();
            var ping = (int) ctx.EntityManager.GetComponentData<Latency>(playerEntity).Value;
            var position = ctx.EntityManager.GetComponentData<Translation>(playerEntity).Value;

            Database.PvPStats.TryGetValue(SteamID, out var pvpStats);
            Database.player_experience.TryGetValue(SteamID, out var exp);

            Output.SendSystemMessage(ctx, $"Name: {Color.White(Name)}");
            Output.SendSystemMessage(ctx, $"SteamID: {Color.White(SteamID.ToString())}");
            Output.SendSystemMessage(ctx, $"Latency: {Color.White(ping.ToString())}s");
            Output.SendSystemMessage(ctx, $"-- Position --");
            Output.SendSystemMessage(ctx, $"X: {Color.White(Math.Round(position.x,2).ToString())} " +
                $"Y: {Color.White(Math.Round(position.y,2).ToString())} " +
                $"Z: {Color.White(Math.Round(position.z,2).ToString())}");
            Output.SendSystemMessage(ctx, $"-- {Color.White("Entities")} --");
            Output.SendSystemMessage(ctx, $"Char Entity: {Color.White(CharacterEntity)}");
            Output.SendSystemMessage(ctx, $"User Entity: {Color.White(UserEntity)}");
            Output.SendSystemMessage(ctx, $"-- {Color.White("Experience")} --");
            Output.SendSystemMessage(ctx, $"Level: {Color.White(ExperienceSystem.convertXpToLevel(exp).ToString())} [{Color.White(exp.ToString())}]");
            Output.SendSystemMessage(ctx, $"-- {Color.White("PvP Stats")} --");

            if (PvPSystem.isHonorSystemEnabled)
            {
                Database.SiegeState.TryGetValue(SteamID, out var siegeState);
                Cache.HostilityState.TryGetValue(playerEntity, out var hostilityState);

                double tLeft = 0;
                if (siegeState.IsSiegeOn)
                {
                    TimeSpan TimeLeft = siegeState.SiegeEndTime - DateTime.Now;
                    tLeft = Math.Round(TimeLeft.TotalHours, 2);
                }

                string hostilityText = hostilityState.IsHostile ? "Aggresive" : "Passive";
                string siegeText = siegeState.IsSiegeOn ? "Sieging" : "Defensive";

                Output.SendSystemMessage(ctx, $"Reputation: {Color.White(pvpStats.Reputation.ToString())}");
                Output.SendSystemMessage(ctx, $"Hostility: {Color.White(hostilityText)}");
                Output.SendSystemMessage(ctx, $"Siege: {Color.White(siegeText)}");
                Output.SendSystemMessage(ctx, $"-- Time Left: {Color.White(tLeft.ToString())} hour(s)");
            }

            Output.SendSystemMessage(ctx, $"K/D: {Color.White(pvpStats.KD.ToString())} " +
                $"Kill: {Color.White(pvpStats.Kills.ToString())} " +
                $"Death: {Color.White(pvpStats.Deaths.ToString())}");
        }
    }

    [Command("myinfo, me", Usage = "myinfo", Description = "Display your information details.")]
    public static class MyInfo
    {
        public static void Initialize(Context ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            string Name = ctx.Event.User.CharacterName.ToString();
            string CharacterEntity = ctx.Event.SenderCharacterEntity.Index.ToString() + ":" + ctx.Event.SenderCharacterEntity.Version.ToString();
            string UserEntity = ctx.Event.SenderUserEntity.Index.ToString() + ":" + ctx.Event.SenderUserEntity.Version.ToString();
            var ping = ctx.EntityManager.GetComponentData<Latency>(ctx.Event.SenderCharacterEntity).Value;
            var position = ctx.EntityManager.GetComponentData<Translation>(ctx.Event.SenderCharacterEntity).Value;

            Output.SendSystemMessage(ctx, $"Name: {Color.White(Name)}");
            Output.SendSystemMessage(ctx, $"SteamID: {Color.White(SteamID.ToString())}");
            Output.SendSystemMessage(ctx, $"Latency: {Color.White(ping.ToString())}s");
            Output.SendSystemMessage(ctx, $"-- Position --");
            Output.SendSystemMessage(ctx, $"X: {Color.White(Math.Round(position.x,2).ToString())} " +
                $"Y: {Color.White(Math.Round(position.y,2).ToString())} " +
                $"Z: {Color.White(Math.Round(position.z,2).ToString())}");
            Output.SendSystemMessage(ctx, $"-- Entities --");
            Output.SendSystemMessage(ctx, $"Char Entity: {Color.White(CharacterEntity)}");
            Output.SendSystemMessage(ctx, $"User Entity: {Color.White(UserEntity)}");
        }
    }
}
