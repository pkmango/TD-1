﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ZoneMarkerController : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public float checkTime = 0.02f; // Время между проверками на занятость участка
    public LayerMask enemyLayer;

    private GameController gameController;
    private GameObject tower;
    private GameObject spawn;
    private GameObject target;
    [HideInInspector]
    public Text blocking; // Мерцающее сообщение "Blocking"
    private IEnumerator enemiesCheck;
    private int count; // Счетчик для BlockingTwinkle
    private List<Vector2> wayPoints;
    private bool constrAlowed; // Если true - постройка разрешена
    private Vector3 cameraPos; // Фиксируем положение камеры

    void Start()
    {
        cameraPos = Camera.main.transform.position;
        constrAlowed = true;

        GameObject gameControllerObject = GameObject.FindWithTag("GameController");
        if (gameControllerObject != null)
        {
            gameController = gameControllerObject.GetComponent<GameController>();
        }

        spawn = GameObject.FindWithTag("Spawn");
        target = GameObject.FindWithTag("Target");
        blocking = GameObject.FindWithTag("BlockingText").GetComponent<Text>();

        enemiesCheck = EnemiesCheck();
        StartCoroutine(enemiesCheck);
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        tower = gameController.selectedTower.gameObject;

        if (constrAlowed && tower.GetComponent<TowerController>().costs[0] <= gameController.currentMoney && Camera.main.transform.position == cameraPos)
        {
            GameObject newTower = Instantiate(tower, transform.position, tower.transform.rotation);
            // Проверяем блокировку от старта до финиша
            wayPoints = gameObject.GetComponent<PathFinder>().GetPath(spawn.transform.position, target.transform.position);
            if(wayPoints.Count == 0)
            {
                Blocking(newTower);
                return;
            }
            else
            {
                if (TrapChecking(newTower))
                {
                    Blocking(newTower);
                    return;
                }
                gameController.ChangeMoney(-newTower.GetComponent<TowerController>().currentCost);
                gameController.AddingNewTower(newTower);
                Destroy(gameObject);
            }
        }
    }

    // Метод вычисляет не попадет ли враг в замкнутый круг из башен. Если True - попадет
    private bool TrapChecking(GameObject tower)
    {
        // Проверяем есть ли враги на карте
        GameObject[] currentEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        if(currentEnemies.Length == 0)
        {
            return false;
        }
        
        // Проверяем достаточно ли соседних башен чтобы сомкнуть кольцо
        LayerMask mask = LayerMask.GetMask("Tower");
        Collider2D[] neighboringTowers = Physics2D.OverlapCircleAll(transform.position, 1f, mask);
        if (neighboringTowers.Length < 3)
        {
            return false;
        }

        // Фомируем массив соседних точек
        Vector2[] neighboringPoints =
        {
            new Vector2(tower.transform.position.x, tower.transform.position.y + 1),
            new Vector2(tower.transform.position.x + 1, tower.transform.position.y),
            new Vector2(tower.transform.position.x, tower.transform.position.y - 1),
            new Vector2(tower.transform.position.x - 1, tower.transform.position.y)
        };
        // Проверяем попала ли точка внутрь замкнутого кольца
        for (int i = 0; i < neighboringPoints.Length; i++)
        {
            if (!Physics2D.OverlapPoint(neighboringPoints[i], LayerMask.GetMask("Tower")))
            {
                wayPoints = gameObject.GetComponent<PathFinder>().GetPath(neighboringPoints[i], target.transform.position);
                if (wayPoints.Count == 0)
                {
                    // Проверяем есть ли внутри замкнутого кольца враги
                    foreach (GameObject j in currentEnemies)
                    {
                        wayPoints = gameObject.GetComponent<PathFinder>().GetPath(neighboringPoints[i], j.transform.position);
                        if (wayPoints.Count > 0)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private void Blocking(GameObject tower)
    {
        blocking.enabled = false;
        StopCoroutine(enemiesCheck);
        StartCoroutine(BlockingTwinkle());
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        constrAlowed = false;
        Debug.Log("БЛОКИРОВКА!");
        Destroy(tower);
    }

    IEnumerator BlockingTwinkle()
    {
        float startTime = Time.time;

        while(Time.time - startTime < 0.6f)
        {
            blocking.enabled = !blocking.enabled;
            yield return new WaitForSeconds(0.12f);
        }

        blocking.enabled = false;
        yield break;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        cameraPos = Camera.main.transform.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

    public IEnumerator EnemiesCheck()
    {
        while (true)
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 1f, enemyLayer);
            if(enemies.Length != 0)
            {
                gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                constrAlowed = false;
            }
            else
            {
                gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                constrAlowed = true;
            }

            yield return new WaitForSeconds(checkTime);
        }
    }
}
