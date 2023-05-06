using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;

public class IPCUnity : MonoBehaviour 
{
    /// <summary>
    /// This buffer collects all messages (commands) received from other application. Execute each command by Dequeueing.
    /// </summary>
    public static ConcurrentQueue<IPCMessage> MessagesRecv;

    private static readonly string pipesFolder = "C:\\tmp";
    private static readonly string unityPipe = pipesFolder + "\\unity.txt";
    private static readonly string appPipe = pipesFolder + "\\app.txt";
    private static IPCUnity Instance;
    private Process process;
    private StreamWriter writer;
    private StreamReader reader;
    private Thread recvMessThread;

    
    /// <summary>
    /// Initializes IPC between Unity and other application. Runs app executable if path is provided.
    /// </summary>
    public static void Initialize(string applicationPath = null, params string[] args)
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("IPCUnity");
            go.AddComponent<IPCUnity>();
            DontDestroyOnLoad(go);
        }
        else
            throw new System.Exception("IPCUnity already instantiated!.");

        MessagesRecv = new ConcurrentQueue<IPCMessage>();

        if (!Directory.Exists(pipesFolder)) Directory.CreateDirectory(pipesFolder);
        if (!File.Exists(unityPipe)) File.Create(unityPipe).Dispose();      
        Instance.writer = new StreamWriter(new FileStream(unityPipe, FileMode.Truncate, FileAccess.Write, FileShare.Read));
        Instance.reader = new StreamReader(new FileStream(appPipe, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Write));
        Instance.reader.ReadToEnd();
        Instance.recvMessThread = new Thread(ReadMessages);
        Instance.recvMessThread.Start();


        if (applicationPath != null)
        {
            Instance.process = new Process();
            Instance.process.StartInfo.FileName = applicationPath;
            Instance.process.StartInfo.UseShellExecute = false;
            Instance.process.StartInfo.Arguments = string.Join(" ", args);
            Instance.process.Start();
        }
    }
    /// <summary>
    /// Send a message (command) to other application.
    /// </summary>
    /// <param name="message"></param>
    public static void SendMessage(IPCMessage message)
    {
        if (Instance == null)
            throw new System.Exception("IPCUnity not instantiated!");

        Instance.writer.WriteLine(JsonUtility.ToJson(message));
        Instance.writer.Flush();
    }
    /// <summary>
    /// Stops IPC between Unity and this application.
    /// </summary>
    public static void Dispose()
    {
        Instance.process?.Close();
        Instance.reader.Close();
        Instance.writer.Close();
        Instance.recvMessThread.Abort();
        Instance = null;
    }



    private static void ReadMessages()
    {
        while (true)
        {
            string appMessage = Instance.reader.ReadLine();
            if (appMessage != null)
            {
                MessagesRecv.Enqueue(new IPCMessage(JsonUtility.FromJson<IPCMessage>(appMessage)));
            }
        }
        Thread.CurrentThread.Abort();
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    private void OnApplicationQuit() => Dispose();
}
