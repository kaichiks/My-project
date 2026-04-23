using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 20f;

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        HealthSystem healthSystem = other.GetComponent<HealthSystem>();

        if(healthSystem != null )
        {
            healthSystem.TakeDamage(damage);
           
        }
        Destroy(gameObject);
    }
}
