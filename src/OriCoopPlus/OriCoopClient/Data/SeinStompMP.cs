using System.Collections.Generic;
using Game;
using UnityEngine;

namespace MP_Client.Data
{
    public class SeinStompMP : MonoBehaviour
    {
        public OriMPPlayer Player;
        public SeinStomp Stomp;

        private void Start()
        {
            Player = GetComponent<OriMPPlayer>();
            if (Characters.Sein != null && Characters.Sein.Abilities != null)
            {
                Stomp = Characters.Sein.Abilities.Stomp;
            }
        }

        public void DoStompBlastEffect()
        {
            if (Stomp == null && Characters.Sein != null && Characters.Sein.Abilities != null)
            {
                Stomp = Characters.Sein.Abilities.Stomp;
            }

            if (Stomp != null && Stomp.StompLandEffect != null)
            {
                InstantiateUtility.Instantiate(Stomp.StompLandEffect, base.transform.position, Quaternion.identity);
                if (Stomp.StompLandSound != null)
                {
                    Stomp.StompLandSound.Play();
                }
            }
        }

        public void Attack()
        {
            DoStompBlastEffect();
            DoBlastRadius();
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
            if (attackable == null)
            {
                return false;
            }
            if (!attackable.CanBeStomped())
            {
                return false;
            }
            float num = Vector3.Distance(attackable.Position, base.transform.position);
            return num <= 5f;
        }

        public void DoBlastRadius()
        {
            if (Stomp == null) return;

            List<IAttackable> allAttackables = GetAllAttackables();
            for (int i = 0; i < allAttackables.Count; i++)
            {
                IAttackable attackable = allAttackables[i];
                if (!InstantiateUtility.IsDestroyed(attackable as Component))
                {
                    Vector3 vector = attackable.Position - base.transform.position;
                    float magnitude = vector.magnitude;
                    if (magnitude < Stomp.StompBlashRadius)
                    {
                        Vector3 normalized = (vector.normalized + Vector3.up * 2f).normalized;
                        GameObject gameObject = ((Component)attackable).gameObject;
                        float stompDamage = Stomp.StompDamage;
                        Damage damage = new Damage(stompDamage, normalized * 3f, attackable.Position, DamageType.StompBlast, gameObject);
                        damage.DealToComponents(gameObject);
                    }
                }
            }
        }
    }
}
