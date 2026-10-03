using HarmonyLib;
using MP_Client.Sync;

namespace MP_Client.Patches
{
    public class WorldPatches
    {
        [HarmonyPatch(typeof(Lever), "OnPushLeverLeft")]
        public class LeverLeftPatch
        {
            public static void Postfix(Lever __instance)
            {
                WorldSyncManager.OnLocalLeverPushed(__instance, Lever.LeverDirections.Left);
            }
        }

        [HarmonyPatch(typeof(Lever), "OnPushLeverRight")]
        public class LeverRightPatch
        {
            public static void Postfix(Lever __instance)
            {
                WorldSyncManager.OnLocalLeverPushed(__instance, Lever.LeverDirections.Right);
            }
        }

        [HarmonyPatch(typeof(Lever), "OnPushLeverMiddle")]
        public class LeverMiddlePatch
        {
            public static void Postfix(Lever __instance)
            {
                WorldSyncManager.OnLocalLeverPushed(__instance, Lever.LeverDirections.Middle);
            }
        }

        [HarmonyPatch(typeof(DoorWithSlots), "FixedUpdate")]
        public class DoorOpenedPatch
        {
            public static void Postfix(DoorWithSlots __instance)
            {
                if (__instance.CurrentState == DoorWithSlots.State.Opened)
                {
                    WorldSyncManager.OnLocalDoorOpened(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(SetWorldEventAction), "Perform")]
        public class WorldEventPatch
        {
            public static void Postfix(SetWorldEventAction __instance)
            {
                if (__instance != null && __instance.WorldEvents != null && __instance.WorldEvents.MoonGuid != null)
                {
                    WorldSyncManager.OnLocalWorldEventChanged(__instance.WorldEvents.MoonGuid, __instance.State);
                }
            }
        }
    }
}
