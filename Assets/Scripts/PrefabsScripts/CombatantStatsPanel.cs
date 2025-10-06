using System;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CombatantStatsPanel : MonoBehaviour
{
    [Header("General Stats")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text defenceText;
    [SerializeField] private TMP_Text skillDmgText;
    [SerializeField] private TMP_Text healPower;

    [Header("Weapon Stats")]
    [SerializeField] private TMP_Text weaponNameText;
    [SerializeField] private TMP_Text weaponDamageText;
    [SerializeField] private TMP_Text weaponCritText;
    [SerializeField] private TMP_Text weaponMissText;

    [Header("Panels")]
    [SerializeField] private GameObject statsPanel;         // Main stats panel
    [SerializeField] private GameObject upgradesPanel;      // Parent container

    [Header("Upgrades UI")]
    [SerializeField] private Transform upgradesContainer;   // Layout parent
    [SerializeField] private GameObject upgradeEntryPrefab; // Prefab for one upgrade

    private CombatantInstance boundInstance;

    public void Setup(CombatantInstance instance)
    {
        boundInstance = instance;
        gameObject.SetActive(true);
        statsPanel.SetActive(true);
        upgradesPanel.SetActive(false);

        UpdateStats();
        PopulateUpgrades();
    }

    private void Update()
    {
        if (boundInstance != null)
            UpdateStats();
    }

    private void UpdateStats()
    {
        if (boundInstance == null) return;

        // General
        nameText.text = boundInstance.CharacterName;
        defenceText.text = $"Defence: {Math.Round(boundInstance.GetEffectiveDefence(), 2)}";
        skillDmgText.text = $"Skill Dmg: {boundInstance.GetEffectiveSkillDamageForUI(boundInstance.CurrentSkillDmg)}";
        healPower.text = $"Heal Power: {boundInstance.GetEffectiveHealPower(boundInstance.CurrentHealPower)}";

        // Weapon
        if (boundInstance.EquippedWeaponInstance != null)
        {
            var weapon = boundInstance.EquippedWeaponInstance;
            weaponNameText.text = $"{weapon.baseSO.weaponName}";
            weaponDamageText.text = $"Damage: {boundInstance.GetEffectiveWeaponDamageAfterEffectsForUI(boundInstance.GetEffectiveWeaponDamage())}";
            weaponCritText.text = $"Crit: {boundInstance.GetEffectiveCritChanceAfterEffects(boundInstance.GetEffectiveCritChance())}%";
            weaponMissText.text = $"Miss: {weapon.MissChance * 100f:F1}%";
        }
        else
        {
            weaponNameText.text = "No Weapon";
            weaponDamageText.text = "-";
            weaponCritText.text = "-";
            weaponMissText.text = "-";
        }
    }

    private void PopulateUpgrades()
    {
        // Clear old entries
        foreach (Transform child in upgradesContainer)
            Destroy(child.gameObject);

        // Add active upgrades
        foreach (Upgrade upgrade in boundInstance.ActiveUpgrades)
        {
            var entry = Instantiate(upgradeEntryPrefab, upgradesContainer);

            var upgradeButton = entry.GetComponent<StatsUpgradeButton>();
            if (upgradeButton != null)
            {
                upgradeButton.SetupUpgrade(upgrade);
            }
        }
    }
    private void PopulateItems()
    {
        // Clear old entries
        foreach (Transform child in upgradesContainer)
            Destroy(child.gameObject);

        // Add active items
        foreach (Item item in boundInstance.ActiveItems)
        {
            var entry = Instantiate(upgradeEntryPrefab, upgradesContainer);

            var upgradeButton = entry.GetComponent<StatsUpgradeButton>();
            if (upgradeButton != null)
            {
                upgradeButton.SetupItem(item);
            }
        }
    }

    public void ShowpgradesPanel()
    {
        statsPanel.SetActive(false);
        upgradesPanel.SetActive(true);

        PopulateUpgrades();
    }

    public void ShowItemsPanel()
    {
        statsPanel.SetActive(false);
        upgradesPanel.SetActive(true);

        PopulateItems();
    }
    public void ReturnToStats()
    {
        statsPanel.SetActive(true);
        upgradesPanel.SetActive(false);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        statsPanel.SetActive(true);
        upgradesPanel.SetActive(false);

        boundInstance = null;
    }
}
