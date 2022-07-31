using ProjectM;
using ProjectM.Network;
using RPGMods.Utils;
using System;
using Unity.Entities;

namespace RPGMods.Systems
{
    public class HunterHunted
    {
        private static EntityManager entityManager = Plugin.Server.EntityManager;

        public static bool isActive = true;
        public static int heat_cooldown = 35;
        public static int bandit_heat_cooldown = 35;
        public static int cooldown_timer = 60;
        public static int ambush_interval = 300;
        public static int ambush_chance = 50;
        public static float ambush_despawn_timer = 300;

        private static Random rand = new Random();
        public static void PlayerUpdateHeat(Entity killerEntity, Entity victimEntity)
        {
            var player = entityManager.GetComponentData<PlayerCharacter>(killerEntity);
            var userEntity = player.UserEntity._Entity;
            var user = entityManager.GetComponentData<User>(userEntity);
            var SteamID = user.PlatformId;

            var victim = entityManager.GetComponentData<FactionReference>(victimEntity);
            var victim_faction = victim.FactionGuid._Value;
            if (Database.faction_heatvalue.TryGetValue(victim_faction, out int heatValue))
            {
                if (victim_faction.GetHashCode() == -413163549) //-- Separate bandit heat level
                {
                    int bandit_heatvalue = rand.Next(1, 10);
                    bool isBanditExist = Cache.bandit_heatlevel.TryGetValue(SteamID, out int player_banditheat);
                    if (isBanditExist) bandit_heatvalue = player_banditheat + bandit_heatvalue;
                    Cache.bandit_heatlevel[SteamID] = bandit_heatvalue;
                }
                bool isExist = Cache.heatlevel.TryGetValue(SteamID, out int player_heat);
                if (isExist) heatValue = player_heat + heatValue;
                Cache.heatlevel[SteamID] = heatValue;
            }
        }

        public static void HeatManager(Entity userEntity, Entity playerEntity, bool InCombat)
        {
            var user = entityManager.GetComponentData<User>(userEntity);
            var SteamID = user.PlatformId;

            DateTime last_update;
            DateTime last_ambushed;
            DateTime bandit_last_ambush;
            Cache.player_heat_timestamp.TryGetValue(SteamID, out last_update);
            Cache.player_last_ambushed.TryGetValue(SteamID, out last_ambushed);
            Cache.bandit_last_ambushed.TryGetValue(SteamID, out bandit_last_ambush);

            TimeSpan elapsed_time = DateTime.Now - last_update;
            if (elapsed_time.TotalSeconds > cooldown_timer)
            {
                int heat_ticks = (int)elapsed_time.TotalSeconds / cooldown_timer;
                if (heat_ticks < 0) heat_ticks = 0;

                int player_heat;
                Cache.heatlevel.TryGetValue(SteamID, out player_heat);
                if (player_heat > 0)
                {
                    player_heat = player_heat - heat_cooldown * heat_ticks;
                    if (player_heat < 0) player_heat = 0;
                    Cache.heatlevel[SteamID] = player_heat;

                    TimeSpan since_ambush = DateTime.Now - last_ambushed;
                    if (since_ambush.TotalSeconds > ambush_interval)
                    {
                        if (rand.Next(0, 100) < ambush_chance && player_heat >= 250 && InCombat)
                        {
                            if (player_heat >= 1500)
                            {
                                SquadList.SpawnSquad(playerEntity, 4, rand.Next(10, 20));
                                SquadList.SpawnSquad(playerEntity, 5, 2);
                                Output.SendLore(userEntity,"<color=#c90e21ff>An extermination squad has found you and wants you DEAD.</color>");
                            }
                            else if (player_heat >= 1000)
                            {
                                if (rand.Next(0, 100) < 50)
                                {
                                    SquadList.SpawnSquad(playerEntity, 5, 1);
                                    SquadList.SpawnSquad(playerEntity, 4, 9);
                                }
                                else
                                {
                                    SquadList.SpawnSquad(playerEntity, 4, rand.Next(15, 20));
                                }
                                Output.SendLore(userEntity, "<color=#c90e21ff>The Vampire Hunters are ambushing you!</color>");
                            }
                            else if (player_heat >= 500)
                            {
                                SquadList.SpawnSquad(playerEntity, 3, rand.Next(10, 15));
                                Output.SendLore(userEntity, "<color=#c90e21ff>An ambush squad from the Church has been sent to kill you!</color>");
                            }
                            else if (player_heat >= 250)
                            {
                                SquadList.SpawnSquad(playerEntity, 2, rand.Next(10, 15));
                                Output.SendLore(userEntity, "<color=#c4515cff>A squad of soldiers is ambushing you!</color>");
                            }
                            else if (player_heat >= 150)
                            {
                                SquadList.SpawnSquad(playerEntity, 1, rand.Next(5, 10));
                                Output.SendLore(userEntity, "<color=#c9999eff>A militia squad is ambushing you!</color>");
                            }
                            Cache.player_last_ambushed[SteamID] = DateTime.Now;
                        }
                    }
                }

                int player_banditheat;
                Cache.bandit_heatlevel.TryGetValue(SteamID, out player_banditheat);
                if (player_banditheat > 0)
                {
                    player_banditheat = player_banditheat - bandit_heat_cooldown * heat_ticks;
                    if (player_banditheat < 0) player_banditheat = 0;
                    Cache.bandit_heatlevel[SteamID] = player_banditheat;

                    TimeSpan since_ambush = DateTime.Now - bandit_last_ambush;
                    if (since_ambush.TotalSeconds > ambush_interval)
                    {
                        if (rand.Next(0, 100) < ambush_chance && player_banditheat >= 250 && InCombat)
                        {
                            if (player_banditheat >= 650)
                            {
                                SquadList.SpawnSquad(playerEntity, 0, rand.Next(20, 25));
                                Output.SendLore(userEntity, "<color=#c90e21ff>The bandits is ambushing you and is not taking chances!</color>");
                            }
                            else if (player_banditheat >= 450)
                            {
                                SquadList.SpawnSquad(playerEntity, 0, rand.Next(10, 15));
                                Output.SendLore(userEntity, "<color=#c90e21ff>A large bandit squads is ambushing you!</color>");
                            }
                            else if (player_banditheat >= 250)
                            {
                                SquadList.SpawnSquad(playerEntity, 0, 5);
                                Output.SendLore(userEntity, "<color=#c4515cff>A small bandit squads is ambushing you!</color>");
                            }
                            else if (player_banditheat >= 150)
                            {
                                SquadList.SpawnSquad(playerEntity, 0, 3);
                                Output.SendLore(userEntity, "<color=#c9999eff>The bandits is ambushing you!</color>");
                            }
                            Cache.bandit_last_ambushed[SteamID] = DateTime.Now;
                        }
                    }
                }
                Cache.player_heat_timestamp[SteamID] = DateTime.Now;
            }
        }
    }
}
