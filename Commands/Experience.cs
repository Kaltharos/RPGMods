using ProjectM.Network;
using RPGMods.Utils;
using Unity.Collections;
using Unity.Entities;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("experience, xp", Usage = "experience", Description = "Shows your currect experience and progression to next level.")]
    public static class Experience
    {
        private static EntityManager entityManager = VWorld.Server.EntityManager;
        public static void Initialize(Context ctx)
        {
            var user = ctx.Event.User;
            var CharName = user.CharacterName.ToString();
            var SteamID = user.PlatformId;
            var PlayerCharacter = ctx.Event.SenderCharacterEntity;

            if (!ExperienceSystem.isEXPActive)
            {
                Utils.CommandOutput.CustomErrorMessage(ctx, "Experience system is not enabled.");
                return;
            }

            if (ctx.Args.Length >= 2 && user.IsAdmin)
            {
                if (ctx.Args[0].Equals("set") && int.TryParse(ctx.Args[1], out int value))
                {
                    if (ctx.Args.Length == 3)
                    {
                        string name = ctx.Args[2];
                        if(CommandHelper.FindPlayer(name, true, out var targetEntity, out var targetUserEntity))
                        {
                            CharName = name;
                            SteamID = entityManager.GetComponentData<User>(targetUserEntity).PlatformId;
                            PlayerCharacter = targetEntity;
                        }
                        else
                        {
                            Utils.CommandOutput.CustomErrorMessage(ctx, $"Could not find specified player \"{name}\".");
                            return;
                        }
                    }
                    Database.player_experience[SteamID] = value;
                    ExperienceSystem.SetLevel(PlayerCharacter, SteamID);
                    user.SendSystemMessage($"Player \"{CharName}\" Experience is now set to be <color=#ffffffff>{ExperienceSystem.getXp(SteamID)}</color>");
                }
                else
                {
                    Utils.CommandOutput.InvalidArguments(ctx);
                    return;
                }
            }
            else
            {
                int userLevel = ExperienceSystem.getLevel(SteamID);
                user.SendSystemMessage($"-- <color=#ffffffff>{CharName}</color> --");
                user.SendSystemMessage(
                    $"Level: <color=#ffffffff>{userLevel}</color>  (<color=#ffffffff>{ExperienceSystem.getLevelProgress(SteamID)}%</color>) " +
                    $" [ XP: <color=#ffffffff>{ExperienceSystem.getXp(SteamID)}</color>/<color=#ffffffff>{ExperienceSystem.convertLevelToXp(userLevel + 1)}</color> ]");
            }
        }
    }
}
