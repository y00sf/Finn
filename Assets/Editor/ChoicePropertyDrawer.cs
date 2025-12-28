using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Choice))]
public class ChoicePropertyDrawer : PropertyDrawer
{
    private const float SPACING = 2f;
    private const float NEXT_DIALOGUE_WIDTH = 100f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float line = EditorGUIUtility.singleLineHeight;

       
        Rect choiceLabelRect = new Rect(
            position.x,
            position.y,
            position.width - NEXT_DIALOGUE_WIDTH - SPACING,
            line
        );

        Rect nextLabelRect = new Rect(
            position.xMax - NEXT_DIALOGUE_WIDTH,
            position.y,
            NEXT_DIALOGUE_WIDTH,
            line
        );

        GUIStyle miniStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.UpperLeft,
            normal = { textColor = Color.white }
        };

        GUIStyle warningStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.UpperCenter,
            normal = { textColor = Color.red }
        };

        EditorGUI.LabelField(choiceLabelRect, "Choice Text", miniStyle);

        SerializedProperty nextDialogueProp = property.FindPropertyRelative("nextDialogue");
        if (nextDialogueProp.objectReferenceValue == null)
        {
            EditorGUI.LabelField(nextLabelRect, "⚠ Missing", warningStyle);
        }
        else
        {
            EditorGUI.LabelField(nextLabelRect, "Next", miniStyle);
        }


       
        Rect secondLine = new Rect(
            position.x,
            position.y + line + SPACING,
            position.width,
            line
        );

        Rect textRect = new Rect(
            secondLine.x,
            secondLine.y,
            secondLine.width - NEXT_DIALOGUE_WIDTH - SPACING,
            line
        );

        Rect nextDialogueRect = new Rect(
            secondLine.xMax - NEXT_DIALOGUE_WIDTH,
            secondLine.y,
            NEXT_DIALOGUE_WIDTH,
            line
        );

        SerializedProperty textProp = property.FindPropertyRelative("text");

        EditorGUI.PropertyField(textRect, textProp, GUIContent.none);
        EditorGUI.PropertyField(nextDialogueRect, nextDialogueProp, GUIContent.none);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;
        return (line * 2) + (SPACING * 2); 
    }
}
