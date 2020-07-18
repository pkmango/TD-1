using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOnTarget : MonoBehaviour
{
    public float turnSpeed;
    public GameObject gun;
    public float updateTime = 0.02f;
    public Transform currentTarget;
    public float accuracy = 7f; // Точность наведения (0 = абсолютная)
    public bool targetDetected; // Цель обнаружена?
    public bool targetLocked; // Цель захвачена?
    public string enemyTag; // Тип врага Enemy или AirEnemy
    public bool anyEnemy; // Если true - башня может атаковать любого врага

    private Vector3 direction;
    private Quaternion rotation;
    private List<Transform> enemies =  new List<Transform>();

    private Quaternion defoultRotation;

    void Start()
    {
        targetDetected = false;
        targetLocked = false;
        defoultRotation = gun.transform.rotation;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (CheckObject(col))
        {            
            enemies.Add(col.gameObject.GetComponent<Transform>());

            if (!targetDetected)
            {
                targetDetected = true;
                currentTarget = col.gameObject.GetComponent<Transform>();
                StartCoroutine(TargetTracking(col.gameObject.GetComponent<Transform>()));
            }
        } 
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (CheckObject(col))
        {
            enemies.Remove(col.gameObject.GetComponent<Transform>());
        }
    }

    // Проверяем что за объект и нужно ли его атаковать, если нужно возвращается true
    private bool CheckObject(Collider2D col)
    {
        bool trackedObject = false;

        if (anyEnemy)
        {
            if (col.CompareTag("Enemy") || col.CompareTag("AirEnemy")) trackedObject = true;
        }
        else
        {
            if (col.CompareTag(enemyTag)) trackedObject = true;
        }

        return trackedObject;
    }

    IEnumerator TargetTracking(Transform enemy)
    {
        CancelInvoke();
        Collider2D thisCollider = gameObject.GetComponent<Collider2D>();
        Collider2D enemyCollider = enemy.gameObject.GetComponent<Collider2D>();

        while (true)
        {
            if (enemyCollider != null)
            {
                if (thisCollider.IsTouching(enemyCollider))
                {
                    direction = enemy.position - gun.transform.position;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                    gun.transform.rotation = Quaternion.Lerp(gun.transform.rotation, rotation, turnSpeed * updateTime);

                    // Если угол поворота башни по направлении к цели меньше accuracy, считаем цель захваченной 
                    if ((rotation.eulerAngles.z - gun.transform.rotation.eulerAngles.z) < accuracy)
                    {
                        targetLocked = true;
                    }
                    else
                    {
                        targetLocked = false;
                    }

                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }

            yield return new WaitForSeconds(updateTime);
        }

        if (enemies.Count != 0)
        {
            currentTarget = enemies[0];
            StartCoroutine(TargetTracking(enemies[0]));
        }
        else
        {
            InvokeRepeating("BackDefoultRotation", 0f, updateTime);
            targetDetected = false;
            targetLocked = false;
            currentTarget = null;
            yield break;
        }

        yield break;
    }

    void BackDefoultRotation()
    {
        gun.transform.rotation = Quaternion.Lerp(gun.transform.rotation, defoultRotation, turnSpeed * updateTime);
        if (Math.Round(gun.transform.rotation.z, 2) == 0f)
        {
            gun.transform.rotation = defoultRotation;
            CancelInvoke("BackDefoultRotation");
        }
    }
}
