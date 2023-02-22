using System.Collections;
using TMPro;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static MatchManager instance;
    [Header("Damageable Sprites Scriptable Object")]
    [SerializeField] private DamageableSprites sprites;
    public static DamageableSprites Sprites { get { return instance.sprites; } }

    private int playerPoints = 0;
    public static int PlayerPoints { get { return instance.playerPoints; } }

    private float secondsLeft;
    private int matchDurationInSeconds = 180;

    [Header("Ultra Secret Stuff")]
    private bool ultraSecretSettings = false;
    public float bermudaTriangleRange = 256f;

    [Header("UI objects")]
    [SerializeField] private TextMeshProUGUI timeRemainingValueTextBox;
    [SerializeField] private TextMeshProUGUI currentPointsValueTextBox;
    [SerializeField] private GameObject matchEndedGUI;
    [SerializeField] private GameObject HUD;
    [SerializeField] private GameObject radarHUD;
    [SerializeField] private GameObject bermudaTriangleWarning;

    [Header("Non UI objects")]
    [SerializeField] private GameObject radar;

    private void Awake()
    {
        //Configura o singleton
        instance = this;
        
        //Ajusta o tempo da partida para a configuração do jogador
        matchDurationInSeconds = PlayerPrefs.GetInt("MatchDuration") * 60;

        //Verifica se a opção UltraSecretSettings foi habilitada e configura seus elementos adequadamente
        ultraSecretSettings = bool.Parse(PlayerPrefs.GetString("UltraSecretSettings"));
        radarHUD.SetActive(ultraSecretSettings);
        radar.SetActive(ultraSecretSettings);

        //Ajusta o tempo da partida 2
        secondsLeft = matchDurationInSeconds;
        //Despause a fisica do jogo, importante caso o jogador tenha reiniciado o level
        Time.timeScale = 1;
    }
    
    private void Update()
    {
        //Atualiza o tempo passado a cada frame. Caso o tempo restante esteja esgotado, ele inicia o fim da partida.
        if (secondsLeft >= 0){
            secondsLeft -= Time.deltaTime;
            timeRemainingValueTextBox.SetText(string.Format("{0:00}:{1:00}", Mathf.FloorToInt(secondsLeft / 60), Mathf.FloorToInt(secondsLeft % 60)));
        } else
            StartCoroutine(EndMatchCoroutine());
    }

    internal IEnumerator EndMatchCoroutine()
    {
        //Esconde a HUD do player
        timeRemainingValueTextBox.transform.parent.gameObject.SetActive(false);
        currentPointsValueTextBox.transform.parent.gameObject.SetActive(false);

        //Caso o player tenha morrido espera alguns segundos para o player ver que morreu
        if (PlayerController.instance.IsDead) { 
            yield return new WaitForSeconds(5);
        }
        //Verifica se já existe um HighScore, se sim ele atualiza, se não ele cria um.
        PlayerPrefs.SetInt("HighScore", 
            (PlayerPrefs.HasKey("HighScore") && PlayerPrefs.GetInt("HighScore") >= playerPoints)
                ? PlayerPrefs.GetInt("HighScore") 
                : playerPoints);
        //Mesma coisa aqui
        PlayerPrefs.SetInt("TotalScore", 
            (PlayerPrefs.HasKey("TotalScore"))
                ? PlayerPrefs.GetInt("TotalScore") + playerPoints
                : playerPoints);
        //Pausa a fisica do jogo
        Time.timeScale = 0;
        //Mostra a UI de fim de jogo ao player
        matchEndedGUI.SetActive(true);
    }
    
    public void AddPoints(int amountToAdd)
    {
        //Verifica se quantidade de pontos é positiva, caso contrario informa ao desenvolvedor
        if (amountToAdd <= 0) { 
            Debug.LogWarning($"Asked to add {amountToAdd} points, which is not handled by this function.");
            return; 
        }
        //Caso a quantidade de pontos seja positiva, ela é adicionada aos pontos atuais do player
        playerPoints += amountToAdd;
        //E então o elemento da HUD é atualizado no devido formato
        currentPointsValueTextBox.SetText(string.Format("{0:0000}", playerPoints));
    }

    public void SetBermudaWarning(bool value)
    {
        //Altera o estado da HUD caso o player esteja fora da area segura (Logica dentro do Entity.cs)
        bermudaTriangleWarning.SetActive(value);
    }
    //Em tese deveria exibir um circulo mostrando a area segura para o desenvolvedor, porem, por algum motivo não está exibindo (estou utilizando EnemySpawner.cs por hora pra remediar)
    
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(this.transform.position, bermudaTriangleRange);
    }
}
