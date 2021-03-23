using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public enum SquareBrush
{
    One,
    Line,
    Cube,
    L
}

[CustomEditor(typeof(MapManager))]
public class MapEditor : Editor
{
    private MapManager _targetMapManager;
    private bool _canEditDatas = true;

    // Variable Square Edit Mode
    private int _squareStateMode;
    private bool _isInSquareStateEditMode = false;
    private SquareBrush _currentSquareBrush = SquareBrush.Line;

    private Vector3 _currentEditedPosition = Vector3.zero;
    private Vector3 _previousEditedPosition = Vector3.zero;

    // Variable Brush
    private int _currentBrushOrientation = 0;
    private readonly Vector3[] _brushArrayLine = new Vector3[] { Vector3.right };
    private readonly Vector3[] _brushArrayCube = new Vector3[] { Vector3.right, Vector3.forward, new Vector3(1, 0, 1) };
    private readonly Vector3[] _brushArrayL = new Vector3[] { Vector3.right, Vector3.forward, new Vector3(0, 0, 2) };

    // Variable Edge Edit Mode
    private bool _IsInEditEdgeMode = false;
    private bool _IsAddOrRemoveEdge = true;

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

    private void OnSceneGUI()
    {
        // Validation des inputs
        // Valide qu'on peut bien editer les squares
        UpdateCanEditDatas();

        if (_canEditDatas)
        {
            if (_isInSquareStateEditMode)
            {
                // Lock des outils d'edition de la scene
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                Tools.current = Tool.None;

                // On verifie si le mode d'edition change
                UpdateEditModeSquareState();

                // On verifie si l'orientation change
                UpdateBrushOrientation();

                // Valider qu'on est bien à l'interieur de la grille
                if (DisplayEditSquareStateMode())
                {
                    EditSquareStateMode();
                }
            }
            else if (_IsInEditEdgeMode)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                Tools.current = Tool.None;

                // Calculate Interact Coordonnee
                Vector3 intersectPos = CalculateInteractPosition();

                int indexEdge = DisplayGizmoEditEdgeInScene(intersectPos);

                EditCurrentEdge(indexEdge, intersectPos);
            }
        }

        SceneView.RepaintAll();
    }

    #region DISPLAY INSPECTOR
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

        _targetMapManager.prefabEdgeHori = (GameObject)EditorGUILayout.ObjectField("Edge Hori Debug", _targetMapManager.prefabEdgeHori, typeof(GameObject), false);

        _targetMapManager.prefabEdgeVert = (GameObject)EditorGUILayout.ObjectField("Edge Vert Debug", _targetMapManager.prefabEdgeVert, typeof(GameObject), false);

        if (EditorGUI.EndChangeCheck())
        {
            SetObjectDirty(_targetMapManager);
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_targetMapManager._waterView)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_targetMapManager.squareFBDropZonePrefab)));

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

        GUILayout.Label("EDIT SQUARE", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Enable Edit Mode : ");
        _isInSquareStateEditMode = EditorGUILayout.Toggle(_isInSquareStateEditMode);
        GUILayout.EndHorizontal();

        if (_isInSquareStateEditMode)
        {
            _IsInEditEdgeMode = false;

            // Affichage des outils de modification du State d'un square
            GUILayout.BeginHorizontal();
            string[] values = Enum.GetNames(typeof(SquareState));
            GUILayout.Label("Square State Mode : ");
            _squareStateMode = EditorGUILayout.Popup(_squareStateMode, values);
            GUILayout.EndHorizontal();

            // Affichage pop up brush
            _currentSquareBrush = (SquareBrush)EditorGUILayout.EnumPopup(_currentSquareBrush);
            GUILayout.Label("Brush orientation 'R' : " + _currentBrushOrientation);
        }

        GUILayout.Label("EDIT EDGE", EditorStyles.boldLabel);
        _IsInEditEdgeMode = GUILayout.Toggle(_IsInEditEdgeMode, "Edit Edge Mode");
        if (_IsInEditEdgeMode)
        {
            _isInSquareStateEditMode = false;
            _IsAddOrRemoveEdge = GUILayout.Toggle(_IsAddOrRemoveEdge, _IsAddOrRemoveEdge ? "Add Edge" : "Remove Edge");
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
    #endregion DISPLAY INSPECTOR

    #region GENERATE VIEW
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
        CreateSquaresView();

        //  ========= Instancier et setter les Edges
        // Intstantiation des views des edges en fonction des datas
        CreateEdgesView(_targetMapManager.MapData.EdgesHori, _targetMapManager.prefabEdgeHori, _targetMapManager.MapData.Width, new Vector3(0.5f, 0, 0));
        CreateEdgesView(_targetMapManager.MapData.EdgesVert, _targetMapManager.prefabEdgeVert, _targetMapManager.MapData.Width + 1, new Vector3(0, 0, 0.5f));

        // ========= Update du NavMesh
        NavMeshSurface navMeshSurface = newSurface.GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();
    }

    private void CreateSquaresView()
    {
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
    }

    // Permet de créer la vue des edges en fonction de leur existance ou non.
    private void CreateEdgesView(bool[] arrayEdges, GameObject prefabEdge, int width, Vector3 adderPosition)
    {
        GameObject container = new GameObject("EdgesContainer");
        container.transform.SetParent(_targetMapManager.transform);

        for (int i = 0; i < arrayEdges.Length; i++)
        {
            // Si Edge presente.
            if (arrayEdges[i])
            {
                // Creation d'une instance de prefab de vue en fonction du state.
                GameObject newSquareView = newSquareView = (GameObject)PrefabUtility.InstantiatePrefab(prefabEdge);
                if (newSquareView != null)
                {
                    newSquareView.transform.SetParent(container.transform);
                    Vector3 newEdgePos = MapData.GetPositionFromIndex(i, width);
                    newEdgePos += adderPosition;
                    newSquareView.transform.position = newEdgePos;
                }
            }
        }
    }
    #endregion GENERATE VIEW

    #region EDIT SQUARE
    private void EditSquareStateMode()
    {
        SquareState currentSquareState = (SquareState)_squareStateMode;
        Handles.color = SquareData.GetColorFromState(currentSquareState);
        Handles.DrawWireCube(_currentEditedPosition, Vector3.one);
        Handles.color = Color.cyan;
        Handles.DrawWireCube(_currentEditedPosition, Vector3.one / 2);

        if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
            && Event.current.button == 0)
        {
            _targetMapManager.MapData.SetSquareData(_currentEditedPosition, currentSquareState);

            // On edit les autres squares en fonction de la brush
            ProcessBrushEdit(_currentEditedPosition);

            SetObjectDirty(_targetMapManager);
        }

        if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            GenerateMapView();
        }
    }

    private void UpdateCanEditDatas()
    {
        Event current = Event.current;
        if (current.keyCode == KeyCode.LeftAlt)
        {
            switch (current.type)
            {
                case EventType.KeyDown:
                    _canEditDatas = false;
                    break;
                case EventType.KeyUp:
                    _canEditDatas = true;
                    break;
            }
        }
    }

    private bool DisplayEditSquareStateMode()
    {
        if (CalculateInteractPosition(out Vector3 position))
        {
            position.x = (int)position.x + 0.5f;
            position.z = (int)position.z + 0.5f;

            if (_targetMapManager.MapData.IsInGrid(position))
            {
                _currentEditedPosition = position;

                DisplayGizmoEditSquareInScene();
                return true;
            }
        }
        return false;
    }

    private void DisplayGizmoEditSquareInScene()
    {
        Vector3 intersectPosInt = GetRoundPos(_currentEditedPosition);

        SquareState currentSquareState = (SquareState)_squareStateMode;
        Handles.color = SquareData.GetColorFromState(currentSquareState);
        Handles.DrawWireCube(_currentEditedPosition, Vector3.one);
        Handles.color = Color.cyan;
        Handles.DrawWireCube(_currentEditedPosition, Vector3.one / 2);

        switch (_currentSquareBrush)
        {
            case SquareBrush.Line:
                DisplayGizmoShape(intersectPosInt, _brushArrayLine);
                break;
            case SquareBrush.Cube:
                DisplayGizmoShape(intersectPosInt, _brushArrayCube);
                break;
            case SquareBrush.L:
                DisplayGizmoShape(intersectPosInt, _brushArrayL);
                break;
        }
    }

    private void DrawEditSquareGizmo(Vector3 intersectPos)
    {
        Handles.color = SquareData.GetColorFromState((SquareState)_squareStateMode);

        intersectPos.x += 0.5f;
        intersectPos.z += 0.5f;
        Vector3 scaleGizmo = Vector3.one;
        scaleGizmo.y = 0.2f;
        Handles.DrawWireCube(intersectPos, scaleGizmo);
    }

    // Methode permettant de changer le mode
    // d'edition des squares en appuyant sur la touche E
    // On passe au suivant tant que c possible.
    // Sinon on recommence de la première valeur d'enum
    private void UpdateEditModeSquareState()
    {
        if (Event.current.keyCode == KeyCode.E)
        {
            if (Event.current.type == EventType.KeyDown)
            {
                _squareStateMode += 1;
                // On redemarre de 0 si on depasse le nombre de valeurs de l'enum;
                _squareStateMode = _squareStateMode % System.Enum.GetNames(typeof(SquareState)).Length;
            }
        }
    }
    #endregion EDIT SQUARE

    #region EDIT EDGE
    private int DisplayGizmoEditEdgeInScene(Vector3 intersectPos)
    {
        int edgeIndex = -1;
        // Affichage du gizmo uniquement si on est dans la grille
        if (TestIfPositionIsInLimit(intersectPos))
        {
            edgeIndex = DrawEdgeSelectedGizmo(intersectPos);
        }
        SceneView.RepaintAll();
        return edgeIndex;
    }

    // Trouver sur internet ici : https://stackoverflow.com/questions/2049582/how-to-determine-if-a-point-is-in-a-2d-triangle
    private float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    // Fonction permettant de tester si un point est dans un triangle
    private bool IsInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        float d1, d2, d3;
        bool has_neg, has_pos;

        d1 = Sign(pt, v1, v2);
        d2 = Sign(pt, v2, v3);
        d3 = Sign(pt, v3, v1);

        has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(has_neg && has_pos);
    }

    // On recupère la position par rapport au square
    // On divise en 4 triangles le Square et on test si le point d'intersection est dans un des triangles
    // Si triangle du bas => edge du bas => return 0
    // Si triangle de gauche => edge de gauche => return 1
    // Si triangle du haut => edge du haut => return 2
    // Si triangle de droite => edge de droite => return 3
    // On retourne un int en fonction de quel edge on edit 
    private int DrawEdgeSelectedGizmo(Vector3 intersectPos)
    {
        // On recupère la partie decimal de la position.
        // Ceci pour pouvoir tester la position de la souris 
        // dans un square à la position (0,0).
        float xDecimal = intersectPos.x - (int)intersectPos.x;
        float zDecimal = intersectPos.z - (int)intersectPos.z;

        // On recupère la position en int
        Vector3 posEdge = Vector3.zero;
        posEdge.x = (int)intersectPos.x;
        posEdge.z = (int)intersectPos.z;

        // On declare une variable int pour recupérer
        // quelle edge on edit : bas (0), gauche (1), haut (2), droite (3) 
        int edgeOrientation = 0;
        Vector3 scaleWireSquare = Vector3.one / 10;

        // Edge du bas
        if (IsInTriangle(new Vector2(xDecimal, zDecimal), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0.5f)))
        {
            posEdge.x += 0.5f;
            scaleWireSquare.x = 1;
        }
        // Edge de gauche
        else if (IsInTriangle(new Vector2(xDecimal, zDecimal), new Vector2(0, 0), new Vector2(0, 1), new Vector2(0.5f, 0.5f)))
        {
            posEdge.z += 0.5f;
            edgeOrientation = 1;
            scaleWireSquare.z = 1;
        }
        // Edge de droite
        else if (IsInTriangle(new Vector2(xDecimal, zDecimal), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f)))
        {
            posEdge.x += 1.0f;
            posEdge.z += 0.5f;
            edgeOrientation = 3;
            scaleWireSquare.z = 1;
        }
        // Edge du haut
        else if (IsInTriangle(new Vector2(xDecimal, zDecimal), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 0.5f)))
        {
            posEdge.x += 0.5f;
            posEdge.z += 1f;
            edgeOrientation = 2;
            scaleWireSquare.x = 1;
        }

        Handles.color = _IsAddOrRemoveEdge ? Color.cyan : Color.red;
        Handles.DrawWireCube(posEdge, scaleWireSquare);
        return edgeOrientation;
    }

    // Edition des dats l'edge montrée par le gizmos
    // EdgeOrientation est l'edge du square  editée (bas, gauche, haut, droite)
    // IntersectPos est la position du square
    private void EditCurrentEdge(int edgeOrientation, Vector3 intersectPos)
    {
        if (EditInputTriggered())
        {
            // Edit les datas de l'edge en fontion de
            // son orientation
            switch (edgeOrientation)
            {
                // Edges Hori
                case 0:
                    _targetMapManager.MapData.SetEdgeData(true, intersectPos, _IsAddOrRemoveEdge);
                    break;
                case 2:
                    _targetMapManager.MapData.SetEdgeData(true, intersectPos + new Vector3(0, 0, 1), _IsAddOrRemoveEdge);
                    break;
                // Edges Vert
                case 1:
                    _targetMapManager.MapData.SetEdgeData(false, intersectPos, _IsAddOrRemoveEdge);
                    break;
                case 3:
                    _targetMapManager.MapData.SetEdgeData(false, intersectPos + new Vector3(1, 0, 0), _IsAddOrRemoveEdge);
                    break;
            }

            // On update la vue de la map
            GenerateMapView();

            // On force la Scene Unity en état "Editée"
            SetObjectDirty(_targetMapManager);
        }
    }
    #endregion

    #region BRUSH
    private void DisplayGizmoShape(Vector3 intersectPos, Vector3[] allFigurePos)
    {
        foreach (Vector3 pos in allFigurePos)
        {
            // On oriente en fonction de l'orientation de la brush
            Vector3 posBrushOriented = SetByBrushOrientation(pos);

            // On affiche le gizmo
            DrawEditSquareGizmo(intersectPos + posBrushOriented);
        }
    }

    private Vector3 SetByBrushOrientation(Vector3 toOrient)
    {
        return Quaternion.Euler(0, _currentBrushOrientation, 0) * toOrient;
    }

    private void UpdateBrushOrientation()
    {
        if (Event.current.keyCode == KeyCode.R)
        {
            if (Event.current.type == EventType.KeyDown)
            {
                _currentBrushOrientation += 90;
                // Lorsque la valuer sera egal 360 ou plus 
                // On redemarre de 0;
                _currentBrushOrientation %= 360;
            }
        }
    }

    private void ProcessBrushEdit(Vector3 intersectPos)
    {
        switch (_currentSquareBrush)
        {
            case SquareBrush.Line:
                EditSquareByShape(intersectPos, _brushArrayLine);
                break;
            case SquareBrush.Cube:
                EditSquareByShape(intersectPos, _brushArrayCube);
                break;
            case SquareBrush.L:
                EditSquareByShape(intersectPos, _brushArrayL);
                break;
        }
    }

    private void EditSquareByShape(Vector3 intersectPos, Vector3[] allFigurePos)
    {
        foreach (Vector3 pos in allFigurePos)
        {
            // On oriente en fonction de l'orientation de la brush
            Vector3 posBrushOriented = SetByBrushOrientation(pos);

            // On affiche le gizmo
            _targetMapManager.MapData.SetSquareData(intersectPos + posBrushOriented, (SquareState)_squareStateMode);
        }
    }
    #endregion BRUSH

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
        _currentSquareBrush = (SquareBrush)EditorPrefs.GetInt(nameof(_currentSquareBrush));
        _IsInEditEdgeMode = EditorPrefs.GetBool(nameof(_IsInEditEdgeMode));
        _IsAddOrRemoveEdge = EditorPrefs.GetBool(nameof(_IsAddOrRemoveEdge));
    }

    private void SavePrefsEditor()
    {
        EditorPrefs.SetBool(nameof(_isInSquareStateEditMode), _isInSquareStateEditMode);
        EditorPrefs.SetInt(nameof(_squareStateMode), _squareStateMode);
        EditorPrefs.SetInt(nameof(_currentSquareBrush), (int)_currentSquareBrush);

        EditorPrefs.SetBool(nameof(_IsInEditEdgeMode), _IsInEditEdgeMode);
        EditorPrefs.SetBool(nameof(_IsAddOrRemoveEdge), _IsAddOrRemoveEdge);
    }
    #endregion  SAVE / LOAD / DIRTY

    #region GENERIQUE METHODE
    private static Transform GetChildByName(GameObject parent, string name)
    {
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            Transform child = parent.transform.GetChild(i);
            if (child.gameObject.name == name)
            {
                return child;
            }
        }
        return null;
    }

    private void DetroyAllChilds(GameObject parent)
    {
        if (parent != null)
        {
            for (int i = parent.transform.childCount; i > 0; i--)
            {
                DestroyImmediate(parent.transform.GetChild(0).gameObject);
            }
        }
    }

    private bool CalculateInteractPosition(out Vector3 interactPosition)
    {
        // Recuperation de la postion de la souris dans la vue de la scene
        Vector3 mousePosition = Event.current.mousePosition;

        // recupération d'un Ray (rayon) à partir de la position de la mouse sur l'ecran
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

        // Creation d'un plan dans l'espace
        // Il n'y a pas de plan créer dans la scene
        // Distance donnera la distance entre le point d'intersection du plan et le rayon et l'origine du rayon
        Plane hPlane = new Plane(Vector3.up, Vector3.zero);

        // On envoi le rayon par rapport à la scene
        // Raycast entre le plan et le rayon 
        if (hPlane.Raycast(ray, out float distance))
        {
            // get the hit point:
            interactPosition = ray.GetPoint(distance);
            return true;
        }
        interactPosition = Vector3.zero;
        return false;
    }

    private bool TestIfPositionIsInLimit(Vector3 position)
    {
        return position.x >= 0 && position.z >= 0 && position.x < _targetMapManager.MapData.Width && position.z < _targetMapManager.MapData.Height;
    }

    private Vector3 CalculateInteractPosition()
    {
        Vector3 mousePosition = Event.current.mousePosition;

        // recupération d'un Ray (rayon) à partir de la position de la mouse sur l'ecran
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

        // Creation d'un plan dans l'espace
        // Il n'y a pas de plan créer dans la scene
        Plane hPlane = new Plane(Vector3.up, Vector3.zero);

        // On envoi le rayon par rapport à la scene
        if (hPlane.Raycast(ray, out float distance))
        {
            // get the hit point:
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    private bool EditInputTriggered()
    {
        if (Event.current.button == 0)
        {
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                case EventType.MouseDrag:
                    return true;
            }
        }
        return false;
    }

    private Vector3 GetRoundPos(Vector3 intersectPos)
    {
        Vector3 intersectPosInt = Vector3.zero;
        intersectPosInt.x = Mathf.FloorToInt(intersectPos.x);
        intersectPosInt.z = Mathf.FloorToInt(intersectPos.z);
        return intersectPosInt;
    }
    #endregion GENERIQUE METHODE

}
