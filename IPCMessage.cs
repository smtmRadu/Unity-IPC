using Newtonsoft.Json;

/// <summary>
/// This class must belong to both Unity and other application.
/// </summary>
public class IPCMessage
{
    // MODIFY
    public CommandType type;
    public string description;
    public double[] otherSerializableField;

    /// <summary>
    /// Creates an IPC message (command) to send to Unity.
    /// </summary>
    public IPCMessage(CommandType type, string description, double[] values)
    {
        this.type = type;
        this.description = description;
        this.otherSerializableField = values;
    }
    public IPCMessage(string encoded)
    {
        var deserialized = JsonConvert.DeserializeObject<IPCMessage>(encoded);
        if (deserialized == null)
            throw new Exception("Unable to deserialize message!.");

        // MODIFY
        this.type = deserialized.type;
        this.description = deserialized.desc;
        this.otherSerializableField = deserialized.vals;
    }


    // DO NOT MODIFY
    public IPCMessage() { } // newtonsoftjs need a default construtor
    public string Encode() => JsonConvert.SerializeObject(this);
}
public enum CommandType
{
    // MODIFY
    Message,
    ForwardValues,
    BackwardValues,

}
