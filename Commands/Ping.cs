using ProjectM.Network;
using RPGMods.Utils;

namespace RPGMods.Commands
{
    [Command("ping, p", Usage = "ping", Description = "Shows your latency.")]
    public static class Ping
    {
        public static void Initialize(Context ctx)
        {
            var ping = ctx.EntityManager.GetComponentData<Latency>(ctx.Event.SenderCharacterEntity).Value;
            Output.SendSystemMessage(ctx, $"Your latency is <color=#ffff00>{ping}</color>s");
        }
    }
}
