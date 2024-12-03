namespace UpdateManager.CoreLibrary.Utilities;
public class ConfigurationKeyNotFoundException : Exception
{
    public ConfigurationKeyNotFoundException() { }

    public ConfigurationKeyNotFoundException(string message)
        : base(message) { }

    public ConfigurationKeyNotFoundException(string message, Exception inner)
        : base(message, inner) { }
}