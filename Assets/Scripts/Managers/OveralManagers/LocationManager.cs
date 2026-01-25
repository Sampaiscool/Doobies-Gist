using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LocationManager : MonoBehaviour
{
    public List<LocationSO> allLocations; // Assign via Inspector
    public PanelManager PanelManager;
    public GameObject locationButtonPrefab;
    public Transform locationButtonHolder;
    public GameObject locationChoicePanel;

    public void GenerateRandomLocations(int amount)
    {
        List<LocationSO> selectedLocations = new List<LocationSO>();

        for (int i = 0; i < amount; i++)
        {
            LocationSO loc = GetRandomLocation(allLocations, selectedLocations);
            if (loc != null) selectedLocations.Add(loc);
        }

        ShowLocations(selectedLocations);
    }

    private LocationSO GetRandomLocation(List<LocationSO> allLocations, List<LocationSO> alreadyChosen)
    {
        List<(LocationSO, float)> weightedList = new List<(LocationSO, float)>();

        foreach (var loc in allLocations)
        {
            if (alreadyChosen.Contains(loc)) continue;

            float weight = 1f / Mathf.Max(loc.locationChance, 1);
            weightedList.Add((loc, weight));
        }

        if (weightedList.Count == 0) return null;

        float totalWeight = weightedList.Sum(x => x.Item2);
        float randomValue = Random.Range(0f, totalWeight);

        float currentSum = 0f;
        foreach (var (location, weight) in weightedList)
        {
            currentSum += weight;
            if (randomValue <= currentSum)
                return location;
        }

        return null;
    }
    public void ShowLocations(List<LocationSO> locations)
    {
        foreach (Transform child in locationButtonHolder)
            Destroy(child.gameObject);

        foreach (var loc in locations)
        {
            GameObject obj = Instantiate(locationButtonPrefab, locationButtonHolder);
            LocationButton btn = obj.GetComponent<LocationButton>();
            btn.Setup(loc, SelectLocation);
        }
    }

    public void SelectLocation(LocationSO selected)
    {
        Debug.Log("You selected: " + selected.locationName);
        //locationChoicePanel.SetActive(false);

        // Call your location's logic here
        TriggerLocationEffect(selected);
    }
    
    public void SkipLocation()
    {
        GameManager.Instance.ChangeDzeef(1, true);
        PanelManager.ShowVangurrPanel();
    }

    /// <summary>
    /// Triggers Effect of location and opens The next panel
    /// </summary>
    /// <param name="location">The chosen location</param>
    void TriggerLocationEffect(LocationSO location)
    {
        if (location.effect != null)
        {
            int oldMult = 1;
            if (GameManager.Instance != null)
            {
                oldMult = Mathf.Max(1, GameManager.Instance.nextLocationMultiplier);
            }

            Debug.Log($"Applying effect of: {location.locationName} x{oldMult}");
            for (int i = 0; i < oldMult; i++)
            {
                location.effect.ApplyEffect();
            }

            // Only reset multiplier if the location effect did not change it.
            if (GameManager.Instance != null && GameManager.Instance.nextLocationMultiplier == oldMult)
            {
                GameManager.Instance.nextLocationMultiplier = 1;
            }
        }
        else
        {
            Debug.LogWarning("No effect assigned to: " + location.locationName);
        }

        PanelManager.ShowVangurrPanel();
    }
}
