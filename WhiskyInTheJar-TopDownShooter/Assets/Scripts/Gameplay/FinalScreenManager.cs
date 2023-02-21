using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalScreenManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI matchScoreText, totalScoreText, highScoreText;
    private int matchScore;
    public void PlayAgain()
    {
        SceneManager.LoadScene(1);
    }
    public void ReturnToTitleScreen()
    {
        SceneManager.LoadScene(0);
    }
    private void OnEnable(){
        matchScore = MatchManager.PlayerPoints;
        matchScoreText.SetText(matchScore.ToString());
        totalScoreText.SetText(PlayerPrefs.GetInt("TotalScore").ToString());
        highScoreText.SetText(PlayerPrefs.GetInt("HighScore").ToString());
    }
}
