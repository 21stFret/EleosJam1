using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class GameUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI enemyCountText;
    public TextMeshProUGUI gameStatusText;
    public TextMeshProUGUI waveTimerText;
    public Button startGameButton;
    public GameObject startgamePanel;
    public Button resetGameButton;
    public Button spawnWaveButton;
    public GameObject winPanel;
    public GameObject gameHUD;
    public GameObject pauseMenu;
    
    [Header("Settings")]
    public int additionalWaveSize = 3;

    [Header("Win Panel")]
    public TextMeshProUGUI winText;
    [Header("Bars")]
    public Image healthbar;
    public Image experienceLivingBar;
    public Image experienceSpiritBar;


    public GameObject minimap;


    void Start()
    {
        // Set up button listeners
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);
            
        if (resetGameButton != null)
            resetGameButton.onClick.AddListener(ResetGame);
            
        if (spawnWaveButton != null)
            spawnWaveButton.onClick.AddListener(SpawnAdditionalWave);
        
        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart.AddListener(OnGameStarted);
            GameManager.Instance.OnGameWin.AddListener(OnGameWon);
            GameManager.Instance.OnEnemyDeath.AddListener(UpdateUI);
            GameManager.Instance.OnAllEnemiesSpawned.AddListener(UpdateUI);
        }
        
        UpdateUI();
    }
    
    void Update()
    {
        UpdateUI();
    }

    public void ShowPauseMenu()
    {
        if (gameHUD != null)
            gameHUD.SetActive(false);
        
        if (startgamePanel != null)
            startgamePanel.SetActive(false);
        if (pauseMenu != null)
            pauseMenu.SetActive(true);
        
        Debug.Log("UI: Pause Menu Shown");
    }

    public void HidePauseMenu()
    {
        if (gameHUD != null)
            gameHUD.SetActive(true);
        
        if (startgamePanel != null)
            startgamePanel.SetActive(false);
        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        Debug.Log("UI: Pause Menu Hidden");
    }

    public void ToggleMinimap()
    {
        minimap.SetActive(!minimap.activeSelf);
    }

    void UpdateUI()
    {
        if (GameManager.Instance == null) return;

        // Update enemy count
        if (enemyCountText != null)
        {
            int activeEnemies = GameManager.Instance.GetActiveEnemyCount();
            int totalSpawned = GameManager.Instance.GetTotalEnemiesSpawned();
            int totalKilled = GameManager.Instance.GetTotalEnemiesKilled();

            enemyCountText.text = $"Enemies: {activeEnemies} / {totalSpawned} (Killed: {totalKilled})";
        }

        // Update game status
        if (gameStatusText != null)
        {
            if (GameManager.Instance.IsGameWon())
            {
                gameStatusText.text = "YOU WIN!";
                gameStatusText.color = Color.green;
            }
            else if (GameManager.Instance.IsGameStarted())
            {
                gameStatusText.text = "FIGHT!";
                gameStatusText.color = Color.red;
            }
            else
            {
                gameStatusText.text = "Press Start to Begin";
                gameStatusText.color = Color.white;
            }
        }

        // Update wave timer
        if (waveTimerText != null)
        {
            float waveTime = GameManager.Instance.enemyWavetimer;
            var maxTime = GameManager.Instance.enemyWaveTime;
            var time = maxTime - waveTime;
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);

            waveTimerText.text = $"Next Wave in: {minutes:D2}:{seconds:D2}";
        }

        // Update button states
        if (startGameButton != null)
        {
            startGameButton.interactable = !GameManager.Instance.IsGameStarted();
        }

        if (spawnWaveButton != null)
        {
            spawnWaveButton.interactable = GameManager.Instance.IsGameStarted() && !GameManager.Instance.IsGameWon();
        }
        
        UpdateBars();
    }

    public void UpdateBars()
    {
        if (experienceLivingBar != null && GameManager.Instance != null)
        {
            float livingExperience = GameManager.Instance.exorcistLeveling.experienceLiving;
            experienceLivingBar.fillAmount = livingExperience / GameManager.Instance.exorcistLeveling.experienceToNextLivingLevel;
        }

        if (experienceSpiritBar != null && GameManager.Instance != null)
        {
            float spiritExperience = GameManager.Instance.exorcistLeveling.experienceSpirit;
            experienceSpiritBar.fillAmount = spiritExperience / GameManager.Instance.exorcistLeveling.experienceToNextSpiritLevel;
        }
    }

    public void StartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
            startgamePanel.SetActive(false);
        }
    }
    
    public void ResetGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGame();
        }
    }
    
    public void SpawnAdditionalWave()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpawnAdditionalWave(additionalWaveSize);
        }
    }
    
    void OnGameStarted()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
            
        if (gameHUD != null)
            gameHUD.SetActive(true);
            
        Debug.Log("UI: Game Started!");
    }
    
    void OnGameWon()
    {
        bool isVictory = GameManager.Instance.IsGameWon();

        if (gameHUD != null)
            gameHUD.SetActive(false);
            
        if (winPanel == null)
        {
            Debug.LogWarning("Win panel not assigned in GameUI!");
            return;
        }

        winPanel.SetActive(true);

        if (isVictory)
        {
            winText.text = "Exorcist Victorious!";
        }
        else
        {
            winText.text = "Exorcist Defeated!";
        }

        Debug.Log("UI: Game Won!");
    }

    public void UpdateHealthBar(float healthPercentage)
    {
        if (healthbar != null)
        {
            healthbar.fillAmount = Mathf.Clamp01(healthPercentage);
            healthbar.color = Color.Lerp(Color.red, Color.green, healthPercentage);
        }
        else
        {
            Debug.LogWarning("Health bar not assigned in GameUI!");
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart.RemoveListener(OnGameStarted);
            GameManager.Instance.OnGameWin.RemoveListener(OnGameWon);
            GameManager.Instance.OnEnemyDeath.RemoveListener(UpdateUI);
            GameManager.Instance.OnAllEnemiesSpawned.RemoveListener(UpdateUI);
        }
    }
}
