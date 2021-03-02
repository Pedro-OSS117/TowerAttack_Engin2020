﻿using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(MapManager))]
public class MapEditor : Editor
{
    private MapManager _targetMapManager;

    private int _squareStateMode;

    private bool _isInSquareStateEditMode = false;

    private Vector3 currentEditedPosition = Vector3.zero;

    private void OnEnable()
    {
        _targetMapManager = (MapManager)target;
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Label("====== MAP PROPERTIES ======", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        GUILayout.BeginHorizontal();
        GUILayout.Label(nameof(_targetMapManager.MapData.Width) + " : ");
        _targetMapManager.MapData.Width = EditorGUILayout.IntSlider(_targetMapManager.MapData.Width, 1, 50);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(nameof(_targetMapManager.MapData.Height) + " : ");
        _targetMapManager.MapData.Height = EditorGUILayout.IntSlider(_targetMapManager.MapData.Height, 1, 50);
        GUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            _targetMapManager.GenerateMap();
            SceneView.RepaintAll();
        }

        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal();
        GUILayout.Label(nameof(_targetMapManager.displayDebugView) + " : ");
        _targetMapManager.displayDebugView = EditorGUILayout.Toggle(_targetMapManager.displayDebugView);
        GUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }

        GUILayout.Label("====== MAP EDITOR ======", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate Map"))
        {
            _targetMapManager.GenerateMap();

            SceneView.RepaintAll();
        }
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Enable Edit Mode : ");
        _isInSquareStateEditMode = EditorGUILayout.Toggle(_isInSquareStateEditMode);
        GUILayout.EndHorizontal();
        if (_isInSquareStateEditMode)
        {
            // Affichage des outils de modification du State d'un square
            GUILayout.BeginHorizontal();
            string[] values = Enum.GetNames(typeof(SquareState));
            GUILayout.Label("Square State Mode : ");
            _squareStateMode = EditorGUILayout.Popup(_squareStateMode, values);
            GUILayout.EndHorizontal();
        }
    }

    private void OnSceneGUI()
    {
        if(_isInSquareStateEditMode)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            Tools.current = Tool.None;

            if (DisplayEditSquareStateMode())
            {
                EditSquareStateMode(currentEditedPosition);
            }
        }

        SceneView.RepaintAll();
    }

    private bool DisplayEditSquareStateMode()
    {
        // Recuperation de la postion de la souris dans la vue de la scene
        Vector2 mousePosition = Event.current.mousePosition;

        // Creation d'un rayon perpendiculaire à la position de la souris dans la scene
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

        // Creation d'un plan un x et z
        Plane horizontalPlane = new Plane(Vector3.up, Vector3.zero);

        // Raycast entre le plan et le rayon 
        // Distance donnera la distance entre le point d'intersection du plan et le rayon et l'origine du rayon
        if (horizontalPlane.Raycast(ray, out float distance))
        {
            Vector3 position = ray.GetPoint(distance);

            position.x = (int)position.x + 0.5f;
            position.z = (int)position.z + 0.5f;

            if (_targetMapManager.MapData.IsInGrid(position))
            {
                currentEditedPosition = position;
                return true;
            }
        }
        return false;
    }

    private void EditSquareStateMode(Vector3 position)
    {
        SquareState currentSquareState = (SquareState)_squareStateMode;
        Handles.color = SquareData.GetColorFromState(currentSquareState);
        Handles.DrawWireCube(position, Vector3.one);

        if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
            && Event.current.button == 0)
        {
            //_targetMapManager.MapData.SetSquareState(position, currentSquareState);

            int indexSquare = _targetMapManager.MapData.GetIndexFromPos(position.x, position.z);
            _targetMapManager.MapData.Grid[indexSquare].State = currentSquareState;

            SetObjectDirty(_targetMapManager);
        }
    }

    private void SetObjectDirty(UnityEngine.Object objectDirty)
    {
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(objectDirty);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}
