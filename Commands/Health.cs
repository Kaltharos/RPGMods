using ProjectM;
using ProjectM.Network;
using RPGMods.Utils;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("health, hp", Usage = "health <percentage> [<player name>]", Description = "Sets your current Health")]
    public static class Health
    {
        public static void Initialize(Context ctx)
        {
            var PlayerName = ctx.Event.User.CharacterName;
            var UserIndex = ctx.Event.User.Index;
            var component = ctx.EntityManager.GetComponentData<ProjectM.Health>(ctx.Event.SenderCharacterEntity);
            int Value = 10000;
            if (ctx.Args.Length != 0)
            {
                if (!int.TryParse(ctx.Args[0], out Value))
                {
                    Utils.CommandOutput.InvalidArguments(ctx);
                    return;
                }
                else Value = int.Parse(ctx.Args[0]);
            }

            if (ctx.Args.Length == 2)
            {
                var targetName = ctx.Args[1];
                if (CommandHelper.FindPlayer(targetName, true, out var targetEntity, out var targetUserEntity))
                {
                    PlayerName = targetName;
                    UserIndex = VWorld.Server.EntityManager.GetComponentData<User>(targetUserEntity).Index;
                    component = VWorld.Server.EntityManager.GetComponentData<ProjectM.Health>(targetEntity);
                }
                else
                {
                    Utils.CommandOutput.CustomErrorMessage(ctx, $"Player \"{targetName}\" not found.");
                }
            }

            float restore_hp = ((component.MaxHealth / 100) * Value) - component.Value;

            var HealthEvent = new ChangeHealthDebugEvent()
            {
                Amount = (int)restore_hp
            };
            VWorld.Server.GetExistingSystem<DebugEventsSystem>().ChangeHealthEvent(UserIndex, ref HealthEvent);

            ctx.Event.User.SendSystemMessage($"Player \"{PlayerName}\" Health set to <color=#ffff00ff>{Value}%</color>");
        }
    }
}
