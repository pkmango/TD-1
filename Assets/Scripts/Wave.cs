using UnityEngine;

public class Wave : MonoBehaviour
{
    public int reward; // Награда
    public int hp;
    public float spawnWait;
    public int randomSpawn; // Координата появления врага меняется случайно в этих пределах
    public GameObject waveTile;

    public GameObject[] enemies;
}
