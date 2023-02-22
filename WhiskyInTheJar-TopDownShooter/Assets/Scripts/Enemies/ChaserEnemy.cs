using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaserEnemy : Enemy
{
    [Header("Attack Settings")]
    [SerializeField] private int damage = 80;
    protected override void Initialize(){
        base.Initialize();
        enemyPointBonus = Random.Range(75f, 125f);
    }
    protected override void TargetDetected(Transform targetTransform){
        MoveTo(targetTransform.position);
    }
    void OnCollisionEnter2D(Collision2D collision){
        if (this.IsDead)
            return;
        if(collision.transform.CompareTag("Player") || collision.transform.CompareTag("Enemy")) {
            collision.transform.GetComponent<Entity>().DamageEntity(damage, false);
            DamageEntity(EntityHealth, false);
        }
    }
}
    