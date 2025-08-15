using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCardUI : MonoBehaviour
{
    public TMP_Text upgradeNameText;
    public TMP_Text upgradeDescriptionText;
    public Image upgradeIcon;
    public Sprite livingScroll;
    public Sprite spiritScroll;
    public Sprite universalScroll;

    public void UpdateUpgradeCard(ExorcistUpgrade upgrade)
    {
        upgradeNameText.text = upgrade.upgradeName;
        upgradeDescriptionText.text = upgrade.description;
        if (upgrade.upgradeType == ExorcistUpgradeType.Universal)
        {
            upgradeIcon.sprite = universalScroll;
        }
        else if (upgrade.upgradeType == ExorcistUpgradeType.Living)
        {
            upgradeIcon.sprite = livingScroll;
        }
        else if (upgrade.upgradeType == ExorcistUpgradeType.Spirit)
        {
            upgradeIcon.sprite = spiritScroll;
        }
    }
}
