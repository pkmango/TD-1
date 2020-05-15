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
    private Text blocking;
    private IEnumerator enemiesCheck;
    private int count; // Счетчик для BlockingTwinkle
    private List<Vector2> wayPoints;
    private bool constrAlowed; // Если true - постройка разрешена

    void Start()
    {
        constrAlowed = true;

        GameObject gameControllerObject = GameObject.FindWithTag("GameController");
        if (gameControllerObject != null)
        {
            gameController = gameControllerObject.GetComponent<GameController>();
        }
        GameObject towersObject = GameObject.FindWithTag("Towers");
        if (towersObject != null)
        {
            tower = towersObject.GetComponent<Towers>().selectedTower;
        }
        spawn = GameObject.FindWithTag("Spawn");
        target = GameObject.FindWithTag("Target");
        blocking = GameObject.FindWithTag("BlockingText").GetComponent<Text>();

        enemiesCheck = EnemiesCheck();
        StartCoroutine(enemiesCheck);
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (constrAlowed && tower.GetComponent<TowerController>().cost <= gameController.currentMoney)
        {
            GameObject newTower = Instantiate(tower, transform.position, tower.transform.rotation);
            // Проверяем блокировку от старта до финиша
            wayPoints = gameObject.GetComponent<PathFinder>().GetPath(spawn.transform.position, target.transform.position);
            if(wayPoints.Count == 0)
            {
                Blocking(newTower);
            }
            else
            {
                // Проверяем блокировку от всех врагов на поле до финиша
                GameObject[] currentEnemies = GameObject.FindGameObjectsWithTag("Enemy");
                foreach(GameObject i in currentEnemies)
                {
                    wayPoints = gameObject.GetComponent<PathFinder>().GetPath(i.transform.position, target.transform.position);
                    if (wayPoints.Count == 0)
                    {
                        Blocking(newTower);
                    }
                    else
                    {
                        gameController.AddingNewTower();
                        gameController.currentMoney -= tower.GetComponent<TowerController>().cost;
                        gameController.moneyText.text = gameController.currentMoney.ToString();
                        Destroy(gameObject);
                    }
                }
            }
        }
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
        //throw new System.NotImplementedException();
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
