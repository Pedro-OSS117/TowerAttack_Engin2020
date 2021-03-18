using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class MapManager : MonoBehaviour
{
#pragma warning disable 0649
    // Data
    [SerializeField]
    private MapData _mapData;
#pragma warning restore 0649

    public MapData MapData { get { return _mapData; } }

#if UNITY_EDITOR
    // Debug
    public bool displayDebugView = true;

    // Debug View
    public GameObject _surfaceView;
    public GameObject _wallView;
    public GameObject _waterView;
#endif

    [ContextMenu("GenerateMap")]
    public void ResetMapData()
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
