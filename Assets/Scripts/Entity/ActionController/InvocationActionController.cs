using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvocationActionController : ActionController
{
#pragma warning disable 0649
    [SerializeField]
    private InvocationActionData _invocationActionData;

    public InvocationActionData InvocationActionData { get { return _invocationActionData; } }
#pragma warning restore 0649

    public List<GameObject> spawnPoints;

    public override ActionData GetData()
    {
        return _invocationActionData;
    }

    protected override void DoAction()
    {
        for(int i = 0; i < _invocationActionData.NumberToInvoc;  i++)
        {
            EntityManager.Instance.CreateEntity(GetPositionToInvoc(), _invocationActionData.EntityToInvoc);
        }
        ResetAction();
    }

    private Vector3 GetPositionToInvoc()
    {
        if(spawnPoints.Count > 0)
        {
            return spawnPoints[Random.Range(0, spawnPoints.Count)].transform.position;
        }
        return transform.position;
    }
}
