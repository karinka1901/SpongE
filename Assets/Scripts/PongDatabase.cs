using SimpleJSON;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class PongDatabase : MonoBehaviour
{
    public static PongDatabase Instance { get; private set; }

    private string playerAddress = "http://127.0.0.1:5000/api/update-score";
    private string leaderboardAddress = "http://127.0.0.1:5000/api/top-scores";

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

   ///NAME AND SCORE
    public void UpdatePlayerScore(string playerName, int score)
    {
        StartCoroutine(SendScoreData(playerName, score));
    }
    private IEnumerator SendScoreData(string playerName, int score)
    {
        WWWForm form = new WWWForm();
        form.AddField("playerName", playerName);
        form.AddField("score", score);

        using (UnityWebRequest request = UnityWebRequest.Post(playerAddress, form))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                DebugUtils.LogColor($"Score updated for {playerName}: {score}", "green");
            }
            else
            {
                DebugUtils.LogColor($"Error updating score: {request.error}", "red");
                DebugUtils.LogColor($"Server Response: {request.downloadHandler.text}", "orange");
            }
        }
    }


    ///GET LEADERBOARD
    public void FetchTopScores()
    {
        StartCoroutine(GetTopScores());
    }

    private IEnumerator GetTopScores()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(leaderboardAddress))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                DebugUtils.LogColor("Received top scores: " + request.downloadHandler.text, "cyan");

                JSONNode data = JSON.Parse(request.downloadHandler.text);
                string leaderboardText = "Top 5 Players:\n";

                for (int i = 0; i < data.Count; i++)
                {
                    string playerName = data[i]["username"];
                    int playerScore = data[i]["score"];
                    leaderboardText += $"{i + 1}. {playerName} - {playerScore}\n";
                }

                // Display leaderboard in UI
                UIManager.Instance.UpdateLeaderboard(leaderboardText);
            }
            else
            {
                DebugUtils.LogColor("Error fetching top scores: " + request.error, "red");
            }
        }
    }
}
