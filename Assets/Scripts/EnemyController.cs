using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public List<Vector2> wayPoints;
    public float speed;

    private Vector2 currentPosition;
    private float progress = 0f;
    private int currentPoint = 0;
    private Vector2 direction;


    void Start()
    {
        currentPosition = transform.position;
        //Debug.Log(mylist);
    }

    void Update()
    {
        Movement();
    }

    void Movement()
    {
        if (wayPoints != null)
        {
            transform.position = Vector2.Lerp(currentPosition, wayPoints[currentPoint], progress);
        }
        

        if(progress < 1f)
        {
            progress += Time.deltaTime * speed;
        }
        else
        {
            if((wayPoints.Count - 1) == currentPoint)
            {
                Destroy(gameObject);
                return;
            }
            progress = 0f;
            currentPosition = wayPoints[currentPoint];
            currentPoint++;
            ChangeRotation();
        }
        
    }

    void ChangeRotation()
    {
        direction = wayPoints[currentPoint] - currentPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

}