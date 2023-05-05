# Unity IPC

This tool uses pipes to provide data transfering between Unity and any other application. The following (class name editable) scripts are provided:

- IPCUnity: This class must belong to your Unity project.
- IPCApp: This class must belong to your secondary app. 
- IPCMessage: This class must belong to both.

### IPCMessage class
This class represents the message sent between the pipes. In order to customize each message, fields must be changed as well the constructor accordingly. In recommandation, each message should contain (not necessarily) a command type ID field in order to simplify the handling, like in the script example.

### IPCUnity and IPCApp classes
Requires Newtonsoft package to be installed on the secondary app via NuGet. Both classes are singleton, similar, featuring the following static methods and components:

- Initialize() -> Initializes the IPC handler. If is the case, Unity method can also start the secondary app by parsing the app executable file path and execution args. Must be called in the beggining by both apps (not simultaneously necessarily).
- SendMessage() -> Sends a message of type IPCMessage from Unity/App to the other one.
- Dispose() -> Stops the connection between the apps. If the secondary app was started from Unity, this method also closes that app.
- MessagesRecv -> A thread-safe Queue that stores all messages received from the counterpart app. In order to safely extract and execute a message, this must be Dequeued.


### Example of use from secondary app
```csharp
using System.Threading;

public class Program
{
    public static void Main(string[] args)
    {
        IPCApp.Initialize();
        Thread messageRecvHandlerThread = new Thread(Execute);
        messageRecvHandlerThread.Start();

        // Send a message to Unity
        IPCApp.SendMessage(new IPCMessage("Hello from 2nd app!"));
    }


    public static void Execute()
    {
        while(true)
        {
            IPCMessage messageToExecute;
            IPCApp.MessagesRecv.TryDequeue(out messageToExecute);
            HandleMessage(messageToExecute);
        }
    }
    private static void HandleMessage(IPCMessage message)
    {
        if(message == null)
            return;

        switch(message.type == ...)
        {
            ...;
        }
    }
}

```
IPCUnity is used in a similar manner.