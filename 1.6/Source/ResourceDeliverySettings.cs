using UnityEngine;
using Verse;

namespace ResourceDeliveryHelper
{
    public class ResourceDeliverySettings : ModSettings
    {
        public float displayRadius = 1f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref displayRadius, "displayRadius", 1f);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            var list = new Listing_Standard();
            list.Begin(inRect);
            string radiusLabel = displayRadius > 10f ? "RDH.AllTiles".Translate() : "RDH.DisplayRadius".Translate(displayRadius.ToString("F0"));
            list.Label(radiusLabel);
            displayRadius = list.Slider(displayRadius, 1f, 11f);
            list.End();
        }
    }
}
