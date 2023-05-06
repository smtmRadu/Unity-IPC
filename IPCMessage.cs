/// <summary>
/// This class belongs to both Unity and other application.
/// </summary>
public class IPCMessage
{
    public CommandType type;
    public string desc;
    public double[] vals;
    public IPCMessage(CommandType type, string description, double[] values)
    {
        this.type = type;
        this.desc = description;
        this.vals = values;
    }
    public IPCMessage(IPCMessage other)
    {
        this.type = other.type;
        this.desc = other.desc;
        this.vals = other.vals;
    }
    public IPCMessage() { /*Default constructor is required.*/ }
}
public enum CommandType
{
    Message,
    ForwardValues,
    BackwardValues,
}
