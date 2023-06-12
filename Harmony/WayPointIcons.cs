using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using UnityEngine;

#pragma warning disable IDE0051 // Remove unused private members

public class WayPointIcons : IModApi
{
    public void InitMod(Mod mod)
    {
        Log.Out("OCB Harmony Patch: " + GetType().ToString());
        Harmony harmony = new Harmony(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    [HarmonyPatch(typeof(XUiC_MapSubPopupEntry))]
    [HarmonyPatch("SetSpriteName")]
    public class XUiC_MapSubPopupEntry_SetSpriteName
    {
        static bool Prefix(ref string ___spriteName, string _s)
        {
            ___spriteName = _s;
            return false;
        }
    }

    [HarmonyPatch(typeof(XUiC_MapSubPopupList))]
    [HarmonyPatch("Init")]
    public class XUiC_MapSubPopupList_Init
    {
        static void Postfix(XUiC_MapSubPopupList __instance)
        {
            var entries = __instance.GetChildrenByType<XUiC_MapSubPopupEntry>();
            for (int i = 0; i < entries.Length; i += 1)
            {
                foreach (var child in entries[i].Parent.Children)
                {
                    if (!child.ViewComponent.ID.EqualsCaseInsensitive("icon")) continue;
                    entries[i].SetSpriteName(((XUiV_Sprite)child.ViewComponent).SpriteName);
                }
                entries[i].SetIndex(i);
            }

            XUiC_CustomWayPointIcons.SetIconCount(entries.Length);
            int cols = XUiC_CustomWayPointIcons.Cols;
            int rows = XUiC_CustomWayPointIcons.Rows;

            if (__instance.ViewComponent is XUiV_Grid grid)
            {
                grid.Columns = cols;
                grid.Rows = rows;
            }
        }
    }

    [HarmonyPatch(typeof(XUiC_MapSubPopupEntry))]
    [HarmonyPatch("onPressed")]
    public class XUiC_MapSubPopupEntry_onPressed
    {
        static void Postfix(XUiC_MapSubPopupEntry __instance, int ___index)
        {
            int y = ___index % XUiC_CustomWayPointIcons.Rows;
            int x = ___index / XUiC_CustomWayPointIcons.Rows + 1;
            __instance.xui.GetWindow("mapAreaEnterWaypointName")
                .Position = new Vector2i(x * 43, y * -43) +
                __instance.xui.GetWindow("mapAreaChooseWaypoint").Position;
        }
    }

    //*************************************************************************
    // Helper functions to parse color hexlets
    // Or which utils function did I miss? :)
    //*************************************************************************

    static private bool ParseHexlet(char low, out byte value)
    {
        switch (low)
        {
            case '0': value = 0; return true;
            case '1': value = 1; return true;
            case '2': value = 2; return true;
            case '3': value = 3; return true;
            case '4': value = 4; return true;
            case '5': value = 5; return true;
            case '6': value = 6; return true;
            case '7': value = 7; return true;
            case '8': value = 8; return true;
            case '9': value = 9; return true;
            case 'A': case 'a': value = 10; return true;
            case 'B': case 'b': value = 11; return true;
            case 'C': case 'c': value = 12; return true;
            case 'D': case 'd': value = 13; return true;
            case 'E': case 'e': value = 14; return true;
            case 'F': case 'f': value = 15; return true;
            default: value = 0; return false;
        }
    }

    static private bool ParseHexByte(char low, char hi, out byte value)
    {
        value = 0;
        if (!ParseHexlet(low, out byte l)) return false;
        if (!ParseHexlet(low, out byte h)) return false;
        value = (byte)(l + h * 16);
        return true;
    }

    // Colorize way-point icon same as text is colored
    private static void ColorizeWaypoint(Waypoint wp)
    {
        var nav = wp.navObject;
        if (wp?.navObject?.DisplayName == null) return;
        // Do nothing if color is already overridden
        if (wp.navObject.UseOverrideColor) return;
        // Get the display name to extract color
        string name = wp.navObject.DisplayName;
        // Check if text has enough room to contain color and check it
        if (name.Length < 8 || name[0] != '[' || name[7] != ']') return;
        // There is certainly a good utility function to do all this ;)
        if (!ParseHexByte(name[1], name[2], out byte r)) return;
        if (!ParseHexByte(name[3], name[4], out byte g)) return;
        if (!ParseHexByte(name[5], name[6], out byte b)) return;
        wp.navObject.OverrideColor = new Color32(r, g, b, 255);
        wp.navObject.UseOverrideColor = true;
    }

    //*************************************************************************
    // Below are the hooks to colorize way-points (incubating feature)
    //*************************************************************************

    // Upgrade navigation object color if name is colored
    // By "abusing" the name we solve the persisting issue
    // This is the hook when way-points are loaded from disk
    [HarmonyPatch(typeof(PlayerDataFile))]
    [HarmonyPatch("ToPlayer")]
    public class PlayerDataFile_ToPlayer
    {

        static void Postfix(EntityPlayer _player)
        {
            foreach (var wp in _player.Waypoints.List)
                ColorizeWaypoint(wp);
        }
    }

    // This is the hook when way-points are newly created
    // The hook below is the one that colorizes the name
    [HarmonyPatch(typeof(XUiC_MapWaypointList))]
    [HarmonyPatch("UpdateWaypointsList")]
    public class XUiC_MapWaypointList_UpdateWaypointsList
    {
        static void Prefix(XUiC_MapWaypointList __instance, Waypoint _selectThisWaypoint)
        {
            if (_selectThisWaypoint == null) return;
            ColorizeWaypoint(_selectThisWaypoint);
        }
    }

    // Hook to add color to the name from UI color picker
    [HarmonyPatch(typeof(XUiC_MapArea))]
    [HarmonyPatch("OnWaypointCreated")]
    public class XUiC_MapArea_OnWaypointCreated
    {
        static void Prefix(XUiC_MapArea __instance, ref string _name)
        {
            if (__instance.Parent.GetChildById("navIconColor") is XUiC_ColorPicker picker)
            {
                _name = string.Format("[{0:X2}{1:X2}{2:X2}]{3}",
                    (byte)(picker.SelectedColor.r * 255),
                    (byte)(picker.SelectedColor.g * 255),
                    (byte)(picker.SelectedColor.b * 255),
                    _name);
            }
        }
    }

    // Make sure to default to vanilla color (white)
    [HarmonyPatch(typeof(XUiC_MapArea))]
    [HarmonyPatch("Init")]
    public class XUiC_MapArea_OnOpen
    {
        static void Prefix(XUiC_MapArea __instance)
        {
            if (__instance.Parent.GetChildById("navIconColor") is XUiC_ColorPicker picker)
            {
                picker.SelectedColor = Color.white;
            }
        }
    }

}
