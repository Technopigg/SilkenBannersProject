public class BuffInstance
{
    public BuffData data;
    public int stacks;
    public float remainingTime;

    public BuffInstance(BuffData data)
    {
        this.data = data;
        stacks = 1;
        remainingTime = data.defaultDuration;
    }
}