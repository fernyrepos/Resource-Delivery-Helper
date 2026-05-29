using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using System;

namespace ResourceDeliveryHelper
{
	[HotSwappable]
	[HarmonyPatch(typeof(ThingOverlays), nameof(ThingOverlays.ThingOverlaysOnGUI))]
	public static class ThingOverlays_ThingOverlaysOnGUI_Patch
	{
		private static readonly List<Thing> drawList = new List<Thing>();
		internal static int cachedMouseCellHash;
		private static GUIStyle cachedLabelStyle;
		private static int cachedLabelStyleSize = -1;

		public static void Postfix()
		{
			var map = Find.CurrentMap;
			var mouseCell = UI.MouseCell();
			var mouseCellHash = HashCode.Combine(mouseCell.GetHashCode(), map.uniqueID);
			var currentViewRect = Find.CameraDriver.CurrentViewRect;
			var currentCameraHash = HashCode.Combine(
				Find.CameraDriver.rootPos.GetHashCode(),
				Find.CameraDriver.rootSize.GetHashCode());

			if (mouseCellHash != cachedMouseCellHash)
			{
				cachedMouseCellHash = mouseCellHash;
				RebuildDrawList(map, mouseCell);
			}

			for (int i = 0; i < drawList.Count; i++)
			{
				var thing = drawList[i];
				if (!currentViewRect.Contains(thing.Position))
					continue;
				var req = ResourceDeliveryCache.Get(thing);
				if (req.cachedCameraPositionHash != currentCameraHash)
				{
					ResourceDeliveryCache.CalculateRects(ref req, thing);
					req.cachedCameraPositionHash = currentCameraHash;
					ResourceDeliveryCache.Update(thing, req);
				}
				DrawRequirement(req);
			}
		}

		private static void RebuildDrawList(Map map, IntVec3 mouseCell)
		{
			drawList.Clear();
			var displayRadius = ResourceDeliveryHelperMod.Settings.displayRadius;
			bool filterByRadius = displayRadius <= ResourceDeliverySettings.AllTilesThreshold;
			int radius = Mathf.RoundToInt(displayRadius);
			var blueprints = map.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint);
			var frames = map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame);
			AddConstructibles(blueprints, map, mouseCell, filterByRadius, radius);
			AddConstructibles(frames, map, mouseCell, filterByRadius, radius);
		}

		private static void AddConstructibles(List<Thing> things, Map map, IntVec3 mouseCell, bool filterByRadius, int radius)
		{
			for (int i = 0; i < things.Count; i++)
			{
				var t = things[i];
				if (t is Blueprint_Install)
					continue;
				if (map.fogGrid.IsFogged(t.Position))
					continue;
				if (t is Frame frame && frame.workDone > 0f)
					continue;
				if (filterByRadius && !t.OccupiedRect().ExpandedBy(radius - 1).Contains(mouseCell))
					continue;
				drawList.Add(t);
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

				var fontSize = Mathf.RoundToInt(UI.CurUICellSize() / 4f);
				if (cachedLabelStyle == null || cachedLabelStyleSize != fontSize)
				{
					cachedLabelStyle = new GUIStyle(Text.CurFontStyle) { fontSize = fontSize };
					cachedLabelStyleSize = fontSize;
				}

				Text.Font = GameFont.Tiny;
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.Label(req.cachedLabelRect, req.cachedCountString, cachedLabelStyle);
			}
		}
	}
}
