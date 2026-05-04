using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ForestUIManager : MonoBehaviour
{
    public Transform treeContainer;
    public GameObject treeSlotPrefab;

    [Header("Resource Display")]
    public TMP_Text harvestText; // We laten nu de harvest zien die je gebruikt om te planten

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

        // Laat zien hoeveel harvest de speler heeft om bomen te kopen
        if (harvestText != null)
            harvestText.text = $"Harvest: {data.harvest}";

        RenderTrees();
    }

    void RenderTrees()
    {
        if (treeContainer == null || treeSlotPrefab == null) return;

        // Verwijder oude slots
        foreach (Transform child in treeContainer)
            Destroy(child.gameObject);

        var trees = ForestManager.Instance.GetAllTrees();

        foreach (var tree in trees)
        {
            // Verberg geclaimde bomen (of laat ze staan als 'leeg', afhankelijk van je smaak)
            if (tree.isClaimed) continue;

            GameObject slot = Instantiate(treeSlotPrefab, treeContainer);
            TMP_Text label = slot.GetComponentInChildren<TMP_Text>();
            Button claimBtn = slot.transform.Find("ClaimButton")?.GetComponent<Button>();

            // Status tekst
            string status = tree.isComplete ? "<color=green>Ready to Harvest!</color>" :
                $"Growth: {tree.growthCurrent}/{tree.growthRequired}";
            
            label.text = $"Ravin Tree\n{status}";

            if (claimBtn != null)
            {
                string id = tree.treeId;
                claimBtn.onClick.AddListener(() =>
                {
                    if (ForestManager.Instance.ClaimTree(id))
                    {
                        // Misschien hier een kleine popup/notificatie: "Item gevonden!"
                        RefreshUI();
                    }
                });
                
                // Knop is alleen klikbaar als de boom 100% is
                claimBtn.interactable = tree.isComplete;
            }
        }
    }

    // Gekoppeld aan de "Plant New Tree" knop in je UI
    public bool PlantTree(bool timeBased)
    {
        if (loader == null) loader = FindFirstObjectByType<TeamLoader>();
        var data = loader.data;

        // Check de limiet van 3 bomen
        if (data.ravinTreeList.Count >= 3) 
        {
            Debug.Log("Je bos is vol! Oogst eerst een boom.");
            return false;
        }

        int treeCost = 5; // De prijs in harvest
        if (data.harvest < treeCost) return false;

        data.harvest -= treeCost;
    
        // Maak de nieuwe boom aan met het gekozen type
        data.ravinTreeList.Add(new RavinTree(timeBased));
    
        loader.SaveGame();
        return true;
    }
}