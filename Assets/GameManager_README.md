# Game Manager System

This system provides a simple but comprehensive game management solution for enemy spawning, tracking, and win conditions.

## Components

### 1. GameManager.cs
The main controller that handles:
- Enemy spawning and tracking
- Game state management
- Win/lose conditions
- Events for UI and other systems

### 2. GameUI.cs
UI controller that displays:
- Enemy count and status
- Game state
- Control buttons (Start, Reset, Spawn Wave)

### 3. EnemySpawnPoint.cs
Individual spawn point controller for:
- Manual enemy spawning
- Visual spawn point indicators
- Integration with GameManager

### 4. EnemyBaseClass.cs (Modified)
Enhanced to work with GameManager for:
- Death notifications
- Proper cleanup

## Setup Instructions

### 1. GameManager Setup
1. Create an empty GameObject in your scene
2. Name it "GameManager"
3. Add the `GameManager` component
4. Configure the following:
   - **Enemy Prefabs**: Drag your enemy prefabs here
   - **Spawn Points**: Create empty GameObjects for spawn locations and drag them here
   - **Enemies Per Wave**: How many enemies to spawn initially
   - **Spawn Delay**: Time between enemy spawns

### 2. Spawn Points Setup
1. Create empty GameObjects where you want enemies to spawn
2. Add the `EnemySpawnPoint` component (optional, for manual spawning)
3. Add these GameObjects to the GameManager's Spawn Points array

### 3. UI Setup (Optional)
1. Create a Canvas if you don't have one
2. Add UI elements:
   - Text elements for enemy count and game status
   - Buttons for Start, Reset, Spawn Wave
3. Add the `GameUI` component to a GameObject
4. Connect the UI elements to the GameUI component

### 4. Enemy Prefabs
Make sure your enemy prefabs have:
- `EnemyBaseClass` component (or a script that inherits from it)
- Proper colliders and rigidbodies
- Any other components needed for your game

## Usage

### Automatic Mode
- Set `spawnOnStart` to true in GameManager
- Game will automatically start and spawn enemies when the scene loads

### Manual Mode
- Use `GameManager.Instance.StartGame()` to begin
- Use UI buttons or call methods directly

### Events
The GameManager provides UnityEvents for:
- `OnGameStart`: When the game begins
- `OnGameWin`: When all enemies are defeated
- `OnEnemyDeath`: Each time an enemy dies
- `OnAllEnemiesSpawned`: When initial wave is complete

### Public Methods
```csharp
// Game control
GameManager.Instance.StartGame();
GameManager.Instance.ResetGame();
GameManager.Instance.PauseGame();
GameManager.Instance.ResumeGame();

// Enemy management
GameManager.Instance.SpawnAdditionalWave(5);
GameManager.Instance.RegisterEnemy(enemyComponent);

// Information
int activeCount = GameManager.Instance.GetActiveEnemyCount();
bool won = GameManager.Instance.IsGameWon();
List<EnemyBaseClass> enemies = GameManager.Instance.GetActiveEnemies();
```

## Features

### Automatic Enemy Tracking
- Enemies are automatically tracked when spawned
- Death detection through monitoring coroutines
- Proper cleanup when enemies die

### Win Condition
- Game wins when all spawned enemies are defeated
- Configurable through events

### Flexible Spawning
- Random enemy selection from prefab array
- Random spawn point selection
- Additional waves can be spawned anytime

### Debug Features
- Context menu options in editor
- Visual spawn point gizmos
- Console logging for debugging

## Tips

1. **Performance**: The system uses coroutines to monitor enemies, which is efficient for small to medium numbers of enemies.

2. **Extensibility**: You can easily extend the system by:
   - Adding more events
   - Creating custom enemy types that inherit from EnemyBaseClass
   - Adding wave progression logic

3. **Integration**: The system works with any existing enemy AI or behavior scripts as long as they inherit from EnemyBaseClass.

4. **Testing**: Use the debug context menu options in the GameManager inspector to quickly test spawning and enemy defeat.
