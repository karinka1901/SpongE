using UnityEngine;
using SimpleJSON;
using System;
using SocketIOClient;

public class PongNetworkManager : MonoBehaviour
{
    private SocketIOUnity socket;

    private void Start()
    {
        socket = new SocketIOUnity("http://localhost:3000");

        socket.OnConnected += (sender, e) =>
        {
            DebugUtils.LogColor("Connected to server", "cyan");
        };

        socket.On("hi", data =>
        {
            Debug.Log(data);

            string res = data.ToString();
            res = res.Substring(1, res.Length - 2);
            Debug.Log(res);
            Test test = JsonUtility.FromJson<Test>(res);

            DebugUtils.LogColor(test.text, "magenta");
        });


        socket.Connect();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            socket.Emit("test", "Hello from Unity");
        }
    }

    private void OnApplicationQuit()
    {
        socket.Disconnect();
    }
}



[System.Serializable]
public struct Test
{
    public string text;
}

