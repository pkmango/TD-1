using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float speed;
    public int hp; // health points
    public bool air;
    [HideInInspector]
    public int reward; // Сумма награды
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
    private Vector2 deviationVector; // Можно задать случайное отклонение от заданное траектории движения 

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
            // Узнаем значения отклонения
            float deviation = gameController.deviation;
            deviationVector = new Vector2(Random.Range(-deviation, deviation), Random.Range(-deviation, deviation));
        }

        currentHp = hp;
        healthBar = Health();

        transform.position = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        currentPosition = transform.position;
        if (!air)
        {
            NewPath();
            nextPosition = wayPoints[currentPoint];
        }
        else
        {
            currentPosition += deviationVector;
            nextPosition = new Vector2 (currentPosition.x, target.transform.position.y + deviationVector.y);
            //nextPosition += deviationVector;
        }
        
    }

    void FixedUpdate()
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
            Destroy(gameObject);
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
            progress += Time.deltaTime * speed * bashMod * freezeMod;
        }
        else
        {
            if(currentPoint == 0)
            {
                gameController.NewTower -= ChangePath;
                gameController.SubtractLife();
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

    public GameObject Health(int dmg = 0, bool freeze = false, bool bash = false)
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

        // Красная полоска
        SpriteRenderer healthBarRedSR = redBar.AddComponent<SpriteRenderer>() as SpriteRenderer;
        healthBarRedSR.sortingLayerName = "Enemy";
        healthBarRedSR.sortingOrder = 4;
        healthBarRedSR.sprite = healthBarSprite;
        healthBarRedSR.color = Color.red;
        redBar.transform.localScale = new Vector3(barLenght, 1f, 1f);
        // Левая граница необходимая для выравнивания healthBar
        float leftBounds = -redBar.GetComponent<SpriteRenderer>().bounds.extents.x;
        redBar.transform.localPosition = new Vector2(leftBounds, barPositionY);

        // Зеленая полоска
        SpriteRenderer healthBarGreenSR = greenBar.AddComponent<SpriteRenderer>() as SpriteRenderer;
        healthBarGreenSR.sortingLayerName = "Enemy";
        healthBarGreenSR.sortingOrder = 5;
        healthBarGreenSR.sprite = healthBarSprite;
        healthBarGreenSR.color = Color.green;

        currentHp -= dmg;

        if (currentHp <= 0)
        {
            if(explosion_vfx != null) Instantiate(explosion_vfx, transform.position, Quaternion.identity);
            if(gameController.rewardText != null)
            {
                GameObject rewardText = Instantiate(gameController.rewardText, transform.position, Quaternion.identity);
                rewardText.GetComponentInChildren<MeshRenderer>().sortingLayerName = "Text";
                rewardText.GetComponentInChildren<TextMesh>().text = "+" + reward.ToString();
            }
            gameController.NewTower -= ChangePath;
            gameController.currentMoney += reward;
            gameController.moneyText.text = gameController.currentMoney.ToString();
            Destroy(gameObject);
            return null;
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

        return healthBar;
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

    private void OnDestroy()
    {

        if(healthBar != null)
        {
            Destroy(healthBar);
        }
    }
}