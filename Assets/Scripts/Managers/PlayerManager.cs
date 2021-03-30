using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class PlayerManager : MonoBehaviour
{
    public Camera currentCamera;

    private EntityManager _entityManager;

    public GameObject placement3DUI;

    private MapManager _mapManager;

    private PlayerUIManager _playerUIManager;

    public Deck deck;

    [SerializeField]
    private int _currentIndexDeck = -1;

    [SerializeField]
    private float _currentStamina = 0;
    private float _maxStamina = 10;

    private bool _isMouseInPlayerZone = false;
    private Vector3 _positionToDrop;

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

        _playerUIManager = GetComponentInChildren<PlayerUIManager>();
        _playerUIManager.InitializePopButtons(deck, OnPopButtonDown, OnPopButtonUp);

        UpdateDropZoneView();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateStamina();

        UpdateInputPlayer();

        UpdateButtonState();
    }

    private void UpdateStamina()
    {
        if(_currentStamina < _maxStamina)
        {
            _currentStamina += Time.deltaTime;
        }
        else
        {
            _currentStamina = _maxStamina;
        }

        _playerUIManager.UpdateStaminaLabel(Mathf.FloorToInt(_currentStamina));
    }

    private void UpdateDropZoneView()
    {
        bool isDisplayed = _currentIndexDeck != -1;
        _mapManager.DisplayDropFeedBack(isDisplayed);
        placement3DUI.SetActive(isDisplayed);
    }

    private void UpdateInputPlayer()
    {
        if (_currentIndexDeck != -1)
        {
            Vector3 mousePosition = Input.mousePosition;

            Ray ray = currentCamera.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("Ground")))
            {
                Debug.DrawRay(ray.origin, ray.direction * 100, Color.green);

                // Vue de la possibilité de placement
                placement3DUI.transform.position = hit.point;

                _isMouseInPlayerZone = _mapManager.TestIsAlignement(hit.point, Alignment.Player);

                Renderer render = placement3DUI.GetComponentInChildren<Renderer>();
                render.material.color = _mapManager.GetColorFromAlignement(_isMouseInPlayerZone ? Alignment.Player : Alignment.IA);

                _positionToDrop = hit.point;
            }
        }
    }

    private void DropEntity()
    {
        if (_isMouseInPlayerZone && _currentIndexDeck != -1)
        {
            _entityManager.CreateEntity(_positionToDrop, deck.Entities[_currentIndexDeck]);
        }
    }

    public void OnPopButtonDown(int index)
    {
        Debug.Log("OnPopButtonDown : " + index);

        _currentIndexDeck = index;
        UpdateDropZoneView();

    }

    public void OnPopButtonUp(int index)
    {
        Debug.Log("OnPopButtonUp : " + index);

        DropEntity();

        _currentStamina -= deck.Entities[index].PopAmount;

        UpdateButtonState();

        _currentIndexDeck = -1;
        UpdateDropZoneView();
    }

    private void UpdateButtonState()
    {
        _playerUIManager.UpdateStatePopButton(_currentStamina, deck);
    }
}
