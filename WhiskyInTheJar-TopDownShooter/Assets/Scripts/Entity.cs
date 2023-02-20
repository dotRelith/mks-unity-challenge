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

    protected virtual void Reset(){
        CapsuleCollider2D tempCapsuleCollider2D = GetComponent<CapsuleCollider2D>();
        tempCapsuleCollider2D.size = new Vector2(1.75f, 0.9f);
        tempCapsuleCollider2D.direction = CapsuleDirection2D.Horizontal;
        Rigidbody2D tempRigidbody2D = GetComponent<Rigidbody2D>();
        tempRigidbody2D.gravityScale = 0f;
    }
    protected virtual void Start()
    {
        entityRigidbody = GetComponent<Rigidbody2D>(); 
        entityHealth = entityMaxHealth;
        entityHealthBar = Instantiate(Resources.Load("HealthBarPivot"),this.transform) as GameObject;
        Instantiate(Resources.Load("BoatRippleParticle"), this.transform);
        onHealthChanged += ((s, e) => { UpdateHealthBar(); });
        onEntityDeath += OnDeath;
        UpdateHealthBar();
    }

    protected virtual void Update(){
        if(entityHealthBar!= null)
            entityHealthBar.transform.rotation = Quaternion.Euler(0, 0, 360 - transform.rotation.z);
    }

    protected virtual void OnDeath(object sender, EventArgs e){
        if(IsDead) return;
        IsDead = true;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        Destroy(Instantiate(Resources.Load("Explosion"), this.transform.position, this.transform.rotation),2);
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        GameObject boatDeathObject = Instantiate(Resources.Load("BoatDeath")) as GameObject;
        boatDeathObject.transform.position = this.transform.position;

        if(this is Enemy){
            if (lastHitByPlayer){
                MatchManager.instance.AddPoints((int)Mathf.Floor(entityMaxHealth + ((Enemy)this).EnemyPointBonus));
            }
        }
        Destroy(this.gameObject, 12);
        Destroy(boatDeathObject, 12);
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
                    cannonballOrigin = -1 * this.transform.right;
                    break;
                case AttackSide.Right:
                    cannonballOrigin = this.transform.right;
                    break;
                case AttackSide.Front:
                    cannonballOrigin = this.transform.up * 1.5f;
                    break;
            }
            cannonballOrigin *= 1.25f;
            Vector3 cannonballDirection = ((transform.position + cannonballOrigin) - this.transform.position).normalized;

            if (attackSide == AttackSide.Left || attackSide == AttackSide.Right){
                cannonballOrigin -= Vector3.up * currentDistance;
                currentDistance -= distanceIncrease;
            }

            cannonball.GetComponent<Cannonball>().isFromPlayer = true;

            cannonball.transform.position = this.transform.position + cannonballOrigin;
            Rigidbody2D cannonballRigidbody = cannonball.GetComponent<Rigidbody2D>();
            cannonballRigidbody.velocity = entityRigidbody.velocity;
            cannonballRigidbody.AddForce(cannonballDirection * cannonballSpeed, ForceMode2D.Impulse);
        }
    }
}
