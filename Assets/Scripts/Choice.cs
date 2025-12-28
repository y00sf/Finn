using UnityEditor;
using UnityEngine;
using UnityEngine.UI;



[System.Serializable]
public struct Choice
{
    public string text;
    public Question nextDialogue;
    private Image icon;
    private string condition;
    private bool isEnabled;

}
