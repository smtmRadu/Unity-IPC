using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;
using Newtonsoft.Json;
using Unity.VisualScripting;

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
    /// Starts a new process by running the executable at path provided. When IPCUnity is destroyed, the app closes.
    /// </summary>
    public static void StartApplication(string applicationPath = null, params string[] args)
    {
        if(Instance.process != null)
            Instance.process.Kill();

        Instance.process = new Process();
        Instance.process.StartInfo.FileName = applicationPath;
        Instance.process.StartInfo.UseShellExecute = false;
        Instance.process.StartInfo.Arguments = string.Join(" ", args);
        Instance.process.Start();
    }
    /// <summary>
    /// Initializes IPC between Unity and other application.
    /// </summary>
    /// <param name="isServer"> The server must start first the IPC. Server app creates the pipes and tmp directory.</param>
    public static new void Instantiate(bool isServer)
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

        if (isServer)
        {
            if (!Directory.Exists(pipesFolder)) Directory.CreateDirectory(pipesFolder);
            if (File.Exists(unityPipe)) File.Delete(unityPipe);
            if (File.Exists(appPipe)) File.Delete(appPipe);
            Instance.writer = new StreamWriter(new FileStream(unityPipe, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite));
            Instance.reader = new StreamReader(new FileStream(appPipe, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite));
        }
        else
        {
            Instance.writer = new StreamWriter(new FileStream(unityPipe, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
            Instance.reader = new StreamReader(new FileStream(appPipe, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
            Instance.reader.ReadToEnd();
        }
       
        
        Instance.recvMessThread = new Thread(ReadMessages);
        Instance.recvMessThread.Start();
    }
    /// <summary>
    /// Send a message (command) to other application.
    /// </summary>
    /// <param name="message"></param>
    public static void SendMessage(IPCMessage message)
    {
        if (Instance == null)
            throw new System.Exception("IPCUnity not instantiated!");

        lock (Instance.writer)
        {
            Instance.writer.WriteLine(JsonConvert.SerializeObject(message));
            Instance.writer.Flush();
        }

        
    }
    /// <summary>
    /// Stops IPC between Unity and this application.
    /// </summary>

    private static void ReadMessages()
    {
        while (true)
        {
            string appMessage = Instance.reader.ReadLine();
            if (appMessage != null)
            {
                MessagesRecv.Enqueue(new IPCMessage(JsonConvert.DeserializeObject<IPCMessage>(appMessage)));
            }
        }
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


    private void OnDestroy()
    {
        Instance.process?.Close();
        Instance.reader.Close();
        Instance.writer.Close();
        Instance.recvMessThread?.Abort();
        Instance = null;
    }
}
