using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterEnemy : Enemy
{
    protected override void TargetDetected(Transform targetTransform)
    {
        if (targetTransform != null)
        {
            // Calculate the direction to the player from the enemy
            Vector2 directionToPlayer = targetTransform.position - transform.position;
            directionToPlayer.Normalize();

            // Calculate the direction parallel to the direction to the player
            Vector2 directionParallel = new Vector2(directionToPlayer.y, -directionToPlayer.x);

            // Apply a force to the enemy in the parallel direction with the desired speed
            //enemyRigidbody.AddForce(directionParallel * EnemyMovementSpeed);
        }
    }
}
