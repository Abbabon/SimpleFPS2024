using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private static readonly int Dead = Animator.StringToHash("Dead");
    
    [SerializeField] private Animator _animator;
    [SerializeField] private NavMeshAgent _navMeshAgent;
    [SerializeField] private int _initialHealth = 3;
    [SerializeField] private GameObject _virtualCamera;

    public GameObject VirtualCamera => _virtualCamera;
    public bool Alive => _alive;
    
    private int _currentHealth;
    private bool _alive = true;

    private void Awake()
    {
        _virtualCamera.SetActive(false);
        EnemySpawner.Instance.RegisterEnemy(this);
    }

    public void Reset()
    {
        _animator.SetBool(Dead, false);
        _alive = true;
        _currentHealth = _initialHealth;
    }
    
    private void Update()
    {
        if (!_alive) 
            return;
        
        _navMeshAgent.SetDestination(PlayerController.Instance.transform.position);
    }
    
    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        _navMeshAgent.isStopped = true;
        _alive = false;
        _animator.SetBool("Dead", true);
        EnemySpawner.Instance.UnregisterEnemy(this);
    }
}