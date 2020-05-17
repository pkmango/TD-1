using System.Collections;
using System.Collections.Generic;
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
    public SpriteRenderer chevron;
    public Sprite[] chevrons;

    public GameObject turret;

    private GameController gameController;
    [HideInInspector]
    public int currentCost, currentDamage;
    [HideInInspector]
    public float currentRange, currentFireRate;

    void Awake()
    {
        currentCost = costs[level];
        currentDamage = damages[level];
        currentRange = ranges[level];
        currentFireRate = fireRates[level];
        turret.GetComponent<CircleCollider2D>().radius = currentRange;
    }

    void Start()
    {
        GameObject gameControllerObject = GameObject.FindWithTag("GameController");
        if (gameControllerObject != null)
        {
            gameController = gameControllerObject.GetComponent<GameController>();
        }

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

    public void Upgrade()
    {
        currentCost += costs[level];
        currentDamage = damages[level];
        currentRange = ranges[level];
        currentFireRate = fireRates[level];
        // В массиве chevrones на 1 меньше членов, т.к. нулевое звание не отображается
        chevron.sprite = level != 0 ? chevrons[level - 1] : null;
        UpdateUpgradeMenu();
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

            if (currentDamage != damages[level + 1])
            {
                gameController.damageTextUp.text += " (" + damages[level + 1].ToString() + ")";
            }

            if (currentRange != ranges[level + 1])
            {
                gameController.rangeTextUp.text += " (" + ranges[level + 1].ToString() + ")";
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
        gameController.pressedTower = this;
        gameController.HideConstrZone();
        gameController.upgradeMenu.SetActive(true);
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
}
