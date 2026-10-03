using HarmonyLib;
using OriCoop;
using UnityEngine;
using WWClient;

namespace MP_Client.Patches
{
    public class AbilityPatches
    {
        public static bool IsSyncing = false;

        [HarmonyPatch(typeof(PlayerAbilities), "SetAbility")]
        public class SetAbilityPatch
        {
            public static void Postfix(AbilityType ability, bool value)
            {
                if (IsSyncing || !value) return;

                if (MPGameManager.Instance != null && MPGameManager.Instance.Config.ShareAbilities)
                {
                    if (MPGameManager.Instance.Config.ShareStoryOnly && !IsStoryAbility(ability))
                    {
                        return;
                    }

                    MPGameManager.Instance.SendAbilityUnlocked(ability);
                    WWClient.Logger.Info("ABILITY_SYNC", $"Habilidade desbloqueada localmente: {ability}, enviando para amigos!");
                }
            }
        }

        public static bool IsStoryAbility(AbilityType type)
        {
            switch (type)
            {
                case AbilityType.Bash:
                case AbilityType.ChargeFlame:
                case AbilityType.WallJump:
                case AbilityType.Stomp:
                case AbilityType.DoubleJump:
                case AbilityType.ChargeJump:
                case AbilityType.Magnet:
                case AbilityType.Climb:
                case AbilityType.Glide:
                case AbilityType.SpiritFlame:
                case AbilityType.WaterBreath:
                case AbilityType.Dash:
                case AbilityType.Grenade:
                case AbilityType.ChargeDash:
                case AbilityType.AirDash:
                    return true;
                default:
                    return false;
            }
        }
    }
}
