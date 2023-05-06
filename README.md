
# Unity IPC

This tool uses file-sharing to provide data transfering between Unity and any other application. The following scripts are provided:

- IPCUnity: This class must belong to your Unity project.
- IPCApp: This class must belong to your the other app. 
- IPCMessage: This class must belong to both.

### IPCMessage class
This class represents the message object sent between the pipes and is fully customizable. If needs modification, change the fields and constructors of this class accordingly. (remember, **parametrized**, **default** and **copy** constructors are necessary). It is highly recommended to specify IPCMessage types for applications joining, as well as for leaving in order handle connection breaking automatically when the counterpart app was closed. **Server application must always instantiate IPC class first.**

### IPCUnity and IPCApp classes
**Requires Newtonsoft.Json** package to be installed on both applications. On each side, they are singleton, similar, featuring the following static methods and components:

- Instatiate(**bool** isServer) -> Initializes the IPC handler. 
- SendMessage(**IPCMessage** message) -> Sends a message from Unity/App to the other one.
- Dispose() -> Stops the connection between the apps. [Unity Only: This method also closes the process started if is the case]
- MessagesRecv -> A thread-safe Queue that stores all messages received from the counterpart app. In order to safely extract and execute a message, this must be Dequeued.

- StartApplication(**string** path, **params string[]** args) [Unity Only] -> Starts a new process running the executable at that path.
### Unity as **server**
```csharp
using UnityEngine;

public class IPCTester : MonoBehaviour
{
    private void Start()
    {
        //Start the other app (if is not already running)
        IPCUnity.StartApplication("...\\app.exe");
        IPCUnity.Instantiate(true);
    }

    private void FixedUpdate()
    {
        // Handle messages from 2nd app
        IPCMessage message;
        IPCUnity.MessagesRecv.TryDequeue(out message);

        if (message != null)        
            Debug.Log("[Other application]: " + System.Convert.ToString(message.data[0]));
        
        // Send a message every 1 second to 2nd app (considering Time.fixedDeltaTime = 0.02)
        if (Time.frameCount % 50 == 0) 
            IPCUnity.SendMessage(new IPCMessage("Hello from Unity!"));
    }
}
```
### App as **client**
```csharp
using System.Threading;

public class Program
{
    public static void Main(string[] args)
    {
        IPCApp.Instantiate(false);

        // Handle messages from Unity
        Thread recvMessages = new Thread(ReadMessages);
        recvMessages.Start();
    
        // Send a message every 1 second to Unity
        Thread sendMessages = new Thread(SendMessages);
        sendMessages.Start();
    }

    public static void SendMessages()
    {  
        IPCApp.SendMessage(new IPCMessage("Hello from 2ndApp!"));
        Thread.CurrentThread.Sleep(1000);
    }
    public static void ReadMessages()
    {
        while(true)
        {
            IPCMessage message;
            IPCApp.MessagesRecv.TryDequeue(out message);

            if(message == null)
                return;

            // Handle message types separatelly (if is the case)
            Console.WriteLine("[Unity]: " + System.Convert.ToString(message.data[0]));
        }
    }
}

```