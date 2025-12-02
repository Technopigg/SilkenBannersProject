using System.Collections.Generic;
using UnityEngine;

public class BuffUIManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject buffIconPrefab;
    public GameObject debuffIconPrefab;

    [Header("Containers")]
    public Transform buffContainer;
    public Transform debuffContainer;
    
    private Dictionary<string, BuffIconUI> activeBuffs = new Dictionary<string, BuffIconUI>();
    private Dictionary<string, BuffIconUI> activeDebuffs = new Dictionary<string, BuffIconUI>();
    
    public void AddBuff(string buffID, Sprite icon, int stacks, float duration)
    {
        if (activeBuffs.TryGetValue(buffID, out BuffIconUI ui))
        {
            ui.UpdateBuff(stacks, duration); 
            return;
        }

        // Create new icon
        GameObject newIcon = Instantiate(buffIconPrefab, buffContainer);
        ui = newIcon.GetComponent<BuffIconUI>();
        ui.Setup(buffID, icon, stacks, duration);

        activeBuffs.Add(buffID, ui);
    }


    public void AddDebuff(string debuffID, Sprite icon, int stacks, float duration)
    {
        if (activeDebuffs.TryGetValue(debuffID, out BuffIconUI ui))
        {
            ui.UpdateBuff(stacks, duration);
            return;
        }

        GameObject newIcon = Instantiate(debuffIconPrefab, debuffContainer);
        ui = newIcon.GetComponent<BuffIconUI>();
        ui.Setup(debuffID, icon, stacks, duration);

        activeDebuffs.Add(debuffID, ui);
    }


    public void UpdateBuffTimer(string buffID, float remaining, float duration)
    {
        if (activeBuffs.TryGetValue(buffID, out BuffIconUI ui))
            ui.UpdateTimer(remaining, duration);
    }

    public void UpdateDebuffTimer(string debuffID, float remaining, float duration)
    {
        if (activeDebuffs.TryGetValue(debuffID, out BuffIconUI ui))
            ui.UpdateTimer(remaining, duration);
    }



    public void RemoveBuff(string buffID)
    {
        if (activeBuffs.TryGetValue(buffID, out BuffIconUI ui))
        {
            Destroy(ui.gameObject);
            activeBuffs.Remove(buffID);
        }
    }

    public void RemoveDebuff(string debuffID)
    {
        if (activeDebuffs.TryGetValue(debuffID, out BuffIconUI ui))
        {
            Destroy(ui.gameObject);
            activeDebuffs.Remove(debuffID);
        }
    }
}
