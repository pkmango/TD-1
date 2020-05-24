﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class GameController : MonoBehaviour
{
    public Wave[] waves; // Массив с волнами врагов
    public float startWait; // Стартовое ожидание
    public float spawnWait; // Пауза между спауном врагов
    public float waveWait; // Пауза между волнами
    public GameObject spawnPoint; // Точка спауна
    public GameObject mainMenu;
    public GameObject startButton;
    public GameObject upgradeButton;
    public Vector3 constrZonePosition; // Начальная точка для построения разрешенной зоны строительства
    public int constrZoneWidth; // Ширина зоны строительства
    public int constrZoneHeight; // Высота зоны строительства
    public GameObject constrZoneMarker;
    public LayerMask stopConstr;
    public delegate void AddingTowers(bool sell = false);
    public event AddingTowers NewTower; // Событие для установки новой башни
    public Text moneyText;
    public int startMoney;
    public Text livesText;
    public int startLives = 20;
    [HideInInspector]
    public int currentMoney, currentLives;
    // Меню с прогрессом, отоброжаемое когда башня апгрейдится
    public GameObject upgradingMenu;
    public RectTransform upgradeProgress;
    // Данные для меню с характеристиками tower
    public GameObject characteristicsMenu;
    public Text costText;
    public Text damageText;
    public Text rangeText;
    public Text fireRateText;
    public Text towerNameText;
    // Данные для меню апгрейда и продажи
    public GameObject upgradeMenu;
    public Text costTextUp;
    public Text damageTextUp;
    public Text rangeTextUp;
    public Text fireRateTextUp;
    public Text towerNameTextUp;
    public Text sellText;
    public TowerController pressedTower; // Башня на поле, на которую кликнул игрок

    public GameObject lastTower; // Ссылка на последнюю установленную башню

    private float ratio; // Соотношение сторон
    private float currentHeight; // Текущая высота
    private float ortSize; // Необходимый orthographicSize, чтобы ширина поля осталась фиксированная (меняется высота)
    private float fixWidth = 9f; // Фиксированная ширина поля
    private List<GameObject> markers = new List<GameObject>();
    private TilemapController ground; // Земля
    private int currentWave = 0;
    //private bool next = false;
    private Coroutine spawnWaveCor;

    void Awake()
    {
        // Вычисление orthographicSize, необходимого для данного устройства
        ratio = (float) Screen.height / Screen.width;
        currentHeight = fixWidth * ratio;
        ortSize = currentHeight / 2f;
        Camera.main.orthographicSize = ortSize;
    }

    private void Start()
    {
        moneyText.text = startMoney.ToString();
        currentMoney = startMoney;
        livesText.text = startLives.ToString();
        currentLives = startLives;

        GameObject groundObject = GameObject.FindWithTag("Ground");
        if (groundObject != null)
        {
            ground = groundObject.GetComponent<TilemapController>();
            ground.Click += HideConstrZone;
        }
    }

    IEnumerator SpawnWaves(float wait)
    {
        yield return new WaitForSeconds(wait);

        for (int i = currentWave; i < waves.Length; i++)
        {
            currentWave = i;

            StartCoroutine(SpawnEnemies(i));

            //for (int j = 0; j < waves[i].enemies.Length; j++)
            //{
            //    Instantiate(waves[i].enemies[j], spawnPoint.transform.position, Quaternion.identity);

            //    yield return new WaitForSeconds(spawnWait);
            //}

            //if (next)
            //{
            //    next = false;
            //    yield break;
            //}

            yield return new WaitForSeconds(waveWait);
        }
    }

    IEnumerator SpawnEnemies(int i)
    {
        for (int j = 0; j < waves[i].enemies.Length; j++)
        {
            Instantiate(waves[i].enemies[j], spawnPoint.transform.position, Quaternion.identity);

            yield return new WaitForSeconds(spawnWait);
        }
    }

    public void Next()
    {
        if(currentWave + 1 < waves.Length)
        {
            StopCoroutine(spawnWaveCor);
            currentWave++;
            spawnWaveCor = StartCoroutine(SpawnWaves(0f));
        }
    }

    public void AddingNewTower(GameObject tower)
    {
        lastTower = tower;
        NewTower?.Invoke();
    }

    public void ShowConstrZone()
    {
        if (markers.Count == 0)
        {
            for (int i = (int)constrZonePosition.x; i < ((int)constrZonePosition.x + constrZoneWidth); i++)
            {
                for (int j = (int)constrZonePosition.y; j < ((int)constrZonePosition.y + constrZoneHeight); j++)
                {
                    var pointContent = Physics2D.OverlapPoint(new Vector2(i, j), stopConstr);

                    if (pointContent == null)
                    {
                        markers.Add(Instantiate(constrZoneMarker, new Vector3(i, j, constrZonePosition.z), transform.rotation));
                    }
                }
            }
        }  
    }

    public void SetCharacteristicsMenu()
    {
        GameObject towersObject = GameObject.FindWithTag("Towers");
        if (towersObject != null)
        {
            characteristicsMenu.SetActive(true);
            upgradeMenu.SetActive(false);
            TowerController tower = towersObject.GetComponent<Towers>().selectedTower.GetComponent<TowerController>();
            costText.text = tower.costs[0].ToString();
            damageText.text = tower.damages[0].ToString();
            rangeText.text = (tower.ranges[0] - 0.5f).ToString();
            fireRateText.text = tower.fireRates[0].ToString();
            towerNameText.text = tower.towerName;
        }

        if (pressedTower != null)
        {
            pressedTower.active = false;
            pressedTower.glow.SetActive(false);
            pressedTower.circle.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    public void HideConstrZone()
    {
        foreach(GameObject i in markers)
        {
            Destroy(i);
        }
        markers.RemoveRange(0, markers.Count);
        characteristicsMenu.SetActive(false);

        GameObject[] towerButtons = GameObject.FindGameObjectsWithTag("TowerButton");
        foreach (GameObject button in towerButtons)
        {
            button.GetComponent<TowerButton>().SetTransparent();
        }
    }

    public void SellTower()
    {
        NewTower?.Invoke(true);
        upgradeMenu.SetActive(false);
        currentMoney += pressedTower.currentCost / 2;
        moneyText.text = currentMoney.ToString();
        pressedTower.StopAllCoroutines();
        Destroy(pressedTower.upgradeProgress.gameObject);
        pressedTower.DestroyThisTower();
    }

    public void LevelUp()
    {
        if(pressedTower.level != pressedTower.maxLevel)
        {
            pressedTower.Upgrade(upgradingMenu);
        }
    }

    public void NewGame()
    {
        mainMenu.SetActive(false);
    }

    public void Started()
    {
        startButton.SetActive(false);
        spawnWaveCor = StartCoroutine(SpawnWaves(startWait));
    }

    public void GameOver()
    {
        Debug.Log("Game Over");
    }

    public void QuitGame ()
    {
        Application.Quit();
    }
}
