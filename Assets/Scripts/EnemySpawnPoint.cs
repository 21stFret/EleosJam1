using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public bool spawnOnStart = false;
    public float spawnDelay = 0f;
    
    [Header("Visual")]
    public bool showGizmo = true;
    public Color gizmoColor = Color.yellow;
    public float gizmoSize = 0.5f;
    
    void Start()
    {
        if (spawnOnStart)
        {
            if (spawnDelay > 0)
            {
                Invoke(nameof(SpawnEnemy), spawnDelay);
            }
            else
            {
                SpawnEnemy();
            }
        }
    }
    
    public GameObject SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning($"No enemy prefab assigned to spawn point {gameObject.name}");
            return null;
        }
        
        GameObject spawnedEnemy = Instantiate(enemyPrefab, transform.position, transform.rotation);
        
        // Register with GameManager if it exists
        if (GameManager.Instance != null)
        {
            EnemyBaseClass enemy = spawnedEnemy.GetComponent<EnemyBaseClass>();
            if (enemy != null)
            {
                GameManager.Instance.RegisterEnemy(enemy);
            }
        }
        
        Debug.Log($"Spawned {enemyPrefab.name} at {transform.position}");
        return spawnedEnemy;
    }
    
    public GameObject SpawnEnemyWithPrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Attempted to spawn null prefab");
            return null;
        }
        
        GameObject spawnedEnemy = Instantiate(prefab, transform.position, transform.rotation);
        
        // Register with GameManager if it exists
        if (GameManager.Instance != null)
        {
            EnemyBaseClass enemy = spawnedEnemy.GetComponent<EnemyBaseClass>();
            if (enemy != null)
            {
                GameManager.Instance.RegisterEnemy(enemy);
            }
        }
        
        Debug.Log($"Spawned {prefab.name} at {transform.position}");
        return spawnedEnemy;
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmo) return;
        
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoSize);
        
        // Draw arrow pointing up
        Vector3 arrowStart = transform.position;
        Vector3 arrowEnd = transform.position + transform.up * (gizmoSize * 2);
        Gizmos.DrawRay(arrowStart, transform.up * (gizmoSize * 2));
        
        // Draw arrow head
        Vector3 right = transform.right * (gizmoSize * 0.3f);
        Vector3 left = -transform.right * (gizmoSize * 0.3f);
        Gizmos.DrawRay(arrowEnd, (left - transform.up * (gizmoSize * 0.3f)));
        Gizmos.DrawRay(arrowEnd, (right - transform.up * (gizmoSize * 0.3f)));
    }
}
