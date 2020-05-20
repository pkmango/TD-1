using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TowerButton : MonoBehaviour, IPointerClickHandler
{
    private Color opaque = new Color(1f, 1f, 1f, 1f);
    private Color transparent = new Color(1f, 1f, 1f, 0f);
    private TilemapController ground;

    //private void Start()
    //{
    //    GameObject groundObject = GameObject.FindWithTag("Ground");
    //    if (groundObject != null)
    //    {
    //        ground = groundObject.GetComponent<TilemapController>();
    //        ground.Click += SetTransparent;
    //    }
    //}

    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject[] towerButtons = GameObject.FindGameObjectsWithTag("TowerButton");
        foreach(GameObject button in towerButtons)
        {
            button.GetComponent<Image>().color = transparent;
        }
        gameObject.GetComponent<Image>().color = opaque;
    }

    public void SetTransparent()
    {
        gameObject.GetComponent<Image>().color = transparent;
    }
}
