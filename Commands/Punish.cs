using ProjectM.Network;
using RPGMods.Utils;

namespace RPGMods.Commands
{
    [Command("punish", Usage = "punish <playername> [<remove>]", Description = "Manually punish someone or lift their debuff.")]
    public static class Punish
    {
        public static void Initialize(Context ctx)
        {
            if (ctx.Args.Length > 0)
            {
                string PlayerName = ctx.Args[0];
                if (Helper.FindPlayer(PlayerName, true, out var CharEntity, out var UserEntity))
                {
                    if (ctx.Args.Length == 2)
                    {
                        if (ctx.Args[1].ToLower().Equals("remove"))
                        {
                            Helper.RemoveBuff(CharEntity, Database.Buff.Severe_GarlicDebuff);
                            Output.SendSystemMessage(ctx, $"Punishment debuff removed from player \"{PlayerName}\"");
                            return;
                        }
                        else
                        {
                            Output.InvalidArguments(ctx);
                            return;
                        }
                    }
                    else
                    {
                        Helper.ApplyBuff(UserEntity, CharEntity, Database.Buff.Severe_GarlicDebuff);
                        Output.SendSystemMessage(ctx, $"Applied punishment debuff to player \"{PlayerName}\"");
                        return;
                    }
                }
                else
                {
                    Output.CustomErrorMessage(ctx, "Player not found.");
                    return;
                }
            }
            else
            {
                Output.InvalidArguments(ctx);
                return;
            }
        }
    }
}
