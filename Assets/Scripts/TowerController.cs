using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TowerController : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public Transform[] spawnPoints;
    public LockOnTarget lockOnTarget;
    public GameObject bullet;
    public bool boost; //Башня типа boost?
    public bool earthquake; // Башня типа earthquake?
    public GameObject quakeEffect;
    public float quakeDuration = 0.1f;
    public AudioSource quakeSound;
    public string towerName;
    public string description;
    public Sprite icon;
    public GameObject darkImgForUp; // Заглушка затеняет башню во время апгрейда
    public Transform upProgress; // Полоска прогресса, которая отображается на башне

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
    private AudioController audioController;
    private Coroutine fireCor, fireEarthquakeCor; // Корутины для выстрелов
    public float buff; // модификатор характеристик (процент/100)

    public bool shot; // для теста

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
            gameController.NewCurrentMoney += SetUpgradeButton;
        }
        GameObject groundObject = GameObject.FindWithTag("Ground");
        if (groundObject != null)
        {
            ground = groundObject.GetComponent<TilemapController>();
            ground.Click += ResetSelection;
        }
        GameObject audioControllerObject = GameObject.FindWithTag("AudioController");
        if(audioControllerObject != null)
        {
            audioController = audioControllerObject.GetComponent<AudioController>();
        }

        GameObject newProgress = Instantiate(progress.gameObject, gameController.upgradingMenu.gameObject.transform);
        upgradeProgress = newProgress.GetComponent<RectTransform>();

        Boost();

        if (earthquake)
        {
            fireEarthquakeCor = StartCoroutine(FireEarthquake());
        }
        else if (boost)
        {
            SetBoost(0.01f * currentDamage);
            gameController.NewTower += NewTowerBoost;
        }
        else
        {
            fireCor = StartCoroutine(Fire());
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
                foreach(Transform i in spawnPoints)
                {
                    GameObject newBullet = Instantiate(bullet, i.position, i.rotation);
                    audioController.PlaySound(GetComponent<AudioSource>());
                    newBullet.GetComponent<BulletController>().targetPosition = lockOnTarget.currentTarget;
                    newBullet.GetComponent<BulletController>().damage = currentDamage;
                    if (spawnPoints.Length > 1) yield return new WaitForSeconds(0.19f);
                    if (!lockOnTarget.targetLocked) break;
                }
                
            }
            shot = !shot;
            yield return new WaitForSeconds(1f / currentFireRate);
        }

        Debug.Log("все-таки это случилось");
    }

    IEnumerator FireEarthquake()
    {
        float quakeWait; // Задержка для анимации эффекта срабатывания башни

        while (true)
        {
            Collider2D[] splashedEnemies = Physics2D.OverlapCircleAll(transform.position, currentRange, LayerMask.GetMask("Enemy"));
            
            if (splashedEnemies.Length > 0)
            {
                bool notOnlyAir = false;

                // Проверяем есть ли хоть один наземный враг
                foreach (Collider2D i in splashedEnemies)
                {
                    if (i.CompareTag("Enemy"))
                    {
                        notOnlyAir = true;
                        break;
                    }
                }

                // Если есть хоть один наземный враг, то мы включаем башню
                if (notOnlyAir)
                {
                    quakeWait = quakeDuration;

                    foreach (Collider2D i in splashedEnemies)
                    {
                        if (i.tag == "AirEnemy") continue;

                        // Вероятность оглушения 17.5%
                        if (Random.value < 0.175f)
                        {
                            i.GetComponent<EnemyController>().Health(currentDamage, false, true);
                        }
                        else
                        {
                            i.GetComponent<EnemyController>().Health(currentDamage);
                        }

                    }
                    quakeEffect.SetActive(true);
                    quakeSound.Play();
                    yield return new WaitForSeconds(quakeWait);
                    quakeEffect.SetActive(false);
                }
                else
                {
                    quakeWait = 0f;
                }
                yield return new WaitForSeconds(1f / currentFireRate - quakeWait);
                continue;
            }
            
            yield return new WaitForSeconds(1f / currentFireRate);
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
                //Debug.Log("Башня " + tower + " забафана");
            }
        }
    }

    private void NewTowerBoost(bool sell)
    {
        if (!sell)
        {
            TowerController tower = gameController.lastTower.GetComponent<TowerController>();
            Collider2D thisCol = turret.GetComponent<Collider2D>();
            if (thisCol.OverlapPoint(tower.transform.position) && !tower.boost)
            {
                tower.buff += 0.01f * currentDamage;
                tower.Boost();
            }
        }
    }

    public void Upgrade(GameObject upgradingMenu)
    {
        upgrading = true;
        if (earthquake) StopCoroutine(fireEarthquakeCor);
        turret.GetComponent<CircleCollider2D>().radius = 0.1f;
        upgradingMenu.SetActive(true);

        GameObject[] progressObjects = GameObject.FindGameObjectsWithTag("Progress");
        foreach (GameObject i in progressObjects)
        {
            i.SetActive(false);
        }
        upgradeProgress.gameObject.SetActive(true);
        darkImgForUp.SetActive(true);

        StartCoroutine(Upgrading(upgradingMenu));
    }

    public void ResetSelection()
    {
        active = false;
        glow.SetActive(false);
        circle.color = new Color(1f, 1f, 1f, 0f);
        gameController.upgradeMenu.SetActive(false);
    }

    private void SetUpgradeButton()
    {
        if (active && level != maxLevel)
        {
            if (gameController.currentMoney >= costs[level + 1])
            {
                gameController.upgradeButton.GetComponent<Button>().interactable = true;
            }
            else
            {
                gameController.upgradeButton.GetComponent<Button>().interactable = false;
            }
        }
    }

    IEnumerator Upgrading(GameObject upgradingMenu)
    {
        // Анимируем прогресс-бар апгрейда в меню
        float progressWidth = upgradeProgress.sizeDelta.x;
        float upProgressX = upProgress.localScale.x;
        for (int i = 1; i <= 100; i++)
        {
            upgradePercent = i;
            if (upgradeProgress == null)
            {
                yield break;
            }
            upgradeProgress.sizeDelta = new Vector2(progressWidth * 0.01f * i, upgradeProgress.sizeDelta.y);
            upProgress.localScale = new Vector2(upProgressX * 0.01f * i, upProgress.localScale.y);
            yield return new WaitForSeconds((level + 1) * 0.01f);
        }

        upgrading = false;
        if (earthquake) fireEarthquakeCor = StartCoroutine(FireEarthquake());
        darkImgForUp.SetActive(false);
        upgradingMenu.SetActive(false);

        level++;
        if (boost) SetBoost(0.01f * (damages[level] - currentDamage));

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
        if (boost) gameController.damageTextUp.text += "%";
        gameController.rangeTextUp.text = currentRange.ToString();
        gameController.fireRateTextUp.text = currentFireRate.ToString();
        gameController.costTextUp.text = currentCost.ToString();
        if (level != maxLevel)
        {
            gameController.upgradeButton.SetActive(true);
            SetUpgradeButton();
            gameController.costTextUpPlus.text = "+" + costs[level + 1].ToString();

            if (damages[level] != damages[level + 1])
            {
                if (boost)
                {
                    gameController.damageTextPlus.text = (damages[level + 1] + (int)(damages[level + 1] * buff)).ToString() + "%";
                }
                else
                {
                    gameController.damageTextPlus.text = (damages[level + 1] + (int)(damages[level + 1] * buff)).ToString();
                }
            }
            else
            {
                gameController.damageTextPlus.text = "";
            }

            if (ranges[level] != ranges[level + 1])
            {
                gameController.rangeTextPlus.text = ranges[level + 1].ToString();
            }
            else
            {
                gameController.rangeTextPlus.text = "";
            }

            if (fireRates[level] != fireRates[level + 1])
            {
                gameController.fireRateTextPlus.text = fireRates[level + 1].ToString();
            }
            else
            {
                gameController.fireRateTextPlus.text = "";
            }
        }
        else
        {
            gameController.costTextUpPlus.text = "";
            gameController.damageTextPlus.text = "";
            gameController.rangeTextPlus.text = "";
            gameController.fireRateTextPlus.text = "";
            gameController.upgradeButton.SetActive(false);
        }

        gameController.sellText.text = "Sell $" + (currentCost / 2).ToString();
        SetBuffCanvas();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("Нажатие на башню");

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
        gameController.descriptionText.text = description;
        gameController.towerIcon.sprite = icon;
    }

    private void SetBuffCanvas()
    {
        if (buff > 0)
        {
            gameController.buffCanvas.SetActive(true);
            gameController.buffValue.text = Mathf.Round(buff * 100f) + "%";
        }
        else
        {
            gameController.buffCanvas.SetActive(false);
        }
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
        StopAllCoroutines();

        if (boost)
        {
            gameController.NewTower -= NewTowerBoost;
            SetBoost(-0.01f * currentDamage);
            
        }
            
        ground.Click -= ResetSelection;
        gameController.NewCurrentMoney -= SetUpgradeButton;
        Destroy(upgradeProgress.gameObject);
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        Debug.Log(gameObject.name + " отключена");
    }

    private void OnEnable()
    {
        Debug.Log(gameObject.name + " включена");
    }
}
