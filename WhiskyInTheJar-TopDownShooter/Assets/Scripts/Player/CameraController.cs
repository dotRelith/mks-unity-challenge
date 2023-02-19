using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera playerCamera;
    private Vector3 cameraVelocity;
    public float cameraSmoothTime = .2f;
    public Vector3 cameraOffset = new Vector3(0f, 0f, -5f);

    void Start()
    {
        playerCamera = Camera.main;
    }

    void FixedUpdate()
    {
        if(playerCamera.transform.localPosition != cameraOffset)
            playerCamera.transform.localPosition = cameraOffset;
        this.transform.position = Vector3.SmoothDamp(this.transform.position, PlayerController.instance.transform.position, ref cameraVelocity, cameraSmoothTime);
    }
}
