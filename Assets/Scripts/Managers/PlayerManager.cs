using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Camera currentCamera;

    private EntityManager _entityManager;

    public EntityData entityData;

    public GameObject placement3DUI;

    private MapManager _mapManager;

    // Start is called before the first frame update
    void Start()
    {
        //_entityManager = Singleton<EntityManager>.Instance;
        _entityManager = EntityManager.Instance;

        _mapManager = FindObjectOfType<MapManager>();
        if (!_mapManager)
        {
            Debug.LogError("NO MAP MANAGER");
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;

        Ray ray = currentCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("Ground")))
        {
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.green);

            // Vue de la possibilité de placement
            placement3DUI.SetActive(true);
            placement3DUI.transform.position = hit.point;

            bool isMouseInPlayerZone = _mapManager.TestIsAlignement(hit.point, Alignment.Player);

            Renderer render = placement3DUI.GetComponentInChildren<Renderer>();
            render.material.color = _mapManager.GetColorFromAlignement(isMouseInPlayerZone ? Alignment.Player : Alignment.IA);

            // Creation d'entité
            if (Input.GetMouseButtonDown(0) && entityData)
            {
                if (isMouseInPlayerZone)
                {
                    _entityManager.CreateEntity(hit.point, entityData);
                }
            }
        }
        else
        {
            placement3DUI.SetActive(false);
        }
    }
}
