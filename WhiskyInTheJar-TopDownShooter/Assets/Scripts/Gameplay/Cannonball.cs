using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Cannonball : MonoBehaviour
{
    public bool isFromPlayer = false;
    private Rigidbody2D cannonballRigidbody;
    public int cannoballDamage = 45;
    private void Start()
    {
        cannonballRigidbody = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        if(cannonballRigidbody.velocity.magnitude <= 0.1f)
            Destroy(this.gameObject);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Entity colidedEntity = collision.transform.GetComponent<Entity>();
        if (colidedEntity != null){
            colidedEntity.DamageEntity(cannoballDamage, isFromPlayer);
            Destroy(this.gameObject);
        }
        print(collision.transform.name);
    }
}
