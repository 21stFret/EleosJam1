using UnityEngine;

public class ExorcistUpgrade : MonoBehaviour
{
    public int level;
    public string upgradeName;
    public string description;
    public float value;
    public ExorcistUpgradeType upgradeType;

    public ExorcistUpgrade(int level, string upgradeName, string description, float value, ExorcistUpgradeType upgradeType)
    {
        this.level = level;
        this.upgradeName = upgradeName;
        this.description = description;
        this.value = value;
        this.upgradeType = upgradeType;
    }
}