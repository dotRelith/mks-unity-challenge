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
        SpawnEnemy(true, 20);
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
                Vector2 randomPosition = Random.insideUnitCircle;
                Vector2 spawnPosition = randomPosition * spawnRadius;
                bool isOverlapingWithLevel = Physics2D.OverlapPoint(spawnPosition, LayerMask.NameToLayer("Obstacle"));
                while (isOverlapingWithLevel){
                    spawnPosition = Random.insideUnitCircle * spawnRadius;
                    isOverlapingWithLevel = Physics2D.OverlapPoint(spawnPosition, LayerMask.NameToLayer("Obstacle"));
                }
                var enemy = new GameObject("Enemy");    
                enemy.transform.position = transform.position + new Vector3(spawnPosition.x, spawnPosition.y, -0.2f);
                if (Random.Range(0, 100) > shooterChaserDistribution*100)
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
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(this.transform.position, 256f);
    }
}
