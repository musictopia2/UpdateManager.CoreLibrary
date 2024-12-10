namespace UpdateManager.CoreLibrary.Utilities;
// Custom exception to make error handling clearer and more specific
public class InvalidProjectTypeException : Exception
{
    public InvalidProjectTypeException(string message) : base(message) { }
}