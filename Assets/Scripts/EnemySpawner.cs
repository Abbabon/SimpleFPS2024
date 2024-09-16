using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

public class EnemySpawner : Singleton<EnemySpawner>
{
    [SerializeField] private bool _enabled;
    [SerializeField] private bool _shouldSpawn = true;
    [SerializeField] private float _spawnCooldown;
    [SerializeField] private Enemy _enemyObjectPool;
    [SerializeField] private List<SpawnLocation> _spawnLocations;
    
    private List<Enemy> _activeEnemies = new();
    
    private WaitForSeconds _spawnCooldownWaitForSeconds;
    private BestObjectPool<Enemy> _enemyPool;

    private void Start()
    {
        _spawnCooldownWaitForSeconds = new WaitForSeconds(_spawnCooldown);
        _enemyPool = new BestObjectPool<Enemy>(_enemyObjectPool, 10);
    }

    private void Update()
    {
        if (!_enabled || !_shouldSpawn) return;
        SpawnEnemy();
        StartCoroutine(CooldownCoroutine());
    }
    
    private IEnumerator CooldownCoroutine()
    {
        _shouldSpawn = false;
        yield return _spawnCooldownWaitForSeconds;
        _shouldSpawn = true;
    }

    private void SpawnEnemy()
    {
        var newEnemy = _enemyPool.Get();
        newEnemy.Reset();
        var randomSpawnLocation = _spawnLocations[UnityEngine.Random.Range(0, _spawnLocations.Count)];
        newEnemy.transform.position = randomSpawnLocation.transform.position;
    }

    private void OnEnemyHit(Enemy enemy)
    {
        _enemyPool.Release(enemy);
    }
    
    public void EnableEnemySpawns()
    {
        _enabled = true;
    }

    public void RegisterEnemy(Enemy enemy)
    {
        _activeEnemies.Add(enemy);
    }
    
    public void UnregisterEnemy(Enemy enemy)
    {
        _activeEnemies.Remove(enemy);
    }
    
    public GameObject GetPOVCamera()
    {
        return _activeEnemies.FirstOrDefault()?.VirtualCamera;
    }
}