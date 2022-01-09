using UnityEngine;

public class XUiC_CustomWayPointIcons : XUiController
{

    public static string ID = "";

    XUiC_MapSubPopupList list;
    XUiV_Panel panel;
    XUiV_Grid grid;

    private static int rows = 1;
    private static int cols = 1;
    private static int count = 0;

    public static int Rows => rows;
    public static int Cols => cols;

    public static int Count => count;

    public static void SetIconCount(int count)
    {
        XUiC_CustomWayPointIcons.count = count;
        cols = (int)Mathf.Ceil(Mathf.Sqrt(.66f * count));
        rows = (int)Mathf.Ceil(count / (float)cols);
    }

    public override void Init()
    {
        ID = WindowGroup.ID;
        panel = GetChildById("content").ViewComponent as XUiV_Panel;
        list = windowGroup.Controller.GetChildByType<XUiC_MapSubPopupList>();
        grid = list.ViewComponent as XUiV_Grid;
        base.Init();
        IsDirty = true;
    }

    public override void OnOpen()
    {
        base.OnOpen();
        IsDirty = true;
    }

    public override void Update(float _dt)
    {
        base.Update(_dt);
        if (!XUi.IsGameRunning()) return;
        if (IsDirty == false) return;
        RefreshBindings();
        IsDirty = false;
    }

    public override bool GetBindingValue(ref string value, string bindingName)
    {
        switch (bindingName)
        {
            case "rows":
                value = rows.ToString();
                return true;
            case "cols":
                value = cols.ToString();
                return true;
            case "outerHeight":
                value = (rows * 43 + 6).ToString();
                return true;
            case "outerWidth":
                value = (cols * 43 + 6).ToString();
                return true;
            case "innerHeight":
                value = (rows * 43).ToString(); ;
                return true;
            case "innerWidth":
                value = (cols * 43).ToString();
                return true;
            case "cellHeight":
                value = "43";
                return true;
            case "cellWidth":
                value = "43";
                return true;
        }
        value = "";
        return false;
    }

}
