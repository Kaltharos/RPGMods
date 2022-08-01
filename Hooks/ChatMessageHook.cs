using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using RPGMods.Utils;

namespace RPGMods.Hooks
{
    public struct VChatEvent
    {
        public Entity SenderUserEntity { get; set; }
        public Entity SenderCharacterEntity { get; set; }
        public string Message { get; set; }
        public ChatMessageType Type { get; set; }
        public User User { get; set; }

        public VChatEvent(Entity senderUserEntity, Entity senderCharacterEntity, string message, ChatMessageType type, User user)
        {
            SenderUserEntity = senderUserEntity;
            SenderCharacterEntity = senderCharacterEntity;
            Message = message;
            Type = type;
            User = user;
        }
    }

    [HarmonyPatch(typeof(ChatMessageSystem), nameof(ChatMessageSystem.OnUpdate))]
    public class ChatMessageSystem_Patch
    {
        public static void Prefix(ChatMessageSystem __instance)
        {
            if (__instance.__ChatMessageJob_entityQuery != null)
            {
                NativeArray<Entity> entities = __instance.__ChatMessageJob_entityQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in entities)
                {
                    var fromData = __instance.EntityManager.GetComponentData<FromCharacter>(entity);
                    var userData = __instance.EntityManager.GetComponentData<User>(fromData.User);
                    var chatEventData = __instance.EntityManager.GetComponentData<ChatMessageEvent>(entity);

                    var messageText = chatEventData.MessageText.ToString();
                    if (messageText.StartsWith(CommandHandler.Prefix, System.StringComparison.Ordinal))
                    {
                        VChatEvent ev = new VChatEvent(fromData.User, fromData.Character, messageText, chatEventData.MessageType, userData);
                        CommandHandler.HandleCommands(ev);
                        __instance.EntityManager.AddComponent<DestroyTag>(entity);
                    }
                }
            }
        }
    }
}
