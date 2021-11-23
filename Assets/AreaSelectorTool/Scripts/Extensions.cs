using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public static class Extensions
{
    public static Vector2 ToXZ(this Vector3 v3)
    {
        return new Vector2(v3.x, v3.z);
    }

    public static Vector3 GetCenter(this Area area)
    {
        var bound = new Bounds(area.Points[0], Vector3.zero);

        for (var i = 1; i < area.Points.Count; i++)
            bound.Encapsulate(area.Points[i]);

        return bound.center;
    }

    public static Color InvertColor(this Color color) {
        return new Color(1.0f-color.r, 1.0f-color.g, 1.0f-color.b);
    }
}

public static class AreaExtensions
{
    public static bool IsPositionWithinArea(this Area area, Vector3 position)
    {
        var point = new Vector2(position.x, position.z);
        var q = area.Points.Count - 1;
        var inside = false;

        if (area.Points.Count < 2)
            return false;

        for (var i = 0; i < area.Points.Count; q = i++)
        {
            if ((area.Points[i].z <= point.y && point.y < area.Points[q].z || area.Points[q].z <= point.y && point.y < area.Points[i].z) && point.x < (area.Points[q].x - area.Points[i].x) * (point.y - area.Points[i].z) / (area.Points[q].z - area.Points[i].z) + area.Points[i].x)
                inside = !inside;
        }

        return inside;
    }

    public static bool IsPositionWithinAreaWithTag(string tag, Vector3 position)
    {
        var areaTools = Object.FindObjectsOfType(typeof(AreaTool)) as AreaTool[];
        var shapes = areaTools?.SelectMany(tool => tool.Areas).ToList();

        return (shapes ?? new List<Area>()).Any(shape => shape.Tag.Equals(tag) && shape.IsPositionWithinArea(position));
    }
}
