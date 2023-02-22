using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Cannonball : MonoBehaviour
{
    [HideInInspector] public bool isFromPlayer = false;
    private Rigidbody2D cannonballRigidbody;
    public int cannoballDamage = 45;
    private void Start()
    {
        cannonballRigidbody = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        if (cannonballRigidbody.velocity.magnitude <= 0.1f)
            DestroyCannonball();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Entity colidedEntity = collision.transform.GetComponent<Entity>();
        if (colidedEntity != null){
            colidedEntity.DamageEntity(cannoballDamage, isFromPlayer);
            DestroyCannonball();
        }
    }
    private void DestroyCannonball(){
        Destroy(Instantiate(Resources.Load("Explosion"), this.transform.position, this.transform.rotation), 7);//7 second is the time needed for the sound to play
        Destroy(this.gameObject);
    }
}
