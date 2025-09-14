using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStatsUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text sploontText;
    [SerializeField] private TMP_Text hpText;

    public void UpdatePlayerInfo()
    {
        // Update Sploont
        sploontText.text = $"Sploont: {GameManager.Instance.CurrentPlayerSploont}";

        hpText.text = $"HP: {GameManager.Instance.CurrentPlayerHP}\n" +
            $"Doobie HP: {GameManager.Instance.currentDoobie.CurrentHealth}";
    }
}
