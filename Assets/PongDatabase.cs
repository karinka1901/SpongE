using SimpleJSON;
using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

public class PongDatabase : MonoBehaviour
{
    public static PongDatabase Instance { get; private set; }

    private string playerAdress = "http://localhost:5000/api/add-player";

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

    public void SendPlayerNameToDatabase(string playerName)
    {
        StartCoroutine(SendNameCoroutine(playerName));
    }

    private IEnumerator SendNameCoroutine(string playerName)
    {
        UnityWebRequest www = UnityWebRequest.Get(playerAdress);

        JSONNode node = JSON.Parse(playerName);
        string name = node["playerName"];


        using (UnityWebRequest request = UnityWebRequest.Post(playerAdress, node))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                DebugUtils.LogColor("Error sending player name: " + request.error, "red");
            }
            else
            {
                DebugUtils.LogColor("Player name sent to database: " + request.downloadHandler.text, "yellow");
            }
        }
    }
}

