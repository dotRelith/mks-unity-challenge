using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : Entity
{
    private enum AttackType { Frontal, Side}
    public static PlayerController instance;
    private Rigidbody2D playerRigidbody;
    private Vector2 playerInput;
    private Vector2 velocitySmoothing;
    private float currentRotation;

    [SerializeField] private Vector3 velocity;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float movementSpeed = 4f;
    [SerializeField] private float maxVelocity = 12f;
    [SerializeField] private float velocitySmoothTime = 2f;

    private float secondsToDeployAttack = 3f; //Amount of seconds player needs to hold the attack button
    private float secondsLeftForAttack;
    private float attackCooldown = 3f;
    [SerializeField] private float sideAttackRange = 5f;
    [SerializeField] private float sideAttackWidth = 3f;
    [SerializeField] private float frontalAttackRange = 5f;
    private Mesh attackMesh;
    [SerializeField] private MeshFilter attackFilter;

    protected override void Start()
    {
        base.Start();
        instance = this;
        playerRigidbody = GetComponent<Rigidbody2D>();

        attackMesh = new Mesh();
        attackFilter.mesh = attackMesh;
    }

    bool attackReady = false;
    bool attackStarted = false;
    protected override void Update()
    {
        base.Update();
        playerInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (Input.GetKey(KeyCode.X)) { attackStarted = true;  attackReady = RenderAttack(AttackType.Side); }
        if (Input.GetKeyUp(KeyCode.X)){
            if(attackReady == true){
                attackMesh.Clear();
                ExecuteAttack(AttackType.Side);
                attackReady = false;
                attackStarted = false;
            }
        }
        if (attackStarted && !Input.GetKey(KeyCode.X) && !Input.GetKey(KeyCode.Z))
        {
            RenderAttack(AttackType.Side, false);
            RenderAttack(AttackType.Frontal, false);
            if (secondsLeftForAttack < 0)
                secondsLeftForAttack = 0;
        }
    }
    void FixedUpdate()
    {   
        Vector2 targetVelocity = (playerInput.y > 0) ? transform.up * playerInput.y * movementSpeed : Vector2.zero;
        velocity = Vector2.SmoothDamp(velocity, targetVelocity, ref velocitySmoothing, velocitySmoothTime, maxVelocity);
        playerRigidbody.velocity = velocity;

        if(velocity.magnitude > 0.1f) {
            currentRotation = (currentRotation - playerInput.x * rotationSpeed * ((velocity.magnitude >= 0.3f) ? velocity.magnitude : 1) * Time.fixedDeltaTime) % 360f;
            transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
        }
    }
    private bool RenderAttack(AttackType attackType, bool increaseTimer = true)
    {
        if(secondsLeftForAttack < secondsToDeployAttack){
            secondsLeftForAttack = (increaseTimer) ? secondsLeftForAttack + Time.deltaTime : secondsLeftForAttack - Time.deltaTime;
            switch (attackType)
            {
                case AttackType.Frontal:
                    break;
                case AttackType.Side:
                    float halfWidth = sideAttackRange * (secondsLeftForAttack / secondsToDeployAttack) / 2f;
                    float halfHeight = sideAttackWidth / 2f;

                    Vector3[] vertices = new Vector3[] {
                        new Vector3(-halfWidth, -halfHeight, 0f), // bottom-left
                        new Vector3(-halfWidth, halfHeight, 0f), // top-left
                        new Vector3(0, -halfHeight, 0f), // bottom-right
                        new Vector3(0, halfHeight, 0f) // top-right
                    };
                    
                    attackMesh.vertices = vertices;
                    break;
            }
            Vector2[] uv = new Vector2[] {
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f)
            };

            int[] triangles = new int[] { 0, 1, 2, 2, 1, 3 };

            attackMesh.uv = uv;
            attackMesh.triangles = triangles;
        }
        else
        {
            return true;
        }
        return false;
    }
    private void ExecuteAttack(AttackType attackType)
    {
        secondsLeftForAttack = 0;
        Debug.Log("Boom!!!");
    }
}
