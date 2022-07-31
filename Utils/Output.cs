using ProjectM;
using ProjectM.Network;
using RPGMods.Hooks;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;

namespace RPGMods.Utils
{
    public static class Output
    {
        public static void CustomErrorMessage(Context ctx, string message)
        {
            ServerChatUtils.SendSystemMessageToClient(ctx.EntityManager, ctx.Event.User, $"<color=#ff0000ff>{message}</color>");
        }

        public static void CustomErrorMessage(VChatEvent ev, string message)
        {
            ServerChatUtils.SendSystemMessageToClient(Plugin.Server.EntityManager, ev.User, $"<color=#ff0000ff>{message}</color>");
        }

        public static void SendSystemMessage(Context ctx, string message)
        {
            ServerChatUtils.SendSystemMessageToClient(ctx.EntityManager, ctx.Event.User, $"{message}");
        }

        public static void SendSystemMessage(VChatEvent ev, string message)
        {
            ServerChatUtils.SendSystemMessageToClient(Plugin.Server.EntityManager, ev.User, $"{message}");
        }

        public static void InvalidCommand(VChatEvent ev)
        {
            ServerChatUtils.SendSystemMessageToClient(Plugin.Server.EntityManager, ev.User, $"<color=#ff0000ff>Invalid command.</color>");
        }

        public static void InvalidArguments(Context ctx)
        {
            ServerChatUtils.SendSystemMessageToClient(ctx.EntityManager, ctx.Event.User, $"<color=#ff0000ff>Invalid command parameters. Check {ctx.Prefix}help [<command>] for more information.</color>");
        }

        public static void MissingArguments(Context ctx)
        {
            ServerChatUtils.SendSystemMessageToClient(ctx.EntityManager, ctx.Event.User, $"<color=#ff0000ff>Missing command parameters. Check {ctx.Prefix}help [<command>] for more information.</color>");
        }

        public static void SendLore(Entity userEntity, string message)
        {
            EntityManager em = Plugin.Server.EntityManager;
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
    }
}
