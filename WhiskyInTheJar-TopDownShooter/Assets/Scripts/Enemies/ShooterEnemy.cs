using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterEnemy : Enemy
{
    [Header("Shooter Enemy Settings")]
    [SerializeField] private float smoothTime = 0.5f;
    [SerializeField] private float distanceFromTarget = 3f;
    [SerializeField] private float attackRayRange = 12f;
    [SerializeField] private float raySpacing = 0.8f;

    protected override void Reset()
    {
        base.Reset();
        enemyPointBonus = 67;
    }
    protected override void TargetDetected(Transform targetTransform)
    {
        if (targetTransform != null)
        {
            Vector2 rightParallelPosition = targetTransform.position + targetTransform.right * distanceFromTarget;
            Vector2 leftParallelPosition = targetTransform.position + (-targetTransform.right) * distanceFromTarget;

            float distanceToRight = Vector2.Distance(this.transform.position, rightParallelPosition);
            float distanceToLeft = Vector2.Distance(this.transform.position, leftParallelPosition);

            MoveTo(distanceToRight < distanceToLeft ? rightParallelPosition : leftParallelPosition);

            (AttackSide, int, Color)[] possibleAttackRegions = {
                (AttackSide.Left,3, Color.green),
                (AttackSide.Right,3, Color.blue),
                (AttackSide.Front,2, Color.cyan)
            };
            foreach (var possibleAttackRegion in possibleAttackRegions){
                Vector2 attackDirection = default;
                Vector2 rayOffsetDirection = default;
                switch(possibleAttackRegion.Item1){
                    case AttackSide.Left:
                        attackDirection = -this.transform.up;
                        rayOffsetDirection = -this.transform.right;
                        break;
                    case AttackSide.Right:
                        attackDirection = this.transform.up;
                        rayOffsetDirection = this.transform.right;
                        break;
                    case AttackSide.Front:
                        attackDirection = this.transform.right;
                        rayOffsetDirection = this.transform.up;
                        break;
                }
                int raysHit = 0;
                for (int i = -1; i < (possibleAttackRegion.Item2 - 1); i++){
                    Vector2 rayCurrentPosition;
                    if (possibleAttackRegion.Item1 == AttackSide.Front)
                        rayCurrentPosition = (Vector2)this.transform.position - rayOffsetDirection * raySpacing * i - rayOffsetDirection / 2;
                    else
                        rayCurrentPosition = (Vector2)this.transform.position - rayOffsetDirection * raySpacing * i;
                    Ray2D ray = new Ray2D(rayCurrentPosition, attackDirection);
                    RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, attackRayRange);
                    Debug.DrawRay(ray.origin, ray.direction * attackRayRange, (hit.collider != null) ? Color.red : possibleAttackRegion.Item3);
                    if (hit.collider != null && hit.transform != this.transform){
                        if(hit.transform.CompareTag("Player") || hit.transform.CompareTag("Enemy")){
                            raysHit++;
                        }
                    }   
                }
                if(raysHit >= (possibleAttackRegion.Item2 - 1)){
                    //Start Coroutine to shoot
                    print("shoot!!!!");
                }
            }
        }
    }
}
