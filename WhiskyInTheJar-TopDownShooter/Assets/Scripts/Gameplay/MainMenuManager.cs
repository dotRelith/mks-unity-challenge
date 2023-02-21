using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public string MatchDuration { get; set; } = "3";
    public string EnemySpawnDelay { get; set; } = "10";
    public void PlayGame(){
        PlayerPrefs.SetInt("MatchDuration", int.Parse(MatchDuration));
        PlayerPrefs.SetInt("EnemySpawnDelay", int.Parse(EnemySpawnDelay));
        SceneManager.LoadScene(1);
    }
}
