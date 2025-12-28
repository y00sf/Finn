
public class ValidationError 
{
    public string message;
    public Question affectedDialogue;
    public int? choiceIndex; // Nullable - not all errors are about choices
    public ErrorSeverity severity;
}
