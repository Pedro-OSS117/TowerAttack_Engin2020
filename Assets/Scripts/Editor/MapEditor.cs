using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(MapManager))]
public class MapEditor : Editor
{
    private MapManager _targetMapManager;

    private int _squareStateMode;
    private bool _isInSquareStateEditMode = false;

    private bool _canEditSquare = true;
    private Vector3 _currentEditedPosition = Vector3.zero;
    private Vector3 _previousEditedPosition = Vector3.zero;

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

        DisplayGenerateMapViewButton();
    }

    private static Transform GetChildByName(GameObject parent, string name)
    {
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            Transform child = parent.transform.GetChild(i);
            if(child.gameObject.name == name)
            {
                return child;
            }
        }
        return null;
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
            _targetMapManager.ResetMapData();
            GenerateMapView();
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

        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_targetMapManager._waterView)));

        serializedObject.ApplyModifiedProperties();
    }

    private void DisplayEditMap()
    {
        GUILayout.Label("====== MAP EDITOR ======", EditorStyles.boldLabel);

        if (GUILayout.Button("Reset Map"))
        {
            _targetMapManager.ResetMapData();
            GenerateMapView();

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

    private void DisplayGenerateMapViewButton()
    {
        GUILayout.Label("====== UPDATE MAP VIEW ======", EditorStyles.boldLabel);

        if (GUILayout.Button("Update View"))
        {
            GenerateMapView();
        }
    }

    private void GenerateMapView()
    {
        // ========= Detruire tous les enfants de MapManager
        DetroyAllChilds(_targetMapManager.gameObject);

        // ========= Instancier et setter la surface             
        GameObject newSurface = (GameObject)PrefabUtility.InstantiatePrefab(_targetMapManager._surfaceView, _targetMapManager.transform);

        // Scale de la surface pour avoir une correspondance
        newSurface.transform.localScale = new Vector3(_targetMapManager.MapData.Width, 1, _targetMapManager.MapData.Height);

        // Set de la position de la surface
        newSurface.transform.position = new Vector3(_targetMapManager.MapData.Width / 2.0f, 0, _targetMapManager.MapData.Height / 2.0f);

        // ========= Instancier et setter les squares

        // Creation d'un container en dessous de map manager
        GameObject squaresContainer = new GameObject("SquaresContainer");
        squaresContainer.transform.SetParent(_targetMapManager.transform);

        // Instantiation des prefabs de square en fonction des datas
        for (int i = 0; i < _targetMapManager.MapData.Width; i++)
        {
            for (int j = 0; j < _targetMapManager.MapData.Height; j++)
            {
                SquareData data = _targetMapManager.MapData.GetSquareData(i, j);
                GameObject newSquare = null;
                switch (data.State)
                {
                    case SquareState.Wall:
                        newSquare = (GameObject)PrefabUtility.InstantiatePrefab(_targetMapManager._wallView, squaresContainer.transform);
                        break;
                    case SquareState.Water:
                        newSquare = (GameObject)PrefabUtility.InstantiatePrefab(_targetMapManager._waterView, squaresContainer.transform);
                        break;
                }
                if (newSquare != null)
                {
                    newSquare.transform.position = new Vector3(i + 0.5f, 0, j + 0.5f);
                }
            }
        }

        // ========= Update du NavMesh
        NavMeshSurface navMeshSurface = newSurface.GetComponent<NavMeshSurface>();

        //NavMeshSurface navMeshSurface2 = _targetMapManager.GetComponentInChildren<NavMeshSurface>();

        /*Transform navMeshChild = GetChildByName(_targetMapManager.gameObject, "NavMeshSurface");
        _targetMapManager.transform.GetChild(0).GetComponent<NavMeshSurface>();*/

        navMeshSurface.BuildNavMesh();
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
                    EditSquareStateMode();
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

    private void EditSquareStateMode()
    {
        SquareState currentSquareState = (SquareState)_squareStateMode;
        Handles.color = SquareData.GetColorFromState(currentSquareState);
        Handles.DrawWireCube(_currentEditedPosition, Vector3.one);
        Handles.color = Color.cyan;
        Handles.DrawWireCube(_currentEditedPosition, Vector3.one/2);
                
        if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
            && Event.current.button == 0)
        {
            int indexSquare = _targetMapManager.MapData.GetIndexFromPos(_currentEditedPosition.x, _currentEditedPosition.z);
            _targetMapManager.MapData.Grid[indexSquare].State = currentSquareState;

            //GenerateMapView();

            SetObjectDirty(_targetMapManager);
        }

        if(Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            GenerateMapView();
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
