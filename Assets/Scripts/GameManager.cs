using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Enemy Spawning")]
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    public int enemiesPerWave = 5;
    public float spawnDelay = 1f;
    public bool spawnOnStart = true;

    [Header("Game State")]
    public bool gameStarted = false;
    public bool gameWon = false;
    public bool gamePaused = false;

    [Header("Events")]
    public UnityEvent OnGameStart;
    public UnityEvent OnGameWin;
    public UnityEvent OnEnemyDeath;
    public UnityEvent OnAllEnemiesSpawned;

    // Private variables
    private List<EnemyBaseClass> activeEnemies = new List<EnemyBaseClass>();
    private List<EnemyBaseClass> allSpawnedEnemies = new List<EnemyBaseClass>();
    private int totalEnemiesSpawned = 0;
    private int totalEnemiesKilled = 0;

    // Singleton instance
    public static GameManager Instance { get; private set; }

    public PlayerInput playerInput;
    public ExorcistCombat _exorcistCombat;
    public ExorcistLeveling exorcistLeveling;

    public float enemyWavetimer;
    public float enemyWaveTime;

    public GameUI gameUI;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }

        gameUI = GetComponent<GameUI>();
    }

    void Start()
    {
        if (playerInput != null)
        {
            playerInput.DeactivateInput();
            _exorcistCombat.Init();
        }
        if (spawnOnStart)
        {
            StartGame();
        }
    }

    private void Update()
    {
        // Handle input for pausing the game
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (gamePaused)
            {
                ResumeGame();
                if (gameUI != null)
                {
                    gameUI.HidePauseMenu();
                }
            }
            else
            {
                PauseGame();
                if (gameUI != null)
                {
                    gameUI.ShowPauseMenu();
                }
            }
        }

        if (gameStarted && !gamePaused)
        {
            // Update game logic here if needed
            enemyWavetimer += Time.deltaTime;
            if (enemyWavetimer >= enemyWaveTime)
            {
                enemyWavetimer = 0f;
                SpawnAdditionalWave(enemiesPerWave);
            }
        }
    }

    public void StartGame()
    {
        gameStarted = true;
        gameWon = false;
        gamePaused = false;

        Debug.Log("Game Started!");
        OnGameStart?.Invoke();
        Time.timeScale = 1f; // Ensure game is running at normal speed
        if (playerInput != null)
        {
            playerInput.ActivateInput();
            _exorcistCombat.Init();
        }

        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        if (enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("No enemy prefabs assigned to GameManager!");
            yield break;
        }

        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points assigned to GameManager!");
            yield break;
        }

        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnRandomEnemy();
            yield return new WaitForSeconds(spawnDelay);
        }

        OnAllEnemiesSpawned?.Invoke();
        Debug.Log($"All {enemiesPerWave} enemies spawned!");
    }

    void SpawnRandomEnemy()
    {
        // Choose random enemy prefab
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        // Choose random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Spawn the enemy
        GameObject enemyObject = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        EnemyBaseClass enemy = enemyObject.GetComponent<EnemyBaseClass>();

        if (enemy != null)
        {
            RegisterEnemy(enemy);
            totalEnemiesSpawned++;
            Debug.Log($"Spawned enemy {enemy.name} at {spawnPoint.position}");
        }
        else
        {
            Debug.LogWarning($"Enemy prefab {enemyPrefab.name} does not have EnemyBaseClass component!");
        }
    }

    public void SpawnSpecificEnemy(GameObject enemyPrefab, Transform spawnPoint)
    {
        GameObject enemyObject = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        EnemyBaseClass enemy = enemyObject.GetComponent<EnemyBaseClass>();

        if (enemy != null)
        {
            RegisterEnemy(enemy);
            totalEnemiesSpawned++;
            Debug.Log($"Spawned enemy {enemy.name} at {spawnPoint.position}");
        }
        else
        {
            Debug.LogWarning($"Enemy prefab {enemyPrefab.name} does not have EnemyBaseClass component!");
        }
    }

    public void RegisterEnemy(EnemyBaseClass enemy)
    {
        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
            allSpawnedEnemies.Add(enemy);

        }
    }

    public void MonitorEnemy(EnemyBaseClass enemy)
    {
        // Enemy died or was destroyed
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            totalEnemiesKilled++;

            Debug.Log($"Enemy died! Remaining: {activeEnemies.Count}");
            OnEnemyDeath?.Invoke();

            // Check win condition
            //CheckWinCondition();
        }
    }

    void CheckWinCondition()
    {
        if (activeEnemies.Count == 0 && totalEnemiesSpawned > 0 && !gameWon)
        {
            gameWon = true;
            Debug.Log("All enemies defeated! You Win!");
            OnGameWin?.Invoke();
            EndGame();
        }
    }

    public void SpawnAdditionalWave(int additionalEnemies)
    {
        StartCoroutine(SpawnAdditionalEnemies(additionalEnemies));
    }

    IEnumerator SpawnAdditionalEnemies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnRandomEnemy();
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    public void EndGame()
    {
        gameStarted = false;
        gameWon = false;
        gamePaused = true;
        Time.timeScale = 0f; // Pause the game

        if (playerInput != null)
        {
            playerInput.DeactivateInput();
        }

        Debug.Log("Game Ended!");
    }

    public void PauseGame()
    {
        gamePaused = true;
        Time.timeScale = 0f;
        Debug.Log("Game Paused!");
    }

    public void ResumeGame()
    {
        gamePaused = false;
        Time.timeScale = 1f;
        Debug.Log("Game Resumed!");
    }

    public void ToggleMinimap()
    {
        if (gameUI != null)
        {
            gameUI.ToggleMinimap();
        }
        else
        {
            Debug.LogWarning("GameUI is not assigned!");
        }
    }

    // Getters for UI and other systems
    public int GetActiveEnemyCount()
    {
        return activeEnemies.Count;
    }

    public int GetTotalEnemiesSpawned()
    {
        return totalEnemiesSpawned;
    }

    public int GetTotalEnemiesKilled()
    {
        return totalEnemiesKilled;
    }

    public bool IsGameWon()
    {
        return gameWon;
    }

    public bool IsGameStarted()
    {
        return gameStarted;
    }
    public GameObject GetEnemyPrefab(EnemyType type)
    {
        return enemyPrefabs[(int)type];
    }

    public List<EnemyBaseClass> GetActiveEnemies()
    {
        return new List<EnemyBaseClass>(activeEnemies);
    }

    // Debug methods
    [ContextMenu("Spawn Single Enemy")]
    public void SpawnSingleEnemyDebug()
    {
        SpawnRandomEnemy();
    }

    [ContextMenu("Kill All Enemies")]
    public void KillAllEnemiesDebug()
    {
        foreach (EnemyBaseClass enemy in activeEnemies)
        {
            if (enemy != null)
            {
                enemy.TakeDamage(enemy.health);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw spawn points
        if (spawnPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                    Gizmos.DrawRay(spawnPoint.position, spawnPoint.up * 2f);
                }
            }
        }
    }
}
public enum EnemyType
{
    Wisp,
    Booky,
    Sulker,

}
