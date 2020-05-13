using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyController : MonoBehaviour
{
    //public List<Vector2> wayPoints;
    public Tilemap ground;
    public GameObject target;
    public float speed;

    private Vector2 currentPosition;
    private float progress = 0f;
    private int currentPoint;
    private Vector2 direction;
    private List<Vector2> wayPoints;
    private GameController gameController;
    //private int nextPoint = 0;
    private Vector2 nextPosition = Vector2.zero;

    void Start()
    {
        GameObject gameControllerObject = GameObject.FindWithTag("GameController");
        if (gameControllerObject != null)
        {
            gameController = gameControllerObject.GetComponent<GameController>();
            gameController.NewTower += NewPath;
        }

        transform.position = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        currentPosition = transform.position;
        NewPath();
        nextPosition = wayPoints[currentPoint];
    }

    void Update()
    {
        Movement();
    }

    void NewPath()
    {
        wayPoints = gameObject.GetComponent<PathFinder>().GetPath(currentPosition, target.transform.position);
        if (wayPoints.Count != 0)
        {
            currentPoint = wayPoints.Count - 1;
        }
        else
        {
            Debug.Log("Блокировка пути!");
            gameController.NewTower -= NewPath;
            Destroy(gameObject);
        }
        Debug.Log("Событие!");
    }

    void Movement()
    {
        if (wayPoints != null)
        {
            transform.position = Vector2.Lerp(currentPosition, nextPosition, progress);
        }
        

        if(progress < 1f)
        {
            progress += Time.deltaTime * speed;
        }
        else
        {
            if(currentPoint == 0)
            {
                gameController.NewTower -= NewPath;
                Destroy(gameObject);
                return;
            }
            progress = 0f;
            currentPosition = nextPosition;
            currentPoint--;
            nextPosition = wayPoints[currentPoint];
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