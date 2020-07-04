using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float speed;
    public int hp; // health points
    public GameObject passengers; // Враги которые дополнительно спавнятся после уничтожения 
    public bool passenger; // Это пассажир?
    public bool air;
    [HideInInspector]
    public int reward = 1; // Сумма награды
    [HideInInspector]
    public GameObject healthBar;
    public float barLenght = 15f; // Длина полоски healthBar
    public float barPositionY = 0.33f; // Позиция по Y
    public GameObject explosion_vfx;
    public GameObject freezeEffect;
    [SerializeField, Range(0, 1)]
    public float freezeSpeed = 0.7f; // Процент от нормальной скорости при заморозке (от 0 до 1)
    private float freezeMod = 1f; // Модификатор для скорости, при получении фриза становится = freezeSpeed
    private Coroutine freezeCor;
    public GameObject bashEffect;
    [SerializeField, Range(0, 1)]
    public float bashSpeed = 0.1f; // Процент от нормальной скорости при оглушении (от 0 до 1)
    private float bashMod = 1f; // Модификатор для скорости, при получении баша становится = bashSpeed
    private Coroutine bashCor;
    public float debuffTime = 2f; // Длительность дебафа скорости

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
    [HideInInspector]
    public Vector2 deviationVector; // Можно задать случайное отклонение от заданное траектории движения 

    //private GameObject bar;
    private GameObject greenBar;
    private GameObject redBar;
    private float leftBounds;

    void Start()
    {
        target = GameObject.FindWithTag("Target");
        GameObject gameControllerObject = GameObject.FindWithTag("GameController");
        if (gameControllerObject != null)
        {
            gameController = gameControllerObject.GetComponent<GameController>();
            if(!air) gameController.NewTower += ChangePath;

            // Узнаем значение награды и hp в настройках текущей волны
            Wave currentWave = gameController.waves[gameController.currentWave];
            reward = currentWave.reward;
            hp = currentWave.hp;
            if (passenger) hp /= 2;

            // Узнаем значения отклонения
            if (deviationVector == Vector2.zero)
            {
                float deviation = gameController.deviation;
                deviationVector = new Vector2(Random.Range(-deviation, deviation), Random.Range(-deviation, deviation));
            }
            
        }

        currentHp = hp;
        healthBar = HealthBar();

        //transform.position = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        currentPosition = transform.position;
        if (!air)
        {
            NewPath();
            nextPosition = wayPoints[currentPoint];
            ChangeRotation();
        }
        else
        {
            currentPosition += deviationVector;
            nextPosition = new Vector2 (currentPosition.x, target.transform.position.y + deviationVector.y);
            //nextPosition += deviationVector;
        }
        
    }

    void Update()
    {
        healthBar.transform.position = transform.position;

        if (!air)
        {
            Movement();
        }
        else
        {
            transform.rotation = Quaternion.AngleAxis(-90f, Vector3.forward);
            MovementForAir();
        }
    }

    void MovementForAir()
    {
        transform.position = Vector2.Lerp(currentPosition, nextPosition, progress);

        if (progress < 1f)
        {
            Vector2 distance = nextPosition - currentPosition;
            progress += 1 / (distance.magnitude / (speed * Time.deltaTime)) * freezeMod;
        }
        else
        {
            gameController.SubtractLife();
            DestroyObject();
        }
    }

    void NewPath()
    {
        wayPoints = gameObject.GetComponent<PathFinder>().GetPath(currentPosition, target.transform.position);

        // Применяем значение отклонения
        for (int i = 0; i < wayPoints.Count; i++)
        {
            wayPoints[i] += deviationVector;
        }

        if (wayPoints.Count != 0)
        {
            currentPoint = wayPoints.Count - 1;
        }
        else
        {
            Debug.Log("Блокировка пути!");
            //gameController.NewTower -= ChangePath;
            DestroyObject();
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
            Vector2 distance = nextPosition - currentPosition;
            progress += 1 / (distance.magnitude / (speed * Time.deltaTime)) * freezeMod * bashMod;
        }
        else
        {
            if(currentPoint == 0)
            {
                //gameController.NewTower -= ChangePath;
                gameController.SubtractLife();
                DestroyObject();
                return;
            }
            progress -= 1f;
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
            progress += Time.deltaTime * speed;
        }
        
    }

    void ChangePath(bool sell)
    {
        changePath = true;
    }

    void ChangeRotation()
    {
        direction = wayPoints[currentPoint] - currentPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private GameObject HealthBar()
    {
        healthBar = new GameObject("healthBar");
        greenBar = new GameObject("greenBar");
        redBar = new GameObject("redBar");

        healthBar.transform.position = transform.position;
        greenBar.transform.parent = healthBar.transform;
        redBar.transform.parent = healthBar.transform;
        Sprite healthBarSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 4f, 4f), new Vector2(0f, 0.5f));

        // Красная полоска
        SpriteRenderer healthBarRedSR = redBar.AddComponent<SpriteRenderer>() as SpriteRenderer;
        healthBarRedSR.sortingLayerName = "Enemy";
        healthBarRedSR.sortingOrder = 4;
        healthBarRedSR.sprite = healthBarSprite;
        healthBarRedSR.color = Color.red;
        redBar.transform.localScale = new Vector3(barLenght, 1f, 1f);
        // Левая граница необходимая для выравнивания healthBar
        leftBounds = -redBar.GetComponent<SpriteRenderer>().bounds.extents.x;
        redBar.transform.localPosition = new Vector2(leftBounds, barPositionY);

        // Зеленая полоска
        SpriteRenderer healthBarGreenSR = greenBar.AddComponent<SpriteRenderer>() as SpriteRenderer;
        healthBarGreenSR.sortingLayerName = "Enemy";
        healthBarGreenSR.sortingOrder = 5;
        healthBarGreenSR.sprite = healthBarSprite;
        healthBarGreenSR.color = Color.green;
        greenBar.transform.localScale = new Vector3(barLenght, 1f, 1f);
        greenBar.transform.localPosition = new Vector2(leftBounds, barPositionY);

        return healthBar;
    }

    public void Health(int dmg = 0, bool freeze = false, bool bash = false)
    {
        if (currentHp <= 0)
            return;

        currentHp -= dmg - Mathf.RoundToInt(dmg * gameController.armor);

        if (currentHp <= 0)
        {
            if(explosion_vfx != null) Instantiate(explosion_vfx, transform.position, Quaternion.identity);
            if(gameController.rewardText != null && !passenger)
            {
                GameObject rewardText = Instantiate(gameController.rewardText, transform.position, Quaternion.identity);
                rewardText.GetComponentInChildren<MeshRenderer>().sortingLayerName = "Text";
                rewardText.GetComponentInChildren<TextMesh>().text = "+" + reward.ToString();
                gameController.ChangeScore(reward);
                gameController.ChangeMoney(reward);
            }
            
            DestroyObject();
            return;
        }

        if (freeze)
        {
            if (freezeCor != null) StopCoroutine(freezeCor);
            freezeCor = StartCoroutine(Freeze());
        }

        if (bash)
        {
            if (bashCor != null) StopCoroutine(bashCor);
            bashCor = StartCoroutine(Bash());
        }

        float newGreenBarLenght = barLenght * currentHp / hp; // Вычисляем новую длину полоски здоровья при получении урона
        greenBar.transform.localScale = new Vector3(newGreenBarLenght, 1f, 1f);
        greenBar.transform.localPosition = new Vector2(leftBounds, barPositionY);

        //return healthBar;
    }

    private IEnumerator Freeze()
    {
        if(freezeEffect != null) freezeEffect.SetActive(true);
        freezeMod = freezeSpeed;
        yield return new WaitForSeconds(debuffTime);
        if (freezeEffect != null) freezeEffect.SetActive(false);
        freezeMod = 1f;
    }

    private IEnumerator Bash()
    {
        if (bashEffect != null) bashEffect.SetActive(true);
        bashMod = bashSpeed;
        yield return new WaitForSeconds(debuffTime);
        if (bashEffect != null) bashEffect.SetActive(false);
        bashMod = 1f;
    }

    public void DestroyObject(bool restart = false)
    {
        if(!air) gameController.NewTower -= ChangePath;

        if (passengers != null && currentPoint > 0 && !restart)
        {
            for(int i = 0; i < 3; i++)
            {
                Vector3 randomPosition = new Vector3(Random.Range(-0.32f, 0.32f), Random.Range(-0.32f, 0.32f), 0f);
                GameObject newEnemy = Instantiate(passengers, transform.position + randomPosition, transform.rotation);
                newEnemy.GetComponent<EnemyController>().deviationVector = randomPosition;
            }
        }

        StopAllCoroutines();
        Destroy(gameObject);
    }
    private void OnDestroy()
    {
        if (healthBar != null)
        {
            Destroy(healthBar);
        }
    }
}