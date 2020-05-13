using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Vector3 constrZonePosition; // Начальная точка для построения разрешенной зоны строительства
    public int constrZoneWidth; // Ширина зоны строительства
    public int constrZoneHeight; // Высота зоны строительства
    public GameObject constrZoneMarker;
    public LayerMask stopConstr;

    private float ratio; // Соотношение сторон
    private float currentHeight; // Текущая высота
    private float ortSize; // Необходимый orthographicSize, чтобы ширина поля осталась фиксированная (меняется высота)
    private float fixWidth = 9f; // Фиксированная ширина поля
    public delegate void AddingTowers();
    public event AddingTowers NewTower;
    public Text moneyText;
    public int startMoney;
    public int currentMoney;
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

    public GameObject pressedTower; // Башня на поле, на которую кликнул игрок

    private List<GameObject> markers = new List<GameObject>();
    //private GameObject[,] markers;

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
        //markers = new GameObject[ConstrZoneWidth, ConstrZoneHeight];
        //ShowConstrZone();
        moneyText.text = startMoney.ToString();
        currentMoney = startMoney;
    }

    public void AddingNewTower()
    {
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
            costText.text = tower.cost.ToString();
            damageText.text = tower.damage.ToString();
            rangeText.text = tower.range.ToString();
            fireRateText.text = tower.fireRate.ToString();
            towerNameText.text = tower.towerName;
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
    }

    public void SellTower()
    {
        NewTower?.Invoke();
        upgradeMenu.SetActive(false);
        if(pressedTower != null)
        {
            currentMoney -= pressedTower.GetComponent<TowerController>().currentCost / 2;
            moneyText.text = currentMoney.ToString();
            Destroy(pressedTower);
        }
        
    }
}
