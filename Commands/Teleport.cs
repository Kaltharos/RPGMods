using ProjectM;
using ProjectM.Scripting;
using RPGMods.Utils;
using Unity.Entities;
using Unity.Transforms;

namespace RPGMods.Commands
{
    [Command("teleport, tp", "teleport <Name>", "Teleport you to another online player within your clan.")]
    public static class Teleport
    {
        public static void Initialize(Context ctx)
        {
            var eventUser = ctx.Event.User;
            var UserCharacter = ctx.Event.SenderCharacterEntity;
            var UserEntity = ctx.Event.SenderUserEntity;
            EntityManager entityManager = Plugin.Server.EntityManager;

            if (Helper.IsPlayerInCombat(UserCharacter))
            {
                Output.CustomErrorMessage(ctx, "Unable to use command! You're in combat!");
                return;
            }
            if (ctx.Args.Length < 1)
            {
                Output.InvalidArguments(ctx);
                return;
            }

            Team user_TeamComponent = entityManager.GetComponentData<Team>(UserCharacter);

            string TargetName = string.Join(' ', ctx.Args);
            LocalToWorld target_WorldComponent;
            Team target_TeamComponent;

            if (Helper.FindPlayer(TargetName, true, out Entity TargetChar, out Entity TargetUserEntity))
            {
                target_TeamComponent = entityManager.GetComponentData<Team>(TargetUserEntity);
                target_WorldComponent = entityManager.GetComponentData<LocalToWorld>(TargetChar);
            }
            else
            {
                Output.CustomErrorMessage(ctx, "Target player not found.");
                return;
            }

            var serverGameManager = Plugin.Server.GetExistingSystem<ServerScriptMapper>()?._ServerGameManager;
            if (!serverGameManager._TeamChecker.IsAllies(user_TeamComponent, target_TeamComponent))
            {
                Output.CustomErrorMessage(ctx, "Unable to teleport to player from another Clan!");
                return;
            }

            if (Helper.IsPlayerInCombat(TargetChar))
            {
                Output.CustomErrorMessage(ctx, $"Unable to teleport! Player \"{TargetName}\" is in combat!");
                return;
            }

            Helper.TeleportTo(ctx, new(target_WorldComponent.Position.x, target_WorldComponent.Position.z));
        }
    }
}
