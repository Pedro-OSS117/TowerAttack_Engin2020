using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Camera currentCamera;

    private EntityManager _entityManager;

    public GameObject placement3DUI;

    private MapManager _mapManager;

    public Deck deck;

    [SerializeField]
    private int _currentIndexDeck = -1;

    // Start is called before the first frame update
    void Start()
    {
        //_entityManager = Singleton<EntityManager>.Instance;

        currentCamera = FindObjectOfType<Camera>();
        _entityManager = EntityManager.Instance;

        _mapManager = FindObjectOfType<MapManager>();
        if (!_mapManager)
        {
            Debug.LogError("NO MAP MANAGER");
        }

        UpdateDropZoneView();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            _currentIndexDeck++;
            if(_currentIndexDeck >= deck.Entities.Count)
            {
                _currentIndexDeck = -1;
            }
            UpdateDropZoneView();
        }

        if(_currentIndexDeck != -1)
        {
            Vector3 mousePosition = Input.mousePosition;

            Ray ray = currentCamera.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("Ground")))
            {
                Debug.DrawRay(ray.origin, ray.direction * 100, Color.green);

                // Vue de la possibilité de placement
                placement3DUI.transform.position = hit.point;

                bool isMouseInPlayerZone = _mapManager.TestIsAlignement(hit.point, Alignment.Player);

                Renderer render = placement3DUI.GetComponentInChildren<Renderer>();
                render.material.color = _mapManager.GetColorFromAlignement(isMouseInPlayerZone ? Alignment.Player : Alignment.IA);

                // Creation d'entité
                if (Input.GetMouseButtonDown(0))
                {
                    if (isMouseInPlayerZone)
                    {
                        _entityManager.CreateEntity(hit.point, deck.Entities[_currentIndexDeck]);
                    }
                }
            }
        }
    }

    private void UpdateDropZoneView()
    {
        bool isDisplayed = _currentIndexDeck != -1;
        _mapManager.DisplayDropFeedBack(isDisplayed);
        placement3DUI.SetActive(isDisplayed);
    }
}
