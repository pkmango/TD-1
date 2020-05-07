using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOnTarget : MonoBehaviour
{
    public float turnSpeed;
    public float updateTime = 0.02f;

    private Vector3 direction;
    //private Transform enemy;
    private Quaternion rotation;
    private List<Transform> enemies =  new List<Transform>();
    private bool targetLocked; // Цель захвачена?
    private Quaternion defoultRotation;

    void Start()
    {
        //Debug.Log(enemies.Count);
        targetLocked = false;
        defoultRotation = transform.rotation;
    }

    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Enemy"))
        {
            enemies.Add(col.gameObject.GetComponent<Transform>());

            if (!targetLocked)
            {
                targetLocked = true;
                StartCoroutine(TargetTracking(enemies[enemies.Count - 1]));
            }
        } 
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Enemy"))
        {
            enemies.Remove(col.gameObject.GetComponent<Transform>());
        }
    }

    IEnumerator TargetTracking(Transform enemy)
    {
        CancelInvoke();
        Collider2D thisCollider = gameObject.GetComponent<Collider2D>();
        Collider2D enemyCollider = enemy.gameObject.GetComponent<Collider2D>();

        while (thisCollider.IsTouching(enemyCollider))
        {
            direction = transform.position - enemy.position;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, turnSpeed * updateTime);

            yield return new WaitForSeconds(updateTime);
        }

        if (enemies.Count != 0)
        {
            StartCoroutine(TargetTracking(enemies[enemies.Count - 1]));
        }
        else
        {
            InvokeRepeating("BackDefoultRotation", 0f, updateTime);
            targetLocked = false;
            yield break;
        }   
    }

    void BackDefoultRotation()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, defoultRotation, turnSpeed * updateTime);
        if (Math.Round(transform.rotation.z, 2) == 0f)
        {
            transform.rotation = defoultRotation;
            CancelInvoke("BackDefoultRotation");
        }
    }
}
