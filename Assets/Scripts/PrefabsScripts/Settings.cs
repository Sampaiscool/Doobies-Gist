using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public GameObject EffectsPanel;

    public void SpawnEffectsPanel()
    {
        if (FindFirstObjectByType<EffectsPanel>() != null)
        {
            Debug.Log("[GameManager] EffectsPanel UI is already active!");
            return;
        }

        Canvas uiCanvas = null;
        Canvas canvasObject = FindFirstObjectByType<Canvas>();
        if (canvasObject.isRootCanvas)
        {
            uiCanvas = canvasObject;
        }

        if (EffectsPanel == null || uiCanvas == null)
        {
            Debug.LogWarning("[GameManager] Missing prefab or no Canvas found in scene!");
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
}
