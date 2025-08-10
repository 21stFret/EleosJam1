using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCardUI : MonoBehaviour
{
    public TMP_Text upgradeNameText;
    public TMP_Text upgradeDescriptionText;
    public Image upgradeIcon;

    public void UpdateUpgradeCard(ExorcistUpgrade upgrade)
    {
        upgradeNameText.text = upgrade.upgradeName;
        upgradeDescriptionText.text = upgrade.description;
        //upgradeIcon.sprite = upgrade.upgradeIcon;
    }
}
