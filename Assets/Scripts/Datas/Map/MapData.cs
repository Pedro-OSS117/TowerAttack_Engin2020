using System;
using UnityEngine;

[Serializable]
public class MapData
{
    [SerializeField]
    [Range(1, 50)]
    private int _width;

    [SerializeField]
    [Range(1, 50)]
    private int _height;

    [SerializeField]
    private SquareData[] _grid;

    [Header("Edges Properties")]
    [SerializeField]
    private bool[] _edgesHori;

    [SerializeField]
    private bool[] _edgesVert;

    public int Width
    {
        get { return _width; }
        set { _width = value; }
    }

    public int Height
    {
        get { return _height; }
        set { _height = value; }
    }

    public SquareData[] Grid { get { return _grid; } }
    public bool[] EdgesHori { get { return _edgesHori; } }
    public bool[] EdgesVert { get { return _edgesVert; } }

    public void ResetMap()
    {
        _grid = new SquareData[_width * _height];

        _edgesHori = new bool[_width * (_height + 1)];
        _edgesVert = new bool[(_width + 1) * _height];
    }

    public void GenSquareRandom()
    {
        SquareState[] squareStates = (SquareState[])Enum.GetValues(typeof(SquareState));
        Alignment[] alignments = (Alignment[])Enum.GetValues(typeof(Alignment));
        for (int i = 0; i <_grid.Length; i++)
        {
            _grid[i].State = squareStates[UnityEngine.Random.Range(0, squareStates.Length)];
            _grid[i].Alignment = alignments[UnityEngine.Random.Range(0, alignments.Length)];
        }
    }

    public int GetIndexFromPos(float x, float z)
    {
        if(IsInGrid(x, z))
        {
            return ((int)z) * Width + (int)x;
        }
        Debug.LogWarning("Not In Grid " + x + " : " + z);
        return -1;
    }

    public SquareData GetSquareData(float x, float z)
    {
        int index = GetIndexFromPos(x, z);
        if(index != -1 && index < _grid.Length)
        {
            return _grid[index];
        }
        else
        {
            Debug.Log($"Error index not exist at {x}, {z}");
            return default(SquareData);
        }
    }

    public void SetSquareData(Vector3 position, SquareState newSquareState)
    {
        int indexSquare = GetIndexFromPos(position.x, position.z);
        _grid[indexSquare].State = newSquareState;

        // Si etat wall
        if (newSquareState == SquareState.Wall)
        {
            // Edges Hori
            // Edge du bas
            SetEdgeData(true, position, false);
            // Edge du haut
            SetEdgeData(true, position + new Vector3(0, 0, 1), false);
            // Edges Vert
            // Edge de gauche
            SetEdgeData(false, position, false);
            // Edge de droite
            SetEdgeData(false, position + new Vector3(1, 0, 0), false);

        }
    }

    public bool IsNotState(int index, params SquareState[] states)
    {
        foreach (SquareState state in states)
        {
            if (_grid[index].State == state)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsOneOfState(int index, params SquareState[] states)
    {
        foreach (SquareState state in states)
        {
            if (_grid[index].State == state)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Return index if position is in Grid.
    /// Return -1 instead.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private int GetIndexSquareFromPos(Vector3 pos)
    {
        // Test si la position sort des limites
        if (IsInGrid(pos))
        {
            return ((int)pos.z * _width) + (int)pos.x;
        }
        // Pas de square à la position demandée => on retourne -1
        return -1;
    }

    public bool IsInGrid(Vector3 position)
    {
        return IsInGrid(position.x, position.z);
    }

    public bool IsInGrid(float x, float z)
    {
        return x >= 0 && x < _width && z >= 0 && z < _height;
    }

    // Modifie les datas de l'edge a la position : position
    public void SetEdgeData(bool isHori, Vector3 position, bool isEnable)
    {
        if (isHori)
        {
            // On recupère l'index dans les edges horizontales.
            int index = GetIndexEdgeFromPos(position, _width, _height + 1);
            if (index != -1)
            {
                // On set l'etat de l'edge.
                _edgesHori[index] = isEnable;
                // Si on ajoute une edge?
                if (isEnable)
                {
                    // On supprime les squares sur les côtés adjacents.
                    RemoveSquareAroundEdge(index, _width, new Vector3(0, 0, -1));
                }
            }
        }
        else
        {
            // On recupère l'index dans les edges verticales.
            int index = GetIndexEdgeFromPos(position, _width + 1, _height);
            if (index != -1)
            {
                // On set l'etat de l'edge.
                _edgesVert[index] = isEnable;
                // Si on ajoute une edge?
                if (isEnable)
                {
                    // On supprime les squares sur les côtés adjacents.
                    RemoveSquareAroundEdge(index, _width + 1, new Vector3(-1, 0, 0));
                }
            }
        }
    }

    public void RemoveSquareAroundEdge(int index, int width, Vector3 adderTestLock)
    {
        // On recupere la position de la edge en fonction de l'index.
        Vector3 newPosSquare = GetPositionFromIndex(index, width);

        // On test à cette position s'il y a un Square Lock.
        ValidateIfNoLockSquare(newPosSquare);

        // Et on test aussi à la position "derrière" la edge.
        newPosSquare += adderTestLock;
        ValidateIfNoLockSquare(newPosSquare);
    }

    private int GetIndexEdgeFromPos(Vector3 pos, int width, int height)
    {
        // Test si la position sort des limites
        if (IsInGrid(pos))
        {
            return (int)pos.z * width + (int)pos.x;
        }
        // Pas de square à la position demandée => on retourne -1.
        return -1;
    }

    public static Vector3 GetPositionFromIndex(int i, int width)
    {
        Vector3 pos = Vector3.zero;
        // Calcul des coordonnées du square dans la grid à partir de l'index dand le tableau.
        // Calcul de la position en x => le reste de la division
        pos.x = i % width;
        // Calcul de la position en z => partie entière de la division  
        pos.z = i / width;
        return pos;
    }

    private void ValidateIfNoLockSquare(Vector3 newPosSquare)
    {
        // On recupere l'index du square en fonction de la position.
        int indexSquare = GetIndexSquareFromPos(newPosSquare);
        // Si le square existe.
        if (indexSquare != -1)
        {
            // On Test si il est lock.
            if (_grid[indexSquare].State == SquareState.Wall)
            {
                // On Set son state.
                _grid[indexSquare].State = SquareState.Normal;

                // On ajoute le debug
                //debugLockRemoved.Add(newPosSquare);
            }
        }
    }

    public bool IsAlignment(int index, Alignment alignment)
    {
        return _grid[index].Alignment == alignment;
    }

    public void SetAlignment(int index, Alignment alignment)
    {
        _grid[index].Alignment = alignment;
    }
}
