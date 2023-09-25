using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

[System.Serializable]
public class Wave
{
    public string waveName;
    public int numberOfEnemies; // 적의 수
    public GameObject[] typeOfEnemies; // 적의 종류
    public float spawnInterval; // 스폰 간격
}
public class EnemyState
{
    public string enemyName;
    public int enemyHP;
    public int enemyDamage;
    public float enemySpeed;
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
    private List<Enemy> enemyDatas = new List<Enemy>(new Enemy[3]);
    //[SerializeField]
    //private GameObject enemy;

    private EnemyState enemy1;

    private int enemyCounts = 0;

    private void Awake()
    {
        Save();
    }

    private void Start()
    {
        Load();
    }

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
        if (!canSpawn)
        {
            return;
        }
    }

    void SpawnWave()
    {
        if (canSpawn && nextSpawnTime < Time.time)
        {
            GameObject randomEnemy = currentWave.typeOfEnemies[Random.Range(0, currentWave.typeOfEnemies.Length)];
            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            var newEnemy = Instantiate(randomEnemy, randomPoint.position, Quaternion.identity).GetComponent<Enemy>();

            newEnemy.enemyID = enemyCounts; // 생성된 적의 id를 추가
            enemyDatas[enemyCounts] = newEnemy;

            if (newEnemy.name == enemy1.enemyName) // 생성한 오브젝트와 enemy1의 이름이 같으면 복사
            {
                newEnemy.enemyName = enemy1.enemyName;
                newEnemy.enemyHP = enemy1.enemyHP;
                newEnemy.enemyDamage = enemy1.enemyDamage;
                newEnemy.enemySpeed = enemy1.enemySpeed;

            }

            //Debug.Log("enemy ID " + newEnemy.getEnemyID());

            if (enemyCounts < 3) enemyCounts++;

            currentWave.numberOfEnemies--;
            nextSpawnTime = Time.time + currentWave.spawnInterval; // 간격을 두고 생성

            if (currentWave.numberOfEnemies == 0)
            {
                //for (int i = 0; i < 3; i++)
                //{
                //    Debug.Log(enemyDatas[i]);
                //    Debug.Log(enemyDatas[i].getEnemyID());
                //}
                    canSpawn = false;
            }
        }
    }
    void Save()
    {
        EnemyState nomalEnemy = new EnemyState();
        nomalEnemy.enemyName = "NomalEnemyPrefab(Clone)"; // 이름이 똑같아야함.
        nomalEnemy.enemyHP = 5;
        nomalEnemy.enemyDamage = 5;
        nomalEnemy.enemySpeed = 5.0f;

        string json = JsonUtility.ToJson(nomalEnemy);
        File.WriteAllText(Application.dataPath + "/EnemyData.json", json);
    }

    void Load()
    {
        string content = File.ReadAllText(Application.dataPath + "/EnemyData.json");

        enemy1 = JsonUtility.FromJson<EnemyState>(content);
    }
}
