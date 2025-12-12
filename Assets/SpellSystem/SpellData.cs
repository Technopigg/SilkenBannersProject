using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Spells/SpellData")]
public class SpellData : ScriptableObject
{
    [Header("Basic Info")]
    public string spellName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Timing")]
    public float castTime = 1f; 
    public float cooldown = 5f; 

    [Header("Targeting")]
    public bool targetsSelf = false;
    public bool targetsAlly = false;
    public bool targetsEnemy = false;

    [Header("Effects")]
    public List<BuffData> buffsToApply; 
    public float damage = 0f;         
}