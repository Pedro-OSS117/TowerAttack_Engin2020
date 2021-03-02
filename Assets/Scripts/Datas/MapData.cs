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

    public int Width { get { return _width; } }

    public int Height { get { return _height; } }

    public SquareData[] Grid { get { return _grid; } }

    public void ResetMap()
    {
        _grid = new SquareData[_width * _height];
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
        return ((int)z) * Width + (int)x;
    }

    public SquareData GetSquareData(float x, float z)
    {
        int index = GetIndexFromPos(x, z);
        if(index < _grid.Length)
        {
            return _grid[index];
        }
        else
        {
            Debug.Log($"Error index not exist at {x}, {z}");
            return default(SquareData);
        }
    }
}
