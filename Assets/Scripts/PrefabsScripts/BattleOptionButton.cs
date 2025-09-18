using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class BattleOptionButton : MonoBehaviour
{
    public Button button;
    public TMP_Text labelText;

    public void Setup(string label, UnityAction callback)
    {
        labelText.text = label;
        button.onClick.RemoveAllListeners();
        if (callback != null)
            button.onClick.AddListener(callback);
    }
}
