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
    public void AddPoints(int amountToAdd)
    {
        if (amountToAdd <= 0)
            return;
        playerPoints += amountToAdd;
        currentPointsValueTextBox.SetText(string.Format("{0:0000}", playerPoints));
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
    }
}
