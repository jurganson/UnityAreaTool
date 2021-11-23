using UnityEngine;

public class SelectionInfo
{
    public int SelectedAreaIndex;
    public int MouseOverAreaIndex;

    public int PointIndex = -1;
    public bool MouseIsOverPoint;
    public bool PointIsSelected;
    public Vector3 PositionAtStartOfDrag;

    public int LineIndex = -1;
    public bool MouseIsOverLine;
}
