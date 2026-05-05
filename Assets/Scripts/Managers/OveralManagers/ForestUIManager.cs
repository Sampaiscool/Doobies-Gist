using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ForestUIManager : MonoBehaviour
{
    public Transform treeContainer;
    public GameObject treeSlotPrefab;

    [Header("Resource Display")]
    public TMP_Text harvestText; 

    private TeamLoader loader;
    private List<GameObject> spawnedSlots = new List<GameObject>();

    void OnEnable() 
    {
        loader = FindFirstObjectByType<TeamLoader>();
        
        // Zorg dat de lijst in de data nooit null is
        if (loader != null && loader.data != null && loader.data.ravinTreeList == null)
        {
            loader.data.ravinTreeList = new List<RavinTree>();
        }

        InitializeSlots();
        RefreshUI();
    }

    void InitializeSlots()
    {
        // Als de lijst al gevuld is maar de objecten zijn vernietigd (bijv. door scene switch)
        spawnedSlots.RemoveAll(item => item == null);

        if (spawnedSlots.Count == 0)
        {
            // Ruim eventuele oude restanten in de container op
            foreach (Transform child in treeContainer) Destroy(child.gameObject);

            for (int i = 0; i < 3; i++)
            {
                GameObject slot = Instantiate(treeSlotPrefab, treeContainer);
                spawnedSlots.Add(slot);
                
                // Zorg dat de tooltip visual standaard UIT staat bij spawn
                TreeTooltip tt = slot.GetComponent<TreeTooltip>();
                if (tt != null && tt.tooltipVisual != null) tt.tooltipVisual.SetActive(false);
            }
        }
    }

    void Update()
    {
        // Alleen updaten als we data hebben
        if (loader == null || loader.data == null) return;

        if (Time.frameCount % 60 == 0) 
        {
            UpdateTooltipDataOnly();
        }
    }

    public void RefreshUI()
    {
        // Check 1: Is de loader er?
        if (loader == null || loader.data == null) return;
        
        // Check 2: Is de ForestManager er?
        if (ForestManager.Instance == null) return;

        if (harvestText != null)
            harvestText.text = $"Harvest: {loader.data.harvest}";

        var trees = ForestManager.Instance.GetAllTrees();

        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            var slot = spawnedSlots[i];
            if (slot == null) continue;

            // Zoek componenten robuuster
            Button actionBtn = slot.GetComponentInChildren<Button>();
            TreeTooltip tooltip = slot.GetComponent<TreeTooltip>();

            // Check 3: Bestaan de componenten op de prefab?
            if (actionBtn == null || tooltip == null)
            {
                Debug.LogError($"Prefab mist Button of TreeTooltip op slot {i}!");
                continue;
            }

            if (i < trees.Count)
            {
                SetupActiveTree(trees[i], actionBtn, tooltip);
            }
            else
            {
                SetupEmptySlot(actionBtn, tooltip);
            }
        }
    }

    void UpdateTooltipDataOnly()
    {
        if (ForestManager.Instance == null) return;

        var trees = ForestManager.Instance.GetAllTrees();
        for (int i = 0; i < trees.Count; i++)
        {
            if (i >= spawnedSlots.Count || spawnedSlots[i] == null) break;

            var tooltip = spawnedSlots[i].GetComponent<TreeTooltip>();
            if (tooltip == null) continue;
            
            tooltip.SetTooltipData(GetTreeStatusString(trees[i]));
            
            if (trees[i].isTimeBased && !trees[i].isComplete)
            {
                if (System.DateTimeOffset.Now.ToUnixTimeSeconds() >= trees[i].finishTimestamp)
                {
                    trees[i].isComplete = true;
                    RefreshUI();
                }
            }
        }
    }

    private string GetTreeStatusString(RavinTree tree)
    {
        if (tree == null) return "";
        if (tree.isComplete) return "<color=green>Ready to harvest!</color>";

        if (tree.isTimeBased)
        {
            long diff = tree.finishTimestamp - System.DateTimeOffset.Now.ToUnixTimeSeconds();
            System.TimeSpan t = System.TimeSpan.FromSeconds(Mathf.Max(0, diff));
            return $"Ancient Tree\nTime left: {(int)t.TotalMinutes}m {t.Seconds}s";
        }
        
        return $"Battle Tree\nBosses: {tree.growthCurrent}/{tree.growthRequired}";
    }

    private void SetupActiveTree(RavinTree tree, Button btn, TreeTooltip tooltip)
    {
        btn.onClick.RemoveAllListeners();
        tooltip.SetTooltipData(GetTreeStatusString(tree));

        TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();

        if (tree.isComplete)
        {
            if (btnText != null) btnText.text = "Harvest";
            btn.interactable = true;
            btn.onClick.AddListener(() => {
                ForestManager.Instance.ClaimTree(tree.treeId);
                RefreshUI();
            });
        }
        else
        {
            btn.interactable = false;
            if (btnText != null) btnText.text = "Growing...";
        }
    }

    private void SetupEmptySlot(Button btn, TreeTooltip tooltip)
    {
        btn.onClick.RemoveAllListeners();
        tooltip.SetTooltipData("Empty plot. Plant something!");
        
        TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();
        if (btnText != null) btnText.text = "Plant";
        
        btn.interactable = true;
        btn.onClick.AddListener(() => {
            // We kiezen hier standaard voor een Battle Tree (false)
            if(PlantTree(false)) RefreshUI(); 
        });
    }

    public bool PlantTree(bool timeBased)
    {
        if (loader == null || loader.data == null) return false;
        if (loader.data.ravinTreeList.Count >= 3) return false;
        if (loader.data.harvest < 5) return false;

        loader.data.harvest -= 5;
        loader.data.ravinTreeList.Add(new RavinTree(timeBased));
        loader.SaveGame();
        return true;
    }
}