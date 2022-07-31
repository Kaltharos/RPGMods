using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using Unity.Entities;

namespace RPGMods.Commands
{
    [Command("heat", Usage = "heat", Description = "Shows your current wanted level.")]
    public static class Heat
    {
        private static EntityManager entityManager = Plugin.Server.EntityManager;
        public static void Initialize(Context ctx)
        {
            var user = ctx.Event.User;
            var SteamID = user.PlatformId;
            var userEntity = ctx.Event.SenderUserEntity;
            var charEntity = ctx.Event.SenderCharacterEntity;

            if (!HunterHunted.isActive)
            {
                Output.CustomErrorMessage(ctx, "HunterHunted system is not enabled.");
                return;
            }

            bool isAllowed = ctx.Event.User.IsAdmin || PermissionSystem.PermissionCheck(ctx.Event.User.PlatformId, "heat_args");
            if (ctx.Args.Length >= 2 && isAllowed)
            {
                string CharName = ctx.Event.User.CharacterName.ToString();
                if (ctx.Args.Length == 3)
                {
                    string name = ctx.Args[2];
                    if (Helper.FindPlayer(name, true, out var targetEntity, out var targetUserEntity))
                    {
                        SteamID = entityManager.GetComponentData<User>(targetUserEntity).PlatformId;
                        CharName = name;
                        userEntity = targetUserEntity;
                        charEntity = targetEntity;
                    }
                    else
                    {
                        Output.CustomErrorMessage(ctx, $"Could not find specified player \"{name}\".");
                        return;
                    }
                }
                if (int.TryParse(ctx.Args[0], out var n)) Cache.heatlevel[SteamID] = n;
                if (int.TryParse(ctx.Args[1], out var nm)) Cache.bandit_heatlevel[SteamID] = nm;
                Output.SendSystemMessage(ctx, $"Player \"{CharName}\" heat value changed.");
                Output.SendSystemMessage(ctx, $"Human: <color=#ffff00ff>{Cache.heatlevel[SteamID]}</color> | Bandit: <color=#ffff00ff>{Cache.bandit_heatlevel[SteamID]}</color>");
                HunterHunted.HeatManager(userEntity, charEntity, false);
                return;
            }

            HunterHunted.HeatManager(userEntity, charEntity, false);

            Cache.heatlevel.TryGetValue(SteamID, out var human_heatlevel);
            if (human_heatlevel >= 3000) Output.SendLore(userEntity,$"<color=#0048ffff>[Humans]</color> <color=#c90e21ff>YOU ARE A MENACE...</color>");
            else if (human_heatlevel >= 2000) Output.SendLore(userEntity, $"<color=#0048ffff>[Humans]</color> <color=#c90e21ff>The Vampire Hunters are hunting you...</color>");
            else if (human_heatlevel >= 1000) Output.SendLore(userEntity, $"<color=#0048ffff>[Humans]</color> <color=#c90e21ff>Humans elite squads are hunting you...</color>");
            else if (human_heatlevel >= 500) Output.SendLore(userEntity, $"<color=#0048ffff>[Humans]</color> <color=#c4515cff>Humans soldiers are hunting you...</color>");
            else if (human_heatlevel >= 250) Output.SendLore(userEntity, $"<color=#0048ffff>[Humans]</color> <color=#c9999eff>The humans are hunting you...</color>");
            else Output.SendLore(userEntity, $"<color=#0048ffff>[Humans]</color> <color=#ffffffff>You're currently anonymous...</color>");

            Cache.bandit_heatlevel.TryGetValue(SteamID, out var bandit_heatlevel);
            if (bandit_heatlevel >= 2000) Output.SendLore(userEntity, $"<color=#ff0000ff>[Bandits]</color> <color=#c90e21ff>The bandits really wants you dead...</color>");
            else if (bandit_heatlevel >= 1000) Output.SendLore(userEntity, $"<color=#ff0000ff>[Bandits]</color> <color=#c90e21ff>A large bandit squads are hunting you...</color>");
            else if (bandit_heatlevel >= 500) Output.SendLore(userEntity, $"<color=#ff0000ff>[Bandits]</color> <color=#c4515cff>A small bandit squads are hunting you...</color>");
            else if (bandit_heatlevel >= 250) Output.SendLore(userEntity,$"<color=#ff0000ff>[Bandits]</color> <color=#c9999eff>The bandits are hunting you...</color>");
            else Output.SendLore(userEntity, $"<color=#ff0000ff>[Bandits]</color> <color=#ffffffff>The bandits doesn't recognize you...</color>");

            if (ctx.Args.Length == 1 && user.IsAdmin)
            {
                if (!ctx.Args[0].Equals("debug") && ctx.Args.Length != 2) return;
                Output.SendSystemMessage(ctx, $"Heat Cooldown: {HunterHunted.heat_cooldown}");
                Output.SendSystemMessage(ctx, $"Bandit Heat Cooldown: {HunterHunted.bandit_heat_cooldown}");
                Output.SendSystemMessage(ctx, $"Cooldown Interval: {HunterHunted.cooldown_timer}");
                Output.SendSystemMessage(ctx, $"Ambush Interval: {HunterHunted.ambush_interval}");
                Output.SendSystemMessage(ctx, $"Ambush Chance: {HunterHunted.ambush_chance}");
                Output.SendSystemMessage(ctx, $"Human: <color=#ffff00ff>{human_heatlevel}</color> | Bandit: <color=#ffff00ff>{bandit_heatlevel}</color>");
            }
        }
    }
}
