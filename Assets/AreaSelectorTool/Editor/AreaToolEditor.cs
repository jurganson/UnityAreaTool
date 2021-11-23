using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using static GUIHelperMethods;

[CustomEditor(typeof(AreaTool))]
public class AreaToolEditor : Editor
{
    private AreaTool AreaTool;
    private SelectionInfo Selection;
    private bool NeedsRedraw;
    private Area SelectedArea => AreaTool.Areas[Selection.SelectedAreaIndex];

    private Vector2 ScrollPosition;
    private bool IsColorCustomizationToggled;

    const float ColorMultiplier = 0.6f;
    private int AreasCreatedSinceReset;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var areaDeleteIndex = -1;
        var foldoutStyle = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };

        IsColorCustomizationToggled = EditorGUILayout.Foldout(IsColorCustomizationToggled, "General color customization", foldoutStyle);
        if (IsColorCustomizationToggled)
        {
            HorizontalSection(() =>
            {
                EditorGUILayout.LabelField(AreaTool.Configuration.SelectedPointColor.Name);
                AreaTool.Configuration.SelectedPointColor.Color = EditorGUILayout.ColorField(AreaTool.Configuration.SelectedPointColor.Color);
            });

            HorizontalSection(() =>
            {
                EditorGUILayout.LabelField(AreaTool.Configuration.HoverSelectionLineColor.Name);
                AreaTool.Configuration.HoverSelectionLineColor.Color = EditorGUILayout.ColorField(AreaTool.Configuration.HoverSelectionLineColor.Color);
            });

            HorizontalSection(() =>
            {
                EditorGUILayout.LabelField(AreaTool.Configuration.HoverSelectionPointColor.Name);
                AreaTool.Configuration.HoverSelectionPointColor.Color = EditorGUILayout.ColorField(AreaTool.Configuration.HoverSelectionPointColor.Color);
            });
        }

        HorizontalLine();

        EditorGUILayout.LabelField("Areas", new GUIStyle
        {
            fontStyle = FontStyle.Bold, 
            alignment = TextAnchor.MiddleCenter, 
            fontSize = 15,
            normal = new GUIStyleState { textColor = Color.white }
        });
        
        VerticalSection(() =>
        {
            ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition, GUILayout.Height(400));
            for (var i = 0; i < AreaTool.Areas.Count; i++)
            {
                var area = AreaTool.Areas[i];

                EditorGUI.indentLevel++;
                area.IsToggledInView = EditorGUILayout.Foldout(area.IsToggledInView, area.Name, foldoutStyle);
                EditorGUI.indentLevel--;

                if (area.IsToggledInView)
                {
                    var areaStyle = new GUIStyle(GUI.skin.box);
                    areaStyle.normal.background = GetColoredTexture2D(new Color(56/255f, 56/255f, 56/255f));
                    areaStyle.padding = new RectOffset(5, 5, 5, 7);

                    VerticalSection(() =>
                    {
                        HorizontalSection(() =>
                        {
                            EditorGUILayout.LabelField("Area name");
                            area.Name = EditorGUILayout.TextField(area.Name);
                        });

                        HorizontalSection(() =>
                        {
                            EditorGUILayout.LabelField("Tag");
                            area.Tag = EditorGUILayout.TagField(area.Tag);
                        });

                        HorizontalSection(() =>
                        {
                            EditorGUILayout.LabelField(area.Color.Name);
                            var areaColor = EditorGUILayout.ColorField(area.Color.Color);
                            area.Color.Color = areaColor;
                            area.DeselectedColor = new Color(areaColor.r * ColorMultiplier, areaColor.g * ColorMultiplier, areaColor.b * ColorMultiplier);
                        });

                        GUILayout.Space(10f);

                        HorizontalSection(() =>
                        {
                            if (GUILayout.Button($"Select {area.Name}"))
                                Selection.SelectedAreaIndex = i;

                            if (GUILayout.Button($"Delete {area.Name}"))
                                areaDeleteIndex = i;
                        });
                    }, areaStyle);
                }
            }
            GUILayout.EndScrollView();
        }, GUI.skin.box);

        GUILayout.Space(10f);

        HorizontalSection(() =>
        {
            GUILayout.Space(100);
            if (GUILayout.Button($"Delete ALL areas"))
            {
                Undo.RecordObject(AreaTool, "Delete all areas");
                AreaTool.Areas.Clear();
            }
            GUILayout.Space(100);
        });
        
        if (areaDeleteIndex != -1)
            DeleteArea(areaDeleteIndex);

        if (!GUI.changed) 
            return;

        NeedsRedraw = true;
        SceneView.RepaintAll();
    }

    public void DeleteArea(int areaIndex, bool enableUndo = true)
    {
        if (enableUndo)
            Undo.RecordObject(AreaTool, "Delete area");

        AreaTool.Areas.RemoveAt(areaIndex);
        Selection.SelectedAreaIndex = Mathf.Clamp(Selection.SelectedAreaIndex, 0, AreaTool.Areas.Count - 1);
    }

    public void DeletePointUnderMouse(bool enableUndo = true)
    {
        if (enableUndo)
            Undo.RecordObject(AreaTool, "Delete point");

        SelectedArea.Points.RemoveAt(Selection.PointIndex);
        Selection.PointIsSelected = false;
        Selection.MouseIsOverPoint = false;

        if (!SelectedArea.Points.Any())
            DeleteArea(Selection.SelectedAreaIndex, false);

        NeedsRedraw = true;
    }

    void OnSceneGUI()
    {
        var guiEvent = Event.current;

        switch (guiEvent.type)
        {
            case EventType.Repaint:
                Draw();
                break;
            case EventType.Layout:
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                break;
            default:
            {
                HandleInput(guiEvent);
                if (NeedsRedraw)
                    HandleUtility.Repaint();

                break;
            }
        }
    }

    void CreateNewArea()
    {
        Undo.RecordObject(AreaTool, "Create area");

        const float colorMultiplier = 0.6f;
        var randomColorSelected = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        var randomColorUnselected = new Color(randomColorSelected.r * colorMultiplier, randomColorSelected.g * colorMultiplier, randomColorSelected.b * colorMultiplier);
        AreasCreatedSinceReset++;

        AreaTool.Areas.Add(new Area
        {
            Name = $"Area {AreasCreatedSinceReset}",
            Color = new CustomizableColor {Color = randomColorSelected, Name = "Color"},
            DeselectedColor = randomColorUnselected
        });

        Selection.SelectedAreaIndex = AreaTool.Areas.Count - 1;
    }

    void CreateNewPoint(Vector3 position)
    {
        var mouseIsOverSelectedArea = Selection.MouseOverAreaIndex == Selection.SelectedAreaIndex;
        var newPointIndex = (Selection.MouseIsOverLine && mouseIsOverSelectedArea) ? Selection.LineIndex + 1 : SelectedArea.Points.Count;

        Undo.RecordObject(AreaTool, "Add point");
        SelectedArea.Points.Insert(newPointIndex, position);
        Selection.PointIndex = newPointIndex;
        Selection.MouseOverAreaIndex = Selection.SelectedAreaIndex;
        NeedsRedraw = true;


        SelectPointUnderMouse();
    }

    void SelectPointUnderMouse()
    {
        Selection.PointIsSelected = true;
        Selection.MouseIsOverPoint = true;
        Selection.MouseIsOverLine = false;
        Selection.LineIndex = -1;

        Selection.PositionAtStartOfDrag = SelectedArea.Points[Selection.PointIndex];
        NeedsRedraw = true;
    }

    void SelectAreaUnderMouse()
    {
        if (Selection.MouseOverAreaIndex == -1) 
            return;

        Selection.SelectedAreaIndex = Selection.MouseOverAreaIndex;
        NeedsRedraw = true;
    }

    void HandleInput(Event guiEvent)
    {
        var mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        var drawPlaneHeight = 0;
        var dstToDrawPlane = (drawPlaneHeight - mouseRay.origin.y) / mouseRay.direction.y;
        var mousePosition = mouseRay.GetPoint(dstToDrawPlane);

        switch (guiEvent.type)
        {
            case EventType.MouseDown when guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Shift:
                HandleShiftLeftMouseDown(mousePosition);
                break;
            case EventType.MouseDown when guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None:
                HandleLeftMouseDown(mousePosition);
                break;
            case EventType.MouseUp when guiEvent.button == 0:
                HandleLeftMouseUp(mousePosition);
                break;
            case EventType.MouseDrag when guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None:
                HandleLeftMouseDrag(mousePosition);
                break;
        }

        if (!Selection.PointIsSelected)
            UpdateMouseOverInfo(mousePosition);

    }

    void HandleShiftLeftMouseDown(Vector3 mousePosition)
    {
        if (Selection.MouseIsOverPoint)
        {
            SelectAreaUnderMouse();
            DeletePointUnderMouse();
        }
        else
        {
            CreateNewArea();
            CreateNewPoint(mousePosition);
        }
    }

    void HandleLeftMouseDown(Vector3 mousePosition)
    {
        if (AreaTool.Areas.Count == 0)
            CreateNewArea();

        SelectAreaUnderMouse();

        if (Selection.MouseIsOverPoint)
            SelectPointUnderMouse();
        else
            CreateNewPoint(mousePosition);
    }

    void HandleLeftMouseUp(Vector3 mousePosition)
    {
        if (!Selection.PointIsSelected) 
            return;

        SelectedArea.Points[Selection.PointIndex] = Selection.PositionAtStartOfDrag;
        Undo.RecordObject(AreaTool, "Move point");
        SelectedArea.Points[Selection.PointIndex] = mousePosition;

        Selection.PointIsSelected = false;
        Selection.PointIndex = -1;
        NeedsRedraw = true;

    }

    void HandleLeftMouseDrag(Vector3 mousePosition)
    {
        if (!Selection.PointIsSelected) 
            return;

        SelectedArea.Points[Selection.PointIndex] = mousePosition;
        NeedsRedraw = true;

    }

    void UpdateMouseOverInfo(Vector3 mousePosition)
    {
        var mouseOverPointIndex = -1;
        var mouseOverAreaIndex = -1;
        for (int areaIndex = 0; areaIndex < AreaTool.Areas.Count; areaIndex++)
        {
            var currentArea = AreaTool.Areas[areaIndex];

            for (var i = 0; i < currentArea.Points.Count; i++)
            {
                if (Vector3.Distance(mousePosition, currentArea.Points[i]) < AreaTool.PointHandleRadius)
                {
                    mouseOverPointIndex = i;
                    mouseOverAreaIndex = areaIndex;
                    break;
                }
            }
        }

        if (mouseOverPointIndex != Selection.PointIndex || mouseOverAreaIndex != Selection.MouseOverAreaIndex)
        {
            Selection.MouseOverAreaIndex = mouseOverAreaIndex;
            Selection.PointIndex = mouseOverPointIndex;
            Selection.MouseIsOverPoint = mouseOverPointIndex != -1;

            NeedsRedraw = true;
        }

        if (Selection.MouseIsOverPoint)
        {
            Selection.MouseIsOverLine = false;
            Selection.LineIndex = -1;
        }
        else
        {
            var mouseOverLineIndex = -1;
            var closestLineDst = AreaTool.PointHandleRadius;
            for (int areaIndex = 0; areaIndex < AreaTool.Areas.Count; areaIndex++)
            {
                var currentArea = AreaTool.Areas[areaIndex];

                for (int i = 0; i < currentArea.Points.Count; i++)
                {
                    var nextPointInArea = currentArea.Points[(i + 1) % currentArea.Points.Count];
                    var dstFromMouseToLine = HandleUtility.DistancePointToLineSegment(mousePosition.ToXZ(), currentArea.Points[i].ToXZ(), nextPointInArea.ToXZ());
                    if (dstFromMouseToLine < closestLineDst)
                    {
                        closestLineDst = dstFromMouseToLine;
                        mouseOverLineIndex = i;
                        mouseOverAreaIndex = areaIndex;
                    }
                }
            }

            if (Selection.LineIndex != mouseOverLineIndex || mouseOverAreaIndex != Selection.MouseOverAreaIndex)
            {
                Selection.MouseOverAreaIndex = mouseOverAreaIndex;
                Selection.LineIndex = mouseOverLineIndex;
                Selection.MouseIsOverLine = mouseOverLineIndex != -1;
                NeedsRedraw = true;
            }
        }
    }

    void Draw()
    {
        for (var areaIndex = 0; areaIndex < AreaTool.Areas.Count; areaIndex++)
        {
            var areaToDraw = AreaTool.Areas[areaIndex];
            var areaIsSelected = areaIndex == Selection.SelectedAreaIndex;
            var mouseIsOverArea = areaIndex == Selection.MouseOverAreaIndex;

            for (var i = 0; i < areaToDraw.Points.Count; i++)
            {
                var nextPoint = areaToDraw.Points[(i + 1) % areaToDraw.Points.Count];
                if (i == Selection.LineIndex && mouseIsOverArea && areaToDraw.Points.Count > 1)
                {
                    var hoverLineColor = AreaTool.Configuration.HoverSelectionLineColor.Color;
                    Handles.DrawBezier(areaToDraw.Points[i], nextPoint, areaToDraw.Points[i], nextPoint, hoverLineColor, null, AreaTool.LineWidth);
                }
                else if (areaToDraw.Points.Count > 1)
                {
                    var lineColor = (areaIsSelected) ? areaToDraw.Color.Color : areaToDraw.DeselectedColor;
                    Handles.DrawBezier(areaToDraw.Points[i], nextPoint, areaToDraw.Points[i], nextPoint, lineColor, null, AreaTool.LineWidth);
                }

                if (i == Selection.PointIndex && mouseIsOverArea)
                    Handles.color = (Selection.PointIsSelected) ? AreaTool.Configuration.SelectedPointColor.Color : AreaTool.Configuration.HoverSelectionPointColor.Color;
                else
                    Handles.color = (areaIsSelected) ? areaToDraw.Color.Color : areaToDraw.DeselectedColor;

                Handles.DrawSolidDisc(areaToDraw.Points[i], Vector3.up, AreaTool.PointHandleRadius);
            }

            var texture = GetColoredTexture2D(Selection.SelectedAreaIndex == areaIndex ? areaToDraw.Color.Color : areaToDraw.DeselectedColor);
            var style = new GUIStyle
            {
                normal = new GUIStyleState {background = texture, textColor = areaToDraw.Color.Color.InvertColor()}, 
                padding = new RectOffset(2,0,2,0), 
                alignment = TextAnchor.UpperCenter
            };

            Handles.Label(areaToDraw.GetCenter(), areaToDraw.Name, style);
        }

        NeedsRedraw = false;
    }

    public Texture2D GetColoredTexture2D(Color backgroundColor)
    {
        var texture = new Texture2D(1, 1) { wrapMode = TextureWrapMode.Repeat };
        texture.SetPixel(1, 1, backgroundColor);
        texture.Apply();
        return texture;
    }

    void OnEnable()
    {
        AreaTool = target as AreaTool;
        Selection = new SelectionInfo();
        Undo.undoRedoPerformed += OnUndoOrRedo;
    }

    void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoOrRedo;
    }

    void OnUndoOrRedo()
    {
        if (Selection.SelectedAreaIndex >= AreaTool.Areas.Count)
            Selection.SelectedAreaIndex = AreaTool.Areas.Count - 1;
    }
}
