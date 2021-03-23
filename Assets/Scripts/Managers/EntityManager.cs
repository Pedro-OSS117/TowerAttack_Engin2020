using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EntityManager : Singleton<EntityManager>
{
    private Dictionary<EntityData, GameObject> _allLoadedPrefabsDico;

    public GameObject globalTargetPlayer, globalTargetIA;

    public int numberKilledIA = 0;

    public GameObject leftTowerIA, rightTowerIA;

    private MapManager _mapManager;

    public void Awake()
    {
        _allLoadedPrefabsDico = new Dictionary<EntityData, GameObject>();
    }

    public void Start()
    {
        _mapManager = FindObjectOfType<MapManager>();
        if (!_mapManager)
        {
            Debug.LogError("NO MAP MANAGER");
        }
    }

    public void CreateEntity(Vector3 point, EntityData entityToCreate)
    {
        GameObject prefabEntity = GetPrefabFromEntityDataBy(entityToCreate);
        if (prefabEntity)
        {
            GameObject newEntity = Instantiate(prefabEntity, point, Quaternion.identity, transform);
            EntityController entityController = newEntity.GetComponent<EntityController>();
            if(entityController is EntityMoveableController)
            {
                EntityMoveableController moveable = (EntityMoveableController)entityController;
                if(moveable.Alignment == Alignment.IA)
                {
                    moveable.globalTarget = globalTargetPlayer;
                }
                else
                {
                    moveable.globalTarget = globalTargetIA;
                }
            }

        }
        else
        {
            Debug.LogError("NO PREFAB " + entityToCreate.NamePrefab + " FOR ENTITY : " + entityToCreate.name , entityToCreate);
        }
    }

    public void DestroyEntity(GameObject toDestroy)
    {
        EntityController entityController = toDestroy.GetComponent<EntityController>();
        if (entityController != null && entityController.Alignment == Alignment.IA)
        {
            numberKilledIA++;
        }

        // Verification si entité clef pour fin du jeu
        bool isEndGame = false;
        if(toDestroy == globalTargetIA)
        {
            Debug.Log("WIN!!!!!!!");
            isEndGame = true;
        }
        else if(toDestroy == globalTargetPlayer)
        {
            Debug.Log("LOSE!!!!!!!");
            isEndGame = true;
        }

        if(leftTowerIA == toDestroy)
        {
            Vector3 origin = Vector3.zero;
            origin.x = 0;
            origin.z = _mapManager.GetHalfHeightMap();
            _mapManager.SetAlignementZone(Alignment.Player, origin, _mapManager.GetHalfWidthtMap(), _mapManager.GetHalfHeightMap() / 2);
        }
        else if(rightTowerIA == toDestroy)
        {
            Vector3 origin = Vector3.zero;
            origin.x = _mapManager.GetHalfWidthtMap();
            origin.z = _mapManager.GetHalfHeightMap();
            _mapManager.SetAlignementZone(Alignment.Player, origin, _mapManager.GetHalfWidthtMap(), _mapManager.GetHalfHeightMap() / 2);
        }

        // Destruction de l'entité
        Destroy(toDestroy);

        if (isEndGame)
        {
            Debug.Log($"You have killed {numberKilledIA} !");

            // Chargement de le suite si fin de jeu
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private GameObject GetPrefabFromEntityDataBy(EntityData entityData)
    {
        if(_allLoadedPrefabsDico.ContainsKey(entityData))
        {
            return _allLoadedPrefabsDico[entityData];
        }

        // Si on ne l'a pas trouvé dans la liste on  la load
        string path = "Entity/" + entityData.NamePrefab;
        GameObject prefabEntity = Resources.Load<GameObject>(path);

        _allLoadedPrefabsDico.Add(entityData, prefabEntity);

        return prefabEntity;
    }
}
