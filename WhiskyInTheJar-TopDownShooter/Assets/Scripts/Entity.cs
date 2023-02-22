using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Entity : MonoBehaviour
{
    protected enum AttackSide { Left, Right, Front }

    protected Rigidbody2D entityRigidbody;

    [Header("Entity Settings")]
    [SerializeField] private int entityMaxHealth = 100;
    private int entityHealth;
    protected int EntityHealth { get { return entityHealth; } }

    public bool IsDead { get; protected set; } = false;
    
    private bool lastHitByPlayer = false;

    private GameObject entityHealthBar;
    protected EventHandler onHealthChanged;
    protected EventHandler onEntityDeath;

    [SerializeField] private test entityBodySprites;
    private SpriteRenderer entityBody;
    [SerializeField] private test entitySailSprites;
    private SpriteRenderer entitySail;

    protected virtual void Reset(){
        //Quando o componente é adicionado ao gameObject ou resetado, ele altera valores importantes
        Initialize();
    }
    protected virtual void Initialize()
    {
        //Altera a regiao de colisao do navio
        CapsuleCollider2D tempCapsuleCollider2D = GetComponent<CapsuleCollider2D>();
        tempCapsuleCollider2D.size = new Vector2(1.75f, 0.9f);
        tempCapsuleCollider2D.direction = CapsuleDirection2D.Horizontal;

        //Remove a gravidade do componente de fisica
        Rigidbody2D tempRigidbody2D = GetComponent<Rigidbody2D>();
        tempRigidbody2D.gravityScale = 0f;
    }
    protected virtual void Start()
    {
        Initialize();

        //Define o componente de fisica da entidade
        entityRigidbody = GetComponent<Rigidbody2D>(); 

        //Define a vida do player para o maximo
        entityHealth = entityMaxHealth;

        //Instancia a barra de vida da entidade
        entityHealthBar = Instantiate(Resources.Load("HealthBarPivot"),this.transform) as GameObject;
        //Instancia os efeitos de velocidade da entidade
        Instantiate(Resources.Load("BoatRippleParticle"), this.transform);

        //Inscreve no evento onHealthChanged duas coisa: a primeira é para que ele atualize o grafico da barra de vida, a segunda é que ele altere os sprites relativo a vida atual da entidade
        onHealthChanged += ((s, e) => {
            UpdateHealthBar();
            entityBody.sprite = GetBodySpriteByHealth();
            entitySail.sprite = GetSailSpriteByHealth();
        });

        //Inscreve a função OnDeath da entidade ao evento que registra se o hp da entidade chegou a 0
        onEntityDeath += OnDeath;

        //Atualiza a barra de vida no primeiro frame do jogo (caso contrario a barra pode informar informaçoes erradas ao player)
        UpdateHealthBar();
        //Instancia o barco da entidade, tambem informa se esse script é uma subclasse do Player ou não
        InstantiateVisuals(this is PlayerController);
    }

    private void InstantiateVisuals(bool isPlayer)
    {
        //Variavel auxiliar pra não ter que chamar outro script toda hora
        DamageableSprites sprites = MatchManager.Sprites;

        //Verifica se a entidade atual é um player ou não
        if(isPlayer){
            //Define os sprites do player como os primeiros da lista de Sprites
            //Gostaria de ter adicionado customização de Sprite. Caso o player tivesse pontos o suficiente, ele poderia comprar novas skins para seu barco.(Se ele comprasse um casco novo ele poderia ter mais HP)
            entitySailSprites = sprites.DamageableSails[0];
            entityBodySprites = sprites.DamageableBody[0];
        } else {
            //Define a cor da vela do Inimigo
            int i = UnityEngine.Random.Range(0, sprites.DamageableSails.Length-1);
            entitySailSprites = sprites.DamageableSails[i];
            //Verifica se uma unidade aleatoria é maior que uma porcentagem, caso for o inimigo spawna com mais vida, caso contrario ele spawna sem nenhuma alteração
            if (UnityEngine.Random.Range(0f, 1f) >= .7f){//30% of spawning a thicc body ship
                entityBodySprites = sprites.DamageableBody[1];
                //Aumenta a vida maxima do inimigo caso o corpo do barco seja mais grosso
                entityMaxHealth = 175;
                entityHealth = entityMaxHealth;
            }else
                entityBodySprites = sprites.DamageableBody[0];
        }

        //Instancia o barco da entidade com sua respectiva vela e corpo
        GameObject aux = Instantiate(Resources.Load("boatPrefab"), this.transform) as GameObject;
        entityBody = aux.transform.Find("Body").GetComponent<SpriteRenderer>();
        entityBody.sprite = GetBodySpriteByHealth();
        entitySail = aux.transform.Find("Sail").GetComponent<SpriteRenderer>();
        entitySail.sprite = GetSailSpriteByHealth();
    }

    private Sprite GetBodySpriteByHealth()
    {
        //Retorna o Sprite desejado para o nivel de vida atual da entidade
        if (entityHealth < entityMaxHealth * 0.30f)//Se vida for menor que 30%
            return entityBodySprites.badlyDamagedSprite;
        else if (entityHealth < entityMaxHealth * 0.60f)//Se vida for menor que 60%
            return entityBodySprites.damagedSprite;
        else//Se vida for maior ou igual que 60%
            return entityBodySprites.wholeSprite;
    }
    private Sprite GetSailSpriteByHealth()
    {
        //Retorna o Sprite desejado para o nivel de vida atual da entidade
        if (entityHealth < entityMaxHealth * 0.30f)//Se vida for menor que 30%
            return entitySailSprites.badlyDamagedSprite;
        else if (entityHealth < entityMaxHealth * 0.60f)//Se vida for menor que 60%
            return entitySailSprites.damagedSprite;
        else//Se vida for maior ou igual que 60%
            return entitySailSprites.wholeSprite;
    }

    protected virtual void Update(){
        //Verifica se a entidade está alem do limite do mapa
        if(Vector2.Distance(this.transform.position,Vector2.zero) >= MatchManager.instance.bermudaTriangleRange){
            //Se a entidade for um player ele exibe uma HUD que informa para que o player volte
            if (this is PlayerController)
                MatchManager.instance.SetBermudaWarning(true);
            //Danifica a entidade
            DamageEntity(5, false);
        }else if (this is PlayerController)//Caso a entidade esteja dentro do limite do mapa e seja um player esconda o aviso do player
            MatchManager.instance.SetBermudaWarning(false);

        //Previne que a barra de vida do player fique girando juntamente do player (Há outras maneiras de fazer isso, como tirando o parentesco da barra de vida com o player, porem, eu acho que a Scene View fica muito bagunçada desse jeito)
        if (entityHealthBar!= null)
            entityHealthBar.transform.rotation = Quaternion.Euler(0, 0, 360 - transform.rotation.z);
    }

    protected virtual void OnDeath(object sender, EventArgs e){
        //Caso a entidade já esteja morta volte
        if(IsDead) return;

        //Defina a entidade como morta
        IsDead = true;

        //Remova a velocidade e a rotação da entidade
        entityRigidbody.velocity = Vector2.zero;
        entityRigidbody.freezeRotation = true;

        //Remova a barra de vida da entidade
        Destroy(entityHealthBar);

        //Instancie uma explosão no local de morte da entidade e destrua ela depois de 7 segundos (7 segundos é o tempo do audio da explosão)
        Destroy(Instantiate(Resources.Load("Explosion"), this.transform.position, this.transform.rotation),7);
        //Executa a animação de morte da entidade
        GetComponentInChildren<Animator>().SetBool("isDead", true);

        //Verifica se esse entidade é um inimigo e verifica tambem se ela morreu devido a um ataque do player
        if(this is Enemy){
            if (lastHitByPlayer){
                //Adiciona ponto para o player baseado no HP da entidade + um bonus determinado pela classe do inimigo
                MatchManager.instance.AddPoints((int)Mathf.Floor(entityMaxHealth + ((Enemy)this).EnemyPointBonus));
            }
        }

        //Destroi a entidade depois de 12 segundos (tempo em que a animação de morte termina)
        Destroy(this.gameObject, 12);
    }

    private void UpdateHealthBar(){
        //Verifica se a barra de vida ainda é esta na cena e caso sim, altera o seu valor de acordo
        if (entityHealthBar != null)
            entityHealthBar.GetComponentInChildren<Image>().fillAmount = (float)entityHealth / entityMaxHealth;
    }

    public void DamageEntity(int damageAmount, bool playerSource)
    {
        //Verifica se quantidade de dano é positiva, caso contrario informa ao desenvolvedor
        if (damageAmount <= 0){
            Debug.LogWarning("Recieved damage amount was negative. Please use HealEntity if the entity should be healed.");
            return;
        }

        //informa a entidade se esse dano foi causado pelo player
        lastHitByPlayer = playerSource;

        //diminui a vida da entidade e limita ela caso o dano mate a entidade
        entityHealth -= damageAmount;
        if (entityHealth < 0)
            entityHealth = 0;

        //Caso o HP da entidade chegue a 0 ele chama o evento onEntityDeath
        if (entityHealth == 0)
            onEntityDeath?.Invoke(this, EventArgs.Empty);
        //Chama o evento onHealthChanged
        onHealthChanged?.Invoke(this, EventArgs.Empty);
    }
    public void HealEntity(int healAmount)
    {
        //Verifica se quantidade de heal é positiva, caso contrario informa ao desenvolvedor
        if (healAmount <= 0){
            Debug.LogWarning("Recieved heal amount was negative. Please use DamageEntity if the entity should be damaged.");
            return;
        }

        //Aumenta o HP da entidade e limita ele
        entityHealth += healAmount;
        if (entityHealth > entityMaxHealth)
            entityHealth = entityMaxHealth;

        //Chama o evento onHealthChanged
        onHealthChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void ExecuteAttack(AttackSide attackSide, float sideAttackWidth, float cannonballSpeed)
    {
        //Define a quantidade de tiros que serão feitos, de acordo com o lado escolhido para o ataque
        int cannonballNumber = 1;
        if (attackSide == AttackSide.Left || attackSide == AttackSide.Right)
            cannonballNumber = 3;

        //Define o incremento de distancia de um tiro pra outro
        float distanceIncrease = sideAttackWidth / cannonballNumber;
        float currentDistance = distanceIncrease;

        //Itera pelo numero de tiros
        for (int i = 0; i < cannonballNumber; i++)
        {
            //Instancia uma bala de canhao
            GameObject cannonball = Instantiate(Resources.Load("Cannonball")) as GameObject;

            //Define a posição da bala de canhão baseado no lado escolhido para o ataque
            Vector3 cannonballOrigin = Vector2.zero;
            switch (attackSide)
            {
                case AttackSide.Left:
                    cannonballOrigin = -1 * this.transform.up;
                    break;
                case AttackSide.Right:
                    cannonballOrigin = this.transform.up;
                    break;
                case AttackSide.Front:
                    cannonballOrigin = this.transform.right * 1.5f;
                    break;
            }
            //Dá uma distancia da entidade para que o tiro não acerte a entidade no começo do tiro
            cannonballOrigin *= 1.25f;
            //Calcula a direção do tiro
            Vector3 cannonballDirection = ((transform.position + cannonballOrigin) - this.transform.position).normalized;

            //Caso o tiro da pra algum do lados ele diminui a distancia entre os tiros
            if (attackSide == AttackSide.Left || attackSide == AttackSide.Right){
                cannonballOrigin -= Vector3.right * currentDistance;
                currentDistance -= distanceIncrease;
            }

            //Altera a variavel que informa se a bala de canhao é do player
            cannonball.GetComponent<Cannonball>().isFromPlayer = this is PlayerController;

            //Informa a posição de origem da bala de canhao
            cannonball.transform.position = this.transform.position + cannonballOrigin;

            //Define a velocidade da bala de canhao para a mesma da entidade e adiciona velocidade para a direção do ataque
            Rigidbody2D cannonballRigidbody = cannonball.GetComponent<Rigidbody2D>();
            cannonballRigidbody.velocity = entityRigidbody.velocity;
            cannonballRigidbody.AddForce(cannonballDirection * cannonballSpeed, ForceMode2D.Impulse);
        }
    }
}
