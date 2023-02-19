using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : Entity
{
    // Enumerations
    private enum AttackType { Frontal, Side }
    private enum AttackSide { Left, Right }

    // Singleton instance
    public static PlayerController instance;

    // Components
    [Header("Components")]
    [SerializeField] private MeshFilter attackFilter;
    private Rigidbody2D playerRigidbody;
    private Mesh attackMesh;

    // Player movement variables
    [Header("Player Movement")]
    private Vector2 playerInput;
    private Vector2 velocitySmoothing;
    private Vector3 velocity;
    private float currentRotation;

    // Movement speed variables
    [Header("Movement Speed")]
    [SerializeField] private float rotationSpeed = 16f;
    [SerializeField] private float movementSpeed = 6f;
    [SerializeField] private float maxVelocity = 12f;
    private float playerMaxVelocity;
    [SerializeField] private float velocitySmoothTime = 2f;

    // Attack variables
    [Header("Attack")]
    [SerializeField] private float sideAttackRange = 12f;
    [SerializeField] private float sideAttackWidth = 1.5f;
    [SerializeField] private float frontalAttackRange = 10f;
    [SerializeField] private float frontalAttackWidth = 1.5f;
    [SerializeField] private float cannonballVelocity = 16f;
    private AttackSide currentAttackSide = AttackSide.Left;
    private float secondsToDeployAttack = 3f;
    private float secondsPassedForAttack;
    bool attackReady = false;
    private AttackType lastAttackType;
    bool attackStarted = false;

    protected override void Start()
    {
        base.Start();
        instance = this;
        playerRigidbody = GetComponent<Rigidbody2D>();

        attackMesh = new Mesh();
        attackFilter.mesh = attackMesh;
    }

    protected override void Update()
    {
        base.Update();
        GetPlayerInput();

        CheckIfKeyIsPressed(KeyCode.Z, AttackType.Frontal);
        CheckIfKeyIsPressed(KeyCode.X, AttackType.Side);

        if (Input.GetKeyDown(KeyCode.C))
            ChangeAttackSide();

        if (attackStarted && !Input.GetKey(KeyCode.X) && !Input.GetKey(KeyCode.Z))
            StopAttack();

        playerMaxVelocity = (attackStarted) ? maxVelocity / 2 : maxVelocity;
    }

    private void GetPlayerInput()
    {
        playerInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    private void CheckIfKeyIsPressed(KeyCode keyCode, AttackType attackType)
    {
        if (Input.GetKey(keyCode)){
            attackStarted = true;
            attackReady = RenderAttack(attackType);
            lastAttackType = attackType;
        }

        if (Input.GetKeyUp(keyCode)){
            if (attackReady){
                ExecuteAttack(attackType);
                attackReady = false;
                attackStarted = false;
                attackMesh.Clear();
            }
        }
    }

    private void ChangeAttackSide()
    {
        currentAttackSide = (currentAttackSide == AttackSide.Left) ? AttackSide.Right : AttackSide.Left;
        secondsPassedForAttack = 0;
        RenderAttack(AttackType.Side, false);
        RenderAttack(AttackType.Frontal, false);
    }

    private void StopAttack()
    {
        switch (lastAttackType){
            case AttackType.Frontal:
                RenderAttack(AttackType.Frontal, false);
                break;
            case AttackType.Side:
                RenderAttack(AttackType.Side, false);
                break;
        }

        if (secondsPassedForAttack < 0)
            secondsPassedForAttack = 0;
    }

    void FixedUpdate()
    {   
        Vector2 targetVelocity = (playerInput.y > 0) ? transform.up * playerInput.y * movementSpeed : Vector2.zero;
        velocity = Vector2.SmoothDamp(velocity, targetVelocity, ref velocitySmoothing, velocitySmoothTime, playerMaxVelocity);
        playerRigidbody.velocity = velocity;

        if(velocity.magnitude > 0.1f) {
            currentRotation = (currentRotation - playerInput.x * rotationSpeed * ((velocity.magnitude >= 0.3f) ? velocity.magnitude : 1) * Time.fixedDeltaTime) % 360f;
            transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
        }
    }
    private bool RenderAttack(AttackType attackType, bool increaseTimer = true)
    {
        if (secondsPassedForAttack >= secondsToDeployAttack)
            return true;

        secondsPassedForAttack = (increaseTimer) ? secondsPassedForAttack + Time.deltaTime : secondsPassedForAttack - Time.deltaTime;
        float progress = secondsPassedForAttack / secondsToDeployAttack;

        Vector3[] vertices = null;
        switch (attackType)
        {
            case AttackType.Frontal:
                vertices = GetFrontalVertices(progress);
                break;
            case AttackType.Side:
                vertices = GetSideVertices(progress);
                break;
        }
        Vector2[] uv = new Vector2[] {
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f)
            };

        attackMesh.vertices = vertices;
        int[] triangles = new int[] { 0, 1, 2, 2, 1, 3 };

        attackMesh.uv = uv;
        attackMesh.triangles = triangles;

        return false;
    }
    
    private Vector3[] GetFrontalVertices(float progress)
    {
        float halfWidth = frontalAttackWidth / 2f;
        float halfHeight = frontalAttackRange * progress / 2f;

        return new Vector3[]
        {
        new Vector3(-halfWidth, 0f, 0f),
        new Vector3(-halfWidth, halfHeight, 0f),
        new Vector3(halfWidth, 0f, 0f),
        new Vector3(halfWidth, halfHeight, 0f)
        };
    }

    private Vector3[] GetSideVertices(float progress)
    {
        float halfWidth = sideAttackRange * progress / 2f;
        if (currentAttackSide == AttackSide.Left)
        {
            halfWidth *= -1;
        }
        float halfHeight = sideAttackWidth / 2f;

        return new Vector3[]
        {
        new Vector3(halfWidth, -halfHeight, 0f),
        new Vector3(halfWidth, halfHeight, 0f),
        new Vector3(0, -halfHeight, 0f),
        new Vector3(0, halfHeight, 0f)
        };
    }

    private void ExecuteAttack(AttackType attackType)
    {
        secondsPassedForAttack = 0;
        int cannonballNumber = 1;
        if (attackType == AttackType.Side)
            cannonballNumber = 3;
        float distanceIncrease = sideAttackWidth / cannonballNumber;
        float currentDistance = distanceIncrease;
        for (int i = 0; i < cannonballNumber; i++)
        {
            GameObject cannonball = Instantiate(Resources.Load("Cannonball")) as GameObject;
            Vector3 cannonballOrigin = Vector2.zero;
            switch (attackType){
                case AttackType.Side:
                    cannonballOrigin = (currentAttackSide == AttackSide.Left) ? -1 * this.transform.right : this.transform.right;
                    break;
                case AttackType.Frontal:
                    cannonballOrigin = this.transform.up * 1.5f;
                    break;
            }
            cannonballOrigin *= 1.25f;
            Vector3 cannonballDirection = ((transform.position + cannonballOrigin) - this.transform.position).normalized;

            if(attackType == AttackType.Side){
                cannonballOrigin -= Vector3.up * currentDistance;
                currentDistance -= distanceIncrease;
            }

            cannonball.GetComponent<Cannonball>().isFromPlayer = true;

            cannonball.transform.position = this.transform.position + cannonballOrigin;
            Rigidbody2D cannonballRigidbody = cannonball.GetComponent<Rigidbody2D>();
            cannonballRigidbody.velocity = playerRigidbody.velocity;
            cannonballRigidbody.AddForce(cannonballDirection * cannonballVelocity, ForceMode2D.Impulse);
        }
    }
}
