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
        Log.Out(" Loading Patch: " + GetType().ToString());
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

}
