using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Entity : MonoBehaviour
{
    [Header("Entity Settings")]
    [SerializeField] private int entityMaxHealth = 100;
    private int entityHealth;

    protected bool IsDead { get; private set; } = false;
    
    private bool lastHitByPlayer = false;

    private GameObject entityHealthBar;
    private EventHandler onHealthChanged;
    private EventHandler onEntityDeath;
    protected virtual void Start()
    {
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
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        GameObject boatDeathObject = Instantiate(Resources.Load("BoatDeath")) as GameObject;
        boatDeathObject.transform.position = this.transform.position;

        if(this is Enemy){
            if (lastHitByPlayer){
                MatchManager.instance.AddPoints((int)Mathf.Floor(entityMaxHealth * 1.23f));
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
}
