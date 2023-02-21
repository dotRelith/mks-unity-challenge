using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static MatchManager instance;
    [SerializeField] private DamageableSprites sprites;
    public static DamageableSprites Sprites { get { return instance.sprites; } }
    private int playerPoints = 0;
    public static int PlayerPoints { get { return instance.playerPoints; } }
    private float secondsLeft;
    private int matchDurationInSeconds = 180;
    [SerializeField] private TextMeshProUGUI timeRemainingValueTextBox;
    [SerializeField] private TextMeshProUGUI currentPointsValueTextBox;
    [SerializeField] private GameObject matchEndedGUI;
    [SerializeField] private GameObject HUD;
    public void AddPoints(int amountToAdd)
    {
        if (amountToAdd <= 0)
            return;
        playerPoints += amountToAdd;
        currentPointsValueTextBox.SetText(string.Format("{0:0000}", playerPoints));
    }
    internal IEnumerator EndMatchCoroutine()
    {
        timeRemainingValueTextBox.transform.parent.gameObject.SetActive(false);
        currentPointsValueTextBox.transform.parent.gameObject.SetActive(false);
        if (PlayerController.instance.IsDead) { 
            //you died screen DS
            yield return new WaitForSeconds(5);
        }
        //Checks if there's a highscore, if it does it updates if necessary if not it creates one
        PlayerPrefs.SetInt("HighScore", (PlayerPrefs.HasKey("HighScore") && PlayerPrefs.GetInt("HighScore") >= playerPoints) ? PlayerPrefs.GetInt("HighScore") : playerPoints);
        //Same thing here
        PlayerPrefs.SetInt("TotalScore", (PlayerPrefs.HasKey("TotalScore")) ? PlayerPrefs.GetInt("TotalScore") + playerPoints : playerPoints);
        Time.timeScale = 0;
        matchEndedGUI.SetActive(true);
    }
    private void Awake()
    {
        instance = this;
        matchDurationInSeconds = PlayerPrefs.GetInt("MatchDuration") * 60;
        secondsLeft = matchDurationInSeconds;
        Time.timeScale = 1;
    }
    private void Update()
    {
        if (secondsLeft >= 0){
            secondsLeft -= Time.deltaTime;
            timeRemainingValueTextBox.SetText(string.Format("{0:00}:{1:00}", Mathf.FloorToInt(secondsLeft / 60), Mathf.FloorToInt(secondsLeft % 60)));
        } else
            StartCoroutine(EndMatchCoroutine());
    }
}
