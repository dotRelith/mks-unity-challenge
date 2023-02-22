using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{
    [SerializeField] private float radarSpeed = 32f;
    [SerializeField] private float radarWidth = 45f;
    private Transform radarStick;
    private Camera radarCamera;
    private Vector3 cameraVelocity;
    [SerializeField] private float cameraSmoothTime;
    private List<Collider2D> colliders = new List<Collider2D>();

    private void Awake(){
        radarStick = this.transform.Find("RadarStick");
        radarCamera = this.GetComponentInChildren<Camera>();
        UpdateDimensions();
    }
    void Update()
    {
        #if UNITY_EDITOR && !UNITY_STANDALONE
            UpdateDimensions();
        #endif
        this.transform.position = Vector3.SmoothDamp(this.transform.position, PlayerController.instance.transform.position, ref cameraVelocity, cameraSmoothTime);
        radarStick.eulerAngles -= new Vector3(0f, 0f, Time.deltaTime * radarSpeed);

        float previousRotation = this.transform.eulerAngles.z % 360;
        float currentRotation = this.transform.eulerAngles.z % 360;

        float angleRad = this.transform.eulerAngles.z * (Mathf.PI / 180f);
        Vector3 rayDirection = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        RaycastHit2D raycastHit2D = Physics2D.Raycast(this.transform.position, rayDirection,radarWidth);
        if (!colliders.Contains(raycastHit2D.collider)){
            colliders.Add(raycastHit2D.collider);
        }
    }
    private void UpdateDimensions(){
        radarStick.localScale = new Vector3(radarWidth, 0.5f, 0f);
        radarCamera.orthographicSize = radarWidth * 2;
    }
}
