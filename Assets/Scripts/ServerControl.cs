using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

public class ServerControlt : MonoBehaviour
{
    public string address;
    public GameObject player;
    public string getPlayerAddress;
    void Start()
    {
        address = "http://localhost:3000/unitytest";
        getPlayerAddress = "http://localhost:3000/api/player/2";

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
           Debug.Log("Space key was pressed.");

            StartCoroutine(GetWebData(address));
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("P key was pressed.");
            StartCoroutine(GetPlayerData(getPlayerAddress));
        }
    }

    IEnumerator GetWebData(string address)
    {
        //web request. set the method adn endpoint
        UnityWebRequest www = UnityWebRequest.Get(address);
       
        yield return www.SendWebRequest();

        if(www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
        }
        else
        {
            // response from the server
            Debug.Log(www.downloadHandler.text);
            ProcessServerResponse(www.downloadHandler.text);
        }

    }
    IEnumerator GetPlayerData(string address)
    {
        //web request. set the method adn endpoint
        UnityWebRequest www = UnityWebRequest.Get(address);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
        }
        else
        {
            // response from the server
            Debug.Log(www.downloadHandler.text);
            HandlePlayerData(www.downloadHandler.text);
        }

    }

    public void HandlePlayerData(string playerData)
    {
        Debug.Log(playerData);
        JSONNode node = JSON.Parse(playerData);

        string firstName = node["firstname"];
        string lastName = node["lastname"];
        string health = node["health"];

        Debug.Log("Player info: " + firstName + " " + lastName + " " + health);
    

    }

    void ProcessServerResponse(string rawResponse)
    {
        //parse json response
        JSONNode node = JSON.Parse(rawResponse);
        if (node["action"] == "instantiatePlayer") {
            Debug.Log("Instantiating the player");
            InstantiatePlayer(node);
        }

    }

    public void InstantiatePlayer(JSONNode nodeInfo)
    {
        Vector3 playerPosition = new Vector3(nodeInfo["position"][0]["value"], nodeInfo["position"][1]["value"], nodeInfo["position"][2]["value"]);
        Instantiate(player, playerPosition, Quaternion.identity);
    }
}
