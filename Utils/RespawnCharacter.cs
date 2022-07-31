using ProjectM;
using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace RPGMods.Utils
{
    struct NullableFloat
    {
        public float3 value;
        public bool has_value;
    }
    public class RespawnCharacter
    {
        private static EntityManager entityManager = Plugin.Server.EntityManager;
        public static void Respawn(Entity VictimEntity, PlayerCharacter player, Entity userEntity)
        {
            var bufferSystem = Plugin.Server.GetOrCreateSystem<EntityCommandBufferSystem>();
            var commandBufferSafe = new EntityCommandBufferSafe(Allocator.Temp)
            {
                Unsafe = bufferSystem.CreateCommandBuffer()
            };

            unsafe
            {
                var playerLocation = player.LastValidPosition;

                var bytes = stackalloc byte[Marshal.SizeOf<NullableFloat>()];
                var bytePtr = new IntPtr(bytes);
                Marshal.StructureToPtr<NullableFloat>(new()
                {
                    value = new float3(playerLocation.x, 0, playerLocation.y),
                    has_value = true
                }, bytePtr, false);
                var boxedBytePtr = IntPtr.Subtract(bytePtr, 0x10);

                var spawnLocation = new Il2CppSystem.Nullable<float3>(boxedBytePtr);
                var server = Plugin.Server.GetOrCreateSystem<ServerBootstrapSystem>();

                server.RespawnCharacter(commandBufferSafe, userEntity, customSpawnLocation: spawnLocation, previousCharacter: VictimEntity, fadeOutEntity: userEntity);
            }
        }
    }
}
