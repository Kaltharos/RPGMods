using RPGMods.Utils;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("heat", Usage = "heat", Description = "Shows your current wanted level.")]
    public static class Heat
    {
        public static void Initialize(Context ctx)
        {
            var user = ctx.Event.User;
            var SteamID = user.PlatformId;

            if (!HunterHunted.isActive)
            {
                Utils.CommandOutput.CustomErrorMessage(ctx, "HunterHunted system is not enabled.");
                return;
            }

            if (ctx.Args.Length == 2 && user.IsAdmin)
            {
                if (int.TryParse(ctx.Args[0], out var n)) Cache.heatlevel[SteamID] = n;
                if (int.TryParse(ctx.Args[1], out var nm)) Cache.bandit_heatlevel[SteamID] = nm;
            }

            HunterHunted.HeatManager(ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, false);

            Cache.heatlevel.TryGetValue(SteamID, out var human_heatlevel);
            if (human_heatlevel >= 3000) user.SendSystemMessage("<color=#c90e21ff>YOU ARE A MENACE...</color>");
            else if (human_heatlevel >= 2000) user.SendSystemMessage("<color=#c90e21ff>The Vampire Hunters are hunting you...</color>");
            else if (human_heatlevel >= 1000) user.SendSystemMessage("<color=#c90e21ff>Humans elite squads are hunting you...</color>");
            else if (human_heatlevel >= 500) user.SendSystemMessage("<color=#c4515cff>Humans soldiers are hunting you...</color>");
            else if (human_heatlevel >= 250) user.SendSystemMessage("<color=#c9999eff>The humans are hunting you...</color>");
            else user.SendSystemMessage("<color=#ffffffff>You're currently anonymous...</color>");

            Cache.bandit_heatlevel.TryGetValue(SteamID, out var bandit_heatlevel);
            if (bandit_heatlevel >= 2000) user.SendSystemMessage("<color=#c90e21ff>The bandits really wants you dead...</color>");
            else if (bandit_heatlevel >= 1000) user.SendSystemMessage("<color=#c90e21ff>A large bandit squads are hunting you...</color>");
            else if (bandit_heatlevel >= 500) user.SendSystemMessage("<color=#c4515cff>A small bandit squads are hunting you...</color>");
            else if (bandit_heatlevel >= 250) user.SendSystemMessage("<color=#c9999eff>The bandits are hunting you...</color>");
            else user.SendSystemMessage("<color=#ffffffff>The bandits doesn't recognize you...</color>");

            if (ctx.Args.Length >= 1 && user.IsAdmin)
            {
                if (!ctx.Args[0].Equals("debug") && ctx.Args.Length != 2) return;
                user.SendSystemMessage($"Heat Cooldown: {HunterHunted.heat_cooldown}");
                user.SendSystemMessage($"Bandit Heat Cooldown: {HunterHunted.bandit_heat_cooldown}");
                user.SendSystemMessage($"Cooldown Interval: {HunterHunted.cooldown_timer}");
                user.SendSystemMessage($"Ambush Interval: {HunterHunted.ambush_interval}");
                user.SendSystemMessage($"Ambush Chance: {HunterHunted.ambush_chance}");
                user.SendSystemMessage($"Human: <color=#ffff00ff>{human_heatlevel}</color> | Bandit: <color=#ffff00ff>{bandit_heatlevel}</color>");
            }
        }
    }
}
