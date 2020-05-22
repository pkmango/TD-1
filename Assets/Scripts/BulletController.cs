using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed;
    public Transform targetPosition;
    public GameObject explosion;
    public bool rocket; // Это ракета?
    public float splash = 1.5f; // Слеш-радиус для ракеты
    [HideInInspector]
    public int damage; // Урон назначается пушкой при выстреле

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

        if (rocket)
        {
            transform.position = Vector2.Lerp(startPosition, endPosition, t * t);
        }
        else
        {
            transform.position = Vector2.Lerp(startPosition, endPosition, t);
        }
            

        if (t < 1f)
        {
            t += 1 / (distance.magnitude / (speed * Time.deltaTime));
        }
        else
        {
            if (rocket)
            {
                Collider2D[] splashedEnemies = Physics2D.OverlapCircleAll(transform.position, splash, LayerMask.GetMask("Enemy"));
                foreach (Collider2D i in splashedEnemies)
                {
                    i.GetComponent<EnemyController>().Health(damage);
                }
            }
            else
            {
                if (targetPosition != null)
                {
                    targetPosition.gameObject.GetComponent<EnemyController>().Health(damage);
                }
            }

            if (explosion != null)
            {
                Instantiate(explosion, transform.position, Quaternion.identity);
            }
            
            Destroy(gameObject);
        }
    }
}