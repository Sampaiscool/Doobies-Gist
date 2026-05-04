using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(CombatManager))]
public class CombatManagerInspector : Editor
{
    private bool showRuntimeSection = true;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(6);
        showRuntimeSection = EditorGUILayout.Foldout(showRuntimeSection, "Runtime Private Fields (Play Mode)");

        if (!showRuntimeSection) return;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to edit private/runtime values.", MessageType.Info);
            return;
        }

        CombatManager cm = (CombatManager)target;

        Type t = typeof(CombatManager);
        var flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

        // Iterate fields and show editable controls for common types and CombatantInstance objects
        FieldInfo[] fields = t.GetFields(flags);

        foreach (var f in fields)
        {
            // skip compiler generated/backing fields
            if (f.Name.Contains("k__BackingField")) continue;

            object value = f.GetValue(cm);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(f.Name, EditorStyles.boldLabel);

            if (value == null)
            {
                EditorGUILayout.LabelField("(null)");
            }
            else if (value is CombatantInstance combatant)
            {
                DrawCombatantInstanceEditor(cm, f, combatant);
            }
            else
            {
                DrawSimpleFieldEditor(cm, f, value);
            }

            EditorGUILayout.EndVertical();
        }
    }

    private void DrawSimpleFieldEditor(CombatManager cm, FieldInfo f, object value)
    {
        Type ft = f.FieldType;
        object newValue = null;

        if (ft == typeof(int))
        {
            int v = (int)f.GetValue(cm);
            int nv = EditorGUILayout.IntField("Value", v);
            newValue = nv;
        }
        else if (ft == typeof(float))
        {
            float v = (float)f.GetValue(cm);
            float nv = EditorGUILayout.FloatField("Value", v);
            newValue = nv;
        }
        else if (ft == typeof(bool))
        {
            bool v = (bool)f.GetValue(cm);
            bool nv = EditorGUILayout.Toggle("Value", v);
            newValue = nv;
        }
        else if (ft.IsEnum)
        {
            Enum v = (Enum)f.GetValue(cm);
            Enum nv = EditorGUILayout.EnumPopup("Value", v);
            newValue = nv;
        }
        else if (typeof(UnityEngine.Object).IsAssignableFrom(ft))
        {
            UnityEngine.Object v = (UnityEngine.Object)f.GetValue(cm);
            UnityEngine.Object nv = EditorGUILayout.ObjectField("Ref", v, ft, true);
            newValue = nv;
        }
        else
        {
            EditorGUILayout.LabelField($"Type {ft.Name} not editable here.");
        }

        if (newValue != null)
        {
            Undo.RecordObject(target as UnityEngine.Object, "Edit Private Field");
            f.SetValue(cm, newValue);
            EditorUtility.SetDirty(target);
        }
    }

    private void DrawCombatantInstanceEditor(CombatManager cm, FieldInfo field, CombatantInstance combatant)
    {
        if (combatant == null)
        {
            EditorGUILayout.LabelField("(null)");
            return;
        }

        EditorGUILayout.LabelField("Name:", combatant.CharacterName);

        // Basic editable abstract properties
        EditorGUI.BeginChangeCheck();
        int cur = EditorGUILayout.IntField("CurrentHealth", combatant.CurrentHealth);
        int max = EditorGUILayout.IntField("MaxHealth", combatant.MaxHealth);
        float def = EditorGUILayout.FloatField("CurrentDefence", combatant.CurrentDefence);
        float weap = EditorGUILayout.FloatField("BonusWeaponDamage", combatant.EquippedWeaponInstance.bonusDamage);
        float spel = EditorGUILayout.FloatField("CurrentSpellDamage", combatant.CurrentSkillDmg);
        int burn = EditorGUILayout.IntField("CurrentBurnLevel", combatant.CurrentBurnLevel);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target as UnityEngine.Object, "Edit Combatant Stats");
            combatant.CurrentHealth = Mathf.Clamp(cur, 0, int.MaxValue);
            combatant.MaxHealth = Math.Max(1, max);
            combatant.CurrentDefence = def;
            combatant.EquippedWeaponInstance.bonusDamage = weap;
            combatant.CurrentSkillDmg = spel;
            combatant.CurrentBurnLevel = Mathf.Clamp(burn, 1, 3);
            EditorUtility.SetDirty(target);
            Repaint();
        }

        // Reflect over fields
        Type ct = combatant.GetType();
        FieldInfo[] cfields = ct.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var cf in cfields)
        {
            if (cf.Name.Contains("k__BackingField")) continue;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(cf.Name, GUILayout.Width(160));
            Type ft = cf.FieldType;
            object val = cf.GetValue(combatant);

            if (typeof(UnityEngine.Object).IsAssignableFrom(ft))
            {
                UnityEngine.Object nv = EditorGUILayout.ObjectField((UnityEngine.Object)val, ft, true);
                if (nv != (UnityEngine.Object)val)
                {
                    cf.SetValue(combatant, nv);
                }
            }
            else if (ft.IsGenericType && ft.GetGenericTypeDefinition() == typeof(List<>))
            {
                DrawListEditor((IList)val);
            }
            else
            {
                EditorGUILayout.LabelField($"({ft.Name})");
            }

            EditorGUILayout.EndHorizontal();
        }

        // Reflect over properties (to handle ActiveEffects with private setter)
        PropertyInfo[] cprops = ct.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var prop in cprops)
        {
            if (prop.GetIndexParameters().Length > 0) continue;
            if (!prop.CanRead) continue;
            if (prop.Name == "CharacterName") continue;

            object val = null;
            try { val = prop.GetValue(combatant); } catch { val = null; }

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(prop.Name, EditorStyles.miniBoldLabel);

            if (val == null)
            {
                EditorGUILayout.LabelField("(null)");
                EditorGUILayout.EndVertical();
                continue;
            }

            Type pt = prop.PropertyType;

            if (typeof(IList).IsAssignableFrom(pt))
            {
                DrawListEditor((IList)val);
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(pt))
            {
                // if writable, allow setting; otherwise show object
                UnityEngine.Object ov = (UnityEngine.Object)val;
                if (prop.CanWrite)
                {
                    UnityEngine.Object nv = EditorGUILayout.ObjectField(ov, pt, true);
                    if (nv != ov) prop.SetValue(combatant, nv);
                }
                else
                {
                    EditorGUILayout.ObjectField(ov, pt, true);
                }
            }
            else
            {
                EditorGUILayout.LabelField($"({pt.Name}) not directly editable via inspector");
            }

            EditorGUILayout.EndVertical();
        }
    }

    private void DrawListEditor(IList list)
    {
        if (list == null)
        {
            EditorGUILayout.LabelField("(null list)");
            return;
        }

        Type elemType = list.GetType().GetGenericArguments()[0];
        int removeIndex = -1;

        EditorGUILayout.LabelField($"Count: {list.Count}");

        for (int i = 0; i < list.Count; i++)
        {
            object elem = list[i];
            EditorGUILayout.BeginHorizontal();
            if (elem == null)
            {
                EditorGUILayout.LabelField($"[{i}] (null)");
            }
                else if (elem is Effect eff)
            {
                var newType = (EffectType)EditorGUILayout.EnumPopup(eff.type, GUILayout.Width(120));
                var newIntensity = EditorGUILayout.IntField(eff.intensity, GUILayout.Width(50));
                var newDuration = EditorGUILayout.IntField(eff.duration, GUILayout.Width(50));
                var newIsDebuff = EditorGUILayout.Toggle(eff.isDebuff, GUILayout.Width(30));
                EditorGUILayout.LabelField("", GUILayout.Width(10));

                if (newType != eff.type || newIntensity != eff.intensity || newDuration != eff.duration || newIsDebuff != eff.isDebuff)
                {
                    Undo.RecordObject(target as UnityEngine.Object, "Edit Effect");
                    eff.type = newType;
                    eff.intensity = newIntensity;
                    eff.duration = newDuration;
                    eff.isDebuff = newIsDebuff;
                    EditorUtility.SetDirty(target);
                    Repaint();
                }
            }
            else if (elem is Upgrade upg)
            {
                var newType = (UpgradeNames)EditorGUILayout.EnumPopup(upg.type, GUILayout.Width(120));
                var newIntensity = EditorGUILayout.IntField(upg.intensity, GUILayout.Width(50));
                if (newType != upg.type || newIntensity != upg.intensity)
                {
                    Undo.RecordObject(target as UnityEngine.Object, "Edit Upgrade");
                    upg.type = newType;
                    upg.intensity = newIntensity;
                    EditorUtility.SetDirty(target);
                    Repaint();
                }
            }
            else if (elem is UnityEngine.Object uobj)
            {
                UnityEngine.Object nv = EditorGUILayout.ObjectField(uobj, elemType, true);
                if (nv != uobj) list[i] = nv;
            }
            else
            {
                EditorGUILayout.LabelField(elem.ToString());
            }

            if (GUILayout.Button("Remove", GUILayout.Width(60))) removeIndex = i;
            EditorGUILayout.EndHorizontal();
        }

        if (removeIndex >= 0) list.RemoveAt(removeIndex);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Default"))
        {
            object newElem = null;
            if (elemType == typeof(Effect)) newElem = new Effect(EffectType.Burn, 3, true, 1);
            else if (elemType == typeof(Upgrade)) newElem = new Upgrade("ya", "BABA", 0, UpgradeNames.WeaponMastery, CharacterPool.None, 1, false);
            else if (typeof(UnityEngine.Object).IsAssignableFrom(elemType)) newElem = null;
            else
            {
                try { newElem = System.Activator.CreateInstance(elemType); } catch { newElem = null; }
            }

            if (newElem != null) list.Add(newElem);
        }
        if (GUILayout.Button("Clear")) list.Clear();
        EditorGUILayout.EndHorizontal();
    }
}
