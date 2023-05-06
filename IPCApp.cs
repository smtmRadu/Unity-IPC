using Newtonsoft.Json;
using System.Collections.Concurrent;


namespace PPOHandler.Scripts.IPC
{
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
        /// <param name="isServer">The server must start first the IPC. Server app creates the pipes and tmp directory.</param>
        public static void Instantiate(bool isServer)
        {
            if (Instance == null)
                Instance = new IPCApp();
            else
                throw new Exception("IPCApp already instantiated!");

            MessagesRecv = new ConcurrentQueue<IPCMessage>();

            if (isServer)
            {
                if (!Directory.Exists(pipesFolder)) Directory.CreateDirectory(pipesFolder);
                if (File.Exists(unityPipe)) File.Delete(unityPipe);
                if (File.Exists(appPipe)) File.Delete(appPipe);   
                Instance.writer = new StreamWriter(new FileStream(appPipe, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite));
                Instance.reader = new StreamReader(new FileStream(unityPipe, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite));
                
            }
            else
            {
                Instance.writer = new StreamWriter(new FileStream(appPipe, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
                Instance.reader = new StreamReader(new FileStream(unityPipe, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));

                Instance.reader.ReadToEnd();
            }
            
            
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
                throw new Exception("IPCApp not instantiated!");

            lock(Instance.writer)
            {
                Instance.writer.WriteLine(JsonConvert.SerializeObject(message));
                Instance.writer.Flush();
            }
            
        }
        /// <summary>
        /// Stops IPC between this application and Unity.
        /// </summary>
        public static void Dispose()
        {
            if (Instance == null)
                return;

            Instance = null;
        }



        private static void ReadMessages()
        {
            while (Instance != null)
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
}
