using UnityEngine;

[CreateAssetMenu(fileName = "NewDoobie", menuName = "Doobies/DoobieInstanceSO")]
public class DoobieInstanceSO : ScriptableObject
{
    public DoobieSO so;

    [Header("Runtime Debug Values")]
    public int CurrentHealth;
    public int MaxHealth;
    public float CurrentDefence;
    public int MainResourceAmount; // for debugging MainResource.Current
}
