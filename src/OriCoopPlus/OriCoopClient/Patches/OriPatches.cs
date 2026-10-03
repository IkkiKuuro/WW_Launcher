using System.Collections.Generic;
using HarmonyLib;
using MP_Client.Sync;
using OriCoop;
using UnityEngine;
using WWClient.API;
using WWClient.Network;

namespace MP_Client.Patches
{
    public class OriPatches
    {
        [HarmonyPatch(typeof(TextureAnimationWithTransitions), "GetTransition")]
        public class CharacterAnimationSystemPatch
        {
            public static void Prefix(TextureAnimationWithTransitions __instance)
            {
                if (MPGameManager.Instance != null)
                {
                    MPGameManager.Instance.AddAnimation(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(SpiritFlameProjectile), "Start")]
        public class SpiritFlameProjectilePatch
        {
            public static void Prefix(SpiritFlameProjectile __instance)
            {
                if (__instance.Sein != null && MPGameManager.Instance != null)
                {
                    MPGameManager.Instance.SendSkillUsed(CoopSkillType.Spirit);
                }
            }
        }

        [HarmonyPatch(typeof(EntityController), "Awake")]
        public class EntityControllerPatch
        {
            public static void Prefix(EntityController __instance)
            {
                if (!Client.NetworkVars.TryGetValue("ES", out var value) || !value.Bool ||
                    __instance.transform.FindComponentUpwards<Entity>()?.gameObject.GetComponent<EntitySync>() != null)
                {
                    return;
                }

                foreach (KeyValuePair<string, GameObject> item in __instance.gameObject.GetObjectTree())
                {
                    if (item.Key.Contains("spriteMirror/sprite"))
                    {
                        MPGameManager.RegisterEntity(__instance, item.Value);
                        break;
                    }
                }
            }
        }
    }
}
