using ProjectM;
using RPGMods.Utils;
using Unity.Entities;

namespace RPGMods.Commands
{
    [Command("resetcooldown, cd", Usage = "resetcooldown [<Player Name>]", Description = "Instantly cooldown all ability & skills for the player.")]
    public static class ResetCooldown
    {
        public static void Initialize(Context ctx)
        {
            Entity PlayerCharacter = ctx.Event.SenderCharacterEntity;
            string CharName = ctx.Event.User.CharacterName.ToString();
            EntityManager entityManager = Plugin.Server.EntityManager;

            if (ctx.Args.Length >= 1)
            {
                string name = string.Join(' ', ctx.Args);
                if (Helper.FindPlayer(name, true, out var targetEntity, out var targetUserEntity))
                {
                    PlayerCharacter = targetEntity;
                    CharName = name;
                }
                else
                {
                    Output.CustomErrorMessage(ctx, $"Could not find the specified player \"{name}\".");
                    return;
                }
            }

            var AbilityBuffer = entityManager.GetBuffer<AbilityGroupSlotBuffer>(PlayerCharacter);
            foreach(var ability in AbilityBuffer)
            {
                var AbilitySlot = ability.GroupSlotEntity._Entity;
                var ActiveAbility = entityManager.GetComponentData<AbilityGroupSlot>(AbilitySlot);
                var ActiveAbility_Entity = ActiveAbility.StateEntity._Entity;

                var b = Helper.GetPrefabGUID(ActiveAbility_Entity);
                if (b.GuidHash == 0) continue;

                var AbilityStateBuffer = entityManager.GetBuffer<AbilityStateBuffer>(ActiveAbility_Entity);
                foreach(var state in AbilityStateBuffer)
                {
                    var abilityState = state.StateEntity._Entity;
                    var abilityCooldownState = entityManager.GetComponentData<AbilityCooldownState>(abilityState);
                    abilityCooldownState.CooldownEndTime = 0;
                    entityManager.SetComponentData(abilityState, abilityCooldownState);
                }
            }
            Output.SendSystemMessage(ctx, $"Player \"{CharName}\" cooldown resetted.");
        }
    }
}