using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaserEnemy : Enemy
{
    [Header("Attack Settings")]
    [SerializeField] private int damage = 80;
    protected override void Initialize(){
        base.Initialize();
        enemyPointBonus = 134;
    }
    protected override void TargetDetected(Transform targetTransform){
        MoveTo(targetTransform.position);
    }
    private void OnCollisionEnter2D(Collision2D collision){
        if(collision.transform.CompareTag("Player") || collision.transform.CompareTag("Enemy")) {
            collision.transform.GetComponent<Entity>().DamageEntity(damage, false);
            DamageEntity(EntityHealth, false);
        }
    }
}
    