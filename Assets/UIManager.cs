using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public TMP_Text gameOvere_txt;
    public TMP_Text playerWon_txt;
    public TMP_Text join_txt;
    public TMP_Text start_txt;
    public TMP_Text waiting_txt;


    public TMP_InputField nameInput;
    public GameObject startPanel;

    public TMP_Text leftPlayerName;
    public TMP_Text rightPlayerName;
    public string playerName;

    public bool nameAssigned = false;

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
        gameOvere_txt.gameObject.SetActive(false);
        playerWon_txt.gameObject.SetActive(false);
        join_txt.gameObject.SetActive(true);
        start_txt.gameObject.SetActive(false);
        waiting_txt.gameObject.SetActive(false);

        leftPlayerName.text = "Player 1";
        rightPlayerName.text = "Player 2";
    }


    public void ActivateUIelement(TMP_Text text, bool active)
    {
        text.gameObject.SetActive(active);
    }

    public void SetPlayerWonText(string player)
    {
        if(player == "left") playerWon_txt.text = $"Player {leftPlayerName.text} Won!";
    
        if (player == "right") playerWon_txt.text = $"Player {rightPlayerName.text} Won!";
    }

    public void SetPlayerName()
    {
        if (nameInput.text != null) playerName = nameInput.text;
        else playerName = "Player";
        startPanel.SetActive(false);
        nameAssigned = true;
        DebugUtils.LogColor($"[ScoreManager.cs] Player Name: {playerName}", "magenta");

    }

    public void AssignName(string playerRole)
    {
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
}
