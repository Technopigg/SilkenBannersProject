using System.Collections.Generic;
using UnityEngine;

public class BuffController : MonoBehaviour
{
    [Header("UI Manager Reference")]
    public BuffUIManager uiManager;

    private Dictionary<string, BuffInstance> activeBuffs = new Dictionary<string, BuffInstance>();


    // ------------------------------------------------------------
    // MAIN UPDATE LOOP
    // ------------------------------------------------------------
    private void Update()
    {
        UpdateTimers();
        TestInput();   // Remove once you no longer need testing
    }


    // ============================================================
    //  UPDATE TIMERS
    // ============================================================
    private void UpdateTimers()
    {
        if (activeBuffs.Count == 0)
            return;

        List<string> expired = new List<string>();

        foreach (var pair in activeBuffs)
        {
            BuffInstance inst = pair.Value;

            inst.remainingTime -= Time.deltaTime;

            // Expired?
            if (inst.remainingTime <= 0)
            {
                expired.Add(pair.Key);
                continue;
            }

            // Send timer update to UI
            if (inst.data.isDebuff)
                uiManager.UpdateDebuffTimer(inst.data.id, inst.remainingTime, inst.data.defaultDuration);
            else
                uiManager.UpdateBuffTimer(inst.data.id, inst.remainingTime, inst.data.defaultDuration);
        }

        // Clean up expired in last step to avoid modifying dictionary during iteration
        foreach (string id in expired)
            RemoveBuff(id);
    }


    // ============================================================
    //  ADD / REFRESH BUFF
    // ============================================================
    public void AddBuff(BuffData data)
    {
        // Already has the buff? → Refresh duration + manage stacks
        if (activeBuffs.TryGetValue(data.id, out BuffInstance inst))
        {
            // Stack if allowed
            if (inst.stacks < data.maxStacks)
                inst.stacks++;

            // Refresh duration
            inst.remainingTime = data.defaultDuration;

            // Send updated UI
            SendBuffToUI(inst);
            return;
        }

        // New buff
        BuffInstance newBuff = new BuffInstance(data);
        activeBuffs.Add(data.id, newBuff);

        // Add UI icon
        SendBuffToUI(newBuff);
    }


    // ============================================================
    //  REMOVE BUFF
    // ============================================================
    public void RemoveBuff(string id)
    {
        if (!activeBuffs.TryGetValue(id, out BuffInstance inst))
            return;

        activeBuffs.Remove(id);

        if (inst.data.isDebuff)
            uiManager.RemoveDebuff(id);
        else
            uiManager.RemoveBuff(id);
    }


    // ============================================================
    //  SEND BUFF TO UI (add or refresh)
    // ============================================================
    private void SendBuffToUI(BuffInstance inst)
    {
        string id = inst.data.id;
        Sprite icon = inst.data.icon;
        int stacks = inst.stacks;
        float duration = inst.remainingTime;

        if (inst.data.isDebuff)
            uiManager.AddDebuff(id, icon, stacks, duration);
        else
            uiManager.AddBuff(id, icon, stacks, duration);
    }


    // ============================================================
    //  TEMPORARY TEST INPUT
    // ============================================================
    [Header("Test Buffs (Press 1 and 2 keys)")]
    public List<BuffData> testBuffs;

    private void TestInput()
    {
        if (testBuffs == null || testBuffs.Count == 0)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1) && testBuffs.Count >= 1)
            AddBuff(testBuffs[0]);

        if (Input.GetKeyDown(KeyCode.Alpha2) && testBuffs.Count >= 2)
            AddBuff(testBuffs[1]);
    }
}
