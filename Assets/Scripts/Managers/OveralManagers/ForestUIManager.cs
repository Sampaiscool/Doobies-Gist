using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ForestUIManager : MonoBehaviour
{
    public Transform treeContainer;
    public GameObject treeSlotPrefab;

    public TMP_Text turipSeedsText;
    public TMP_Text doobieSeedsText;
    public TMP_Text playerSeedsText;

    private TeamLoader loader;

    void Start()
    {
        loader = FindFirstObjectByType<TeamLoader>();
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (loader == null || loader.data == null) return;
        var data = loader.data;

        if (turipSeedsText != null)
            turipSeedsText.text = $"Turip Seeds: {data.turipSeeds}";
        if (doobieSeedsText != null)
            doobieSeedsText.text = $"Doobie Seeds: {data.doobieSeeds}";
        if (playerSeedsText != null)
            playerSeedsText.text = $"Player Seeds: {data.playerSeeds}";

        RenderTrees();
    }

    void RenderTrees()
    {
        if (treeContainer == null || treeSlotPrefab == null) return;

        // Clear existing slots
        foreach (Transform child in treeContainer)
            Destroy(child.gameObject);

        var trees = ForestManager.Instance.GetAllTrees();

        foreach (var tree in trees)
        {
            GameObject slot = Instantiate(treeSlotPrefab, treeContainer);
            TMP_Text label = slot.GetComponentInChildren<TMP_Text>();
            Button visitBtn = slot.transform.Find("VisitButton")?.GetComponent<Button>();
            Button claimBtn = slot.transform.Find("ClaimButton")?.GetComponent<Button>();

            string status = tree.isComplete ? "Complete" :
                $"{tree.growthCurrent}/{tree.growthRequired}";
            label.text = $"{tree.seedType} Tree - {status}";

            if (visitBtn != null)
            {
                string id = tree.treeId;
                visitBtn.onClick.AddListener(() =>
                {
                    ForestManager.Instance.VisitTree(id);
                    RefreshUI();
                });
                visitBtn.interactable = !tree.isComplete;
            }

            if (claimBtn != null)
            {
                string id = tree.treeId;
                claimBtn.onClick.AddListener(() =>
                {
                    ForestManager.Instance.ClaimTree(id);
                    RefreshUI();
                });
                claimBtn.interactable = tree.isComplete && !tree.isClaimed;
            }
        }
    }

    // Call these from UI buttons
    public void PlantTurip()
    {
        if (ForestManager.Instance.PlantSeed(SeedType.Turip))
            RefreshUI();
    }

    public void PlantDoobie()
    {
        if (ForestManager.Instance.PlantSeed(SeedType.Doobie))
            RefreshUI();
    }

    public void PlantPlayer()
    {
        if (ForestManager.Instance.PlantSeed(SeedType.Player))
            RefreshUI();
    }
}
