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

    // Drop Zone FeedBack
    public GameObject squareFBDropZonePrefab;
    private GameObject m_dropZoneFBContainer;

    [ContextMenu("GenerateMap")]
    public void ResetMapData()
    {
        _mapData.ResetMap();
    }

    public void Awake()
    {
        CreateFeebBackContainer();

        SetAlignementZone(Alignment.Player, Vector3.zero, _mapData.Width, _mapData.Height / 2);

        Vector3 origin = new Vector3(0, 0, _mapData.Height / 2);
        if(_mapData.Height % 2 != 0)
        {
            origin.z += 1;
        }
        SetAlignementZone(Alignment.IA, origin, _mapData.Width, _mapData.Height / 2);
    }

    #region ALIGNMENT
    public bool TestIsAlignement(Vector3 pos, Alignment alignment)
    {
        int index = _mapData.GetIndexFromPos(pos.x, pos.z);
        if (index != -1)
        {
            return _mapData.IsAlignment(index, alignment);
        }
        return false;
    }

    private void SetAlignement(Alignment alignment, Vector3 pos)
    {
        int index = _mapData.GetIndexFromPos(pos.x, pos.z);
        if (index != -1)
        {
            SetAlignement(alignment, index);
        }
    }

    private void SetAlignement(Alignment alignment, int indexSquare)
    {
        _mapData.SetAlignment(indexSquare, alignment);

        UpdateViewAlignement(alignment, indexSquare);
    }

    public void SetAlignementZone(Alignment alignment, Vector3 origin, int width, int height)
    {
        Vector3 tmpPos = origin;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tmpPos = origin + new Vector3(i, 0, j);
                int index = _mapData.GetIndexFromPos(tmpPos.x, tmpPos.z);
                if (index != -1)
                {
                    SetAlignement(alignment, index);
                }
            }
        }
    }

    public void SetAlignementTopRightOrLeftZone(Alignment alignment, bool isLeft)
    {
        Vector3 origin = new Vector3(0, 0, _mapData.Height / 2);
        if (!isLeft)
        {
            origin = new Vector3(_mapData.Width / 2, 0, _mapData.Height / 2);
        }
        Vector3 tmpPos = origin;
        int width = _mapData.Width / 2;
        int height = _mapData.Height / 3;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tmpPos = origin + new Vector3(i, 0, j);
                int index = _mapData.GetIndexFromPos(tmpPos.x, tmpPos.z);
                if (index != -1)
                {
                    SetAlignement(alignment, index);
                }
            }
        }
    }
    #endregion ALIGNMENT

    #region ALIGNEMENT DROP ZONE FEEDBACK
    private void CreateFeebBackContainer()
    {
        m_dropZoneFBContainer = new GameObject(nameof(m_dropZoneFBContainer));
        m_dropZoneFBContainer.transform.SetParent(transform);
        for (int j = 0; j < _mapData.Height; j++)
        {
            for (int i = 0; i < _mapData.Width; i++)
            {
                GameObject newSquareFB = Instantiate(squareFBDropZonePrefab);
                newSquareFB.transform.SetParent(m_dropZoneFBContainer.transform);
                newSquareFB.transform.position = new Vector3(i + 0.5f, 0.05f, j + 0.5f);
            }
        }
    }

    public bool TestIfCanDropAtPos(Vector3 pos)
    {
        if (TestIsAlignement(pos, Alignment.Player))
        {
            int index = _mapData.GetIndexFromPos(pos.x, pos.z);
            if (index != -1)
            {
                return _mapData.IsNotState(index, SquareState.Wall, SquareState.Water);
            }
        }
        return false;
    }

    public void DisplayDropFeedBack(bool enable)
    {
        m_dropZoneFBContainer.SetActive(enable);
    }

    private void UpdateViewAlignement(Alignment alignment, int indexSquare)
    {
        if (m_dropZoneFBContainer && m_dropZoneFBContainer.transform.childCount > indexSquare)
        {
            if (_mapData.IsOneOfState(indexSquare, SquareState.Wall, SquareState.Water))
            {
                m_dropZoneFBContainer.transform.GetChild(indexSquare).gameObject.SetActive(false);
            }
            else
            {
                MeshRenderer mr = m_dropZoneFBContainer.transform.GetChild(indexSquare).GetComponent<MeshRenderer>();
                Color newColor = GetColorFromAlignement(alignment);
                newColor.a = 0.75f;
                mr.material.color = newColor;
            }
        }
    }
    #endregion ALIGNEMENT DROP ZONE FEEDBACK

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

    public Color GetColorFromAlignement(Alignment alignement)
    {
        switch (alignement)
        {
            case Alignment.IA:
                return Color.red;
            case Alignment.Player:
                return Color.blue;
            default:
                return Color.white;
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
