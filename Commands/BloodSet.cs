using ProjectM;
using ProjectM.Network;
using RPGMods.Utils;

namespace RPGMods.Commands
{
    [Command("blood", Usage = "blood <Type> [<Quality>] [<Value>]", Description = "Sets your current Blood Type, Quality and Value")]
    public static class BloodSet
    {
        public static void Initialize(Context ctx)
        {
            if (ctx.Args.Length != 0)
            {
                try
                {
                    PrefabGUID type = new PrefabGUID();
                    float quality = 100;
                    int value = 100;

                    if (ctx.Args.Length >= 1) type = Helper.GetSourceTypeFromName(ctx.Args[0]);
                    if (ctx.Args.Length >= 2)
                    {
                        quality = float.Parse(ctx.Args[1]);
                        if (float.Parse(ctx.Args[1]) < 0) quality = 0;
                        if (float.Parse(ctx.Args[1]) > 100) quality = 100;
                    }
                    if (ctx.Args.Length >= 3) value = int.Parse(ctx.Args[2]);

                    var BloodEvent = new ChangeBloodDebugEvent()
                    {
                        Amount = value,
                        Quality = quality,
                        Source = type
                    };
                    Plugin.Server.GetExistingSystem<DebugEventsSystem>().ChangeBloodEvent(ctx.Event.User.Index, ref BloodEvent);
                    Output.SendSystemMessage(ctx, $"Changed Blood Type to <color=#ffff00ff>{ctx.Args[0]}</color> with <color=#ffff00ff>{quality}</color>% quality");
                }
                catch
                {
                    Output.InvalidArguments(ctx);
                }

            }
            else
            {
                Output.MissingArguments(ctx);
            }
        }
    }
}
