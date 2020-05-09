using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerController : MonoBehaviour
{
    public Transform spawnPoint;
    public LockOnTarget lockOnTarget;
    public GameObject bullet;
    public float fireRate;

    void Start()
    {
        StartCoroutine(Fire());
    }

    void Update()
    {
        
    }

    IEnumerator Fire()
    {
        //yield return new WaitForSeconds(fireRate); // Задержка чтобы не было двойных пулек

        while (true)
        {
            if (lockOnTarget.targetLocked)
            {
                GameObject newBullet = Instantiate(bullet, spawnPoint.position, spawnPoint.rotation);
                newBullet.GetComponent<BulletController>().targetPosition = lockOnTarget.currentTarget;
            }
            

            yield return new WaitForSeconds(fireRate);
        }
        
    }
}
