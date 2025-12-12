using UnityEngine;

public class SpellInstance
{
    public SpellData data;
    public float remainingCooldown;

    public SpellInstance(SpellData data)
    {
        this.data = data;
        remainingCooldown = 0f;
    }

    public bool IsReady => remainingCooldown <= 0f;

    public void TickCooldown(float deltaTime)
    {
        if (remainingCooldown > 0)
            remainingCooldown -= deltaTime;
    }

    public void TriggerCooldown()
    {
        remainingCooldown = data.cooldown;
    }
}