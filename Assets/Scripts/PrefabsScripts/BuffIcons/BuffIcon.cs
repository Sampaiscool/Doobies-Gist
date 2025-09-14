using UnityEngine;
using UnityEngine.UI;

public class BuffIcon : MonoBehaviour
{
    public Image iconImage;
    public GameObject EffectPrefab;
    public DebuffIconHover hoverPrefab;
    public GameObject TooltipPrefab;

    public void Initialize(Buff buff, Sprite iconSprite, GameObject tooltipPrefab, GameObject effectPrefab)
    {
        EffectPrefab = effectPrefab;
        TooltipPrefab = tooltipPrefab;

        if (iconSprite != null)
            iconImage.sprite = iconSprite;

        //if (hoverPrefab != null)
        //{
        //    DebuffIconHover hoverInstance = Instantiate(hoverPrefab, transform);
        //    hoverInstance.linkedBuff = buff;
        //    hoverInstance.tooltipPrefab = tooltipPrefab;
        //}
    }

    public void PlayEffect()
    {
        if (EffectPrefab == null)
        {
            Debug.LogWarning("Effect prefab is not assigned.");
            return;
        }

        GameObject spawned = Instantiate(EffectPrefab, transform);
        spawned.transform.localPosition = Vector3.zero;
        spawned.transform.localScale.Normalize();

        var ps = spawned.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var renderer = ps.GetComponent<Renderer>();
            renderer.sortingLayerName = "Foreground";
            renderer.sortingOrder = 10;
        }

        Destroy(spawned, 2f); // effect lasts 2 seconds
    }
}
