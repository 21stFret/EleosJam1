using System.Collections.Generic;
using UnityEngine;

public class ExorcistLeveling : MonoBehaviour
{
    public int levelLiving = 1;
    public int experienceLiving = 0;
    public int experienceToNextLivingLevel = 100;
    public int levelSpirit = 1;
    public int experienceSpirit = 0;
    public int experienceToNextSpiritLevel = 100;
    public List<ExorcistUpgrade> availableUpgrades = new List<ExorcistUpgrade>();
    public List<ExorcistUpgrade> allUpgrades = new List<ExorcistUpgrade>();
    public List<ExorcistUpgrade> currentUpgrades = new List<ExorcistUpgrade>();
    public ExorcistCombat exorcistCombat;
    public ExorcistController exorcistController;
    public List<ExorcistUpgrade> levelUpUpgrades = new List<ExorcistUpgrade>();
    public List<UpgradeCardUI> upgradeCardUIs = new List<UpgradeCardUI>();
    public GameObject levelUpUI;


    public void AddExperience(int amount, RealityType realityType)
    {
        bool deadLevel = realityType == RealityType.Spirit;
        if (deadLevel)
        {
            experienceSpirit += amount;
            if (experienceSpirit >= experienceToNextSpiritLevel)
            {
                LevelUp(true);
            }
        }
        else
        {
            experienceLiving += amount;
            if (experienceLiving >= experienceToNextLivingLevel)
            {
                LevelUp();
            }
        }
    }

    private void LevelUp(bool deadLevel = false)
    {
        GameManager.Instance.PauseGame();
        if (deadLevel)
        {
            levelSpirit++;
            experienceSpirit -= experienceToNextSpiritLevel;
            experienceToNextSpiritLevel = Mathf.RoundToInt(experienceToNextSpiritLevel * 1.2f);
            Debug.Log($"Leveled up! New level: {levelSpirit}, Experience to next level: {experienceToNextSpiritLevel}");
        }
        else
        {
            levelLiving++;
            experienceLiving -= experienceToNextLivingLevel;
            experienceToNextLivingLevel = Mathf.RoundToInt(experienceToNextLivingLevel * 1.2f);
            Debug.Log($"Leveled up! New level: {levelLiving}, Experience to next level: {experienceToNextLivingLevel}");
        }

        // Optionally, you can add more functionality here like increasing stats, unlocking abilities, etc.
        GenerateUpgrades(deadLevel);
    }

    private void GenerateUpgrades(bool deadLevel)
    {
        // Clear previous upgrades
        availableUpgrades.Clear();

        foreach (var upgrade in allUpgrades)
        {
            if (upgrade.upgradeType == ExorcistUpgradeType.Living && !deadLevel)
            {
                availableUpgrades.Add(upgrade);
            }
            else if (upgrade.upgradeType == ExorcistUpgradeType.Spirit && deadLevel)
            {
                availableUpgrades.Add(upgrade);
            }
            else if (upgrade.upgradeType == ExorcistUpgradeType.Universal)
            {
                availableUpgrades.Add(upgrade);
            }
        }
        // Randomly select upgrades for the level-up screen
        levelUpUpgrades.Clear();

        // Create a copy of available upgrades to remove from
        List<ExorcistUpgrade> upgradesPool = new List<ExorcistUpgrade>(availableUpgrades);

        int upgradesNeeded = Mathf.Min(3, upgradesPool.Count); // Don't try to get more than available

        for (int i = 0; i < upgradesNeeded; i++)
        {
            if (upgradesPool.Count > 0)
            {
                int randomIndex = Random.Range(0, upgradesPool.Count);
                levelUpUpgrades.Add(upgradesPool[randomIndex]);
                upgradesPool.RemoveAt(randomIndex); // Remove so we don't pick it again
            }
        }
        // Update UI with new upgrades
        for (int i = 0; i < upgradeCardUIs.Count; i++)
        {
            var cardUI = upgradeCardUIs[i];
            if (cardUI != null)
            {
                cardUI.UpdateUpgradeCard(levelUpUpgrades[i]);
            }
        }
        // Show level-up UI
        if (levelUpUI != null)
        {
            levelUpUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Level Up UI is not assigned!");
        }
    }

    public void OnSelectCard(int index)
    {
        if (index >= 0 && index < levelUpUpgrades.Count)
        {
            var selectedUpgrade = levelUpUpgrades[index];
            ApplyUpgrade(selectedUpgrade);
        }
    }

    public void ApplyUpgrade(ExorcistUpgrade upgrade)
    {
        if (currentUpgrades.Contains(upgrade))
        {
            upgrade.level++;
            Debug.Log($"Upgrade {upgrade.upgradeName} already applied, increasing level to {upgrade.level}");
        }
        else
        {
            currentUpgrades.Add(upgrade);
            Debug.Log($"Applied upgrade: {upgrade.upgradeName}");
        }

        switch (upgrade.upgradeName)
        {
            case "Increased Speed":
                exorcistController.moveSpeed += upgrade.value;
                break;
            case "Increased Attack Speed":
                exorcistCombat.meleeCooldown -= upgrade.value;
                break;
            case "Increased Damage":
                exorcistCombat.meleeDamage += upgrade.value;
                break;
            case "Increased Health":
                exorcistController.maxHealth += (int)upgrade.value;
                exorcistController.currentHealth += (int)upgrade.value; // Heal the player
                GameManager.Instance.gameUI.UpdateHealthBar(exorcistController.currentHealth / (float)exorcistController.maxHealth);
                break;
            // Living upgrades
            case "Enemy Stun":
                exorcistCombat.stunTime += upgrade.value;
                break;
            case "Extend Attack Range":
                exorcistCombat.meleeRange += upgrade.value;
                exorcistCombat.UpdateMeleeCollider();
                break;
            case "Knockback":
                exorcistCombat.meleeKnockbackForce += upgrade.value;
                break;
            // Dead upgrades
            case "Multi-Shot":
                exorcistCombat.projectileAmount += upgrade.value;
                break;
            case "Quick Talisman":
                exorcistCombat.projectileSpeed += upgrade.value * 10;
                exorcistCombat.rangedCooldown -= upgrade.value;
                break;
            case "Exploding Talisman":
                exorcistCombat.explosionRadius += upgrade.value * 10;
                exorcistCombat.explosionPercentage += upgrade.value;
                exorcistCombat.isExplosive = true;
                break;
        }

        levelUpUI.SetActive(false);
        GameManager.Instance.ResumeGame();
    }
}

public enum ExorcistUpgradeType
{
    Living,
    Spirit,
    Universal
}
    