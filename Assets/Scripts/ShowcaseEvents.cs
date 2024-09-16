using System;
using UnityEngine;

public class ShowcaseEvents : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            EnemySpawner.Instance.EnableEnemySpawns();
        }
    }
}