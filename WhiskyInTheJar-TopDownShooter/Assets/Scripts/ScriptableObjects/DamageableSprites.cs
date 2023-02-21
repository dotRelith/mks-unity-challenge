using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Damageable Sprites", menuName = "Damageable Sprites")]
public class DamageableSprites : ScriptableObject
{
    public test[] DamageableSails;
    public test[] DamageableBody;
}
[System.Serializable]
public struct test
{
    public string name;
    public Sprite wholeSprite, damagedSprite, badlyDamagedSprite;
}
