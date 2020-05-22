using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerController : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public Transform spawnPoint;
    public LockOnTarget lockOnTarget;
    public GameObject bullet;
    public string towerName;

    // Характеристики башни
    public int level = 0;
    public int maxLevel = 4;
    public int[] costs; // Цена
    public int[] damages; // Наносимый урон
    public float[] ranges; // Радиус атаки
    public float[] fireRates; // Скоростельность (выстрелов в секунду)
    public GameObject glow; // Подсвечиваем выделенную башню
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

    void Awake()
    {
        currentCost = costs[level];
        currentDamage = damages[level];
        currentRange = ranges[level];
        currentFireRate = fireRates[level];
        turret.GetComponent<CircleCollider2D>().radius = currentRange;
        //circle.color = new Color(1f, 1f, 1f, 1f);
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
        StartCoroutine(Fire());
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
        //gameController.HideConstrZone();
        gameController.upgradeMenu.SetActive(false);
    }

    IEnumerator Upgrading(GameObject upgradingMenu)
    {
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
        level++;
        gameController.currentMoney -= costs[level];
        gameController.moneyText.text = gameController.currentMoney.ToString();

        currentCost += costs[level];
        currentDamage = damages[level];
        currentRange = ranges[level];
        turret.GetComponent<CircleCollider2D>().radius = currentRange;
        circle.gameObject.transform.localScale = new Vector2(currentRange, currentRange);
        currentFireRate = fireRates[level];
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
        gameController.rangeTextUp.text = (currentRange - 0.5f).ToString();
        gameController.fireRateTextUp.text = currentFireRate.ToString();
        if (level != maxLevel)
        {
            gameController.upgradeButton.SetActive(true);
            gameController.costTextUp.text = currentCost.ToString() + " (" + costs[level + 1].ToString() + ")";

            if (currentDamage != damages[level + 1])
            {
                gameController.damageTextUp.text += " (" + damages[level + 1].ToString() + ")";
            }

            if (currentRange != ranges[level + 1])
            {
                gameController.rangeTextUp.text += " (" + (ranges[level + 1] - 0.5).ToString() + ")";
            }

            if (currentFireRate != fireRates[level + 1])
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
        ground.Click -= ResetSelection;
        Destroy(gameObject);
    }
}
