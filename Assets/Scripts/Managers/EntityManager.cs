using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : Singleton<EntityManager>
{
    private Dictionary<EntityData, GameObject> _allLoadedPrefabsDico;

    public void Awake()
    {
        _allLoadedPrefabsDico = new Dictionary<EntityData, GameObject>();
    }

    public void CreateEntity(Vector3 point, EntityData entityToCreate)
    {
        Debug.Log("Create Entity");
        GameObject prefabEntity = GetPrefabFromEntityDataBy(entityToCreate);
        if (prefabEntity)
        {
            Instantiate(prefabEntity, point, Quaternion.identity, transform);
        }
        else
        {
            Debug.LogError("NO PREFAB " + entityToCreate.NamePrefab + " FOR ENTITY : " + entityToCreate.name , entityToCreate);
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
