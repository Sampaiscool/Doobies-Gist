using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HarvestShopManager : MonoBehaviour
{
    public TMP_Text harvestText;
    public TMP_Text turipSeedsText;

    private TeamLoader loader;

    void Start()
    {
        loader = FindFirstObjectByType<TeamLoader>();
    }

    public void DoneShopping()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
