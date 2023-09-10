using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Data", menuName = "Scriptable Object/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [SerializeField]
    private string enemyName;
    public string getEnemyName { get { return enemyName; } }
    [SerializeField]
    private int enemyHP;
    public int getEnemyHP { get { return enemyHP; } }
    [SerializeField]
    private int enemyDamage;
    public int getEnemyDamage { get { return enemyDamage; } }
    [SerializeField]
    private float enemySpeed;
    public float getEnemySpeed { get { return enemySpeed; } }

    private Vector3 enemyPosition;
    public Vector3 getEnemyPosition { get { return enemyPosition; } }

    private int enemyID;
    public void setEnemyID(int id_in) { enemyID = id_in; }
    public int getEnemyID { get { return enemyID; } }
    public void setEnemyPosition(Vector3 position)
    {
        enemyPosition = position;
    }
    public float enemyAndBuildingDistance;
    //public float getnEmyAndBuildingDistance { get { return enemyAndBuildingDistance; } }
    public void setEmyAndBuildingDistance(float distance)
    {
        enemyAndBuildingDistance = distance;
    }
}
