using System.Collections.Generic;
using UnityEngine;

public class BrokenReferenceRule : ValidationRule
{
    public List<ValidationError> Validate(DialogueGraph graph)
    {
        List<ValidationError> errors = new List<ValidationError>();
       HashSet<Question> node = graph.GetAllNodes();
       foreach (var question in node)
       {
           if (question.choices == null || question.choices.Count == 0)
           {
               continue;
           }

           for (int i = 0; i < question.choices.Count; i++)
           {
               var choice = question.choices[i];
               
               if (choice.nextDialogue == null)
               {
                   string errorMessage = $"Broken Reference: Choice '{choice.text}' leads nowhere.";

                  
                   errors.Add(CreateError(
                       errorMessage, 
                       question, 
                       i, 
                       ErrorSeverity.WARNING 
                   ));  
               }
           }
       }
       return errors;
    }
}
