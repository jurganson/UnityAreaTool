using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class GUIHelperMethods : Editor
{
    public static void HorizontalSection(Action layout, GUIStyle style = null)
    {
        if (style != null)
            GUILayout.BeginHorizontal(style);
        else
            GUILayout.BeginHorizontal();

        layout.Invoke();
        GUILayout.EndHorizontal();
    }
    public static void VerticalSection(Action layout, GUIStyle style = null)
    {
        if (style != null)
            GUILayout.BeginVertical(style);
        else
            GUILayout.BeginVertical();

        layout.Invoke();
        GUILayout.EndVertical();
    }

    public static void HorizontalLine()
    {
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    }
}
