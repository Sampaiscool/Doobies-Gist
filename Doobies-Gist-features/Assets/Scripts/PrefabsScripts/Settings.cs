using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    [Header("UI")]
    public GameObject EffectsPanel;

    [Header("Debug Tools")]
    public bool targetDoobie = true;
    public VangurrSO debugVangurrSO;
    public UpgradeSO debugUpgradeSO;

    [Header("Effect Debug Options")]
    public EffectType debugEffectType;
    public int debugEffectIntensity = 1;
    public int debugEffectDuration = 3;
    public bool debugEffectIsDebuff = false;

    [Header("Item Debug Options")]
    public ItemType debugItemType = ItemType.None;

    // === PANEL CONTROL ===
    public void SpawnEffectsPanel()
    {
        if (FindFirstObjectByType<EffectsPanel>() != null)
        {
            Debug.Log("[Settings] EffectsPanel UI is already active!");
            return;
        }

        Canvas uiCanvas = null;
        Canvas canvasObject = FindFirstObjectByType<Canvas>();
        if (canvasObject != null && canvasObject.isRootCanvas)
        {
            uiCanvas = canvasObject;
        }

        if (EffectsPanel == null || uiCanvas == null)
        {
            Debug.LogWarning("[Settings] Missing prefab or no Canvas found in scene!");
            return;
        }

        GameObject obj = Instantiate(EffectsPanel, uiCanvas.transform);

        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }
    }

    public void CloseSettingsPanel()
    {
        Destroy(gameObject);
    }

    // === DEBUG / CHEAT METHODS ===

    [ContextMenu("Spawn Debug Vangurr")]
    public void SpawnDebugVangurr()
    {
        if (debugVangurrSO == null)
        {
            Debug.LogWarning("[Debug] No VangurrSO assigned!");
            return;
        }

        var vangurrInstance = new VangurrInstance(debugVangurrSO);
        vangurrInstance.Init();
        GameManager.Instance.currentVangurr = vangurrInstance;

        Debug.Log($"[Debug] Spawned Vangurr: {vangurrInstance.CharacterName}");
    }

    [ContextMenu("Give Upgrade")]
    public void GiveUpgrade()
    {
        CombatantInstance target = GetTarget();

        if (target == null || debugUpgradeSO == null)
        {
            Debug.LogWarning("[Debug] Missing Target or UpgradeSO!");
            return;
        }

        var newUpgrade = new Upgrade(
            debugUpgradeSO.upgradeName,
            debugUpgradeSO.description,
            debugUpgradeSO.cost,
            debugUpgradeSO.type,
            debugUpgradeSO.pool,
            debugUpgradeSO.intensity,
            debugUpgradeSO.isCurse
        )
        { icon = debugUpgradeSO.icon };

        target.AddUpgrade(newUpgrade);
        Debug.Log($"[Debug] Added upgrade '{debugUpgradeSO.upgradeName}' to {target.CharacterName}!");
    }

    [ContextMenu("Give Item")]
    public void GiveItem()
    {
        CombatantInstance target = GetTarget();

        if (target == null || debugItemType == ItemType.None)
        {
            Debug.LogWarning("[Debug] Missing Target or UpgradeSO!");
            return;
        }

        target.AddItem(new Item(debugItemType.ToString(), debugItemType.ToString(), 0, debugItemType, CharacterPool.None, true));
        Debug.Log($"[Debug] Added item " + debugItemType.ToString() + " to {target.CharacterName}!");
    }

    [ContextMenu("Give Effect")]
    public void GiveEffect()
    {
        CombatantInstance target = GetTarget();

        if (target == null)
        {
            Debug.LogWarning("[Debug] No valid target found!");
            return;
        }

        target.AddEffect(new Effect(
            debugEffectType,
            debugEffectDuration,
            debugEffectIsDebuff,
            debugEffectIntensity
        ));

        Debug.Log($"[Debug] Added effect '{debugEffectType}' to {target.CharacterName}!");
    }

    // === Helper ===
    private CombatantInstance GetTarget()
    {
        if (targetDoobie)
            return GameManager.Instance.currentDoobie;
        else
            return GameManager.Instance.currentVangurr;
    }
}
