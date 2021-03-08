﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class MapManager : MonoBehaviour
{
    // Data
    [SerializeField]
    private MapData _mapData;

    public MapData MapData { get { return _mapData; } }

    // Debug
    public bool displayDebugView = true;

    // Debug View
    public GameObject _surfaceView;

    [ContextMenu("GenerateMap")]
    public void GenerateMap()
    {
        _mapData.ResetMap();

        //_mapData.GenSquareRandom();
    }

    private void OnDrawGizmos()
    {
        if (displayDebugView)
        {
            Vector3 sizeSquareMap = new Vector3(0.8f, 0.2f, 0.8f);
            for (int i = 0; i < _mapData.Width; i++)
            {
                for (int j = 0; j < _mapData.Height; j++)
                {
                    SquareData currentSquareData = _mapData.GetSquareData(i, j);

                    // State Square
                    Gizmos.color = SquareData.GetColorFromState(currentSquareData.State);
                    Gizmos.DrawWireCube(new Vector3(i + 0.5f, 0, j + 0.5f), sizeSquareMap);

                    // Alignment
                    Gizmos.color = SquareData.GetColorFromAlignment(currentSquareData.Alignment);
                    Gizmos.DrawSphere(new Vector3(i + 0.75f, 0.1f, j + 0.75f), 0.1f);
                }
            }
        }
    }
}
