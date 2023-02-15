using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    private int EnemyHealth = 100;
    private float EnemyMovementSpeed = 16f;
    private float EnemyRotationSpeed = 64f;

    private Rigidbody2D enemyRigidbody;
    void Start()
    {
        enemyRigidbody = GetComponent<Rigidbody2D>();
    }
    void FixedUpdate()
    {
        enemyRigidbody.AddForce(this.transform.up);
    }
}