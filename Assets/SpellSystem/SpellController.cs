using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BuffController))]
[RequireComponent(typeof(Animator))]
public class SpellController : MonoBehaviour
{
    [Header("Spells")]
    public List<SpellData> knownSpells;

    private Dictionary<string, SpellInstance> activeSpells = new Dictionary<string, SpellInstance>();
    private BuffController buffController;
    private Animator animator;

    private bool isCasting = false;

    void Awake()
    {
        buffController = GetComponent<BuffController>();
        animator = GetComponent<Animator>();
        foreach (var spell in knownSpells)
            if (!activeSpells.ContainsKey(spell.spellName))
                activeSpells.Add(spell.spellName, new SpellInstance(spell));
    }

    void Update()
    {
        foreach (var si in activeSpells.Values)
            si.TickCooldown(Time.deltaTime);
        TestInput();
    }

    public void CastSpell(string spellName, GameObject target)
    {
        if (isCasting) return;

        if (!activeSpells.TryGetValue(spellName, out SpellInstance spell))
        {
            Debug.LogError($"{name} does not know spell '{spellName}'");
            return;
        }

        if (!spell.IsReady) return;

        StartCoroutine(CastRoutine(spell, target));
    }

    private IEnumerator CastRoutine(SpellInstance spell, GameObject target)
    {
        isCasting = true;
        animator.SetBool("IsCasting", true);
        yield return new WaitForSeconds(spell.data.castTime);
        if (target != null)
        {
            BuffController targetBuffs = target.GetComponent<BuffController>();
            if (targetBuffs != null)
            {
                foreach (var buff in spell.data.buffsToApply)
                    targetBuffs.AddBuff(buff);
            }

 
            if (spell.data.damage > 0)
            {
                if (target.TryGetComponent<UnitHealth>(out var hp))
                    hp.TakeDamage(spell.data.damage);
            }
        }

        spell.TriggerCooldown();
        animator.SetBool("IsCasting", false);
        isCasting = false;
    }

    private void TestInput()
    {
        if (Input.GetKeyDown(KeyCode.Q) && knownSpells.Count > 0)
            CastSpell(knownSpells[0].spellName, gameObject); 
    }
}
