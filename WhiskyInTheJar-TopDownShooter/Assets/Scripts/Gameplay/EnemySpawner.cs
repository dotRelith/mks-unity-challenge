using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
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
        SpawnEnemy(true, 5);
    }
    private void Update()
    {
        SpawnEnemy();
        timer += Time.deltaTime;
    }
    private void SpawnEnemy(bool ignoreCounter = false, int spawnQuantity = 1)
    {
        if (((maxSpawnCount == 0 || spawnCount < maxSpawnCount) && timer >= spawnInterval) || ignoreCounter == true){
            for (int i = 0; i < spawnQuantity; i++){
                Vector2 spawnPosition = Random.insideUnitCircle * Random.Range(16, spawnRadius);
                var enemy = new GameObject("Enemy");
                enemy.transform.position = transform.position + new Vector3(spawnPosition.x, spawnPosition.y, -0.2f);
                if (Random.Range(0f, 1f) > shooterChaserDistribution)
                    enemy.AddComponent<ChaserEnemy>();
                else
                    enemy.AddComponent<ShooterEnemy>();
                enemy.tag = "Enemy";
                enemy.layer = 3;
                timer = 0;
                spawnCount++;
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, spawnRadius);
    }
}
