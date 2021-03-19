using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Camera currentCamera;

    private EntityManager _entityManager;

    public EntityData entityData;

    // Start is called before the first frame update
    void Start()
    {
        //_entityManager = Singleton<EntityManager>.Instance;
        _entityManager = EntityManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;

            Vector3 originRay = currentCamera.ScreenToWorldPoint(mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(originRay, currentCamera.transform.forward, out hit, 100, LayerMask.GetMask("Ground")))
            {
                Debug.DrawRay(originRay, currentCamera.transform.forward * 100, Color.green);

                if (entityData)
                {
                    _entityManager.CreateEntity(hit.point, entityData);
                }
            }
        }
    }
}
