using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestEntity : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;

    public Transform target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            navMeshAgent.SetDestination(target.position);
        }
    }
}
