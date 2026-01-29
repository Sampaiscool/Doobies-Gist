using UnityEngine;

[CreateAssetMenu(menuName = "SO/Locations/PalmHuertoEffect")]
public class PalmHuertoEffect : LocationEffectSO
{
    public override void ApplyEffect()
    {
        GameManager.Instance.ChangeHp((GameManager.Instance.CurrentPlayerHP / 2), false, false);

        GameManager.Instance.ChangeSploont(600, true);
    }
}
