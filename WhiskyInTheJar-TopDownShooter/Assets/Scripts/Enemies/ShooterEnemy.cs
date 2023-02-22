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

    private float secondsLockedToTarget = 0f;
    [Header("Attack")]
    [SerializeField] private float sideAttackRange = 12f;
    [SerializeField] private float sideAttackWidth = 1.5f;
    [SerializeField] private float frontalAttackRange = 10f;
    [SerializeField] private float frontalAttackWidth = 1.5f;
    [SerializeField] private float cannonballVelocity = 16f;
    [SerializeField] private float secondsToDeployAttack = 3.5f;

    protected override void Initialize()
    {
        base.Initialize();
        enemyPointBonus = Random.Range(25f, 75f);
        movementSpeed = 5.5f;
    }

    protected override void HandleRotation()
    {
        /*
        if (targetTransform != null){
            Vector2 toTarget = targetTransform.position - transform.position;
            float angle = Vector2.SignedAngle(transform.up, toTarget);
            if (angle > 0)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, 90), smoothTime);
            else
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, -90), smoothTime);
        }
        else
        */
            base.HandleRotation();
    }

    protected override void TargetDetected(Transform targetTransform)
    {
        if (targetTransform != null)
        {
            Vector2 rightParallelPosition = targetTransform.position + targetTransform.up * distanceFromTarget;
            Vector2 leftParallelPosition = targetTransform.position + (-targetTransform.up) * distanceFromTarget;

            float distanceToRight = Vector2.Distance(this.transform.position, rightParallelPosition);
            float distanceToLeft = Vector2.Distance(this.transform.position, leftParallelPosition);

            MoveTo(distanceToRight < distanceToLeft ? rightParallelPosition : leftParallelPosition);
            
            (AttackSide, int, Color)[] possibleAttackRegions = {
                (AttackSide.Left,3, Color.green),
                (AttackSide.Right,3, Color.blue),
                (AttackSide.Front,2, Color.cyan)
            };
            bool isRayTouching = false;
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
                    if (hit.collider != null && hit.transform != this.transform){
                        if(hit.transform.CompareTag("Player") || hit.transform.CompareTag("Enemy")){
                            Debug.DrawRay(ray.origin, ray.direction * attackRayRange, Color.red);
                            raysHit++;
                        }
                    }else
                        Debug.DrawRay(ray.origin, ray.direction * attackRayRange, possibleAttackRegion.Item3);
                }
                if (raysHit >= (possibleAttackRegion.Item2 - 1))
                {
                    secondsLockedToTarget += Time.fixedDeltaTime;
                    isRayTouching = true;
                    if(secondsLockedToTarget >= secondsToDeployAttack){
                        secondsLockedToTarget = 0;
                        float attackWidth = (possibleAttackRegion.Item1 == AttackSide.Left || possibleAttackRegion.Item1 == AttackSide.Right) ? sideAttackWidth : frontalAttackWidth;
                        ExecuteAttack(possibleAttackRegion.Item1, attackWidth, cannonballVelocity);
                    }
                }
            }
            if(!isRayTouching)
                secondsLockedToTarget = (secondsLockedToTarget < 0) ? 0 : secondsLockedToTarget - Time.fixedDeltaTime;
        }
    }
}
