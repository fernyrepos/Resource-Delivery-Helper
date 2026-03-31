using HarmonyLib;
using RimWorld;
using Verse;

namespace ResourceDeliveryHelper
{
    [HarmonyPatch(typeof(ThingOwner), nameof(ThingOwner.NotifyRemoved))]
    public static class ThingOwner_NotifyRemoved_Patch
    {
        public static void Postfix(ThingOwner __instance)
        {
            if (__instance.Owner is Frame frame)
            {
                ResourceDeliveryCache.Clear(frame);
            }
        }
    }
}
