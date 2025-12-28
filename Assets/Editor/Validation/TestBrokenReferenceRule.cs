using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class TestBrokenReferenceRule
{
   
    [MenuItem("Tools/Test Broken Reference Rule")]
    public static void TestBrokenReferenceDetection()
    {
        Debug.Log("Starting Validation Test...");

        
        string validPath = "Assets/Tests/TestDialogue_Valid.asset";
        string brokenPath = "Assets/Tests/TestDialogue_Broken.asset";

        Question validNode = AssetDatabase.LoadAssetAtPath<Question>(validPath);
        Question brokenNode = AssetDatabase.LoadAssetAtPath<Question>(brokenPath);

        // Safety Check: Did we actually find the files?
        if (validNode == null || brokenNode == null)
        {
            Debug.LogError("Could not find test assets! Please ensure 'TestDialogue_Valid.asset' and 'TestDialogue_Broken.asset' exist in 'Assets/Tests/'.");
            return;
        }
        
        Question[] questions = new Question[] { validNode, brokenNode };
        DialogueGraph graph = new DialogueGraph(questions);

        
        BrokenReferenceRule rule = new BrokenReferenceRule();
        List<ValidationError> errors = rule.Validate(graph);
        
        int expectedErrors = 2;

        if (errors.Count == expectedErrors)
        {
            Debug.Log($"<color=green> TEST PASSED!</color> Found exactly {errors.Count} errors as expected.");
            
           
            foreach(var error in errors)
            {
                Debug.Log($"   - Detected: {error.message} (Severity: {error.severity})");
            }
        }
        else
        {
            Debug.LogError($" <color=red> TEST FAILED.</color> Expected {expectedErrors} errors, but found {errors.Count}.");
            
          
            foreach(var error in errors)
            {
                Debug.Log($"   - Found: {error.message}");
            }
        }
    }
}
