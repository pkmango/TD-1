using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerController : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public Transform spawnPoint;
    public LockOnTarget lockOnTarget;
    public GameObject bullet;
    public bool boost; //Башня типа boost?
    public bool earthquake; // Башня типа earthquake?
    public GameObject quakeEffect;
    public float quakeDuration = 0.1f;
    public string towerName;

    // Характеристики башни
    public int level = 0;
    public int maxLevel = 4;
    public int[] costs; // Цена
    public int[] damages; // Наносимый урон
    public float[] ranges; // Радиус атаки
    public float[] fireRates; // Скоростельность (выстрелов в секунду)
    public GameObject glow; // Подсвечиваем выделенную башню
    public GameObject buffGlow; // Подсвечиваем башню с бафом
    public SpriteRenderer circle; // Подсвечиваем радиус атаки
    public SpriteRenderer chevron;
    public Sprite[] chevrons;
    public RectTransform progress;

    public GameObject turret;
    [HideInInspector]
    public bool upgrading = false; // Если башня находится в процессе апгрейда, то true
    [HideInInspector]
    public bool active = false; // Башня выделена?

    private float upgradePercent = 100; // Процент апгрейда (прогресс)
    [HideInInspector]
    public RectTransform upgradeProgress;
    private GameController gameController;
    [HideInInspector]
    public int currentCost, currentDamage;
    [HideInInspector]
    public float currentRange, currentFireRate;
    private TilemapController ground;
    public float buff; // модификатор характеристик (процент/100)

    void Awake()
    {
        currentCost = costs[level];
        currentDamage = damages[level];
        currentRange = ranges[level];
        currentFireRate = fireRates[level];
        turret.GetComponent<CircleCollider2D>().radius = currentRange;
        circle.gameObject.transform.localScale = new Vector2(currentRange, currentRange);
    }

    void Start()
    {
        GameObject gameControllerObject = GameObject.FindWithTag("GameController");
        if (gameControllerObject != null)
        {
            gameController = gameControllerObject.GetComponent<GameController>();
        }
        GameObject groundObject = GameObject.FindWithTag("Ground");
        if (groundObject != null)
        {
            ground = groundObject.GetComponent<TilemapController>();
            ground.Click += ResetSelection;
        }

        GameObject newProgress = Instantiate(progress.gameObject, gameController.upgradingMenu.gameObject.transform);
        upgradeProgress = newProgress.GetComponent<RectTransform>();

        Boost();

        if (earthquake)
        {
            StartCoroutine(FireEarthquake());
        }
        else if (boost)
        {
            SetBoost(0.01f * currentDamage);
            gameController.NewTower += NewTowerBoost;
        }
        else
        {
            StartCoroutine(Fire());
        }
    }

    private void FixedUpdate()
    {
        if (buffGlow != null)
        {
            if(buff > 0f)
            {
                buffGlow.SetActive(true);
            }
            else
            {
                buffGlow.SetActive(false);
            }
        }
    }

    IEnumerator Fire()
    {
        while (true)
        {
            if (lockOnTarget.targetLocked)
            {
                GameObject newBullet = Instantiate(bullet, spawnPoint.position, spawnPoint.rotation);
                newBullet.GetComponent<BulletController>().targetPosition = lockOnTarget.currentTarget;
                newBullet.GetComponent<BulletController>().damage = currentDamage;
            }

            yield return new WaitForSeconds(1f / currentFireRate);
        }
        
    }

    IEnumerator FireEarthquake()
    {
        float quakeWait; // Задержка для анимации эффекта срабатывания башни

        while (true)
        {
            Collider2D[] splashedEnemies = Physics2D.OverlapCircleAll(transform.position, currentRange, LayerMask.GetMask("Enemy"));

            if (splashedEnemies.Length > 0)
            {
                quakeWait = quakeDuration;

                foreach (Collider2D i in splashedEnemies)
                {
                    // Вероятность оглушения 10%
                    if (Random.value < 0.1f)
                    {
                        i.GetComponent<EnemyController>().Health(currentDamage, false, true);
                    }
                    else
                    {
                        i.GetComponent<EnemyController>().Health(currentDamage);
                    }
                    
                }
                quakeEffect.SetActive(true);
                yield return new WaitForSeconds(quakeWait);
                quakeEffect.SetActive(false);
            }
            else
            {
                quakeWait = 0f;
            }
            
            yield return new WaitForSeconds(1f / currentFireRate - quakeWait);
        }
    }

    public void Boost()
    {
        currentDamage = damages[level] + (int)(damages[level] * buff);
        currentRange = ranges[level];
        currentFireRate = fireRates[level];
        turret.GetComponent<CircleCollider2D>().radius = currentRange;
        circle.gameObject.transform.localScale = new Vector2(currentRange, currentRange);
    }

    private void SetBoost(float newBuff)
    {
        Collider2D[] neighboringTowers = Physics2D.OverlapCircleAll(transform.position, currentRange, LayerMask.GetMask("Tower"));
        foreach (Collider2D i in neighboringTowers)
        {
            TowerController tower = i.GetComponent<TowerController>();
            if(tower != null && !tower.boost)
            {
                tower.buff += newBuff;
                tower.Boost();
            }
        }
    }

    private void NewTowerBoost(bool sell)
    {
        if (!sell)
        {
            TowerController tower = gameController.lastTower.GetComponent<TowerController>();
            Collider2D thisCol = turret.GetComponent<Collider2D>();
            if (thisCol.OverlapPoint(tower.transform.position))
            {
                Debug.Log(tower);
                tower.buff += 0.01f * currentDamage;
                tower.Boost();
            }
        }
    }

    public void Upgrade(GameObject upgradingMenu)
    {
        upgrading = true;
        turret.GetComponent<CircleCollider2D>().radius = 0.1f;
        upgradingMenu.SetActive(true);

        GameObject[] progressObjects = GameObject.FindGameObjectsWithTag("Progress");
        foreach (GameObject i in progressObjects)
        {
            i.SetActive(false);
        }
        upgradeProgress.gameObject.SetActive(true);

        StartCoroutine(Upgrading(upgradingMenu));
    }

    public void ResetSelection()
    {
        active = false;
        glow.SetActive(false);
        circle.color = new Color(1f, 1f, 1f, 0f);
        gameController.upgradeMenu.SetActive(false);
    }

    IEnumerator Upgrading(GameObject upgradingMenu)
    {
        // Анимируем прогресс-бар апгрейда в меню
        float progressWidth = upgradeProgress.sizeDelta.x;
        for (int i = 1; i <= 100; i++)
        {
            upgradePercent = i;
            if (upgradeProgress == null)
            {
                yield break;
            }
            upgradeProgress.sizeDelta = new Vector2(progressWidth * 0.01f * i, upgradeProgress.sizeDelta.y);
            yield return new WaitForSeconds((level + 1) * 0.01f);
        }

        upgrading = false;
        upgradingMenu.SetActive(false);
        if (boost) SetBoost(0.01f * (damages[level + 1] - currentDamage)); // Увеличиваем баф соседних башен на разницу значений
        level++;
        gameController.currentMoney -= costs[level];
        gameController.moneyText.text = gameController.currentMoney.ToString();

        currentCost += costs[level];
        Boost(); // Увеличиваем боевые параметры с учетом бафа
        // В массиве chevrones на 1 меньше членов, т.к. нулевое звание не отображается
        chevron.sprite = level != 0 ? chevrons[level - 1] : null;
        if (active)
        {
            UpdateUpgradeMenu();
        }
    }

    private void UpdateUpgradeMenu()
    {
        gameController.damageTextUp.text = currentDamage.ToString();
        gameController.rangeTextUp.text = currentRange.ToString();
        gameController.fireRateTextUp.text = currentFireRate.ToString();
        if (level != maxLevel)
        {
            gameController.upgradeButton.SetActive(true);
            gameController.costTextUp.text = currentCost.ToString() + " (" + costs[level + 1].ToString() + ")";

            if (damages[level] != damages[level + 1])
            {
                gameController.damageTextUp.text += " (" + (damages[level + 1] + (int)(damages[level + 1] * buff)).ToString() + ")"; ;
            }

            if (ranges[level] != ranges[level + 1])
            {
                gameController.rangeTextUp.text += " (" + ranges[level + 1].ToString() + ")";
            }

            if (fireRates[level] != fireRates[level + 1])
            {
                gameController.fireRateTextUp.text += " (" + fireRates[level + 1].ToString() + ")";
            }
        }
        else
        {
            gameController.costTextUp.text = currentCost.ToString();
            gameController.upgradeButton.SetActive(false);
        }

        gameController.sellText.text = "Sell $" + (currentCost / 2).ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Нажатие на башню");

        GameObject[] towerButtons = GameObject.FindGameObjectsWithTag("TowerButton");
        foreach (GameObject button in towerButtons)
        {
            button.GetComponent<TowerButton>().SetTransparent();
        }

        GameObject[] allTowers = GameObject.FindGameObjectsWithTag("Tower");
        foreach(GameObject i in allTowers)
        {
            i.GetComponent<TowerController>().active = false;
            i.GetComponent<TowerController>().glow.SetActive(false);
            i.GetComponent<TowerController>().circle.color = new Color(1f, 1f, 1f, 0f);
        }
        active = true;
        glow.SetActive(true);
        circle.color = new Color(1f, 1f, 1f, 1f);

        gameController.pressedTower = this;
        gameController.HideConstrZone();
        gameController.upgradeMenu.SetActive(true);
        if (upgrading)
        {
            gameController.upgradingMenu.SetActive(true);

            GameObject[] progressObjects = GameObject.FindGameObjectsWithTag("Progress");
            foreach (GameObject i in progressObjects)
            {
                i.SetActive(false);
            }
            upgradeProgress.gameObject.SetActive(true);
        }
        else
        {
            gameController.upgradingMenu.SetActive(false);
        }
        UpdateUpgradeMenu();
        gameController.towerNameTextUp.text = towerName;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

    public void DestroyThisTower()
    {
        if (boost)
        {
            gameController.NewTower -= NewTowerBoost;
            SetBoost(-0.01f * currentDamage);
            
        }
            
        ground.Click -= ResetSelection;
        Destroy(gameObject);
    }
}
