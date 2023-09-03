using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyData enemyData;
    private GameObject target;

    private float dist;

    private void Start()
    {
        //printEnemyData();
        target = GameObject.FindGameObjectWithTag("Building");
    }

    private void Update()
    {
        MoveAndCalculate();
    }

    public void printEnemyData()
    {
        Debug.Log("적 이름 : " + enemyData.getEnemyName);
        Debug.Log("체력  : " + enemyData.getEnemyHP);
        Debug.Log("데미지 : " + enemyData.getEnemyDamage);
        Debug.Log("속도 : " + enemyData.getEnemySpeed);
        Debug.Log("위치 : " + enemyData.getEnemyPosition);
        Debug.Log("거리 : " + enemyData.enemyAndBuildingDistance);
        Debug.Log("========================================");
    }
    private void MoveAndCalculate()
    {
        transform.position = Vector3.MoveTowards(gameObject.transform.position, target.transform.position, enemyData.getEnemySpeed * 0.0003f);
        enemyData.setEnemyPosition(gameObject.transform.position);

        dist = Vector3.Distance(gameObject.transform.position, target.transform.position);
        enemyData.setEmyAndBuildingDistance(dist);
    }
}
