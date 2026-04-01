using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ResourceDeliveryHelper
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }

    [HotSwappable]
    public static class ResourceDeliveryCache
    {
        private static readonly Dictionary<int, CachedRequirement> cache = new Dictionary<int, CachedRequirement>();

        private static int CameraHash()
        {
            var d = Find.CameraDriver;
            return HashCode.Combine(d.rootPos.GetHashCode(), d.rootSize.GetHashCode());
        }

        public struct CachedRequirement
        {
            public ThingDef resource;
            public int count;
            public bool isComplete;
            public int cachedCameraPositionHash;
            public string cachedCountString;
            public Rect cachedRect;
            public Rect cachedLabelRect;
            public Texture2D cachedIconTexture;
            public bool cachedIsVisible;
            public int cachedMouseCellHash;
            public bool cachedRadiusOnly;
        }

        public static CachedRequirement Get(Thing thing)
        {
            if (cache.TryGetValue(thing.thingIDNumber, out var req))
            {
                var cameraHash = CameraHash();
                if (req.cachedCameraPositionHash == cameraHash)
                {
                    return req;
                }
            }

            req = Calculate(thing);
            cache[thing.thingIDNumber] = req;
            return req;
        }

        public static void Clear(Thing thing)
        {
            cache.Remove(thing.thingIDNumber);
        }

        public static void Update(Thing thing, CachedRequirement req)
        {
            cache[thing.thingIDNumber] = req;
        }

        public static bool ShouldDisplay(Thing thing, Map map)
        {
            if (!cache.TryGetValue(thing.thingIDNumber, out var req))
            {
                req = Calculate(thing);
                cache[thing.thingIDNumber] = req;
            }

            var mouseCell = UI.MouseCell();
            var mouseCellHash = mouseCell.GetHashCode();
            var radiusOnly = ResourceDeliveryHelperMod.Settings.radiusOnly;

            if (req.cachedMouseCellHash == mouseCellHash && req.cachedRadiusOnly == radiusOnly)
            {
                return req.cachedIsVisible;
            }
            bool isVisible;
            if (radiusOnly)
            {
                isVisible = mouseCell.InBounds(map) && mouseCell.DistanceTo(thing.Position) <= ResourceDeliverySettings.DisplayRadius;
            }
            else
            {
                isVisible = thing.OccupiedRect().Contains(mouseCell);
            }

            req.cachedIsVisible = isVisible;
            req.cachedMouseCellHash = mouseCellHash;
            req.cachedRadiusOnly = radiusOnly;
            cache[thing.thingIDNumber] = req;

            return isVisible;
        }

        public static void CalculateRects(ref CachedRequirement req, Thing thing)
        {
            var drawPos = GenMapUI.LabelDrawPosFor(thing, 0);
            var scale = Prefs.UIScale;
            req.cachedRect = new Rect(drawPos.x - 14f * scale, drawPos.y - 14f * scale, 28f * scale, 28f * scale);
            req.cachedLabelRect = new Rect(req.cachedRect.xMax - 4f * scale, req.cachedRect.yMax - 12f * scale, 30f * scale, 16f * scale);
        }

        private static CachedRequirement Calculate(Thing thing)
        {
            var req = new CachedRequirement
            {
                cachedCameraPositionHash = CameraHash(),
                cachedCountString = string.Empty
            };
            CalculateRects(ref req, thing);
            if (thing is not IConstructible constructible) return req;

            var costs = constructible.TotalMaterialCost();
            if (costs.NullOrEmpty())
            {
                req.isComplete = true;
                return req;
            }

            var frame = thing as Frame;
            foreach (var cost in costs)
            {
                int countNeeded = cost.count;
                if (frame != null)
                {
                    countNeeded -= frame.resourceContainer.TotalStackCountOfDef(cost.thingDef);
                }

                if (countNeeded > 0)
                {
                    req.resource = cost.thingDef;
                    req.count = countNeeded;
                    req.cachedCountString = countNeeded.ToString();
                    req.cachedIconTexture = Widgets.GetIconFor(req.resource);
                    return req;
                }
            }

            req.isComplete = true;
            return req;
        }
    }
}
