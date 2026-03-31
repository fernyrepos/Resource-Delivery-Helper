using HarmonyLib;
using UnityEngine;
using Verse;

namespace ResourceDeliveryHelper
{
    public class ResourceDeliveryHelperMod : Mod
    {
        public static ResourceDeliverySettings Settings;

        public ResourceDeliveryHelperMod(ModContentPack pack) : base(pack)
        {
            Settings = GetSettings<ResourceDeliverySettings>();
            new Harmony("ferny.ResourceDeliveryHelper").PatchAll();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() => Content.Name;
    }
}
