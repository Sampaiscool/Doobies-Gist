using UnityEngine;

[CreateAssetMenu(menuName = "SO/Locations/HellishHole")]
public class HellishHoleEffect : LocationEffectSO
{
    public override void ApplyEffect()
    {
        GameManager.Instance.currentDoobie.CurrentBurnDamage += 1;
    }
}
