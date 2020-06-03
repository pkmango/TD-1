using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TowerButton : MonoBehaviour, IPointerClickHandler
{
    public TowerController tower; // Башня за которую отвечает эта кнопка
    public GameObject notActive; // Затемненная версия кнопки, появляется когда не хватает денег

    private Color opaque = new Color(1f, 1f, 1f, 1f);
    private Color transparent = new Color(1f, 1f, 1f, 0f);
    private TilemapController ground;
    private GameController gameController;
    private bool active; // Кнопка активна?

    void Start()
    {
        GameObject gameControllerObject = GameObject.FindWithTag("GameController");
        if (gameControllerObject != null)
        {
            gameController = gameControllerObject.GetComponent<GameController>();
        }

        AvailabilityCheck();
        gameController.NewCurrentMoney += AvailabilityCheck;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject[] towerButtons = GameObject.FindGameObjectsWithTag("TowerButton");
        foreach (GameObject button in towerButtons)
        {
            button.GetComponent<Image>().color = transparent;
        }

        if (active)
        {
            gameController.ShowConstrZone();
        }
        else
        {
            gameController.HideConstrZone();
        }

        gameController.selectedTower = tower;
        gameController.SetCharacteristicsMenu();
        gameObject.GetComponent<Image>().color = opaque;
    }

    public void SetTransparent()
    {
        gameObject.GetComponent<Image>().color = transparent;
    }

    public void AvailabilityCheck()
    {
        if (tower.GetComponent<TowerController>().costs[0] > gameController.currentMoney)
        {
            notActive.SetActive(true);
            active = false;

            if (gameObject.GetComponent<Image>().color == opaque)
            {
                gameController.DestroyConstrMarkers();
            }
        }
        else
        {
            notActive.SetActive(false);
            active = true;

            if (gameObject.GetComponent<Image>().color == opaque)
            {
                gameController.ShowConstrZone();
            }
        }
    }
}
