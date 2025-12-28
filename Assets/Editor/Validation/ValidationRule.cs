using System.Collections.Generic;
using UnityEngine;

public abstract class ValidationRule
{
    public virtual List<ValidationError> Validate( DialogueGraph dialogueGraph )
    {
        return new List<ValidationError>();
    }

    protected ValidationError CreateError(string message, Question dialogue, ErrorSeverity severity)
    {
       ValidationError validationError = new ValidationError();
       validationError.message = message;
       validationError.affectedDialogue = dialogue;
       validationError.severity = severity;
       return validationError;
    }
    protected  ValidationError CreateError(string message, Question dialogue, int choiceIndex ,ErrorSeverity severity)
    {
        ValidationError validationError = new ValidationError();
        validationError.message = message;
        validationError.affectedDialogue = dialogue;
        validationError.choiceIndex = choiceIndex;
        validationError.severity = severity;
        return validationError;
    }

}
