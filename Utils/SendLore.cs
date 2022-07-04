using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;
using Wetstone.API;
using Wetstone.Hooks;

namespace RPGMods.Utils
{
    public static class SendLore
    {
        private static EntityManager em = VWorld.Server.EntityManager;
        public static void LoreMessage(Entity userEntity, string message)
        {
            EntityManager em = VWorld.Server.EntityManager;
            int index = em.GetComponentData<User>(userEntity).Index;
            NetworkId id = em.GetComponentData<NetworkId>(userEntity);

            var entity = em.CreateEntity(
                ComponentType.ReadOnly<NetworkEventType>(), //event type
                ComponentType.ReadOnly<SendEventToUser>(),  //send it to user
                ComponentType.ReadOnly<ChatMessageServerEvent>()    // what event
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
