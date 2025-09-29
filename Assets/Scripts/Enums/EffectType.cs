using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    None,

    Harden,                     // Buff - Increases defense against all damage types
    WeaponStrenghten,           // Buff - Increases weapon damage
    SpellStrenghten,            // Buff - Increases spell damage
    Deflecion,                  // Buff - deflect incoming attacks
    BloomBlossom,               // Buff - If you deflect an attack, gain 10 deflection, if you deflect with 10 or more deflection, gain 1 harden

    Burn,                       // Debuff - Takes damage each turn
    Poison,                     // Debuff - Takes damage each turn, ...
    WeaponWeaken,               // Debuff - Decreases weapon damage
    SpellWeaken,                // Debuff - Decreases spell damage
    DefenceDown,                // Debuff - Decreases defense against all damage types
    Stun,                       // Debuff - Skip turn
    Blind,                      // Debuff - Miss all basic attacks
    Slow,                       // Debuff - ...
    VampireCurse,               // Debuff - Any damage taken heals the opponent for half the damage dealt

    Evasion,                    // Buff that gives a chance to completely avoid an attack
    CriticalEye,                // Buff that gives an increased chance to land a critical hit
    Bleed,                      // Debuff - Takes damage when performing Weapon-Style attacks
    Shield,                     // Buff - Absorbs damage
    Hidden,                     // Buff - Cannot take damage but also cannot attack
    TargetLocked,               // Debuff - Takes its in
    Regeneration,               // Buff - Each turn; heal equal to its intensity
    HealingStrenghten,          // Buff - Increases your healing
    HealingWeaken,              // Debuff - Decreases your healing
    Barrel,                     // ??? - Once somebody uses a basic attack; explode the barrel, Deal damage to the one who did not destroy the barrel equel to the intensity.
    Enflame,                    // Buff - On a Weapon Style Attack; Give the target "burn"
    Vines,                      // Debuff - Take damage if you heal
    HardHitter,                 // Buff - On a Weapon Style Attack; have a chance to stun the enemy
    Spores,                     // Debuff - At the end of the turn gain debuffs for the amount of intensity: At 3 stacks; weapon weaken, at 5 stacks; healing weaken, at 10 strength; defence down
    BlessedShield,              // Buff - if this shield is destroyed you gain 5 faith and "Healing Strenghten".
    Holy,                       // Debuff - If you gain another debuff; take damage.
    NutouCurse,                 // Debuff - Any damage taken gives you "Healing Weaken"
    Rage,                       // Buff - Next weapon style attack you do consumes the rage; it deals twice the damage.
    ConvertOverheal,           // Buff - If you overheal; gain "Shield" for each "Convert Overheal" intensity.
}
