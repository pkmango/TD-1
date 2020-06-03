using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed;
    public Transform targetPosition;
    public GameObject explosion;
    public bool rocket; // Это ракета?
    public float splash = 1.5f; // Слеш-радиус для ракеты
    public bool frost; // Пуля морозит?
    //public float slowdown = 30f; // Процент замедления от заморозки
    [HideInInspector]
    public int damage; // Урон назначается пушкой при выстреле

    private Vector2 startPosition;
    private Vector2 endPosition;
    private float t; // Прогресс для Lerp
    private Vector2 distance; // Растояние между стартовой и конечной точкой
    private Vector2 direction;
    private Quaternion rotation;

    void Start()
    {
        startPosition = transform.position;
        endPosition = Vector2.zero;
    }

    private void FixedUpdate()
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

            direction = endPosition - startPosition;
            //float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float angle = Vector2.SignedAngle(Vector2.right, direction);
            rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, t);
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

                var ps = GetComponentInChildren<ParticleSystem>();
                var em = ps.emission;
                em.enabled = false; // Отключаем дым
                transform.DetachChildren(); // переносим объект на world space
            }
            else
            {
                if (targetPosition != null)
                {
                    if (frost)
                    {
                        targetPosition.gameObject.GetComponent<EnemyController>().Health(damage, true);
                    }
                    else
                    {
                        targetPosition.gameObject.GetComponent<EnemyController>().Health(damage);
                    }
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