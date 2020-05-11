using UnityEngine;
using System.Collections.Generic;

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
    }

    public void AddingNewTower()
    {
        NewTower();
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

    public void HideConstrZone()
    {
        foreach(GameObject i in markers)
        {
            Destroy(i);
        }
        markers.RemoveRange(0, markers.Count);
    }
}
