using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyData enemyData;
    private GameObject target;

    private void Start()
    {
        printEnemyData();
        target = GameObject.FindGameObjectWithTag("Building");
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(gameObject.transform.position, target.transform.position, enemyData.getEnemySpeed * 0.0003f);
    }

    public void printEnemyData()
    {
        Debug.Log("적 이름 : " + enemyData.getEnemyName);
        Debug.Log("체력  : " + enemyData.getEnemyHP);
        Debug.Log("데미지 : " + enemyData.getEnemyDamage);
        Debug.Log("속도 : " + enemyData.getEnemySpeed);
        Debug.Log("========================================");
    }
}
