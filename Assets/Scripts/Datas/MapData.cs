using System;
using UnityEngine;

[Serializable]
public class MapData
{
    [SerializeField]
    private int _width;

    [SerializeField]
    private int _height;

    [SerializeField]
    private SquareData[] _grid;

    public MapData(int width, int height)
    {
        _grid = new SquareData[width * height];
    }
}
