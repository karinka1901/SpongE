using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Game Over Text Boxes")]
    public TMP_Text gameOver_txt;
    public TMP_Text playerWon_txt;

    [Header("Game Start Text Boxes")]
    public TMP_Text join_txt;
    public TMP_Text start_txt;
    public TMP_Text waiting_txt;

    [Header("Player Name")]
    public TMP_Text leftPlayerName;
    public TMP_Text rightPlayerName;
    public TMP_InputField nameInput;
    public string playerName;
    public bool nameAssigned = false;

    [Header("Panels")]
    public GameObject startPanel;
    public GameObject leaderboardPanel;

    [Header("Leaderboard stuff")]
    public TMP_Text leaderboardResult;

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
        gameOver_txt.gameObject.SetActive(false);
        playerWon_txt.gameObject.SetActive(false);

        join_txt.gameObject.SetActive(true);
        start_txt.gameObject.SetActive(false);
        waiting_txt.gameObject.SetActive(false);

        leaderboardPanel.SetActive(false);
        startPanel.SetActive(true);

        leftPlayerName.text = "Player 1";
        rightPlayerName.text = "Player 2";
    }


    public void ActivateUIelement(TMP_Text text, bool active)
    {
        text.gameObject.SetActive(active);
    }

    public void DisplayVictoryText(string player)
    {
        if(player == "left") playerWon_txt.text = $"Player {leftPlayerName.text} Won! \n Press SPACE to continue.";
    
        if (player == "right") playerWon_txt.text = $"Player {rightPlayerName.text} Won! \n Press SPACE to continue.";
    }

    public void SetPlayerName()
    {
        if (nameInput.text != null) playerName = nameInput.text;
        else playerName = "Noname";
        startPanel.SetActive(false);
        nameAssigned = true;
        DebugUtils.LogColor($"[ScoreManager.cs] Player Name: {playerName}", "magenta");

    }

    public void AssignName(string playerRole)
    {
        if (string.IsNullOrEmpty(playerName)) 
        {
            playerName = "Player_" + Random.Range(1, 9); // Random 4-digit number
        }


        if (playerRole == "left")
        {
            leftPlayerName.text = playerName;
            DebugUtils.LogColor($"[ScoreManager.cs] Left Player Name: {playerName}", "green");
        }
        else if (playerRole == "right")
        {
            rightPlayerName.text = playerName;
            DebugUtils.LogColor($"[ScoreManager.cs] Right Player Name: {playerName}", "green");
        }

    }

    public void OpenLeaderboard()
    {
        leaderboardPanel.SetActive(true);
        startPanel.SetActive(false);
        PongDatabase.Instance.FetchTopScores();
        DebugUtils.LogColor($"[ScoreManager.cs] fetching OpenLeaderboard", "yellow");
    }

    public void CloseLeaderboard()
    {
        leaderboardPanel.SetActive(false);
        startPanel.SetActive(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void UpdateLeaderboard(string data)
    {
        
        leaderboardResult.text = data;
    }
}
