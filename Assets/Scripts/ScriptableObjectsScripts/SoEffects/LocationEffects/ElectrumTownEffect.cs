using UnityEngine;

[CreateAssetMenu(menuName = "SO/Locations/ElectrumTownEffect")]
public class ElectrumTownEffect : LocationEffectSO
{
    public override void ApplyEffect()
    {
        if (GameManager.Instance == null) return;

        int current = Mathf.Max(1, GameManager.Instance.nextLocationMultiplier);
        int next = current * 3;
        if (next > 9) next = 9;
        GameManager.Instance.nextLocationMultiplier = next;
    }
}
