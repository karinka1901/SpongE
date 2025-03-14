using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public TMP_Text leftScore_txt;
    public TMP_Text rightScore_text;

    public int leftScore = 0;
    public int rightScore = 0;
 

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


    public void UpdateScore(string player)
    {
        if (player == "left")
        {
            leftScore++;
            leftScore_txt.text = $"Score: {leftScore}";
        }
        else if (player == "right")
        {
            rightScore++;
            rightScore_text.text = $"Score: {rightScore}";
        }

        DebugUtils.LogColor($"[ScoreManager.cs] Left Score: {leftScore} | Right Score: {rightScore}", "purple");
    }

    public void SyncScore(int left, int right)
    {
        leftScore = left;
        rightScore = right;

        leftScore_txt.text = $"Score: {leftScore}";
        rightScore_text.text = $"Score: {rightScore}";

        DebugUtils.LogColor($"[ScoreManager.cs] Scores synced - Left: {leftScore} | Right: {rightScore}", "cyan");
    }

    
    }
