using UnityEngine;
using UnityEngine.EventSystems;

public class TilemapController : MonoBehaviour, IPointerClickHandler
{
    public delegate void EmptyClick();
    public event EmptyClick Click;

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("Клик по земле");
        Click?.Invoke();
    }

}
