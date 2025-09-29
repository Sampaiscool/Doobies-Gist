using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EffectsPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform contentParent; // The Content object of your ScrollView
    [SerializeField] private GameObject effectEntryPrefab; // Prefab for a single effect entry
    public List<EffectDescriptionSO> effectDescriptions;

    private readonly List<GameObject> spawnedEntries = new List<GameObject>();

    /// <summary>
    /// Populates the scroll view with the given effects.
    /// </summary>
    void Awake()
    {
        PopulateEffects();
    }

    public void PopulateEffects()
    {
        ClearEffects();

        foreach (var effect in effectDescriptions)
        {
            GameObject entry = Instantiate(effectEntryPrefab, contentParent);
            spawnedEntries.Add(entry);

            // Get the EffectEntryUI component and fill it
            EffectEntryUI entryUI = entry.GetComponent<EffectEntryUI>();
            if (entryUI != null)
            {
                entryUI.SetData(effect);
            }
            else
            {
                Debug.LogWarning("EffectEntryUI component missing on prefab!");
            }
        }
    }
    public void CloseEffectsPanel()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Clears all existing entries.
    /// </summary>
    private void ClearEffects()
    {
        foreach (var entry in spawnedEntries)
        {
            if (entry != null) Destroy(entry);
        }
        spawnedEntries.Clear();
    }
}
