using System.Collections;
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

        StartCoroutine(EnemiesCheck());
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (constrAlowed && tower.GetComponent<TowerController>().cost <= gameController.currentMoney)
        {
            GameObject newTower = Instantiate(tower, transform.position, tower.transform.rotation);
            wayPoints = gameObject.GetComponent<PathFinder>().GetPath(spawn.transform.position, target.transform.position);
            if(wayPoints.Count == 0)
            {
                count = 0;
                InvokeRepeating("BlockingTwinkle", 0f, 0.12f);
                Debug.Log("БЛОКИРОВКА!");
                Destroy(newTower);
            }
            else
            {
                gameController.AddingNewTower();
                gameController.currentMoney -= tower.GetComponent<TowerController>().cost;
                gameController.moneyText.text = gameController.currentMoney.ToString();
                //gameController.NewTower();
                Destroy(gameObject);
            }
        }
        
        //gameController.HideConstrZone();
    }

    void BlockingTwinkle()
    {
        if (blocking != null)
        {
            blocking.enabled = !blocking.enabled;
            if (count == 5)
            {
                CancelInvoke("BlockingTwinkle");
            }
            count++;
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

    IEnumerator EnemiesCheck()
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
