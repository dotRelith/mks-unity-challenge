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
        PlayerPrefs.SetString("UltraSecretSettings", UltraSecretSettings.ToString());
        SceneManager.LoadScene(1);
    }
}
public enum Languages { English, Português}
