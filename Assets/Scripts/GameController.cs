using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class GameController : MonoBehaviour
{
    public Wave[] waves; // Массив с волнами врагов
    //public float startWait; // Стартовое ожидание
    public float spawnWait; // Пауза между спауном врагов
    public float waveWait; // Пауза между волнами
    public GameObject spawnPoint; // Точка спауна
    public GameObject rewardText; // При уничтожении врага показывается текст с суммой награды
    public GameObject mainMenu;
    public GameObject pauseMenu;
    public GameObject gameOverMenu;
    public GameObject startButton;
    public GameObject upgradeButton;
    public Vector3 constrZonePosition; // Начальная точка для построения разрешенной зоны строительства
    public int constrZoneWidth; // Ширина зоны строительства
    public int constrZoneHeight; // Высота зоны строительства
    public GameObject constrZoneMarker;
    public LayerMask stopConstr;
    public delegate void AddingTowers(bool sell = false);
    public event AddingTowers NewTower; // Событие для установки новой башни
    public delegate void ChangeCurrentMoney();
    public event ChangeCurrentMoney NewCurrentMoney; // Событие для изменение текущих денег
    public GameObject enemyTiles; // Канвас на который располагается очередь из плиток с обозначением слудющей волны
    public int enemyTilesStep = 269; // Ширина шага в пикселях, через который размещены плитки enemyTiles
    public Text clockText; // Текстовое поле для отображения времени до начала новой волны
    public Text moneyText;
    public int startMoney;
    public Text livesText;
    public int startLives = 20;
    public Text waveNumberText;
    public int score;
    public Text scoreText;
    public Text gameOverScoreText;
    public float deviation = 0.1f; // Предел случайного отклонения
    public int randomSpawn = 3; // Координата спауна меняется в этих пределах случайным образом
    [HideInInspector]
    public int currentMoney, currentLives;
    // Меню с прогрессом, отоброжаемое когда башня апгрейдится
    public GameObject upgradingMenu;
    public RectTransform upgradeProgress;
    public GameObject buffCanvas;
    public Text buffValue;
    // Данные для меню с характеристиками tower
    public GameObject characteristicsMenu;
    public Text costText;
    public Text damageText;
    public Text rangeText;
    public Text fireRateText;
    public Text towerNameText;
    public Text descriptionText;
    public Image towerIcon;
    // Данные для меню апгрейда и продажи
    public GameObject upgradeMenu;
    public Text costTextUp;
    public Text damageTextUp;
    public Text rangeTextUp;
    public Text fireRateTextUp;
    public Text costTextUpPlus;
    public Text damageTextPlus;
    public Text rangeTextPlus;
    public Text fireRateTextPlus;
    public Text towerNameTextUp;
    public Text sellText;
    [HideInInspector]
    public TowerController pressedTower; // Башня на поле, на которую кликнул игрок

    public GameObject lastTower; // Ссылка на последнюю установленную башню
    public TowerController selectedTower; // Выбранный через кнопку вид башен

    private float ratio; // Соотношение сторон
    private float currentHeight; // Текущая высота
    private float ortSize; // Необходимый orthographicSize, чтобы ширина поля осталась фиксированная (меняется высота)
    private float fixWidth = 9f; // Фиксированная ширина поля
    private List<GameObject> markers = new List<GameObject>();
    private TilemapController ground; // Земля
    public int currentWave = 0;
    private Coroutine spawnWaveCor;
    private float enemyTilesX; // Начальная позиция enemyTiles по оси х
    private float waveStartTime; // Время когда стартовала текущая волна

    void Awake()
    {
        // Вычисление orthographicSize, необходимого для данного устройства
        ratio = (float) Screen.height / Screen.width;
        currentHeight = fixWidth * ratio;
        ortSize = currentHeight / 2f;
        Camera.main.orthographicSize = ortSize;

        currentMoney = startMoney;
    }

    private void Start()
    {
        moneyText.text = currentMoney.ToString();
        livesText.text = startLives.ToString();
        currentLives = startLives;
        waveNumberText.text = currentWave.ToString() +"/" + waves.Length.ToString();

        GameObject groundObject = GameObject.FindWithTag("Ground");
        if (groundObject != null)
        {
            ground = groundObject.GetComponent<TilemapController>();
            ground.Click += HideConstrZone;
        }

        CreateWaveIcons();
    }

    void Update()
    {
        if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }

    private void CreateWaveIcons()
    {
        enemyTilesX = enemyTiles.transform.localPosition.x;

        for (int i = 0; i<waves.Length; i++)
        {
            GameObject icon = Instantiate(waves[i].waveTile, enemyTiles.transform);
            icon.transform.localPosition = new Vector2(enemyTilesStep * i, 0f);
        }
    }

    IEnumerator EnemyTilesMove()
    {
        while (true)
        {
            float deltaX = (enemyTilesStep / waveWait) * Time.fixedDeltaTime;
            enemyTiles.transform.localPosition = new Vector2(enemyTiles.transform.localPosition.x - deltaX, enemyTiles.transform.localPosition.y);
            // Таймер для отсчета времени до следующей волны
            clockText.text = Mathf.Round(waveWait + waveStartTime - Time.time).ToString();
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator SpawnWaves()
    {
        for (int i = currentWave; i < waves.Length; i++)
        {
            waveStartTime = Time.time;
            currentWave = i;
            waveNumberText.text = (currentWave + 1).ToString() + "/" + waves.Length.ToString();

            StartCoroutine(SpawnEnemies(i));

            yield return new WaitForSeconds(waveWait);
        }
    }

    IEnumerator SpawnEnemies(int i)
    {
        // Назначаем задержку спауна
        float wait = waves[i].spawnWait != 0 ? waves[i].spawnWait : spawnWait;
        // Назначаем ширину случайного спауна
        int _randomSpawn = waves[i].randomSpawn != 0 ? waves[i].randomSpawn : randomSpawn;

        for (int j = 0; j < waves[i].enemies.Length; j++)
        {
            Vector3 randomSpawnPosition = spawnPoint.transform.position + new Vector3(Random.Range(-_randomSpawn, _randomSpawn), 0f, 0f);
            Instantiate(waves[i].enemies[j], randomSpawnPosition, Quaternion.identity);

            yield return new WaitForSeconds(wait);
        }
    }

    public void Next()
    {
        if(currentWave + 1 < waves.Length)
        {
            StopCoroutine(spawnWaveCor);
            currentWave++;
            waveNumberText.text = (currentWave + 1).ToString() + "/" + waves.Length.ToString();
            spawnWaveCor = StartCoroutine(SpawnWaves());

            enemyTiles.transform.localPosition = new Vector2(enemyTilesX - enemyTilesStep * currentWave, enemyTiles.transform.localPosition.y);
        }
    }

    public void AddingNewTower(GameObject tower)
    {
        lastTower = tower;
        NewTower?.Invoke();
    }

    public void ChangeMoney(int money)
    {
        currentMoney += money;
        moneyText.text = currentMoney.ToString();
        NewCurrentMoney?.Invoke();
    }

    public void SubtractLife()
    {
        currentLives--;
        livesText.text = currentLives.ToString();
        if (currentLives <= 0)
        {
            GameOver();
        }
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
        characteristicsMenu.SetActive(true);
        upgradeMenu.SetActive(false);
        costText.text = selectedTower.costs[0].ToString();
        damageText.text = selectedTower.damages[0].ToString();
        if (selectedTower.boost) damageText.text += "%"; // Для усиливающих башен вместо урона отображается процент усиления
        rangeText.text = (selectedTower.ranges[0] - 0.5f).ToString();
        fireRateText.text = selectedTower.fireRates[0].ToString();
        towerNameText.text = selectedTower.towerName;

        if (pressedTower != null)
        {
            pressedTower.active = false;
            pressedTower.glow.SetActive(false);
            pressedTower.circle.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    public void HideConstrZone()
    {
        DestroyConstrMarkers();
        characteristicsMenu.SetActive(false);

        GameObject[] towerButtons = GameObject.FindGameObjectsWithTag("TowerButton");
        foreach (GameObject button in towerButtons)
        {
            button.GetComponent<TowerButton>().SetTransparent();
        }
    }

    public void DestroyConstrMarkers()
    {
        foreach (GameObject i in markers)
        {
            Destroy(i);
        }
        markers.RemoveRange(0, markers.Count);
    }

    public void SellTower()
    {
        NewTower?.Invoke(true);
        upgradeMenu.SetActive(false);
        ChangeMoney(pressedTower.currentCost / 2);
        //pressedTower.StopAllCoroutines();
        //Destroy(pressedTower.upgradeProgress.gameObject);
        pressedTower.DestroyThisTower();
    }

    public void LevelUp()
    {
        if(pressedTower.level != pressedTower.maxLevel)
        {
            if(pressedTower.costs[pressedTower.level + 1] <= currentMoney)
            {
                ChangeMoney(-pressedTower.costs[pressedTower.level + 1]);
                pressedTower.Upgrade(upgradingMenu);
            }
        }
    }

    public void NewGame()
    {
        mainMenu.SetActive(false);
    }

    public void Started()
    {
        startButton.SetActive(false);
        spawnWaveCor = StartCoroutine(SpawnWaves());

        StartCoroutine(EnemyTilesMove());
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        gameOverMenu.SetActive(false);
    }

    public void Restart()
    {
        HideConstrZone();
        Zeroing();
        upgradeMenu.SetActive(false);
        startButton.SetActive(true);
        Resume();
    }

    public void GoToMainMenu()
    {
        Restart();
        mainMenu.SetActive(true);
    }

    // Обнуление всего для нового старта
    public void Zeroing()
    {
        
        StopAllCoroutines();
        currentMoney = startMoney;
        moneyText.text = currentMoney.ToString();
        currentLives = startLives;
        livesText.text = startLives.ToString();
        score = 0;
        scoreText.text = "0";
        currentWave = 0;
        waveNumberText.text = currentWave.ToString() + "/" + waves.Length.ToString();
        enemyTiles.transform.localPosition = new Vector2(enemyTilesX, enemyTiles.transform.localPosition.y);
        clockText.text = waveWait.ToString();

        GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");
        foreach (GameObject i in towers)
        {
            i.GetComponent<TowerController>().DestroyThisTower();
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach(GameObject i in enemies)
        {
            i.GetComponent<EnemyController>().DestroyObject(true);
        }

        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (GameObject i in bullets)
        {
            Destroy(i);
        }
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        gameOverMenu.SetActive(true);
        gameOverScoreText.text = score.ToString();
    }

    public void QuitGame ()
    {
        Application.Quit();
    }
}
