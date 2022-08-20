using ProjectM;
using RPGMods.Utils;
using Unity.Entities;

namespace RPGMods.Systems
{
    public static class SiegeSystem
    {
        public static ModifyUnitStatBuff_DOTS GolemPDef = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.PhysicalResistance,
            Value = 0.5f,
            ModificationType = ModificationType.Set,
            Id = ModificationId.NewId(0)
        };

        public static ModifyUnitStatBuff_DOTS GolemSDef = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.SpellResistance,
            Value = 0.5f,
            ModificationType = ModificationType.Set,
            Id = ModificationId.NewId(0)
        };

        public static bool isSiegeBuff = false;

        private static EntityManager em = Plugin.Server.EntityManager;

        public static void BuffReceiver(Entity BuffEntity, PrefabGUID GUID)
        {
            if (GUID.Equals(Database.Buff.SiegeGolem_T01) || GUID.Equals(Database.Buff.SiegeGolem_T02))
            {
                var Buffer = em.GetBuffer<ModifyUnitStatBuff_DOTS>(BuffEntity);
                Buffer.Add(GolemPDef);
                Buffer.Add(GolemSDef);
            }
        }
    }
}
