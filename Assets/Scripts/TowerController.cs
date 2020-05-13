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
    public int cost; // Цена
    public float damage;
    public float range;
    public float fireRate; // Скоростельность (выстрелов в секунду)

    private GameController gameController;
    public int currentCost;

    void Start()
    {
        GameObject gameControllerObject = GameObject.FindWithTag("GameController");
        if (gameControllerObject != null)
        {
            gameController = gameControllerObject.GetComponent<GameController>();
        }

        StartCoroutine(Fire());
        currentCost = cost;
    }

    IEnumerator Fire()
    {
        //yield return new WaitForSeconds(fireRate); // Задержка чтобы не было двойных пулек

        while (true)
        {
            if (lockOnTarget.targetLocked)
            {
                GameObject newBullet = Instantiate(bullet, spawnPoint.position, spawnPoint.rotation);
                newBullet.GetComponent<BulletController>().targetPosition = lockOnTarget.currentTarget;
            }
            

            yield return new WaitForSeconds(1f / fireRate);
        }
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Нажатие на башню");
        gameController.pressedTower = gameObject;
        gameController.HideConstrZone();
        gameController.upgradeMenu.SetActive(true);
        gameController.costTextUp.text = cost.ToString();
        gameController.damageTextUp.text = damage.ToString();
        gameController.rangeTextUp.text = range.ToString();
        gameController.fireRateTextUp.text = fireRate.ToString();
        gameController.towerNameTextUp.text = towerName;
        gameController.sellText.text = "Sell $" + (currentCost / 2).ToString();
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
