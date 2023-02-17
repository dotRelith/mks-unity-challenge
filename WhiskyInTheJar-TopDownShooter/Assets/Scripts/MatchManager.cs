using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    private int playerPoints = 0;
    private float secondsLeft;
    private int matchDurationInSeconds = 180;
    [SerializeField] private TextMeshProUGUI timeRemainingValueTextBox;
    public void AddPoints()
    {

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
        if (secondsLeft > 0){
            secondsLeft -= Time.deltaTime;
            timeRemainingValueTextBox.SetText(string.Format("{0:00}:{1:00}", Mathf.FloorToInt(secondsLeft / 60), Mathf.FloorToInt(secondsLeft % 60)));
        }
    }
}
