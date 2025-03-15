using System.Diagnostics;
using System.IO;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    private Process serverProcess;
   

    void Start()
    {
        string serverPath = Path.Combine(Application.streamingAssetsPath, "server");

        string nodePath = Path.Combine(serverPath, "node.exe"); 
        if (!File.Exists(nodePath)) nodePath = "node"; 

        StartServer(serverPath, nodePath);
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
