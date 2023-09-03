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
}
