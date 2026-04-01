using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace ResourceDeliveryHelper
{
	[HarmonyPatch(typeof(ThingOverlays), nameof(ThingOverlays.ThingOverlaysOnGUI))]
	public static class ThingOverlays_ThingOverlaysOnGUI_Patch
	{
		public static void Postfix()
		{
			var map = Find.CurrentMap;
			var constructibles = map.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint).Concat(map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame));
			if (constructibles.Any())
			{
				var currentViewRect = Find.CameraDriver.CurrentViewRect;
				var currentCameraHash = HashCode.Combine(Find.CameraDriver.rootPos.GetHashCode(), Find.CameraDriver.rootSize.GetHashCode());
				foreach (var constructible in constructibles)
				{
					TryDisplayOverlay(constructible, currentViewRect, map, currentCameraHash);
				}
			}
		}

		private static void TryDisplayOverlay(Thing thing, CellRect currentViewRect, Map map, int currentCameraHash)
		{
			if (!currentViewRect.Contains(thing.Position) || map.fogGrid.IsFogged(thing.Position))
			{
				return;
			}

			if (!ResourceDeliveryCache.ShouldDisplay(thing, map))
			{
				return;
			}

			var req = ResourceDeliveryCache.Get(thing);
			UpdateCachedRectsIfCameraMoved(req, currentCameraHash, thing);
			DrawRequirement(req);
		}

		private static void UpdateCachedRectsIfCameraMoved(ResourceDeliveryCache.CachedRequirement req, int currentCameraHash, Thing thing)
		{
			if (req.cachedCameraPositionHash != currentCameraHash)
			{
				ResourceDeliveryCache.CalculateRects(ref req, thing);
				req.cachedCameraPositionHash = currentCameraHash;
				ResourceDeliveryCache.Update(thing, req);
			}
		}

		private static void DrawRequirement(ResourceDeliveryCache.CachedRequirement req)
		{
			if (req.isComplete)
			{
				GUI.DrawTexture(req.cachedRect, Widgets.CheckboxOnTex);
			}
			else if (req.resource != null)
			{
				GUI.DrawTexture(req.cachedRect, req.cachedIconTexture);
				Text.Font = GameFont.Tiny;
				Text.Anchor = TextAnchor.UpperLeft;
				Widgets.Label(req.cachedLabelRect, req.cachedCountString);
			}
		}
	}
}
