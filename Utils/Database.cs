using ProjectM;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace RPGMods.Utils
{
    public class Cache
    {
        //-- Cache (Wiped on plugin reload, server restart, and shutdown.)

        //-- -- Player Cache
        public static Dictionary<FixedString64, PlayerData> NamePlayerCache = new();
        public static Dictionary<ulong, PlayerData> SteamPlayerCache = new();
        public static Dictionary<Entity, PlayerGroup> PlayerAllies = new();
        public static Dictionary<Entity, LocalToWorld> PlayerLocations = new();

        //-- -- Commands
        public static Dictionary<ulong, float> command_Cooldown = new();

        //-- -- HunterHunted System
        public static Dictionary<ulong, int> heatlevel = new();
        public static Dictionary<ulong, int> bandit_heatlevel = new();
        public static Dictionary<ulong, int> undead_heatlevel = new();    //-- Not Implemented Yet
        public static Dictionary<ulong, DateTime> player_heat_timestamp = new();
        public static Dictionary<ulong, DateTime> player_last_ambushed = new();
        public static Dictionary<ulong, DateTime> bandit_last_ambushed = new();

        //-- -- Mastery System
        public static Dictionary<ulong, DateTime> player_last_combat = new();
        public static Dictionary<ulong, int> player_combat_ticks = new();

        //-- -- Experience System
        public static Dictionary<ulong, float> player_level = new();

        //-- -- PvP System
        public static Dictionary<Entity, LevelData> PlayerLevelCache = new();
        public static Dictionary<ulong, PvPOffenseLog> OffenseLog = new();
        public static Dictionary<ulong, ReputationLog> ReputationLog = new();
        public static Dictionary<Entity, StateData> HostilityState = new();

        //-- -- CustomNPC Spawner
        public static SizedDictionaryAsync<float, SpawnNPCListen> spawnNPC_Listen = new(500);
    }

    public class Database
    {
        public static JsonSerializerOptions JSON_options = new()
        {
            WriteIndented = false,
            IncludeFields = false
        };
        public static JsonSerializerOptions Pretty_JSON_options = new()
        {
            WriteIndented = true,
            IncludeFields = true
        };
        //-- Dynamic Database (Saved on a JSON file on plugin reload, server restart, and shutdown.)
        //-- Initialization for the data loading is on each command or related CS file.

        //-- -- Commands
        public static Dictionary<ulong, bool> sunimmunity { get; set; }
        public static Dictionary<ulong, bool> nocooldownlist { get; set; }
        public static Dictionary<ulong, bool> godmode { get; set; }
        public static Dictionary<ulong, bool> speeding { get; set; }
        public static Dictionary<ulong, bool> autoRespawn { get; set; }
        public static Dictionary<string, WaypointData> globalWaypoint { get; set; }
        public static Dictionary<string, WaypointData> waypoints { get; set; }
        public static Dictionary<ulong, int> waypoints_owned { get; set; }
        public static Dictionary<ulong, int> user_permission { get; set; }
        public static Dictionary<string, int> command_permission { get; set; }
        public static Dictionary<ulong, PowerUpData> PowerUpList { get; set; }

        //-- -- Ban System
        public static Dictionary<ulong, BanData> user_banlist { get; set; }

        //-- -- EXP System
        public static Dictionary<ulong, int> player_experience { get; set; }
        public static Dictionary<ulong, bool> player_log_exp { get; set; }

        //-- -- PvP System
        //-- -- -- NEW Database
        public static ConcurrentDictionary<ulong, PvPData> PvPStats { get; set; }
        public static Dictionary<ulong, SiegeData> SiegeState = new();
        //-- -- -- OLD Database (To be removed)
        public static Dictionary<ulong, int> pvpkills { get; set; }
        public static Dictionary<ulong, int> pvpdeath { get; set; }
        public static Dictionary<ulong, double> pvpkd { get; set; }

        //-- -- Mastery System
        public static Dictionary<ulong, WeaponMasterData> player_weaponmastery { get; set; }
        public static Dictionary<ulong, DateTime> player_decaymastery_logout { get; set; }
        public static Dictionary<ulong, bool> player_log_mastery { get; set; }

        //-- -- World Event System
        public static ConcurrentDictionary<int, FactionData> FactionStats { get; set; }
        public static HashSet<string> IgnoredMonsters { get; set; }
        public static HashSet<PrefabGUID> IgnoredMonstersGUID { get; set; }

        //-- Static Database (Data that will never be changed in runtime)
        public static Dictionary<string, PrefabGUID> database_units = new()
        {
            { "CHAR_AncientTreant", new PrefabGUID(1843624663) },
            { "CHAR_AncientTreant_Spitter_Summon", new PrefabGUID(457857316) },
            { "CHAR_ArchMage_FlameSphere", new PrefabGUID(2138173476) },
            { "CHAR_ArchMage_Summon", new PrefabGUID(805231073) },
            { "CHAR_ArchMage_VBlood", new PrefabGUID(-2013903325) },
            { "CHAR_Bandit_Bomber", new PrefabGUID(-1128238456) },
            { "CHAR_Bandit_Bomber_Servant", new PrefabGUID(-450600397) },
            { "CHAR_Bandit_Bomber_VBlood", new PrefabGUID(1896428751) },
            { "CHAR_Bandit_Deadeye", new PrefabGUID(-1030822544) },
            { "CHAR_Bandit_Deadeye_Chaosarrow_VBlood", new PrefabGUID(763273073) },
            { "CHAR_Bandit_Deadeye_Frostarrow_VBlood", new PrefabGUID(1124739990) },
            { "CHAR_Bandit_Deadeye_Servant", new PrefabGUID(-2086044081) },
            { "CHAR_Bandit_Foreman_VBlood", new PrefabGUID(2122229952) },
            { "CHAR_Bandit_GraveDigger_VBlood", new PrefabGUID(936169687) },
            { "CHAR_Bandit_Hunter", new PrefabGUID(-1301144178) },
            { "CHAR_Bandit_Hunter_Servant", new PrefabGUID(-370708253) },
            { "CHAR_Bandit_Leader_VBlood", new PrefabGUID(-175381832) },
            { "CHAR_Bandit_Leader_Wolf_Summon", new PrefabGUID(-671059374) },
            { "CHAR_Bandit_Miner_Standard", new PrefabGUID(-2039670689) },
            { "CHAR_Bandit_Miner_Standard_Servant", new PrefabGUID(1112903312) },
            { "CHAR_Bandit_Miner_VBlood", new PrefabGUID(276934707) },
            { "CHAR_Bandit_Mugger", new PrefabGUID(2057508774) },
            { "CHAR_Bandit_Mugger_Servant", new PrefabGUID(1727426580) },
            { "CHAR_Bandit_Prisoner_Villager_Female", new PrefabGUID(1069072707) },
            { "CHAR_Bandit_Prisoner_Villager_Male", new PrefabGUID(286320185) },
            { "CHAR_Bandit_Prisoner_Villager_Male_VBlood", new PrefabGUID(-1620871789) },
            { "CHAR_Bandit_Stalker", new PrefabGUID(-309264723) },
            { "CHAR_Bandit_Stalker_Servant", new PrefabGUID(1453520986) },
            { "CHAR_Bandit_Stalker_VBlood", new PrefabGUID(1106149033) },
            { "CHAR_Bandit_StoneBreaker_VBlood", new PrefabGUID(-2025101517) },
            { "CHAR_Bandit_Thief", new PrefabGUID(923140362) },
            { "CHAR_Bandit_Thief_Servant", new PrefabGUID(-872078546) },
            { "CHAR_Bandit_Thief_VBlood", new PrefabGUID(2139023341) },
            { "CHAR_Bandit_Thug", new PrefabGUID(-301730941) },
            { "CHAR_Bandit_Thug_Servant", new PrefabGUID(1466015976) },
            { "CHAR_Bandit_Tourok_VBlood", new PrefabGUID(-1659822956) },
            { "CHAR_Bandit_Trapper", new PrefabGUID(-589412777) },
            { "CHAR_Bandit_Trapper_Servant", new PrefabGUID(2112911542) },
            { "CHAR_Bandit_Wolf", new PrefabGUID(-1554428547) },
            { "CHAR_Bandit_Woodcutter_Standard", new PrefabGUID(1309418594) },
            { "CHAR_Bandit_Woodcutter_Standard_Servant", new PrefabGUID(51737727) },
            { "CHAR_BatVampire_VBlood", new PrefabGUID(1112948824) },
            { "CHAR_CopperGolem", new PrefabGUID(1107541186) },
            { "CHAR_CreatureSheep_AnimalSheep", new PrefabGUID(-1312962006) },
            { "CHAR_Critter_Bloodmaggot", new PrefabGUID(2054208006) },
            { "CHAR_Critter_Mosquito", new PrefabGUID(-2046154890) },
            { "CHAR_Critter_Rat", new PrefabGUID(-2072914343) },
            { "CHAR_Critter_Silkworm", new PrefabGUID(-1587402408) },
            { "CHAR_Critter_VerminNest_Rat", new PrefabGUID(-372256748) },
            { "CHAR_Cultist_Pyromancer", new PrefabGUID(2055824593) },
            { "CHAR_Cultist_Slicer", new PrefabGUID(1807491570) },
            { "CHAR_Cursed_Bear_Spirit", new PrefabGUID(1105583702) },
            { "CHAR_Cursed_Bear_Standard", new PrefabGUID(-559819989) },
            { "CHAR_Cursed_MonsterToad", new PrefabGUID(575918722) },
            { "CHAR_Cursed_MonsterToad_Minion", new PrefabGUID(-38041784) },
            { "CHAR_Cursed_Mosquito", new PrefabGUID(-744966291) },
            { "CHAR_Cursed_MountainBeast_SpiritDouble", new PrefabGUID(-935560085) },
            { "CHAR_Cursed_MountainBeast_VBlood", new PrefabGUID(-1936575244) },
            { "CHAR_Cursed_Nightlurker", new PrefabGUID(-2046268156) },
            { "CHAR_Cursed_ToadKing_VBlood", new PrefabGUID(-203043163) },
            { "CHAR_Cursed_ToadSpitter", new PrefabGUID(1478790879) },
            { "CHAR_Cursed_Treant_Dark", new PrefabGUID(-1708902190) },
            { "CHAR_Cursed_Witch", new PrefabGUID(-56441915) },
            { "CHAR_Cursed_Witch_VBlood", new PrefabGUID(-910296704) },
            { "CHAR_Cursed_WitheredVampire", new PrefabGUID(-279978174) },
            { "CHAR_Cursed_Wolf", new PrefabGUID(-218175217) },
            { "CHAR_Cursed_Wolf_Spirit", new PrefabGUID(407089231) },
            { "CHAR_Cursed_WormTerror", new PrefabGUID(658578725) },
            { "CHAR_Demon", new PrefabGUID(-978021978) },
            { "CHAR_Farmlands_Cow", new PrefabGUID(721166952) },
            { "CHAR_Farmlands_Farmer", new PrefabGUID(-1342764880) },
            { "CHAR_Farmlands_Farmer_Servant", new PrefabGUID(516718373) },
            { "CHAR_Farmlands_HostileVillager_Female_FryingPan", new PrefabGUID(729746981) },
            { "CHAR_Farmlands_HostileVillager_Female_Pitchfork", new PrefabGUID(1576267559) },
            { "CHAR_Farmlands_HostileVillager_Male_Club", new PrefabGUID(-164116132) },
            { "CHAR_Farmlands_HostileVillager_Male_Shovel", new PrefabGUID(-864975423) },
            { "CHAR_Farmlands_HostileVillager_Male_Torch", new PrefabGUID(-81727312) },
            { "CHAR_Farmlands_HostileVillager_Male_Unarmed", new PrefabGUID(-1353870145) },
            { "CHAR_Farmlands_HostileVillager_Werewolf", new PrefabGUID(-951976780) },
            { "CHAR_Farmlands_Hound_VBlood", new PrefabGUID(-1373413273) },
            { "CHAR_Farmlands_HoundMaster_VBlood", new PrefabGUID(-784265984) },
            { "CHAR_Farmlands_Militia_Summon", new PrefabGUID(-213868361) },
            { "CHAR_Farmlands_Nun", new PrefabGUID(-700632469) },
            { "CHAR_Farmlands_Nun_Servant", new PrefabGUID(-1788957652) },
            { "CHAR_Farmlands_Nun_VBlood", new PrefabGUID(-99012450) },
            { "CHAR_Farmlands_Pig", new PrefabGUID(-1356006948) },
            { "CHAR_Farmlands_Ram", new PrefabGUID(947731555) },
            { "CHAR_Farmlands_Sheep", new PrefabGUID(1012307512) },
            { "CHAR_Farmlands_SheepOld", new PrefabGUID(1635167941) },
            { "CHAR_Farmlands_SmallPig", new PrefabGUID(1420480270) },
            { "CHAR_Farmlands_Villager_Female", new PrefabGUID(525027204) },
            { "CHAR_Farmlands_Villager_Female_Servant", new PrefabGUID(1532829342) },
            { "CHAR_Farmlands_Villager_Female_Sister", new PrefabGUID(1772642154) },
            { "CHAR_Farmlands_Villager_Female_Sister_Servant", new PrefabGUID(-444945115) },
            { "CHAR_Farmlands_Villager_Male", new PrefabGUID(1887807944) },
            { "CHAR_Farmlands_Villager_Male_Servant", new PrefabGUID(1426964824) },
            { "CHAR_Farmlands_Woodcutter_Standard", new PrefabGUID(-893091615) },
            { "CHAR_Farmlands_Woodcutter_Standard_Servant", new PrefabGUID(-1659842473) },
            { "CHAR_Geomancer_Golem_Guardian", new PrefabGUID(-2092246077) },
            { "CHAR_Geomancer_Golem_VBlood", new PrefabGUID(-1317534496) },
            { "CHAR_Geomancer_Human_VBlood", new PrefabGUID(-1065970933) },
            { "CHAR_Ghoul_Unholy_Summon", new PrefabGUID(679136439) },
            { "CHAR_Harpy_Dasher", new PrefabGUID(-1846851895) },
            { "CHAR_Harpy_Dasher_SUMMON", new PrefabGUID(1635780151) },
            { "CHAR_Harpy_FeatherDuster", new PrefabGUID(-1407234470) },
            { "CHAR_Harpy_Matriarch_VBlood", new PrefabGUID(685266977) },
            { "CHAR_Harpy_Scratcher", new PrefabGUID(1462269123) },
            { "CHAR_Harpy_Sorceress", new PrefabGUID(1224283123) },
            { "CHAR_IronGolem", new PrefabGUID(763796308) },
            { "CHAR_Manticore_Flame_UNUSED", new PrefabGUID(-78083191) },
            { "CHAR_Manticore_HomePos", new PrefabGUID(980068444) },
            { "CHAR_Manticore_VBlood", new PrefabGUID(-393555055) },
            { "CHAR_Mantrap_Dull", new PrefabGUID(-878541676) },
            { "CHAR_Mantrap_Nest", new PrefabGUID(2016963774) },
            { "CHAR_Mantrap_PlantMother_Boss", new PrefabGUID(1407911482) },
            { "CHAR_Mantrap_PlantMother_Boss_Summon", new PrefabGUID(1430167878) },
            { "CHAR_Mantrap_Standard", new PrefabGUID(173817657) },
            { "CHAR_Militia_BellRinger", new PrefabGUID(-1670130821) },
            { "CHAR_Militia_BellRinger_Servant", new PrefabGUID(-1433235567) },
            { "CHAR_Militia_BishopOfDunley_VBlood", new PrefabGUID(-680831417) },
            { "CHAR_Militia_Bomber", new PrefabGUID(847893333) },
            { "CHAR_Militia_Bomber_Servant", new PrefabGUID(232701971) },
            { "CHAR_Militia_ConstrainingPole", new PrefabGUID(85290673) },
            { "CHAR_Militia_Crossbow", new PrefabGUID(956965183) },
            { "CHAR_Militia_Crossbow_Servant", new PrefabGUID(1481842114) },
            { "CHAR_Militia_Crossbow_Summon", new PrefabGUID(2036785949) },
            { "CHAR_Militia_Devoted", new PrefabGUID(1660801216) },
            { "CHAR_Militia_Devoted_Servant", new PrefabGUID(-823557242) },
            { "CHAR_Militia_EyeOfGod", new PrefabGUID(-1254618756) },
            { "CHAR_Militia_Guard", new PrefabGUID(1730498275) },
            { "CHAR_Militia_Guard_Servant", new PrefabGUID(-1447279513) },
            { "CHAR_Militia_Guard_Summon", new PrefabGUID(1050151632) },
            { "CHAR_Militia_Guard_VBlood", new PrefabGUID(-29797003) },
            { "CHAR_Militia_Heavy", new PrefabGUID(2005508157) },
            { "CHAR_Militia_Heavy_Servant", new PrefabGUID(-1773935659) },
            { "CHAR_Militia_Hound", new PrefabGUID(-249647316) },
            { "CHAR_Militia_Leader_VBlood", new PrefabGUID(1688478381) },
            { "CHAR_Militia_Light", new PrefabGUID(-63435588) },
            { "CHAR_Militia_Light_Servant", new PrefabGUID(169329980) },
            { "CHAR_Militia_Light_Summon", new PrefabGUID(1772451421) },
            { "CHAR_Militia_Longbowman", new PrefabGUID(203103783) },
            { "CHAR_Militia_Longbowman_LightArrow_Vblood", new PrefabGUID(850622034) },
            { "CHAR_Militia_Longbowman_Servant", new PrefabGUID(-242295780) },
            { "CHAR_Militia_Longbowman_Summon", new PrefabGUID(1083647444) },
            { "CHAR_Militia_Miner_Standard", new PrefabGUID(-1072754152) },
            { "CHAR_Militia_Miner_Standard_Servant", new PrefabGUID(-1363137425) },
            { "CHAR_Militia_Torchbearer", new PrefabGUID(37713289) },
            { "CHAR_Militia_Torchbearer_Servant", new PrefabGUID(986768339) },
            { "CHAR_Moose_VBlood", new PrefabGUID(1730711592) },
            { "CHAR_Paladin_DivineAngel", new PrefabGUID(-1737346940) },
            { "CHAR_Paladin_FallenAngel", new PrefabGUID(-76116724) },
            { "CHAR_Paladin_HomePos", new PrefabGUID(-502558061) },
            { "CHAR_Paladin_VBlood", new PrefabGUID(-740796338) },
            { "CHAR_Pixie", new PrefabGUID(1434914085) },
            { "CHAR_Plantmother", new PrefabGUID(1194309425) },
            { "CHAR_Scarecrow", new PrefabGUID(-1750347680) },
            { "CHAR_Spectral_Assassin", new PrefabGUID(-830249769) },
            { "CHAR_Spectral_Guardian", new PrefabGUID(304726480) },
            { "CHAR_Spectral_SpellSlinger", new PrefabGUID(2065149172) },
            { "CHAR_Spider_Baneling", new PrefabGUID(-764515001) },
            { "CHAR_Spider_Baneling_Summon", new PrefabGUID(-1004061470) },
            { "CHAR_Spider_Broodmother", new PrefabGUID(342127250) },
            { "CHAR_Spider_Melee", new PrefabGUID(2136899683) },
            { "CHAR_Spider_Melee_Summon", new PrefabGUID(2119230788) },
            { "CHAR_Spider_Queen_VBlood", new PrefabGUID(-548489519) },
            { "CHAR_Spider_Range", new PrefabGUID(2103131615) },
            { "CHAR_Spider_Range_Summon", new PrefabGUID(1974733695) },
            { "CHAR_Spiderling", new PrefabGUID(1078424589) },
            { "CHAR_Spiderling_Summon", new PrefabGUID(-18289884) },
            { "CHAR_Spiderling_VerminNest", new PrefabGUID(1767714956) },
            { "CHAR_StoneColossus", new PrefabGUID(-1779340970) },
            { "CHAR_StoneGolem", new PrefabGUID(-779411607) },
            { "CHAR_SUMMON_Wolf", new PrefabGUID(1825512527) },
            { "CHAR_Town_Archer", new PrefabGUID(426583055) },
            { "CHAR_Town_Archer_Servant", new PrefabGUID(-915884427) },
            { "CHAR_Town_Cardinal_VBlood", new PrefabGUID(114912615) },
            { "CHAR_Town_CardinalAide", new PrefabGUID(1745498602) },
            { "CHAR_Town_Cleric", new PrefabGUID(-1464869978) },
            { "CHAR_Town_Cleric_Servant", new PrefabGUID(1218339832) },
            { "CHAR_Town_EnchantedCross", new PrefabGUID(-1449314709) },
            { "CHAR_Town_Footman", new PrefabGUID(2128996433) },
            { "CHAR_Town_Footman_Servant", new PrefabGUID(-1719944550) },
            { "CHAR_Town_Horse", new PrefabGUID(1149585723) },
            { "CHAR_Town_Knight_2H", new PrefabGUID(-930333806) },
            { "CHAR_Town_Knight_2H_Servant", new PrefabGUID(17367048) },
            { "CHAR_Town_Knight_Shield", new PrefabGUID(794228023) },
            { "CHAR_Town_Knight_Shield_Servant", new PrefabGUID(-694328454) },
            { "CHAR_Town_Lightweaver", new PrefabGUID(1185952775) },
            { "CHAR_Town_Lightweaver_Servant", new PrefabGUID(-383158562) },
            { "CHAR_Town_Miner_Standard", new PrefabGUID(924132254) },
            { "CHAR_Town_Miner_Standard_Servant", new PrefabGUID(-1988959460) },
            { "CHAR_Town_Paladin", new PrefabGUID(1728773109) },
            { "CHAR_Town_Paladin_Servant", new PrefabGUID(1649578802) },
            { "CHAR_Town_Priest", new PrefabGUID(1406393857) },
            { "CHAR_Town_Priest_Servant", new PrefabGUID(-1728284448) },
            { "CHAR_Town_Rifleman", new PrefabGUID(1148936156) },
            { "CHAR_Town_Rifleman_Servant", new PrefabGUID(-268935837) },
            { "CHAR_Town_SmiteOrb", new PrefabGUID(1917502536) },
            { "CHAR_Town_Villager_Female", new PrefabGUID(-1224027101) },
            { "CHAR_Town_Villager_Female_Servant", new PrefabGUID(1157537604) },
            { "CHAR_Town_Villager_Male", new PrefabGUID(-2025921616) },
            { "CHAR_Town_Villager_Male_Servant", new PrefabGUID(-1786031969) },
            { "CHAR_Trader_T01", new PrefabGUID(-1168705805) },
            { "CHAR_Trader_T02", new PrefabGUID(-2096362182) },
            { "CHAR_Trader_T03", new PrefabGUID(604519200) },
            { "CHAR_Treant", new PrefabGUID(-1089337069) },
            { "CHAR_Undead_ArmoredSkeletonCrossbow_Dunley", new PrefabGUID(-861407720) },
            { "CHAR_Undead_ArmoredSkeletonCrossbow_Farbane", new PrefabGUID(-195077008) },
            { "CHAR_Undead_Banshee", new PrefabGUID(-1146194149) },
            { "CHAR_Undead_BishopOfDeath_VBlood", new PrefabGUID(577478542) },
            { "CHAR_Undead_BishopOfShadows_VBlood", new PrefabGUID(939467639) },
            { "CHAR_Undead_FeralGhoul", new PrefabGUID(1730616829) },
            { "CHAR_Undead_Ghoul_Armored_Farmlands", new PrefabGUID(2105565286) },
            { "CHAR_Undead_Ghoul_Castle_Tomb", new PrefabGUID(937597711) },
            { "CHAR_Undead_Leader", new PrefabGUID(-1365931036) },
            { "CHAR_Undead_Mage_VBlood", new PrefabGUID(295582261) },
            { "CHAR_Undead_Necromancer", new PrefabGUID(-572568236) },
            { "CHAR_Undead_Priest", new PrefabGUID(-1653554504) },
            { "CHAR_Undead_Priest_VBlood", new PrefabGUID(153390636) },
            { "CHAR_Undead_RottingGhoul", new PrefabGUID(-1722506709) },
            { "CHAR_Undead_ShadowSoldier", new PrefabGUID(678628353) },
            { "CHAR_Undead_Skeleton_Unholy_Summon", new PrefabGUID(1604500740) },
            { "CHAR_Undead_SkeletonCrossbow_Base", new PrefabGUID(597386568) },
            { "CHAR_Undead_SkeletonCrossbow_Farbane_OLD", new PrefabGUID(1250474035) },
            { "CHAR_Undead_SkeletonCrossbow_Graveyard", new PrefabGUID(1395549638) },
            { "CHAR_Undead_SkeletonMage", new PrefabGUID(-1287507270) },
            { "CHAR_Undead_SkeletonSoldier_Armored_Dunley", new PrefabGUID(952695804) },
            { "CHAR_Undead_SkeletonSoldier_Armored_Farbane", new PrefabGUID(-837329073) },
            { "CHAR_Undead_SkeletonSoldier_Base", new PrefabGUID(-603934060) },
            { "CHAR_Undead_SkeletonSoldier_Unholy_Minion", new PrefabGUID(-1779239433) },
            { "CHAR_Undead_SkeletonSoldier_Withered", new PrefabGUID(-1584807109) },
            { "CHAR_Undead_UndyingGhoul", new PrefabGUID(1640311129) },
            { "CHAR_Undead_ZealousCultist_Ghost", new PrefabGUID(128488545) },
            { "CHAR_Undead_ZealousCultist_VBlood", new PrefabGUID(-1208888966) },
            { "CHAR_Unholy_Baneling", new PrefabGUID(-1823987835) },
            { "CHAR_Unholy_FallenAngel", new PrefabGUID(-1928607398) },
            { "CHAR_Unholy_MosquitoBomb", new PrefabGUID(30054246) },
            { "CHAR_Vampire_Withered", new PrefabGUID(-1117581429) },
            { "CHAR_Vampire_WitheredBatMinion", new PrefabGUID(-989999571) },
            { "CHAR_VampireMale", new PrefabGUID(38526109) },
            { "CHAR_Vermin_DireRat_VBlood", new PrefabGUID(-2039908510) },
            { "CHAR_Vermin_GiantRat", new PrefabGUID(-1722278689) },
            { "CHAR_VHunter_Jade_VBlood", new PrefabGUID(-1968372384) },
            { "CHAR_VHunter_Leader_VBlood", new PrefabGUID(-1449631170) },
            { "CHAR_Villager_Tailor_VBlood", new PrefabGUID(-1942352521) },
            { "CHAR_Wendigo_VBlood", new PrefabGUID(24378719) },
            { "CHAR_Werewolf", new PrefabGUID(-1554760905) },
            { "CHAR_WerewolfChieftain_Human", new PrefabGUID(-1505705712) },
            { "CHAR_WerewolfChieftain_ShadowClone", new PrefabGUID(-1699898875) },
            { "CHAR_WerewolfChieftain_VBlood", new PrefabGUID(-1007062401) },
            { "CHAR_Wildlife_Bear_Dire_Vblood", new PrefabGUID(-1391546313) },
            { "CHAR_Wildlife_Bear_Standard", new PrefabGUID(1043643344) },
            { "CHAR_Wildlife_Deer", new PrefabGUID(1897056612) },
            { "CHAR_Wildlife_Moose", new PrefabGUID(-831097925) },
            { "CHAR_Wildlife_Poloma_VBlood", new PrefabGUID(-484556888) },
            { "CHAR_Wildlife_Wolf", new PrefabGUID(-1418430647) },
            { "CHAR_Wildlife_Wolf_VBlood", new PrefabGUID(-1905691330) },
            { "CHAR_Winter_Bear_Standard", new PrefabGUID(2041915372) },
            { "CHAR_Winter_Moose", new PrefabGUID(-1211580130) },
            { "CHAR_Winter_Wolf", new PrefabGUID(134039094) },
            { "CHAR_Winter_Wolf_OLD", new PrefabGUID(1000142548) },
            { "CHAR_Winter_Yeti_VBlood", new PrefabGUID(-1347412392) }
        };

        public static Dictionary<PrefabGUID, int> faction_heatvalue = new ()
        {
            { new PrefabGUID(1094603131), 10 }, //-- Citizen & Soldier
            { new PrefabGUID(-413163549), 1 },  //-- Bandit
            { new PrefabGUID(1057375699), 5 },  //-- Militia
            { new PrefabGUID(2395673), 25 }     //-- Church of Lumination
        };

        public static class Buff
        {
            public static PrefabGUID EquipBuff = new PrefabGUID(343359674);
            public static PrefabGUID WolfStygian = new PrefabGUID(-1158884666);
            public static PrefabGUID WolfNormal = new PrefabGUID(-351718282);
            public static PrefabGUID BatForm = new PrefabGUID(1205505492);
            public static PrefabGUID NormalForm = new PrefabGUID(1352541204);
            public static PrefabGUID RatForm = new PrefabGUID(902394170);

            public static PrefabGUID DownedBuff = new PrefabGUID(-1992158531);
            public static PrefabGUID BloodSight = new PrefabGUID(1199823151);

            public static PrefabGUID InCombat = new PrefabGUID(581443919);
            public static PrefabGUID InCombat_PvP = new PrefabGUID(697095869);
            public static PrefabGUID OutofCombat = new PrefabGUID(897325455);
            public static PrefabGUID BloodMoon = new PrefabGUID(-560523291);

            public static PrefabGUID Severe_GarlicDebuff = new PrefabGUID(1582196539);          //-- Using this for PvP Punishment debuff
            public static PrefabGUID General_GarlicDebuff = new PrefabGUID(-1701323826);

            public static PrefabGUID Buff_VBlood_Perk_Moose = new PrefabGUID(-1464851863);      //-- Using this for commands & mastery buff
            public static PrefabGUID PerkMoose = new PrefabGUID(-1464851863);

            public static PrefabGUID SiegeGolem_T01 = new PrefabGUID(-148535031);
            public static PrefabGUID SiegeGolem_T02 = new PrefabGUID(914043867);

            //-- Coffin Buff
            public static PrefabGUID AB_Interact_GetInside_Owner_Buff_Stone = new PrefabGUID(569692162); //-- Inside Stone Coffin
            public static PrefabGUID AB_Interact_GetInside_Owner_Buff_Base = new PrefabGUID(381160212); //-- Inside Base/Wooden Coffin

            public static PrefabGUID AB_ExitCoffin_Travel_Phase_Stone = new PrefabGUID(-162820429);
            public static PrefabGUID AB_ExitCoffin_Travel_Phase_Base = new PrefabGUID(-997204628);
            public static PrefabGUID AB_Interact_TombCoffinSpawn_Travel = new PrefabGUID(722466953);

            public static PrefabGUID AB_Interact_WaypointSpawn_Travel = new PrefabGUID(-66432447);
            public static PrefabGUID AB_Interact_WoodenCoffinSpawn_Travel = new PrefabGUID(-1705977973);
            public static PrefabGUID AB_Interact_StoneCoffinSpawn_Travel = new PrefabGUID(-1276482574);

            //-- LevelUp Buff
            public static PrefabGUID LevelUp_Buff = new PrefabGUID(-1133938228);

            //-- Nice Effect...
            public static PrefabGUID AB_Undead_BishopOfShadows_ShadowSoldier_Minion_Buff = new PrefabGUID(450215391);   //-- Impair cast & movement

            //-- The Only Potential Buff we can use for hostile mark
            //Buff_Cultist_BloodFrenzy_Buff - PrefabGuid(-106492795)

            //-- Relic Buff
            //[-238197495]          AB_Interact_UseRelic_Manticore_Buff
            //[-1161197991]		    AB_Interact_UseRelic_Paladin_Buff
            //[-1703886455]		    AB_Interact_UseRelic_Behemoth_Buff

            //-- Fun
            public static PrefabGUID HolyNuke = new PrefabGUID(-1807398295);
            public static PrefabGUID AB_Manticore_Flame_Buff_UNUSED = new PrefabGUID(1502566434); //-- And Dangerous~
            public static PrefabGUID Pig_Transform_Debuff = new PrefabGUID(1356064917);

            //[505018388]		    AB_Nightlurker_Rush_Buff


            //-- Possible Buff use
            public static PrefabGUID EquipBuff_Chest_Base = new PrefabGUID(1872694456);         //-- Hmm... not sure what to do with this right now...
            public static PrefabGUID Buff_VBlood_Perk_ProgTest = new PrefabGUID(1614409699);    //-- What does this do??
            public static PrefabGUID AB_BloodBuff_VBlood_0 = new PrefabGUID(20081801);          //-- Does it do anything negative...? How can i check for this, seems like it's a total blank o.o

            //-- Just putting it here for no reason at all...
            //public static PrefabGUID Admin_Observe_Ghost_Buff = new PrefabGUID(77473184);       //-- Not sure what to do with it
            //[1258181143]		    AB_Undead_Priest_Elite_RaiseHorde_Minion_Buff
            //[1502566434]		    AB_Manticore_Flame_Buff_UNUSED
            //[-1133938228]		    AB_Town_Priest_HealBomb_Buff        //-- Good Heal Effect
            //[-225445080]          AB_Nun_AoE_ApplyLight_Buff          //-- Low Healing Effect
            //[-2115732274]		    AB_Manticore_Flying_Buff

            //[-474441982]		    Buff_General_Teleport_Travel        //-- Usefull for imprissoning someone?


        }
    }
}
