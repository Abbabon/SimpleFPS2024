using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _navMeshAgent;

    private void Update()
    {
        _navMeshAgent.SetDestination(PlayerController.Instance.transform.position);
    }
}