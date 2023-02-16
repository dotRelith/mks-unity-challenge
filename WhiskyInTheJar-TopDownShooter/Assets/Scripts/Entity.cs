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

    private GameObject entityHealthBar;
    private EventHandler onHealthChanged;
    protected virtual void Start()
    {
        entityHealth = entityMaxHealth;
        entityHealthBar = Instantiate(Resources.Load("HealthBarPivot"),this.transform) as GameObject;
        onHealthChanged += ((s, e) => { UpdateHealthBar(); });
        UpdateHealthBar();
    }

    protected virtual void Update()
    {
        entityHealthBar.transform.rotation = Quaternion.Euler(0, 0, 360 - transform.rotation.z);
    }

    private void UpdateHealthBar(){
        entityHealthBar.GetComponentInChildren<Image>().fillAmount = entityMaxHealth / entityHealth;
    }
}
