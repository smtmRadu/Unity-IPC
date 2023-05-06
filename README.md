
# Unity IPC

This tool uses pipes to provide data transfering between Unity and any other application. The following (class name editable) scripts are provided:

- IPCUnity: This class must belong to your Unity project.
- IPCApp: This class must belong to your secondary app. 
- IPCMessage: This class must belong to both.

### IPCMessage class
This class represents the message object sent between the pipes and is fully customizable. In order to modify each message format, change the fields and constructors of this class accordingly. Though, **parametrized**, **default** and **copy** constructors are necessary (see script example). It is highly recommended to specify IPCMessage types for applications joining, as well as for leaving in order handle connection breaking automatically when the counterpart app was closed.

### IPCUnity and IPCApp classes
Requires Newtonsoft package to be installed on the non-Unity app via NuGet. Both classes are singleton, similar, featuring the following static methods and components:

- Initialize() -> Initializes the IPC handler. If is the case, Unity method can also start the secondary app by parsing the app executable file path and execution args. Must be called in the beggining by both apps (not simultaneously necessarily).
- SendMessage() -> Sends a message of type IPCMessage from Unity/App to the other one.
- Dispose() -> Stops the connection between the apps. If the secondary app was started from Unity, this method also closes that app.
- MessagesRecv -> A thread-safe Queue that stores all messages received from the counterpart app. In order to safely extract and execute a message, this must be Dequeued.

### Example of use from Unity
```csharp
using UnityEngine;

public class IPCTester : MonoBehaviour
{
    private void Start()
    {
        IPCUnity.Initialize();
    }

    private void FixedUpdate()
    {
        // Handle messages from 2nd app
        IPCMessage message;
        IPCUnity.MessagesRecv.TryDequeue(out message);
        if (message != null && message.type == CommandType.Message)        
            Debug.Log("[Other application]: " + message.desc);
        

        // Send a message every 1 second to 2nd app
        if (Time.frameCount % 50 == 0) IPCUnity.SendMessage(new IPCMessage(CommandType.Message, "Hello from Unity!"));
    }
}
```
### Example of use from secondary app
```csharp
using System.Threading;

public class Program
{
    public static void Main(string[] args)
    {
        IPCApp.Initialize();

        // Handle messages from Unity
        Thread recvMessages = new Thread(ReadMessages);
        recvMessages.Start();

        // Send a message every 1 second to Unity
        Thread sendMessages = new Thread(SendMessages);
        sendMessages.Start();
    }

    public static void SendMessages()
    {
        IPCApp.SendMessage(new IPCMessage.SendMessage(new IPCMessage(CommandType.Message, "Hello from 2ndApp!"));
        Thread.CurrentThread.Sleep(1000);
    }
    public static void ReadMessages()
    {
        while(true)
        {
            IPCMessage message;
            IPCApp.MessagesRecv.TryDequeue(out message);

            // Handle message commands separately
            if(message == null)
                return;

            if(message.type == CommandType.Message)
            {
                Console.WriteLine(message.desc);
                return;
            }
            
            if(...)
            {
                ...;
                return;
            }
        }
    }
}

```