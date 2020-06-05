using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraDrag : MonoBehaviour, IDragHandler
{
    private Vector3 startPosition;
    private float deltaMax;
    private float deltaMin;
    private float deltaLimit; // Предел смещения камеры по y
    private float maxOrthSize = 10f; // Максимальный размер orthographicSize

    void Start()
    {
        startPosition = transform.position;
        deltaLimit = maxOrthSize - Camera.main.orthographicSize;
        // Если лимит меньше четверти ячейки то нет смысла делать drag
        if (deltaLimit < 0.25f) deltaLimit = 0f;
        deltaMax = Camera.main.transform.position.y + deltaLimit;
        deltaMin = Camera.main.transform.position.y - deltaLimit;
        Debug.Log(deltaMax);
        Debug.Log(deltaMin);
    }

    //public void OnBeginDrag(PointerEventData eventData)
    //{
    //    //Debug.Log(Input.mousePosition);
    //    //Debug.Log(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    //}

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 oldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition - new Vector3(eventData.delta.x, eventData.delta.y, 0f));
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 deltaPos = mousePos - oldPos;
        Camera.main.transform.position = new Vector3(0f, Mathf.Clamp(Camera.main.transform.position.y - deltaPos.y, deltaMin, deltaMax), -10f);   
    }

    //public void OnEndDrag(PointerEventData eventData)
    //{
    //    //transform.position = startPosition;
    //    //Debug.Log("энд драг");
    //}
}
