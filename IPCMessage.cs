/// <summary>
/// This class belongs to both Unity and other application.
/// </summary>
public class IPCMessage
{
    public object[] data;

    public IPCMessage(params object[] data)
    {
        this.data = data;
    }
    public IPCMessage(IPCMessage other)
    {
        this.data = other.data;
    }
    public IPCMessage() { /*Default constructor is required.*/ }
}
