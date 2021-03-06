﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

public class GameController : MonoBehaviour
{
    public Wave[] waves; // Массив с волнами врагов
    public TowerController[] typesOfTowers;
    //public float startWait; // Стартовое ожидание
    public float spawnWait; // Пауза между спауном врагов
    public float waveWait; // Пауза между волнами
    public GameObject spawnPoint; // Точка спауна
    public Transform target; // Точка-цель 
    public GameObject rewardText; // При уничтожении врага показывается текст с суммой награды
    public Image menuTransitionImg; // Черная заглушка для перехода между меню
    public float transitionTime = 0.5f; 
    public GameObject mainMenu;
    public GameObject pauseMenu;
    public GameObject gameOverMenu;
    public GameObject winMenu;
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
    public Text blockingText; // Мерцающее сообщение "Blocking"
    public Text clockText; // Текстовое поле для отображения времени до начала новой волны
    public Text moneyText;
    public int startMoney;
    public int nextBtnReward; // Награда за нажатие кнопки Next
    public Text livesText;
    public int startLives = 20;
    public AudioSource missSound; // Звук если враг прорвался и жизнь отнялась
    public AudioSource newTowerSound; // Звук при постановке новой башни
    public AudioSource sellTowerSound; //  Звук при продаже башни
    public Text waveNumberText;

    // Система очков
    public int score;
    public Text scoreText;
    public Text gameOverScoreText;
    public Text winScoreText;
    public Text livesBonusScoreText;
    public Text difficultyBunusScoreText;
    public Text difficultyText;

    public float armor; // Процент поглащаемого урона
    public float easy, normal, hard; // Настройки сложности для armor
    public int difficultyReward; // Награда за сложность
    public float deviation = 0.15f; // Предел случайного отклонения
    public int randomSpawn = 0; // Координата спауна меняется в этих пределах случайным образом
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
    [HideInInspector]
    public GameObject lastTower; // Ссылка на последнюю установленную башню
    [HideInInspector]
    public TowerController selectedTower; // Выбранный через кнопку вид башен

    public GameObject placeholderNext; // Заглушка для Next
    public GameObject attackDirection; // Анимированные стрелочки, которые показывают направление атаки

    private float ratio; // Соотношение сторон
    private float currentHeight; // Текущая высота
    private float ortSize; // Необходимый orthographicSize, чтобы ширина поля осталась фиксированная (меняется высота)
    private float fixWidth = 9f; // Фиксированная ширина поля
    private List<GameObject> markers = new List<GameObject>();
    private TilemapController ground; // Земля
    public int currentWave = 0;

    // Корутины, котороми нужно управлять
    private Coroutine spawnWaveCor, tilesMoveCor, checkWinningCor;
    public Coroutine spawnPassangersCor;
    private List<Coroutine> spawnEnemiesCorList = new List<Coroutine>();

    private float enemyTilesX; // Начальная позиция enemyTiles по оси х
    private float waveStartTime; // Время когда стартовала текущая волна
    private float delayNext = 1f; // Задержка после нажатия Next
    private int difficultyLevel = 1; // Уровень сложности 0:easy, 1:normal, 2:hard

    private GameObject[] enemyIcons; // Иконки врагов
    private int enabledIconsNumber = 3; // Количество отображаемых иконок врагов

    private const string LEAD_HIGH_SCORES = "CgkIsYanlt4XEAIQAQ";
    private const string ACH_1000 = "CgkIsYanlt4XEAIQAg";
    private const string ACH_2500 = "CgkIsYanlt4XEAIQAw";
    private const string ACH_5000 = "CgkIsYanlt4XEAIQBA";
    private const string ACH_10000 = "CgkIsYanlt4XEAIQCQ";
    private const string ACH_TOP_FOR_ALL = "CgkIsYanlt4XEAIQCw";
    private const int ACH_TOP_FOR_ALL_STEPS = 1000; // Количество шагов для ачивки
    private const string ACH_EASY_WALK = "CgkIsYanlt4XEAIQBQ";
    private const string ACH_THE_CHAMPIONS_LEAP = "CgkIsYanlt4XEAIQBg";
    private const string ACH_HARD_WORK = "CgkIsYanlt4XEAIQBw";
    private const string ACH_FLAWLESS_VICTORY = "CgkIsYanlt4XEAIQCA";
    // Соибираем все ачивки прогресс которых связан с накоплением очков в один массив
    private string[] pointAchievements = { ACH_1000, ACH_2500, ACH_5000, ACH_10000 };

    void Awake()
    {
        // Чтобы экран не гас
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Вычисление orthographicSize, необходимого для данного устройства
        ratio = (float) Screen.height / Screen.width;
        currentHeight = fixWidth * ratio;
        ortSize = currentHeight / 2f;
        Camera.main.orthographicSize = ortSize;

        currentMoney = startMoney;
    }

    private void Start()
    {
        // Для активации Google Play Services
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.DebugLogEnabled = true;
        Social.localUser.Authenticate((bool success) =>
        {
            if (success)
                Debug.Log("Authenticate удачно");
            else
                Debug.Log("Authenticate неудачно");
        });

        armor = normal;
        difficultyText.text = "normal";
        scoreText.text = score.ToString();
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

        enemyIcons = new GameObject[waves.Length];
        CreateWaveIcons();
    }

    void Update()
    {
        if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
        {
            if(!gameOverMenu.activeInHierarchy && !winMenu.activeInHierarchy)
            {
                Pause();
            }
        }
    }

    private void CreateWaveIcons()
    {
        enemyTilesX = enemyTiles.transform.localPosition.x;

        for (int i = 0; i<waves.Length; i++)
        {
            enemyIcons[i] = Instantiate(waves[i].waveTile, enemyTiles.transform);
            enemyIcons[i].transform.localPosition = new Vector2(enemyTilesStep * i, 0f);

            if (i > enabledIconsNumber)
                enemyIcons[i].SetActive(false);
        }
    }

    IEnumerator EnemyTilesMove()
    {
        while (true)
        {
            float deltaX = (enemyTilesStep / waveWait) * Time.fixedDeltaTime;
            enemyTiles.transform.localPosition = new Vector2(enemyTiles.transform.localPosition.x - deltaX, enemyTiles.transform.localPosition.y);

            // Таймер для отсчета времени до следующей волны
            float newTime = Mathf.Round(waveWait + waveStartTime - Time.time);
            if (newTime >= 0f && clockText.text != newTime.ToString()) clockText.text = newTime.ToString();

            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator SpawnWaves()
    {
        for (int i = currentWave; i < waves.Length; i++)
        {
            StartCoroutine(SetPlaceholderForNext());

            waveStartTime = Time.time;
            currentWave = i;
            waveNumberText.text = (currentWave + 1).ToString() + "/" + waves.Length.ToString();
            // Для оптимизации отключаем лишние иконки
            if (currentWave + enabledIconsNumber < enemyIcons.Length)
                enemyIcons[currentWave + enabledIconsNumber].SetActive(true);
            if (currentWave - 1 >= 0)
                enemyIcons[currentWave -1].SetActive(false);

            spawnEnemiesCorList.Add(StartCoroutine(SpawnEnemies(i)));

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
            EnemyController enemyController = waves[i].enemies[j].GetComponent<EnemyController>();
            enemyController.reward = waves[i].reward;
            enemyController.hp = waves[i].hp;
            Instantiate(waves[i].enemies[j], randomSpawnPosition, Quaternion.identity);

            yield return new WaitForSeconds(wait);

            if(i == waves.Length - 1 && j == waves[i].enemies.Length - 1)
            {
                checkWinningCor = StartCoroutine(CheckWinningCondition());
            }
        }
    }

    public IEnumerator SpawnPassengers(GameObject passenger, Vector3 pos, Quaternion rot, int hp)
    {
        for (int i = 0; i < 3; i++)
        {
            Vector3 randomPosition = new Vector3(Random.Range(-0.32f, 0.32f), Random.Range(-0.32f, 0.32f), 0f);
            EnemyController enemyController = passenger.GetComponent<EnemyController>();
            enemyController.deviationVector = randomPosition;
            enemyController.hp = hp / 2;
            Instantiate(passenger, pos + randomPosition, rot);

            yield return new WaitForFixedUpdate();
        }
    }

    public void Next()
    {
        StartCoroutine(SetPlaceholderForNext());

        if(currentWave + 1 < waves.Length)
        {
            StopCoroutine(spawnWaveCor);
            currentWave++;
            waveNumberText.text = (currentWave + 1).ToString() + "/" + waves.Length.ToString();
            spawnWaveCor = StartCoroutine(SpawnWaves());

            enemyTiles.transform.localPosition = new Vector2(enemyTilesX - enemyTilesStep * currentWave, enemyTiles.transform.localPosition.y);

            int nextReward = nextBtnReward + currentWave / 10; // Добавляем бонус за Next, за каждые 10 волн +1
            ChangeMoney(nextReward); // Награда за Next
            if (currentWave < waves.Length) ChangeScore(currentWave);
            // Визуализация награды за нажатие Next
            Vector2 rewardPos = Camera.main.ScreenToWorldPoint(startButton.transform.position);
            rewardPos += new Vector2(-1f, 1f); // Смещаем на единицу
            GameObject nextBtnRewardText = Instantiate(rewardText, rewardPos, Quaternion.identity);
            nextBtnRewardText.GetComponentInChildren<MeshRenderer>().sortingLayerName = "Text";
            nextBtnRewardText.GetComponentInChildren<TextMesh>().text = "+" + nextReward.ToString();
        }
    }

    IEnumerator SetPlaceholderForNext()
    {
        placeholderNext.SetActive(true);
        yield return new WaitForSeconds(delayNext);
        placeholderNext.SetActive(false);
    }

    IEnumerator CheckWinningCondition()
    {
        while (true)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            GameObject[] airEnemies = GameObject.FindGameObjectsWithTag("AirEnemy");
            if (enemies.Length == 0 && airEnemies.Length == 0)
            {
                Win();
                yield break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void AddingNewTower(GameObject tower)
    {
        lastTower = tower;
        NewTower?.Invoke();
        newTowerSound.time = 0.05f;
        newTowerSound.Play();
    }

    public void ChangeMoney(int money)
    {
        currentMoney += money;
        moneyText.text = currentMoney.ToString();
        NewCurrentMoney?.Invoke();
    }

    public void ChangeScore(int deltaScore)
    {
        score += deltaScore;
        scoreText.text = score.ToString();
    }

    public void SubtractLife()
    {
        GameObject subtractLifeText = Instantiate(rewardText, target.position, Quaternion.identity);
        subtractLifeText.GetComponentInChildren<MeshRenderer>().sortingLayerName = "Text";
        subtractLifeText.GetComponentInChildren<TextMesh>().text = "-1";
        subtractLifeText.GetComponentInChildren<TextMesh>().color = new Color(0.9f, 0f, 0f);

        currentLives--;
        livesText.text = currentLives.ToString();
        missSound.Play();
        if (currentLives == 0)
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
        rangeText.text = selectedTower.ranges[0].ToString();
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
        blockingText.enabled = false;
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
        sellTowerSound.time = 0.05f;
        sellTowerSound.Play();
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

    public void ClickNewGame()
    {
        StartCoroutine(NewGame());
    }

    IEnumerator NewGame()
    {
        yield return StartCoroutine(Transition(0f));
        mainMenu.SetActive(false);
        yield return StartCoroutine(Transition(1f));
    }

    public void Started()
    {
        startButton.SetActive(false);
        spawnWaveCor = StartCoroutine(SpawnWaves());
        tilesMoveCor = StartCoroutine(EnemyTilesMove());
        Destroy(attackDirection);
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
        winMenu.SetActive(false);
    }

    public void ClickRestart()
    {
        StartCoroutine(Restart());
    }

    IEnumerator Restart()
    {
        Time.timeScale = 1f;
        HideConstrZone();
        Zeroing();
        upgradeMenu.SetActive(false);
        startButton.SetActive(true);
        yield return StartCoroutine(Transition(0f));
        Resume();
        yield return StartCoroutine(Transition(1f));
    }

    public void ClickMainMenu()
    {
        StartCoroutine(GoToMainMenu());
    }

    IEnumerator GoToMainMenu()
    {
        Time.timeScale = 1f;
        HideConstrZone();
        Zeroing();
        upgradeMenu.SetActive(false);
        startButton.SetActive(true);
        yield return StartCoroutine(Transition(0f));
        Resume();
        mainMenu.SetActive(true);
        yield return StartCoroutine(Transition(1f));
    }

    // Обнуление всего для нового старта
    private void Zeroing()
    {
        if (spawnWaveCor != null) StopCoroutine(spawnWaveCor);
        foreach(Coroutine i in spawnEnemiesCorList)
        {
            if (i != null) StopCoroutine(i);
        }
        spawnEnemiesCorList.Clear();
        if (tilesMoveCor != null) StopCoroutine(tilesMoveCor);
        if (checkWinningCor != null) StopCoroutine(checkWinningCor);
        if (spawnPassangersCor != null) StopCoroutine(spawnPassangersCor);

        currentMoney = startMoney;
        moneyText.text = currentMoney.ToString();
        NewCurrentMoney?.Invoke();
        currentLives = startLives;
        livesText.text = startLives.ToString();
        score = 0;
        scoreText.text = "0";
        currentWave = 0;
        waveNumberText.text = currentWave.ToString() + "/" + waves.Length.ToString();
        enemyTiles.transform.localPosition = new Vector2(enemyTilesX, enemyTiles.transform.localPosition.y);
        clockText.text = waveWait.ToString();

        for (int i = 0; i < waves.Length; i++)
        {
            if (i > enabledIconsNumber)
            {
                enemyIcons[i].SetActive(false);
            }
            else
            {
                enemyIcons[i].SetActive(true);
            }  
        }

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

        GameObject[] airEnemies = GameObject.FindGameObjectsWithTag("AirEnemy");
        foreach (GameObject i in airEnemies)
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
        ReportScore(score);
        gameOverScoreText.text = score.ToString();
    }

    public void Win()
    {
        Time.timeScale = 0f;
        UnlockingWinAchievements();
        winMenu.SetActive(true);
        difficultyBunusScoreText.text = (difficultyReward * difficultyLevel).ToString();
        livesBonusScoreText.text = (currentLives * 20).ToString();
        score += difficultyReward * difficultyLevel + currentLives * 20;
        ReportScore(score);
        winScoreText.text = score.ToString();
    }

    public void ChangeDifficulty(Toggle toggle)
    {
        if (toggle.isOn)
        {
            difficultyText.text = toggle.name;

            switch (toggle.name)
            {
                case "easy":
                    armor = easy;
                    difficultyLevel = 0;
                    break;
                case "normal":
                    armor = normal;
                    difficultyLevel = 1;
                    break;
                case "hard":
                    armor = hard;
                    difficultyLevel = 2;
                    break;
            }
            Debug.Log(armor);
        }
    }

    IEnumerator Transition(float alpha)
    {
        int stepsNumber = (int)(transitionTime / Time.fixedDeltaTime); // Количество шагов необходимое для перехода
        float step = 1 / (float)stepsNumber;
        menuTransitionImg.color = new Color(0f, 0f, 0f, alpha);

        if (alpha == 0f)
        {
            
            menuTransitionImg.gameObject.SetActive(true);

            for (int i = 0; i <= stepsNumber; i++)
            {
                alpha += step;
                menuTransitionImg.color = new Color(0f, 0f, 0f, alpha);
                yield return new WaitForFixedUpdate();
            }

            alpha = 1f;
            menuTransitionImg.color = new Color(0f, 0f, 0f, alpha);
        }
        else
        {
            for (int i = 0; i <= stepsNumber; i++)
            {
                alpha -= step;
                menuTransitionImg.color = new Color(0f, 0f, 0f, alpha);
                yield return new WaitForFixedUpdate();
            }

            menuTransitionImg.gameObject.SetActive(false);
        }
    }

    public void ClickHighScore()
    {
        //Social.ShowLeaderboardUI();
        PlayGamesPlatform.Instance.ShowLeaderboardUI(LEAD_HIGH_SCORES);
        Debug.Log("Click High Score Btn");
    }

    public void ClickAchievements()
    {
        Social.ShowAchievementsUI();
        Debug.Log("Click Achievements Btn");
    }

    private void ReportScore(int finalScore)
    {
        Social.ReportScore(finalScore, LEAD_HIGH_SCORES, (bool success) =>
        {
            if (success)
                Debug.Log("Результат удачно добавлен в таблицу лидеров");
        });

        foreach (string i in pointAchievements)
        {
            Debug.Log("Отправляем прогресс для " + i);

            PlayGamesPlatform.Instance.IncrementAchievement(i, finalScore, (bool success) =>
            {
                if (success)
                    Debug.Log("Прогресс " + i + " обновлен");
            });
        }
    }

    private void UnlockingWinAchievements()
    {
        switch (difficultyLevel)
        {
            case 0:
                UnlockAchievement(ACH_EASY_WALK);
                break;
            case 1:
                UnlockAchievement(ACH_THE_CHAMPIONS_LEAP);
                break;
            case 2:
                UnlockAchievement(ACH_HARD_WORK);
                break;
        }

        if (currentLives == startLives)
        {
            UnlockAchievement(ACH_FLAWLESS_VICTORY);
        }
    }

    public void CheckAllTopAchievement(TowerController tower)
    {
        if (PlayerPrefs.GetInt(tower.towerName, 0) == 0)
        {
            PlayerPrefs.SetInt(tower.towerName, 1);
            Debug.Log(tower.towerName + " получила топ уровень первый раз!");

            PlayGamesPlatform.Instance.IncrementAchievement(ACH_TOP_FOR_ALL, ACH_TOP_FOR_ALL_STEPS / typesOfTowers.Length + 1, (bool success) =>
            {
                if (success)
                    Debug.Log("Прогресс ACH_TOP_FOR_ALL обновлен");
            });
        }

        // Для исключения ошибок с начислением прогресса в случае добалвения в игру новых башен делаем общую проверку
        int topSum = 0;
        for (int i = 0; i < typesOfTowers.Length; i++)
        {
            topSum += PlayerPrefs.GetInt(typesOfTowers[i].towerName, 0);
        }

        if (topSum == typesOfTowers.Length)
        {
            PlayGamesPlatform.Instance.IncrementAchievement(ACH_TOP_FOR_ALL, ACH_TOP_FOR_ALL_STEPS, (bool success) =>
            {
                if (success)
                    Debug.Log("Прогресс ACH_TOP_FOR_ALL обновлен максимально");
            });
            Debug.Log("Все башни достигали топ-уровня");
        }
    }

    public void UnlockAchievement(string achievement)
    {
        Social.ReportProgress(achievement, 100.0f, (bool success) => {
            if (success)
                Debug.Log("Открыто достижение " + achievement);
        });
    }

    public void QuitGame ()
    {
        PlayGamesPlatform.Instance.SignOut();
        Application.Quit();
    }
}
