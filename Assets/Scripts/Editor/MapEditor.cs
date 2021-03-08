using System;
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

    private bool _canEditSquare = true;
    private Vector3 _currentEditedPosition = Vector3.zero;

    private void OnEnable()
    {
        _targetMapManager = (MapManager)target;
        LoadPrefsEditor();
    }

    private void OnDisable()
    {
        SavePrefsEditor();
    }

    public override void OnInspectorGUI()
    {
        DisplayMapProperties();

        DisplayMapViewProperties();

        DisplayEditMap();
        
        GUILayout.Label("====== UPDATE MAP VIEW ======", EditorStyles.boldLabel);

        if(GUILayout.Button("Update View"))
        {
            // Detruire tous les enfants de MapManager
            DetroyAllChilds(_targetMapManager.gameObject);

            // Instancier et setter la surface             
            GameObject newSurface = (GameObject)PrefabUtility.InstantiatePrefab(_targetMapManager._surfaceView, _targetMapManager.transform);

            // Instancier et setter les squares

            // Generer la surface
        }
    }

    private void DetroyAllChilds(GameObject parent)
    {
        if(parent != null)
        {
            for(int i = parent.transform.childCount; i > 0; i--)
            {
                DestroyImmediate(parent.transform.GetChild(0).gameObject);
            }
        }
    }

    private void DisplayMapProperties()
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
            SetObjectDirty(_targetMapManager);
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
    }

    private void DisplayMapViewProperties()
    {
        GUILayout.Label("====== MAP VIEW ======", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        _targetMapManager._surfaceView = (GameObject)EditorGUILayout.ObjectField("Surface Debug", _targetMapManager._surfaceView, typeof(GameObject), false);
        _targetMapManager._wallView = (GameObject)EditorGUILayout.ObjectField("Wall Debug", _targetMapManager._wallView, typeof(GameObject), false);
        if (EditorGUI.EndChangeCheck())
        {
            SetObjectDirty(_targetMapManager);
        }
    }

    private void DisplayEditMap()
    {
        GUILayout.Label("====== MAP EDITOR ======", EditorStyles.boldLabel);

        if (GUILayout.Button("Reset Map"))
        {
            _targetMapManager.GenerateMap();
            SetObjectDirty(_targetMapManager);
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
        // Validation des inputs
        // Valide qu'on peut bien editer les squares
        CanEditSquare();

        if (_isInSquareStateEditMode)
        {
            // Lock des outils d'edition de la scene
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            Tools.current = Tool.None;

            // Valider qu'on est bien à l'interieur de la grille
            if (DisplayEditSquareStateMode())
            {
                if(_canEditSquare)
                {
                    EditSquareStateMode(_currentEditedPosition);
                }
            }
        }

        SceneView.RepaintAll();
    }

    private void CanEditSquare()
    {
        Event current = Event.current;
        if(current.keyCode == KeyCode.LeftAlt)
        {
            switch (current.type)
            {
                case EventType.KeyDown:
                    _canEditSquare = false;
                    break;
                case EventType.KeyUp:
                    _canEditSquare = true;
                    break;
            }
        }
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
                _currentEditedPosition = position;
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
        Handles.color = Color.cyan;
        Handles.DrawWireCube(position, Vector3.one/2);

        if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
            && Event.current.button == 0)
        {
            //_targetMapManager.MapData.SetSquareState(position, currentSquareState);

            int indexSquare = _targetMapManager.MapData.GetIndexFromPos(position.x, position.z);
            _targetMapManager.MapData.Grid[indexSquare].State = currentSquareState;

            SetObjectDirty(_targetMapManager);
        }
    }

    #region SAVE / LOAD / DIRTY
    public static void SetObjectDirty(UnityEngine.Object objectDirty)
    {
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(objectDirty);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }

    private void LoadPrefsEditor()
    {
        _isInSquareStateEditMode = EditorPrefs.GetBool(nameof(_isInSquareStateEditMode));
        _squareStateMode = EditorPrefs.GetInt(nameof(_squareStateMode));
    }

    private void SavePrefsEditor()
    {
        EditorPrefs.SetBool(nameof(_isInSquareStateEditMode), _isInSquareStateEditMode);
        EditorPrefs.SetInt(nameof(_squareStateMode), _squareStateMode);
    }
    #endregion  SAVE / LOAD / DIRTY
}
