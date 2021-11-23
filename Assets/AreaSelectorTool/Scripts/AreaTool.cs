#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;
public class AreaTool : MonoBehaviour
{
    [HideInInspector] public List<Area> Areas = new List<Area>();

    [HideInInspector] public AreaToolConfiguration Configuration;

    [Range(0.5f, 20f)] public float PointHandleRadius = .5f;

    [Range(1f, 20f)] public float LineWidth = 1f;

    public AreaTool()
    {
        Configuration = new AreaToolConfiguration();
    }
}

[System.Serializable]
public class Area
{
    public string Name;
    public string Tag;
    public List<Vector3> Points = new List<Vector3>();
    public bool IsToggledInView;
    public CustomizableColor Color;
    public Color DeselectedColor;

    public Area()
    {
        Tag = "";
    }
}

[System.Serializable]
public class CustomizableColor
{
    public Color Color;
    public string Name;
}

[System.Serializable]
public class AreaToolConfiguration
{
    public CustomizableColor HoverSelectionPointColor;
    public CustomizableColor SelectedPointColor;
    public CustomizableColor HoverSelectionLineColor;

    public AreaToolConfiguration()
    {
        // Default values
        HoverSelectionPointColor = new CustomizableColor {Color = Color.magenta, Name = "Hovering over points"};
        SelectedPointColor = new CustomizableColor {Color = Color.magenta.InvertColor(), Name = "Dragging point"};
        HoverSelectionLineColor = new CustomizableColor {Color = Color.magenta, Name = "Hovering over lines"};
    }
}

