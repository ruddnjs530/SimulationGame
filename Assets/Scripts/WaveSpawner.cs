using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Wave
{
    public string waveName;
    public int numberOfEnemies; // 적의 수
    public GameObject[] typeOfEnemies; // 적의 종류
    public float spawnInterval; // 스폰 간격
}

public enum EnemyType { Nomal, Power, Speed }

public class WaveSpawner : MonoBehaviour
{
    public Wave[] waves;
    public Transform[] spawnPoints; // 스폰할 지점

    private Wave currentWave; // 현재 실행되는 웨이브가 몇번째인지 확인
    private int currentWaveNumber; // 실행되고 있는 웨이브 번호

    private bool canSpawn = true;
    private float nextSpawnTime;

    //[SerializeField]
    private List<EnemyData> enemyDatas = new List<EnemyData>(new EnemyData[3]);
    //[SerializeField]
    //private GameObject enemy;

    private int enemyCounts = 0;
    private List<EnemyData> sortList;


    // Update is called once per frame
    void Update()
    {
        currentWave = waves[currentWaveNumber];
        SpawnWave();
        GameObject[] totalEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (totalEnemies.Length == 0 && !canSpawn && currentWaveNumber + 1 != waves.Length) // 마지막은 모든 웨이브가 끝나면 다시 시작되지 않게 하기 위함
        {
            currentWaveNumber++;
            canSpawn = true;
        }
        SortEnemyData();
        if (!canSpawn)
        {
            //Debug.Log(sortList[0]);
            return;
        }
    }

    void SpawnWave()
    {
        if (canSpawn && nextSpawnTime < Time.time)
        {
            GameObject randomEnemy = currentWave.typeOfEnemies[Random.Range(0, currentWave.typeOfEnemies.Length)];
            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            //enemyDatas[enemyCounts] = Instantiate(randomEnemy, randomPoint.position, Quaternion.identity).GetComponent<Enemy>();
            var newEnemy = Instantiate(randomEnemy, randomPoint.position, Quaternion.identity).GetComponent<Enemy>();

            newEnemy.enemyData.setEnemyID(enemyCounts); // 생성된 적의 id를 추가
            enemyDatas[enemyCounts] = newEnemy.enemyData;
            //Debug.Log("정렬 전 " + enemyDatas[0]);

            Debug.Log("enemy ID " + newEnemy.enemyData.getEnemyID);
                      
            if (enemyCounts < 3) enemyCounts++;

            currentWave.numberOfEnemies--;
            nextSpawnTime = Time.time + currentWave.spawnInterval; // 간격을 두고 생성

            if (currentWave.numberOfEnemies == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    Debug.Log(enemyDatas[i]);
                    Debug.Log(enemyDatas[i].getEnemyID);
                }
                    canSpawn = false;
            }
        }
    }

    void SortEnemyData()
    {
        if (!canSpawn)
        {
            sortList = enemyDatas.OrderBy(x => x.enemyAndBuildingDistance).ToList();
        }
    }
}
