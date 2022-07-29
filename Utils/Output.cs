using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;
using Wetstone.API;
using Wetstone.Hooks;
using RPGMods.Systems;

namespace RPGMods.Utils
{
    public static class Output
    {
        public static void CustomErrorMessage(Context ctx, string message)
        {
            message = LangSystem.t(message);
            ctx.Event.User.SendSystemMessage($"<color=#ff0000ff>{message}</color>");
        }

        public static void CustomMessage(Context ctx, string message, params object [] vars)
        {
            message = LangSystem.t(message, vars);
            ctx.Event.User.SendSystemMessage(message);
        }

        public static void InvalidCommand(VChatEvent ev)
        {
            ev.User.SendSystemMessage($"<color=#ff0000ff>{ LangSystem.t("Invalid command.") }</color>");
        }

        public static void InvalidArguments(Context ctx)
        {
            ctx.Event.User.SendSystemMessage($"<color=#ff0000ff>{LangSystem.t("Invalid command parameters. Check {0} help [<command>] for more information.</color>", ctx.Prefix)}");
        }
         
        public static void MissingArguments(Context ctx)
        {
            ctx.Event.User.SendSystemMessage($"<color=#ff0000ff>{LangSystem.t("Missing command parameters. Check {0} help [<command>] for more information.", ctx.Prefix)}</color>");
        }
       

        public static void SendLore(Entity userEntity, string message)
        {
            EntityManager em = VWorld.Server.EntityManager;
            int index = em.GetComponentData<User>(userEntity).Index;
            NetworkId id = em.GetComponentData<NetworkId>(userEntity);

            var entity = em.CreateEntity(
                ComponentType.ReadOnly<NetworkEventType>(),
                ComponentType.ReadOnly<SendEventToUser>(),
                ComponentType.ReadOnly<ChatMessageServerEvent>()
            );

            var ev = new ChatMessageServerEvent();
            ev.MessageText = message;
            ev.MessageType = ServerChatMessageType.Lore;
            ev.FromUser = id;
            ev.TimeUTC = DateTime.Now.ToFileTimeUtc();

            em.SetComponentData<SendEventToUser>(entity, new()
            {
                UserIndex = index
            });
            em.SetComponentData<NetworkEventType>(entity, new()
            {
                EventId = NetworkEvents.EventId_ChatMessageServerEvent,
                IsAdminEvent = false,
                IsDebugEvent = false
            });

            em.SetComponentData(entity, ev);
        }

        public static void Duplicate(FromCharacter fromCharacter, ChatMessageEvent _event)
        {
            EntityManager em = VWorld.Server.EntityManager;

            var entity = em.CreateEntity(
                ComponentType.ReadOnly<FromCharacter>(),
                ComponentType.ReadOnly<NetworkEventType>(),
                ComponentType.ReadOnly<ChatMessageEvent>()
            );

            em.SetComponentData<FromCharacter>(entity, fromCharacter);
            em.SetComponentData<NetworkEventType>(entity, new()
            {
                EventId = NetworkEvents.EventId_ChatMessageEvent,
                IsAdminEvent = false,
                IsDebugEvent = false
            });

            em.SetComponentData(entity, _event);
        }

        //-- Not Really Functional/Usefull?
        public static void SendBack(FromCharacter fromChar, string message)
        {
            EntityManager em = VWorld.Server.EntityManager;
            int index = em.GetComponentData<User>(fromChar.User).Index;
            NetworkId id = em.GetComponentData<NetworkId>(fromChar.User);

            var entity = em.CreateEntity(
                ComponentType.ReadOnly<NetworkEventType>(),
                ComponentType.ReadOnly<SendEventToUser>(),
                ComponentType.ReadOnly<ChatMessageServerEvent>()
            );

            var ev = new ChatMessageServerEvent();
            ev.MessageText = message;
            ev.MessageType = ServerChatMessageType.System;
            ev.FromUser = id;
            ev.TimeUTC = DateTime.Now.ToFileTimeUtc();

            em.SetComponentData<SendEventToUser>(entity, new()
            {
                UserIndex = index
            });
            em.SetComponentData<NetworkEventType>(entity, new()
            {
                EventId = NetworkEvents.EventId_ChatMessageServerEvent,
                IsAdminEvent = false,
                IsDebugEvent = false
            });

            em.SetComponentData(entity, ev);
        }
    }
}
