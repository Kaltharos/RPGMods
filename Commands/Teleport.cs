using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using RPGMods.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("teleport, tp", "teleport <Name>", "Teleport you to another online player within your clan.")]
    public class Teleport
    {
        public void Initialize(Context ctx)
        {
            var eventUser = ctx.Event.User;
            var UserCharacter = ctx.Event.SenderCharacterEntity;
            var UserEntity = ctx.Event.SenderUserEntity;
            EntityManager entityManager = VWorld.Server.EntityManager;

            if (CommandHelper.IsPlayerInCombat(UserCharacter))
            {
                eventUser.SendSystemMessage("Unable to use command! You're <color=#ff0000ff>in combat</color>!");
                return;
            }
            if (ctx.Args.Length < 1)
            {
                eventUser.SendSystemMessage("Missing parameters.");
                return;
            }

            Team user_TeamComponent = entityManager.GetComponentData<Team>(UserEntity);

            string TargetName = string.Join(' ', ctx.Args);
            LocalToWorld target_WorldComponent;
            Team target_TeamComponent;

            if(CommandHelper.FindPlayer(TargetName, true, out Entity TargetChar, out Entity TargetUserEntity))
            {
                target_TeamComponent = entityManager.GetComponentData<Team>(TargetUserEntity);
                target_WorldComponent = entityManager.GetComponentData<LocalToWorld>(TargetChar);
            }
            else
            {
                Utils.CommandOutput.CustomErrorMessage(ctx, "Target player not found.");
                return;
            }

            var serverGameManager = VWorld.Server.GetExistingSystem<ServerScriptMapper>()?._ServerGameManager;
            if (!serverGameManager._TeamChecker.IsAllies(user_TeamComponent, target_TeamComponent))
            {
                Utils.CommandOutput.CustomErrorMessage(ctx, "Unable to teleport to player from another Clan!");
                return;
            }

            if (CommandHelper.IsPlayerInCombat(TargetChar))
            {
                Utils.CommandOutput.CustomErrorMessage(ctx, $"Unable to teleport! Player \"{TargetName}\" is in combat!");
                return;
            }

            CommandHelper.TeleportTo(ctx, new(target_WorldComponent.Position.x, target_WorldComponent.Position.z));
        }
    }
}
