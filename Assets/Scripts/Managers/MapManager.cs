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
    public GameObject prefabEdgeHori;
    public GameObject prefabEdgeVert;
#endif

    [ContextMenu("GenerateMap")]
    public void ResetMapData()
    {
        _mapData.ResetMap();
    }

    private void OnDrawGizmos()
    {
        if (displayDebugView)
        {
            ShowGizmoMapSquares();

            ShowGizmoMapEdges();
        }
    }

    private void ShowGizmoMapSquares()
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

    private void ShowGizmoMapEdges()
    {
        Vector3 pos = Vector3.zero;
        Vector3 scaleHori = (Vector3.one) / 10;
        scaleHori.x = 0.8f;

        if (_mapData.EdgesHori != null)
        {
            // Parcours des élements du tableau via un for.
            for (int i = 0; i < _mapData.EdgesHori.Length; i++)
            {
                if (_mapData.EdgesHori[i])
                {
                    pos = MapData.GetPositionFromIndex(i, _mapData.Width);
                    Gizmos.color = Color.red;
                    pos.x += 0.5f;
                    Gizmos.DrawCube(pos, scaleHori);
                }
            }
        }

        if (_mapData.EdgesVert != null)
        {
            Vector3 scaleVert = (Vector3.one) / 10;
            scaleVert.z = 0.8f;
            for (int i = 0; i < _mapData.EdgesVert.Length; i++)
            {
                if (_mapData.EdgesVert[i])
                {
                    Gizmos.color = Color.blue;
                    pos = MapData.GetPositionFromIndex(i, _mapData.Width + 1);
                    pos.z += 0.5f;
                    Gizmos.DrawCube(pos, scaleVert);
                }
            }
        }
    }
}
