using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    //public EnemyData enemyData;
    private GameObject target;

    private float dist;

    [SerializeField]
    private string enemyName;
    private int enemyHP;
    private int enemyDamage;
    private float enemySpeed = 5;
    private Vector3 enemyPos;
    [SerializeField]
    private int enemyID;

    private void Start()
    {
        //printEnemyData();
        target = GameObject.FindGameObjectWithTag("Building");
    }

    private void Update()
    {
        Move();
    }

    public void printEnemyData()
    {
        Debug.Log("적 이름 : " + enemyName);
        Debug.Log("체력  : " + enemyHP);
        Debug.Log("데미지 : " + enemyDamage);
        Debug.Log("속도 : " + enemySpeed);
        Debug.Log("위치 : " + enemyPos);
        Debug.Log("ID : " + enemyID);
        Debug.Log("========================================");
    }
    private void Move()
    {
        transform.position = Vector3.MoveTowards(gameObject.transform.position, target.transform.position, enemySpeed * 0.0003f);
        enemyPos = gameObject.transform.position;

        //dist = Vector3.Distance(gameObject.transform.position, target.transform.position);
    }

    public void setEnemyID(int id_in)
    {
        enemyID = id_in;
    }

    public int getEnemyID() { return enemyID; }
}
