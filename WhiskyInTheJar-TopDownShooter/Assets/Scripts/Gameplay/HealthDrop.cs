using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealthDrop : MonoBehaviour
{
    public int healAmount = 35;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Entity colidedEntity = collision.transform.GetComponent<Entity>();
        if (colidedEntity != null)
        {
            colidedEntity.HealEntity(healAmount);
            Destroy(this.gameObject);
        }
    }
}
