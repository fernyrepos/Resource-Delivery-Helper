using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ResourceDeliveryHelper
{
	[HotSwappable]
	[HarmonyPatch(typeof(ThingOverlays), nameof(ThingOverlays.ThingOverlaysOnGUI))]
	public static class ThingOverlays_ThingOverlaysOnGUI_Patch
	{
		public static void Postfix()
		{
			var map = Find.CurrentMap;
			var constructibles = map.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint).Where(t => !(t is Blueprint_Install)).Concat(map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame));
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
				var color = GUI.color;
				GUI.color = new Color(1f, 1f, 1f, 0.25f);
				GUI.DrawTexture(req.cachedRect, Widgets.CheckboxOnTex);
				GUI.color = color;
			}
			else if (req.resource != null)
			{
				var color = GUI.color;
				GUI.color = req.resource.uiIconColor;
				GUI.DrawTexture(req.cachedRect, req.cachedIconTexture);
				GUI.color = color;

				Text.Font = GameFont.Tiny;
				Text.Anchor = TextAnchor.UpperLeft;
				var scaledStyle = new GUIStyle(Text.CurFontStyle);
				scaledStyle.fontSize = Mathf.RoundToInt(UI.CurUICellSize() / 4f);
				GUI.Label(req.cachedLabelRect, req.cachedCountString, scaledStyle);
			}
		}
	}
}
