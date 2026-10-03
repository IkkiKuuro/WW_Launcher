using System.Collections.Generic;
using Game;
using UnityEngine;

namespace MP_Client.Data
{
    public class SeinPickupMPProcessor : MonoBehaviour
    {
        private SeinCharacter Sein;
        private SeinPickupProcessor PickupProcessor;
        private OriMPPlayer Player;

        private void Start()
        {
            Player = GetComponentInParent<OriMPPlayer>();
            Sein = Characters.Sein;
            PickupProcessor = Object.FindObjectOfType<SeinPickupProcessor>();
            if (Player.LocalOri)
            {
                InvokeRepeating("CheckItems", 0.1f, 0.1f);
            }
        }

        private void CheckItems()
        {
            if (!Player.LocalOri || PickupProcessor == null)
            {
                return;
            }

            PickupBase[] array = Object.FindObjectsOfType<PickupBase>();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == null || array[i].gameObject == null || array[i].IsCollected)
                {
                    continue;
                }

                if (MPGameManager.Instance == null) continue;

                foreach (KeyValuePair<int, OriMPPlayer> player in MPGameManager.Instance.Players)
                {
                    if (player.Value == null || Vector3.Distance(array[i].gameObject.transform.position, player.Value.gameObject.transform.position) > 2.5f)
                    {
                        continue;
                    }

                    KeystonePickup keystone = array[i].gameObject.GetComponent<KeystonePickup>();
                    if (keystone != null)
                    {
                        PickupProcessor.OnCollectKeystonePickup(keystone);
                        continue;
                    }

                    SkillPointPickup skillPoint = array[i].gameObject.GetComponent<SkillPointPickup>();
                    if (skillPoint != null)
                    {
                        PickupProcessor.OnCollectSkillPointPickup(skillPoint);
                        continue;
                    }

                    RestoreHealthPickup health = array[i].gameObject.GetComponent<RestoreHealthPickup>();
                    if (health != null)
                    {
                        PickupProcessor.OnCollectRestoreHealthPickup(health);
                        continue;
                    }

                    MaxHealthContainerPickup maxHealth = array[i].gameObject.GetComponent<MaxHealthContainerPickup>();
                    if (maxHealth != null)
                    {
                        PickupProcessor.OnCollectMaxHealthContainerPickup(maxHealth);
                        continue;
                    }

                    MaxEnergyContainerPickup maxEnergy = array[i].gameObject.GetComponent<MaxEnergyContainerPickup>();
                    if (maxEnergy != null)
                    {
                        PickupProcessor.OnCollectMaxEnergyContainerPickup(maxEnergy);
                        continue;
                    }

                    MapStonePickup mapStone = array[i].gameObject.GetComponent<MapStonePickup>();
                    if (mapStone != null)
                    {
                        PickupProcessor.OnCollectMapStonePickup(mapStone);
                        continue;
                    }

                    ExpOrbPickup exp = array[i].gameObject.GetComponent<ExpOrbPickup>();
                    if (exp != null)
                    {
                        PickupProcessor.OnCollectExpOrbPickup(exp);
                        continue;
                    }

                    EnergyOrbPickup energy = array[i].gameObject.GetComponent<EnergyOrbPickup>();
                    if (energy != null)
                    {
                        PickupProcessor.OnCollectEnergyOrbPickup(energy);
                    }
                }
            }
        }
    }
}
