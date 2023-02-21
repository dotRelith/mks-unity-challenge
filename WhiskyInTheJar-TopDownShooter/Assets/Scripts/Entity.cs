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
    // Enumerations
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
        Initialize();
    }
    protected virtual void Initialize()
    {
        CapsuleCollider2D tempCapsuleCollider2D = GetComponent<CapsuleCollider2D>();
        tempCapsuleCollider2D.size = new Vector2(1.75f, 0.9f);
        tempCapsuleCollider2D.direction = CapsuleDirection2D.Horizontal;
        Rigidbody2D tempRigidbody2D = GetComponent<Rigidbody2D>();
        tempRigidbody2D.gravityScale = 0f;
    }
    protected virtual void Start()
    {
        Initialize();
        entityRigidbody = GetComponent<Rigidbody2D>(); 
        entityHealth = entityMaxHealth;
        entityHealthBar = Instantiate(Resources.Load("HealthBarPivot"),this.transform) as GameObject;
        Instantiate(Resources.Load("BoatRippleParticle"), this.transform);
        onHealthChanged += ((s, e) => {
            UpdateHealthBar();
            entityBody.sprite = GetBodySpriteByHealth();
            entitySail.sprite = GetSailSpriteByHealth();
        });
        onEntityDeath += OnDeath;
        UpdateHealthBar();
        InstantiateVisuals(this is PlayerController);
    }

    private void InstantiateVisuals(bool isPlayer)
    {
        DamageableSprites sprites = MatchManager.Sprites;
        if(isPlayer){
            entitySailSprites = sprites.DamageableSails[0];
            entityBodySprites = sprites.DamageableBody[0];
        } else {
            //Sail Color
            int i = UnityEngine.Random.Range(0, sprites.DamageableSails.Length-1);
            entitySailSprites = sprites.DamageableSails[i];
            //Body
            if (UnityEngine.Random.Range(0, 1) >= .7f){//30% of spawning a thicc body ship
                entityBodySprites = sprites.DamageableBody[1];
                entityMaxHealth = 175;
                entityHealth = entityMaxHealth;
            }else
                entityBodySprites = sprites.DamageableBody[0];
        }
        //Instantiate Sprites to Entity
        GameObject aux = Instantiate(Resources.Load("boatPrefab"), this.transform) as GameObject;
        entityBody = aux.transform.Find("Body").GetComponent<SpriteRenderer>();
        entityBody.sprite = GetBodySpriteByHealth();
        entitySail = aux.transform.Find("Sail").GetComponent<SpriteRenderer>();
        entitySail.sprite = GetSailSpriteByHealth();
    }

    private Sprite GetBodySpriteByHealth()
    {
        if (entityHealth < entityMaxHealth * 0.30f)
            return entityBodySprites.badlyDamagedSprite;
        else if (entityHealth < entityMaxHealth * 0.60f)
            return entityBodySprites.damagedSprite;
        else
            return entityBodySprites.wholeSprite;
    }
    private Sprite GetSailSpriteByHealth()
    {
        if (entityHealth < entityMaxHealth * 0.30f)
            return entitySailSprites.badlyDamagedSprite;
        else if (entityHealth < entityMaxHealth * 0.60f)
            return entitySailSprites.damagedSprite;
        else
            return entitySailSprites.wholeSprite;
    }

    protected virtual void Update(){
        if(entityHealthBar!= null)
            entityHealthBar.transform.rotation = Quaternion.Euler(0, 0, 360 - transform.rotation.z);
    }

    protected virtual void OnDeath(object sender, EventArgs e){
        if(IsDead) return;
        IsDead = true;
        //GetComponent<Collider2D>().enabled = false; //if entity has velocity on death its slides on islands, etc
        entityRigidbody.velocity = Vector2.zero;
        entityRigidbody.freezeRotation = true;
        Destroy(entityHealthBar);
        Destroy(Instantiate(Resources.Load("Explosion"), this.transform.position, this.transform.rotation),2);
        GetComponentInChildren<Animator>().SetBool("isDead", true);

        if(this is Enemy){
            if (lastHitByPlayer){
                MatchManager.instance.AddPoints((int)Mathf.Floor(entityMaxHealth + ((Enemy)this).EnemyPointBonus));
            }
        }
        Destroy(this.gameObject, 12);
    }

    private void UpdateHealthBar(){
        if (entityHealthBar != null)
            entityHealthBar.GetComponentInChildren<Image>().fillAmount = (float)entityHealth / entityMaxHealth;
    }

    public void DamageEntity(int damageAmount, bool playerSource)
    {
        if (damageAmount <= 0){
            Debug.LogWarning("Recieved damage amount was negative. Please use HealEntity if the entity should be healed.");
            return;
        }

        lastHitByPlayer = playerSource;

        entityHealth -= damageAmount;
        if (entityHealth < 0)
            entityHealth = 0;
        if (entityHealth == 0)
            onEntityDeath?.Invoke(this, EventArgs.Empty);
        onHealthChanged?.Invoke(this, EventArgs.Empty);
    }
    public void HealEntity(int healAmount)
    {
        if (healAmount <= 0){
            Debug.LogWarning("Recieved heal amount was negative. Please use DamageEntity if the entity should be damaged.");
            return;
        }
        entityHealth += healAmount;
        if (entityHealth > entityMaxHealth)
            entityHealth = entityMaxHealth;
        onHealthChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void ExecuteAttack(AttackSide attackSide, float sideAttackWidth, float cannonballSpeed)
    {
        int cannonballNumber = 1;
        if (attackSide == AttackSide.Left || attackSide == AttackSide.Right)
            cannonballNumber = 3;
        float distanceIncrease = sideAttackWidth / cannonballNumber;
        float currentDistance = distanceIncrease;
        for (int i = 0; i < cannonballNumber; i++)
        {
            GameObject cannonball = Instantiate(Resources.Load("Cannonball")) as GameObject;
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
            cannonballOrigin *= 1.25f;
            Vector3 cannonballDirection = ((transform.position + cannonballOrigin) - this.transform.position).normalized;

            if (attackSide == AttackSide.Left || attackSide == AttackSide.Right){
                cannonballOrigin -= Vector3.right * currentDistance;
                currentDistance -= distanceIncrease;
            }

            cannonball.GetComponent<Cannonball>().isFromPlayer = this is PlayerController;

            cannonball.transform.position = this.transform.position + cannonballOrigin;
            Rigidbody2D cannonballRigidbody = cannonball.GetComponent<Rigidbody2D>();
            cannonballRigidbody.velocity = entityRigidbody.velocity;
            cannonballRigidbody.AddForce(cannonballDirection * cannonballSpeed, ForceMode2D.Impulse);
        }
    }
}
