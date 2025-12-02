using UnityEngine;

[CreateAssetMenu(menuName = "Status Effects/BuffData")]
public class BuffData : ScriptableObject
{
    public string id;
    public string displayName;
    [TextArea] public string description;

    public Sprite icon;

    public bool isDebuff;

    public float defaultDuration = 5f;
    public int maxStacks = 1;
}