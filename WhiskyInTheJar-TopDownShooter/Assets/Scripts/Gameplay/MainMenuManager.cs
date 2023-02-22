using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public string MatchDuration { get; set; } = "3";
    public string EnemySpawnDelay { get; set; } = "10";
    public bool UltraSecretSettings { get; set; } = false;
    public Languages interfaceLanguage = Languages.English;
    public void PlayGame(){
        PlayerPrefs.SetInt("MatchDuration", int.Parse(MatchDuration));
        PlayerPrefs.SetInt("EnemySpawnDelay", int.Parse(EnemySpawnDelay));
        PlayerPrefs.SetInt("UltraSecretSettings", (UltraSecretSettings) ? 1 : 0);
        SceneManager.LoadScene(1);
    }
    private void Update()
    {
        if (UltraSecretSettings){
            //Display description
        }
    }
}
public enum Languages { English, Português}
