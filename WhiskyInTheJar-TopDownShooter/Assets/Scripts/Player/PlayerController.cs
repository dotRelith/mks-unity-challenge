using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Entity
{
    //Enumerations
    protected enum AttackType { Frontal, Side }

    // Singleton instance
    public static PlayerController instance;

    // Components
    [Header("Components")]
    [SerializeField] private MeshFilter attackFilter;
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

        attackMesh = new Mesh();
        attackFilter.mesh = attackMesh;
        onEntityDeath += (s,e) => { StartCoroutine(MatchManager.instance.EndMatchCoroutine()); }; 
    }

    protected override void Update()
    {
        if (IsDead){
            playerInput = Vector2.zero;
            return;
        } 

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
                float attackWidth = (attackType == AttackType.Side) ? sideAttackWidth : frontalAttackWidth;
                AttackSide attackSide = (attackType == AttackType.Frontal) ? AttackSide.Front : currentAttackSide;
                ExecuteAttack(attackSide, attackWidth, cannonballVelocity);
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
        Vector2 targetVelocity = (playerInput.y > 0) ? transform.right * playerInput.y * movementSpeed : Vector2.zero;
        velocity = Vector2.SmoothDamp(velocity, targetVelocity, ref velocitySmoothing, velocitySmoothTime, playerMaxVelocity);
        entityRigidbody.velocity = velocity;

        if (entityRigidbody.velocity.magnitude > 0.1f){
            currentRotation = (currentRotation - playerInput.x * rotationSpeed * ((entityRigidbody.velocity.magnitude >= 0.3f) ? entityRigidbody.velocity.magnitude : 1) * Time.fixedDeltaTime) % 360f;
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

        return new Vector3[]{
            new Vector3(-halfWidth, 0f, 0f),
            new Vector3(-halfWidth, halfHeight, 0f),
            new Vector3(halfWidth, 0f, 0f),
            new Vector3(halfWidth, halfHeight, 0f)
        };
    }

    private Vector3[] GetSideVertices(float progress)
    {
        float halfWidth = sideAttackRange * progress / 2f;
        if (currentAttackSide == AttackSide.Right)
        {
            halfWidth *= -1;
        }
        float halfHeight = sideAttackWidth / 2f;

        return new Vector3[]{
            new Vector3(halfWidth, -halfHeight, 0f),
            new Vector3(halfWidth, halfHeight, 0f),
            new Vector3(0, -halfHeight, 0f),
            new Vector3(0, halfHeight, 0f)
        };
    }
    protected override void ExecuteAttack(AttackSide attackSide, float sideAttackWidth, float cannonballSpeed)
    {
        secondsPassedForAttack = 0;
        base.ExecuteAttack(attackSide, sideAttackWidth, cannonballSpeed);
    }
}
