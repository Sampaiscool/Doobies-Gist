using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamSelectUI : MonoBehaviour
{
    public Transform doobieHolder;
    public GameObject doobieButtonPrefab;

    public static TeamSelectUI Instance;
    public MenuManager menuManager;

    public GameObject[] teamSlots;
    public GameObject doobieSelectionPanel;
    public GameObject addButtonPrefab;

    private DoobieSO selectedDoobie;
    public int selectedSlot;

    private DoobieSO[] team = new DoobieSO[4];

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        LoadDoobies();
    }

    void LoadDoobies()
    {
        if (doobieHolder == null || doobieButtonPrefab == null)
        {
            Debug.LogError("DoobieHolder or Prefab not assigned!");
            return;
        }

        DoobieSO[] allDoobies = Resources.LoadAll<DoobieSO>("Doobies");

        foreach (Transform child in doobieHolder)
            Destroy(child.gameObject);

        foreach (DoobieSO doobie in allDoobies)
        {
            bool isUnlocked = GameManager.Instance.debugMode || doobie.unlockedByDefault;

            if (isUnlocked)
            {
                GameObject buttonObj = Instantiate(doobieButtonPrefab, doobieHolder);
                DoobieButton buttonScript = buttonObj.GetComponent<DoobieButton>();
                buttonScript?.SetupButton(doobie);
            }
        }
    }

    public void UpdateTeamUI()
    {
        LoadDoobies();

        // Clear previous contents
        foreach (Transform child in teamSlots[0].transform)
            Destroy(child.gameObject);

        if (selectedDoobie != null)
        {
            GameObject buttonObj = Instantiate(doobieButtonPrefab, teamSlots[0].transform);
            DoobieButton btn = buttonObj.GetComponent<DoobieButton>();
            btn.SetupButton(selectedDoobie, true, 0);
        }
        else
        {
            GameObject addButton = Instantiate(addButtonPrefab, teamSlots[0].transform);
            AddButton add = addButton.GetComponent<AddButton>();
            add.teamSlotIndex = 0;
            Button btn = addButton.GetComponent<Button>();
            btn.onClick.AddListener(() => OpenDoobieSelection(0));
        }
    }

    public void OpenDoobieSelection(int slotIndex)
    {
        selectedSlot = slotIndex;
        doobieSelectionPanel.SetActive(true);
        LoadDoobies();
    }

    public void OnDoobieSelected(DoobieSO doobie, int teamSlotIndex)
    {
        selectedDoobie = doobie;
        UpdateTeamUI();
        doobieSelectionPanel.SetActive(false);

        menuManager.ShowPanel(menuManager.TownPanel);
    }

    public void UnlockDoobie(DoobieSO doobie)
    {
        PlayerPrefs.SetInt("Unlocked_" + doobie.doobieName, 1);
    }

    public void SaveTeamData()
    {
        if (selectedDoobie != null)
        {
            PlayerPrefs.SetString("SelectedDoobie_Name", selectedDoobie.doobieName);
            PlayerPrefs.SetInt("SelectedDoobie_BaseResourceMax", selectedDoobie.baseResourceMax);
            PlayerPrefs.SetInt("SelectedDoobie_BaseHealth", selectedDoobie.baseHealth);
            PlayerPrefs.SetInt("SelectedDoobie_HasHealth", selectedDoobie.hasHealth ? 1 : 0);
            PlayerPrefs.SetString("SelectedDoobie_Weapon", selectedDoobie.defaultWeapon != null ? selectedDoobie.defaultWeapon.name : "");

            PlayerPrefs.Save();
        }
    }

    public void LoadTeamData()
    {
        string name = PlayerPrefs.GetString("SelectedDoobie_Name", "");
        if (!string.IsNullOrEmpty(name))
        {
            selectedDoobie = Resources.Load<DoobieSO>($"Doobies/{name}");
        }
    }
}
