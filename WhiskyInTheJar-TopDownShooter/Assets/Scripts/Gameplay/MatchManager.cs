using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static MatchManager instance;
    private int playerPoints = 0;
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
        matchEndedGUI.SetActive(true);
        Time.timeScale = 0;
    }
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        secondsLeft = matchDurationInSeconds;
        //load match duration in config
    }
    private void Update()
    {
        if (timeRemainingValueTextBox == null)
            Debug.LogError("timeRemainingValueTextBox not assigned");
        if (secondsLeft >= 0){
            secondsLeft -= Time.deltaTime;
            timeRemainingValueTextBox.SetText(string.Format("{0:00}:{1:00}", Mathf.FloorToInt(secondsLeft / 60), Mathf.FloorToInt(secondsLeft % 60)));
        }
        else
        {
            StartCoroutine(EndMatchCoroutine());
        }
    }
}
