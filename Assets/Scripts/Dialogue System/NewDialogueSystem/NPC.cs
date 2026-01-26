using UnityEngine;


[CreateAssetMenu(fileName = "NewNPC", menuName = "Dialogue System/NPC")]
public class NPC : ScriptableObject
{
    public string npcName;
    public Color npcBubbleColor;
    public Color npcTextColor;
}
