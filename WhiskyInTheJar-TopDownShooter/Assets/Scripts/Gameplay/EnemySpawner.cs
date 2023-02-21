using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject boatPrefab;  
    [SerializeField] private float spawnRadius = 128f;  
    [SerializeField] private int maxSpawnCount = 50;
    // 0.7f = 70% shooter 30% chaser
    [Range(0, 1)] [SerializeField] private float shooterChaserDistribution = 0.7f;  

    private float spawnInterval;
    private int spawnCount;    
    private float timer;

    private void Start()
    {
        spawnInterval = PlayerPrefs.GetInt("EnemySpawnDelay");
    }
    private void Update()
    {
        if ((maxSpawnCount == 0 || spawnCount < maxSpawnCount) && timer >= spawnInterval){
            Vector2 spawnPosition = Random.insideUnitCircle * Random.Range(5, spawnRadius);
            var enemy = Instantiate(boatPrefab , transform.position + new Vector3(spawnPosition.x, spawnPosition.y, -0.2f), Quaternion.identity);
            if (Random.Range(0f, 1f) > shooterChaserDistribution)
                enemy.AddComponent<ChaserEnemy>();
            else
                enemy.AddComponent<ShooterEnemy>();
            timer = 0;
            spawnCount++;
        }

        timer += Time.deltaTime;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, spawnRadius);
    }
}
