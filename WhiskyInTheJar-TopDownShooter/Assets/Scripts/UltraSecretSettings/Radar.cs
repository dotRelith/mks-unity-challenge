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
        //Smoothly follow player
        this.transform.position = Vector3.SmoothDamp(this.transform.position, PlayerController.instance.transform.position, ref cameraVelocity, cameraSmoothTime);

        float previousRotation = (radarStick.eulerAngles.z % 360) - 180;
        //makes the radar rotate
        radarStick.eulerAngles -= new Vector3(0f, 0f, Time.deltaTime * radarSpeed);
        float currentRotation = (radarStick.eulerAngles.z % 360) - 180;


        if (previousRotation < 0 && currentRotation >= 0)
            colliders.Clear();

        float angle = radarStick.eulerAngles.z * Mathf.Deg2Rad;
        Vector3 rayDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));
        RaycastHit2D raycastHit2D = Physics2D.Raycast(this.transform.position, rayDirection, radarWidth);
        Debug.DrawRay(this.transform.position, rayDirection * radarWidth);
        if (raycastHit2D.collider != null){
            if (!colliders.Contains(raycastHit2D.collider)){
                if (raycastHit2D.collider.transform.CompareTag("Enemy")){
                    GameObject enemyDetected = Instantiate(Resources.Load("EnemyDetected"), raycastHit2D.point, Quaternion.identity) as GameObject;
                    Destroy(enemyDetected, 3);
                    colliders.Add(raycastHit2D.collider);
                }
            }
        }
    }
    private void UpdateDimensions(){
        radarStick.localScale = new Vector3(radarWidth, 0.5f, 0f);
        radarCamera.orthographicSize = radarWidth * 2;
    }
    private void OnDrawGizmos(){
        foreach (var collider in colliders){
            if (collider != null)
                Gizmos.DrawWireSphere(collider.transform.position, 5f);
        }
    }
}
