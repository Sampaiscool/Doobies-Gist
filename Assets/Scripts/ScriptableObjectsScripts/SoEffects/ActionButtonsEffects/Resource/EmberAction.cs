using UnityEngine;


[CreateAssetMenu(menuName = "SO/ResourceActions/EmberAction")]
public class EmberAction : ScriptableObject, IResourceAction
{
    public string ActionName => "Unleash Ember";
    public string Description => "Gain 3 burn;\nGain 5/10 ember";
    public bool Execute(CombatantInstance user, CombatantInstance target)
    {
        // stamp this burn with the user's burn level so it respects the caster's level
        var initial = new Effect(EffectType.Burn, 3, true, 3);
        initial.sourceBurnLevel = user.CurrentBurnLevel;
        user.AddEffect(initial);
        
        int gainAmount = UnityEngine.Random.Range(5, 11);
        
        if (user is DoobieInstance doobie)
        {
            
            
            doobie.MainResource.Gain(gainAmount);

            if (gainAmount >= 25)
            {
                // For the unleashed high roll, apply burns stamped with caster's +1 level
                int stampedLevel = Mathf.Min(user.CurrentBurnLevel + 1, 3);

                var selfHigh = new Effect(EffectType.Burn, 3, true, 2) { sourceBurnLevel = stampedLevel };
                var targetHigh = new Effect(EffectType.Burn, 3, true, 2) { sourceBurnLevel = stampedLevel };

                user.AddEffect(selfHigh);
                target.AddEffect(targetHigh);

                BattleUIManager.Instance.AddLog($"{user.CharacterName} reached too far, unleashing fire at a higher level!");
            }
        }
        
        BattleUIManager.Instance.AddLog($"{user.CharacterName} has gained {gainAmount} ember by unleashing!");
        
        return true;
    }
}