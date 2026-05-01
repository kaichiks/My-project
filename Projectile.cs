using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float damage = 10f;  
    private bool isEnemyProjectile;

    public void Init(bool isEnemy)
    {
        isEnemyProjectile = isEnemy;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isEnemyProjectile && other.CompareTag("Player"))
        {
            other.GetComponent<HealthSystem>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (!isEnemyProjectile && other.CompareTag("Enemy"))
        {
            other.GetComponentInParent<HealthSystem>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
