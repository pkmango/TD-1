using UnityEngine;

public class CameraDrag : MonoBehaviour//, IDragHandler, IBeginDragHandler
{
    public float startPositionY = 2f; // Начальная координат y для камеры
    private float delayDrag = 0.1f; // Время задержки перед драгом

    private float deltaMax;
    private float deltaMin;
    private float deltaLimit; // Предел смещения камеры по y
    private float maxOrthSize = 10f; // Максимальный размер orthographicSize
    private float screenLimit; // Нижняя граница для drag
    private float dragTime;

    bool allowDrag = false;

    // The key we will use for dragging
    public KeyCode dragKey = KeyCode.Mouse0;

    // The ground plane that we will drag along
    // is defined by an origin point and a normal
    public Vector3 groundOrigin = Vector3.zero;
    public Vector3 groundNormal = Vector3.up;

    Plane _groundPlane;
    Vector3 _dragOrigin;

    // We'll save references to the components
    // which we'll need repeatedly
    Camera _camera;
    Transform _transform;

    public void Start()
    {
        // initialisation
        _camera = GetComponent<Camera>();
        _transform = GetComponent<Transform>();
        _groundPlane = new Plane(groundNormal, groundOrigin);

        screenLimit = Screen.height / Camera.main.orthographicSize * 2f;

        deltaLimit = maxOrthSize - Camera.main.orthographicSize;
        // Если лимит меньше четверти ячейки то нет смысла делать drag
        if (deltaLimit < 0.25f) deltaLimit = 0f;
        deltaMax = startPositionY + deltaLimit;
        deltaMin = startPositionY - deltaLimit;
    }

    public void Update()
    {
        float distanceToIntersection;
        Ray mouseRay = _camera.ScreenPointToRay(Input.mousePosition);
        
        // start drag
        if (Input.GetKeyDown(dragKey))
        {
            if (Input.mousePosition.y > screenLimit)
            {
                dragTime = Time.time;
                allowDrag = true;
            }
            else
            {
                allowDrag = false;
            }
        }

        if (allowDrag && Time.time - dragTime >= delayDrag)
        {
            // continue drag
            if (Input.GetKey(dragKey))
            {
                _groundPlane.Raycast(mouseRay, out distanceToIntersection);
                Vector3 intersection = mouseRay.GetPoint(distanceToIntersection);
                Vector3 deltaPos = _dragOrigin - intersection;
                _transform.position = new Vector3(0f, Mathf.Clamp(_transform.position.y + deltaPos.y, deltaMin, deltaMax), -10f);
            }
        }
        else
        {
            _groundPlane.Raycast(mouseRay, out distanceToIntersection);
            _dragOrigin = mouseRay.GetPoint(distanceToIntersection);
        }
    }
}
