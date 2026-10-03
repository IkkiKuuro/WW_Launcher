using System.Collections.Generic;
using Game;
using UnityEngine;

namespace MP_Client.Data
{
    public class SeinSpiritMP : MonoBehaviour
    {
        public OriMPPlayer Player;

        private void Start()
        {
            Player = GetComponent<OriMPPlayer>();
        }

        public void Attack()
        {
            List<ISpiritFlameAttackable> attackables = GetAttackables();
            if (attackables.Count > 0 && Characters.Sein != null)
            {
                SpiritFlame currentSpiritFlame = Characters.Sein.Abilities.StandardSpiritFlame.CurrentSpiritFlame;
                ThrowSpiritFlames(currentSpiritFlame, attackables);
            }
        }

        public void ThrowSpiritFlames(SpiritFlame spiritFlame, List<ISpiritFlameAttackable> ClosestAttackables)
        {
            if (spiritFlame == null || ClosestAttackables == null) return;

            for (int i = 0; i < ClosestAttackables.Count; i++)
            {
                ISpiritFlameAttackable spiritFlameAttackable = ClosestAttackables[i];
                if (spiritFlameAttackable == null) continue;

                Vector3 startPos = Characters.Ori != null ? Characters.Ori.transform.position : base.transform.position;
                GameObject gameObject = (GameObject)InstantiateUtility.Instantiate(spiritFlame.Projectile, startPos, Quaternion.identity);
                SpiritFlameProjectile component = gameObject.GetComponent<SpiritFlameProjectile>();
                component.AttackableTargetTransform = ((Component)spiritFlameAttackable).transform;
                component.SpiritFlame = spiritFlame;
                component.Sein = null;
                component.StartPosition = base.transform.position;
                component.Damage = spiritFlame.Damage;
                component.StartTarget = base.transform;
                component.ImpactOffset = spiritFlameAttackable.GenerateSpiritFlameProjectileOffset(base.transform.position);
                component.DoImpact = true;
                component.HasARealTarget = true;
            }
        }

        private List<ISpiritFlameAttackable> GetAttackables()
        {
            List<ISpiritFlameAttackable> list = new List<ISpiritFlameAttackable>();
            List<ISpiritFlameAttackable> list2 = new List<ISpiritFlameAttackable>();
            List<IAttackable> allAttackables = GetAllAttackables();
            if (allAttackables.Count == 0)
            {
                return list;
            }

            Vector3 position = base.transform.position;
            for (int i = 0; i < allAttackables.Count; i++)
            {
                list2.Add(allAttackables[i] as ISpiritFlameAttackable);
            }

            for (int j = 0; (float)j < 5f; j++)
            {
                ISpiritFlameAttackable spiritFlameAttackable = null;
                float num = float.MaxValue;
                int num2 = int.MinValue;
                for (int k = 0; k < list2.Count; k++)
                {
                    ISpiritFlameAttackable spiritFlameAttackable2 = list2[k];
                    IAttackable attackable = spiritFlameAttackable2 as IAttackable;
                    float num3 = Vector3.Distance(attackable.Position, position);
                    int spiritFlamePriority = spiritFlameAttackable2.SpiritFlamePriority;
                    if (spiritFlamePriority > num2 || (num3 <= num && spiritFlamePriority == num2))
                    {
                        num = num3;
                        num2 = spiritFlamePriority;
                        spiritFlameAttackable = spiritFlameAttackable2;
                    }
                }
                if (spiritFlameAttackable == null)
                {
                    break;
                }
                list2.Remove(spiritFlameAttackable);
                list.Add(spiritFlameAttackable);
            }
            return list;
        }

        private List<IAttackable> GetAllAttackables()
        {
            List<IAttackable> list = new List<IAttackable>();
            foreach (IAttackable attackable in Targets.Attackables)
            {
                if (IsShootableTarget(attackable))
                {
                    list.Add(attackable);
                }
            }
            return list;
        }

        public bool IsShootableTarget(IAttackable attackable)
        {
            if (!(attackable is ISpiritFlameAttackable))
            {
                return false;
            }
            if (!attackable.CanBeSpiritFlamed())
            {
                return false;
            }
            float num = Vector3.Distance(attackable.Position, base.transform.position);
            return num <= 10f;
        }
    }
}
