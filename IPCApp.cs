using Newtonsoft.Json;
using System.Collections.Concurrent;

/// <summary>
/// This class belongs to your console application.
/// </summary>
public class IPCApp
{
    /// <summary>
    /// This buffer collects all messages (commands) received from Unity. Execute each command by Dequeueing.
    /// </summary>
    public static ConcurrentQueue<IPCMessage> MessagesRecv;

    private static readonly string pipesFolder = "C:\\tmp";
    private static readonly string unityPipe = pipesFolder + "\\unity.txt";
    private static readonly string appPipe = pipesFolder + "\\app.txt";
    private static IPCApp Instance;
    private StreamReader reader;
    private StreamWriter writer;
    private Thread recvMessThread;
    


    /// <summary>
    /// Initializes IPC between this application and Unity.
    /// </summary>
    public static void Initialize() 
    {
        if (Instance == null)
            Instance = new IPCApp();
        else
            throw new Exception("IPCApp already instantiated!");

        MessagesRecv = new ConcurrentQueue<IPCMessage> ();

        if (!Directory.Exists(pipesFolder)) Directory.CreateDirectory(pipesFolder);
        if (!File.Exists(appPipe)) File.Create(appPipe).Dispose();
        Instance.writer = new StreamWriter(new FileStream(appPipe, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite));
        Instance.reader = new StreamReader(new FileStream(unityPipe, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite));          
        Instance.reader.ReadToEnd();

        Instance.recvMessThread = new Thread(ReadMessages); 
        Instance.recvMessThread.Start();
    }
    /// <summary>
    /// Send a message (command) to Unity.
    /// </summary>
    /// <param name="message"></param>
    public static void SendMessage(IPCMessage message)
    {
        if (Instance == null)
            throw new System.Exception("IPCApp not instantiated!");

        Instance.writer.WriteLine(JsonConvert.SerializeObject(message));
        Instance.writer.Flush();
    }
    /// <summary>
    /// Stops IPC between this application and Unity.
    /// </summary>
    public static void Dispose()
    {
        Instance.recvMessThread?.Abort();
        Instance = null;
    }


    
    private static void ReadMessages()
    {
        while (true)
        {
            string unityMessage = Instance.reader.ReadLine();
            if (unityMessage != null)
            {
                MessagesRecv.Enqueue(new IPCMessage(JsonConvert.DeserializeObject<IPCMessage>(unityMessage)));
            }
        }
    }
    private IPCApp() { }
}

