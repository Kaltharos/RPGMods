using ProjectM;
using ProjectM.Network;
using RPGMods.Utils;
using Unity.Collections;
using Unity.Entities;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("resetcooldown, cd", Usage = "resetcooldown [<Player Name>]", Description = "Instantly cooldown all ability & skills for the player.")]
    public static class ResetCooldown
    {
        public static void Initialize(Context ctx)
        {
            Entity PlayerCharacter = ctx.Event.SenderCharacterEntity;
            string CharName = ctx.Event.User.CharacterName.ToString();
            EntityManager entityManager = VWorld.Server.EntityManager;

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
                    Utils.Output.CustomErrorMessage(ctx, $"Could not find the specified player \"{name}\".");
                    return;
                }
            }

            var AbilityBuffer = entityManager.GetBuffer<AbilityGroupSlotBuffer>(PlayerCharacter);
            for (int i = 0; i < AbilityBuffer.Length; i++)
            {
                var AbilitySlot = AbilityBuffer[i].GroupSlotEntity._Entity;
                var ActiveAbility = entityManager.GetComponentData<AbilityGroupSlot>(AbilitySlot);
                var ActiveAbility_Entity = ActiveAbility.StateEntity._Entity;

                var b = Helper.GetPrefabGUID(ActiveAbility_Entity);
                if (b.GuidHash == 0) continue;

                var AbilityStateBuffer = entityManager.GetBuffer<AbilityStateBuffer>(ActiveAbility_Entity);
                for (int c_i = 0; c_i < AbilityStateBuffer.Length; c_i++)
                {
                    var abilityState = AbilityStateBuffer[c_i].StateEntity._Entity;
                    var abilityCooldownState = entityManager.GetComponentData<AbilityCooldownState>(abilityState);
                    abilityCooldownState.CooldownEndTime = 0;
                    entityManager.SetComponentData(abilityState, abilityCooldownState);
                }
            }
            ctx.Event.User.SendSystemMessage($"Player \"{CharName}\" cooldown resetted.");
        }
    }
}