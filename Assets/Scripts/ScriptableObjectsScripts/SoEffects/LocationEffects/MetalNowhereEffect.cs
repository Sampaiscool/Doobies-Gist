using UnityEngine;

[CreateAssetMenu(menuName = "SO/Locations/MetalNowhereEffect")]
public class MetalNowhereEffect : LocationEffectSO
{
    public override void ApplyEffect()
    {
        GameManager.Instance.currentDoobie.CurrentDefence += 1;
    }
}
