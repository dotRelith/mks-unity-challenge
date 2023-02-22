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
        //Se inimigo está morto não execute codigo algum
        if (IsDead)
            return;

        //executa o Update da superclasse
        base.Update();
        //Limpa todo e qualquer entidade detectada
        colliders.Clear();
        //Define uma lista de entidades detectadas
        Physics2D.OverlapCircle(transform.position, aggroRange, new ContactFilter2D().NoFilter(), colliders);
    }

    private void FixedUpdate()
    {
        //Se inimigo está morto não execute codigo algum
        if (IsDead)
            return;

        //procure na lista de entidades detectadas pelo player ou por inimigos
        Collider2D playerCollider = default;
        Collider2D enemyCollider = default;
        foreach (var collider in colliders){
            if (collider != null) { 
                if (collider.CompareTag("Player"))
                    playerCollider = collider;
                if (collider.CompareTag("Enemy"))
                    enemyCollider = collider;
            }
        }
        //verifica primeiramente se o player foi detectado
        if (playerCollider != null){
            TargetDetected(playerCollider.transform);
            targetTransform= playerCollider.transform;
        }
        //verifica se algum inimigo foi detectado, fora esse
        else if (enemyCollider != null && enemyCollider.transform != this.transform){
            TargetDetected(enemyCollider.transform);
            targetTransform= enemyCollider.transform;
        }
        //caso nenhum player ou inimigo foi detectado verifica se o inimigo já está vagando e caso contrario chama Roam()
        else if (arrived){
            Roam();
            targetTransform = null;
        }

        //caso o inimigo nao chegou no destino e tem um alvo, mova atpe o alvo
        if (!arrived && moveTarget != null)
            MoveTo(moveTarget.Value);

        //Lida com a rotação do inimigo
        HandleRotation();
    }

    protected virtual void HandleRotation(){
        //Altera a rotação do inimigo para a direção do movimento
        Vector2 direction = entityRigidbody.velocity.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
    }

    private void Roam()
    {
        //Caso a posição inicial de vagar do inimigo não foi definida no editor defina para a atual
        if (startRoamPosition == Vector2.zero)
            startRoamPosition = transform.position;

        //aumenta o temporizador de espera
        timeSinceDirectionChange += Time.fixedDeltaTime;

        //caso o temporizador de espera esja maior que o tempo necessario pra mudar de direção 
        if (timeSinceDirectionChange >= timeToChangeDirection){
            //pegue uma posição aleatoria dentro do raio de vagar(não sei como traduzir isso bem :V)
            Vector2 randomDirection = Random.insideUnitCircle.normalized * Random.Range(roamRadius/6,roamRadius);
            //defina a posição para o inimigo ir
            Vector2 newRoamPosition = startRoamPosition + randomDirection;

            //define essa posição como alvo para o inimigo
            moveTarget = newRoamPosition;
            //informe que não chegamos na posiçao
            arrived = false;
            //resete o temporizador de espera para 0
            timeSinceDirectionChange = 0f;
        }
    }

    protected void MoveTo(Vector2 position)
    {
        //direção do alvo ao inimigo
        Vector2 direction = (position - (Vector2)transform.position).normalized;

        //distancia do inimigo a posiçao
        float distance = Vector2.Distance(transform.position, position);

        //defina a velocidade de acordo com a distancia do alvo
        float speedMultiplier = Mathf.Clamp(distance / stoppingDistance, 0f, 1f);
        float currentSpeed = speedMultiplier * movementSpeed;

        //caso a distance entre o inimigo e a posição seja menor que a distancia de diminuir velocidade
        if (distance < stoppingDistance)
        {
            //diminua a velocidade do inimigo
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, stoppingSpeed * Time.deltaTime);
            //se a velocidade atual ficar muito baixa informe que chegamos no alvo
            if (currentSpeed <= 0.1f)
                arrived = true;
        }
        //informe a velocidade do inimigo
        entityRigidbody.velocity = direction * currentSpeed;
    }

    //Funcção para ser sobreescrevida pelo tipo de inimigo
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