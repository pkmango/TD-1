using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed;
    public Transform targetPosition;
    public int damage;

    private Vector2 startPosition;
    private Vector2 endPosition;
    private float t; // Прогресс для Lerp
    private Vector2 distance; // Растояние между стартовой и конечной точкой

    void Start()
    {
        startPosition = transform.position;
        endPosition = Vector2.zero;
    }

    private void Update()
    {
        if (targetPosition != null)
        {
            endPosition = targetPosition.position;
        }
        else if(endPosition == Vector2.zero)
        {
            Destroy(gameObject);
        }

        distance = endPosition - startPosition;
        transform.position = Vector2.Lerp(startPosition, endPosition, t);

        if (t < 1f)
        {
            t += 1 / (distance.magnitude / (speed * Time.deltaTime));
        }
        else
        {
            if (targetPosition != null)
            {
                targetPosition.gameObject.GetComponent<EnemyController>().Health(damage);
            }
            
            Destroy(gameObject);
        }
    }
}