using UnityEngine;
using Verse;

namespace ResourceDeliveryHelper
{
    public class ResourceDeliverySettings : ModSettings
    {
        public bool radiusOnly;
        public const float DisplayRadius = 5f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref radiusOnly, "radiusOnly");
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            var list = new Listing_Standard();
            list.Begin(inRect);
            list.CheckboxLabeled("RDH.RadiusOnly".Translate(), ref radiusOnly, "RDH.RadiusOnlyDesc".Translate());
            list.End();
        }
    }
}
