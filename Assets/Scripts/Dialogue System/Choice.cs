using UnityEditor;
using UnityEngine;
using UnityEngine.UI;



[System.Serializable]
public struct Choice
{
    public string text;
    public Question nextDialogue;
    public AudioClip choiceAudio;
    private Image icon;
    private string condition;
    private bool isEnabled;

}
