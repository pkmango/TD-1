using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyController : MonoBehaviour
{
    public float speed;
    public int hp; // health points
    public GameObject healthBar;
    public float barLenght = 15f; // Длина полоски healthBar
    public GameObject explosion_vfx;

    private int currentHp;
    private Vector2 currentPosition;
    private float progress = 0f;
    private int currentPoint;
    private Vector2 direction;
    private List<Vector2> wayPoints;
    private GameController gameController;
    private Vector2 nextPosition = Vector2.zero;
    private GameObject target;
    private bool changePath = false; // Нужна смена пути


    void Start()
    {
        currentHp = hp;
        healthBar = Health();

        target = GameObject.FindWithTag("Target");
        GameObject gameControllerObject = GameObject.FindWithTag("GameController");
        if (gameControllerObject != null)
        {
            gameController = gameControllerObject.GetComponent<GameController>();
            gameController.NewTower += ChangePath;
        }

        transform.position = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        currentPosition = transform.position;
        NewPath();
        nextPosition = wayPoints[currentPoint];
    }

    void Update()
    {
        healthBar.transform.position = transform.position;
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
            gameController.NewTower -= ChangePath;
            Destroy(gameObject);
        }
        //Debug.Log("Событие!");
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
                gameController.NewTower -= ChangePath;
                gameController.currentLives--;
                gameController.livesText.text = gameController.currentLives.ToString();
                if (gameController.currentLives <= 0)
                {
                    gameController.GameOver();
                }
                Destroy(gameObject);
                return;
            }
            progress = 0f;
            currentPosition = nextPosition;
            if (changePath)
            {
                NewPath();
                changePath = false;
            }
            else
            {
                currentPoint--;
            }
            
            nextPosition = wayPoints[currentPoint];
            ChangeRotation();
        }
        
    }

    void ChangePath()
    {
        changePath = true;
    }

    void ChangeRotation()
    {
        direction = wayPoints[currentPoint] - currentPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public GameObject Health(int dmg = 0)
    {
        if (dmg != 0)
        {
            Destroy(healthBar);
        }
        healthBar = new GameObject("healthBar");
        healthBar.transform.position = transform.position;
        GameObject greenBar = new GameObject("greenBar");
        GameObject redBar = new GameObject("redBar");
        greenBar.transform.parent = healthBar.transform;
        redBar.transform.parent = healthBar.transform;
        Sprite healthBarSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 4f, 4f), new Vector2(0f, 0.5f));
        float leftBounds = -gameObject.GetComponent<SpriteRenderer>().bounds.extents.x; // Левая граница необходимая для выравнивания healthBar по левому краю
        // Красная полоска
        SpriteRenderer healthBarRedSR = redBar.AddComponent<SpriteRenderer>() as SpriteRenderer;
        healthBarRedSR.sortingOrder = 1;
        healthBarRedSR.sprite = healthBarSprite;
        healthBarRedSR.color = Color.red;
        redBar.transform.localScale = new Vector3(barLenght, 1f, 1f);
        redBar.transform.localPosition = new Vector2(leftBounds, 0.33f);
        // Зеленая полоска
        SpriteRenderer healthBarGreenSR = greenBar.AddComponent<SpriteRenderer>() as SpriteRenderer;
        healthBarGreenSR.sortingOrder = 2;
        healthBarGreenSR.sprite = healthBarSprite;
        healthBarGreenSR.color = Color.green;
        currentHp -= dmg;
        if (currentHp <= 0)
        {
            Instantiate(explosion_vfx, transform.position, Quaternion.identity);
            gameController.NewTower -= ChangePath;
            Destroy(gameObject);
            return null;
        }
        float newGreenBarLenght = barLenght * currentHp / hp; // Вычисляем новую длину полоски здоровья при получении урона
        greenBar.transform.localScale = new Vector3(newGreenBarLenght, 1f, 1f);
        greenBar.transform.localPosition = new Vector2(leftBounds, 0.33f);

        return healthBar;
    }

    private void OnDestroy()
    {

        if(healthBar != null)
        {
            Destroy(healthBar);
        }
    }
}