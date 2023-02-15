using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
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
    void Start()
    {
        instance = this;
        playerRigidbody = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        playerInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));        
    }
    void FixedUpdate()
    {   
        Vector2 targetVelocity = (playerInput.y > 0) ? transform.up * playerInput.y * movementSpeed : Vector2.zero;
        velocity = Vector2.SmoothDamp(velocity, targetVelocity, ref velocitySmoothing, velocitySmoothTime, maxVelocity);
        playerRigidbody.velocity = velocity;

        // Calculate the boat's rotation based on the input
        if(velocity.magnitude > 0.1f) {
            currentRotation = (currentRotation - playerInput.x * rotationSpeed * velocity.magnitude * Time.fixedDeltaTime) % 360f;
            transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
        }
        if (playerRigidbody.velocity.magnitude < 0.1f)
            playerRigidbody.velocity = Vector2.zero;
    }
}
