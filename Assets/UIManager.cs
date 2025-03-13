using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TMP_Text gameOvere_txt;
    public TMP_Text playerWon_txt;
    public TMP_Text join_txt;
    public TMP_Text start_txt;
    public TMP_Text waiting_txt;
    void Start()
    {
        gameOvere_txt.gameObject.SetActive(false);
        playerWon_txt.gameObject.SetActive(false);
        join_txt.gameObject.SetActive(true);
        start_txt.gameObject.SetActive(false);
        waiting_txt.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateUIelement(TMP_Text text, bool active)
    {
        text.gameObject.SetActive(active);
    }

    public void SetPlayerWonText(string player)
    {
        if(player == "left") playerWon_txt.text = $"Player 1 Won!";
    
        if (player == "right") playerWon_txt.text = $"Player 2 Won!";



    }
}
