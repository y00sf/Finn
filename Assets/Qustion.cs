using System;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
[CreateAssetMenu(fileName = "Dialuage", menuName = "Scriptable Objects/Dialuage")]
public class Question : ScriptableObject
{
   public string SpeakerName;
   public Sprite sprite;
   [TextArea]
   public string questionText;
   public List<Choice> choices;
   
   
   public Rect rect;
   



   
}