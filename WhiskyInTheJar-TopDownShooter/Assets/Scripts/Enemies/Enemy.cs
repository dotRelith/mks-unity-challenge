using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : Entity
{
    [Header("Enemy Settings")]
    [SerializeField] private float aggroRange = 16f;
    [SerializeField] protected float movementSpeed = 3.6f;
    [SerializeField] private float rotationSpeed = 9f;
    [SerializeField] private float stoppingDistance = 2f;
    [SerializeField] protected float enemyPointBonus = 0f;
    public float EnemyPointBonus { get { return enemyPointBonus; } }

    [Header("Roaming Settings")]
    [SerializeField] private float roamRadius = 8f;
    [SerializeField] private float timeToChangeDirection = 2f;
    [SerializeField] private float stoppingSpeed = 1f;
    protected Transform targetTransform;


    private Vector2 startRoamPosition;
    private float timeSinceDirectionChange = 0f;
    private List<Collider2D> colliders = new List<Collider2D>();
    private Vector2? moveTarget;
    private bool arrived = true;

    protected override void Update()
    {
        if (IsDead)
            return;

        base.Update();
        colliders.Clear();
        Physics2D.OverlapCircle(transform.position, aggroRange, new ContactFilter2D().NoFilter(), colliders);
    }

    private void FixedUpdate()
    {
        if (IsDead)
            return;

        Collider2D playerCollider = colliders.Find(c => c.CompareTag("Player"));
        Collider2D enemyCollider = colliders.Find(c => c.CompareTag("Enemy"));
        if (playerCollider != null){
            TargetDetected(playerCollider.transform);
            targetTransform= playerCollider.transform;
        }
        else if (enemyCollider != null && enemyCollider.transform != this.transform){
            TargetDetected(enemyCollider.transform);
            targetTransform= enemyCollider.transform;
        }
        else if (arrived){
            Roam();
            targetTransform = null;
        }

        if (!arrived && moveTarget != null)
            MoveTo(moveTarget.Value);

        HandleRotation();
    }

    protected virtual void HandleRotation(){
        Vector2 direction = entityRigidbody.velocity.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
    }

    private void Roam()
    {
        if (startRoamPosition == Vector2.zero)
            startRoamPosition = transform.position;

        timeSinceDirectionChange += Time.fixedDeltaTime;

        if (timeSinceDirectionChange >= timeToChangeDirection){
            Vector2 randomDirection = Random.insideUnitCircle.normalized * roamRadius;
            Vector2 newRoamPosition = startRoamPosition + randomDirection;
            moveTarget = newRoamPosition;
            arrived = false;
            timeSinceDirectionChange = 0f;
        }
    }

    protected void MoveTo(Vector2 position)
    {
        Vector2 direction = (position - (Vector2)transform.position).normalized;

        float distance = Vector2.Distance(transform.position, position);

        float speedMultiplier = Mathf.Clamp(distance / stoppingDistance, 0f, 1f);
        float currentSpeed = speedMultiplier * movementSpeed;

        if (distance < stoppingDistance)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, stoppingSpeed * Time.deltaTime);
            if (currentSpeed <= 0.1f)
                arrived = true;
        }
        //make smoothdamp
        entityRigidbody.velocity = direction * currentSpeed;
    }

    protected virtual void TargetDetected(Transform targetTransform) {}

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        Gizmos.color = Color.blue;
        Vector2 center = startRoamPosition != Vector2.zero ? startRoamPosition : (Vector2)transform.position;
        Gizmos.DrawWireSphere(center, roamRadius);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}