using System.Diagnostics;
using System.IO;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    public static ServerManager Instance { get; private set; }

    private Process serverProcess;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        if (IsServerComputer()) // Only start server on the host
        {
            string serverPath = Path.Combine(Application.streamingAssetsPath, "server");

            string nodePath = Path.Combine(serverPath, "node.exe");
            if (!File.Exists(nodePath)) nodePath = "node";

            StartServer(serverPath, nodePath);
        }
        else
        {
            DebugUtils.LogColor("Client mode: Not starting the server.", "yellow");
        }
    }

    bool IsServerComputer()
    {
        return SystemInfo.deviceName == "LAPTOP-LK6VUOAI"; 
    }


    void StartServer(string serverPath, string nodePath)
    {
        serverProcess = new Process();
        serverProcess.StartInfo.FileName = nodePath;
        serverProcess.StartInfo.Arguments = "pong-server.js";
        serverProcess.StartInfo.WorkingDirectory = serverPath;
        serverProcess.StartInfo.CreateNoWindow = true;
        serverProcess.StartInfo.UseShellExecute = false;
        serverProcess.StartInfo.RedirectStandardOutput = true;
        serverProcess.StartInfo.RedirectStandardError = true;
        serverProcess.OutputDataReceived += (sender, e) => DebugUtils.LogColor(e.Data, "green");
        serverProcess.ErrorDataReceived += (sender, e) => DebugUtils.LogColor(e.Data, "red");

        serverProcess.Start();
        serverProcess.BeginOutputReadLine();
        serverProcess.BeginErrorReadLine();
    }

    void OnApplicationQuit()
    {
        if (serverProcess != null && !serverProcess.HasExited)
        {
            serverProcess.Kill();
        }
    }
}
