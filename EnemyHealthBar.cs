using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Transform healthBarFill;
     private Transform cam;

    private HealthSystem healthSystem;
    private Canvas canvas;

    private void Awake()
    {
        healthSystem = GetComponentInParent<HealthSystem>();
        canvas = GetComponent<Canvas>();
        cam = Camera.main.transform;

        healthSystem.OnHealthChanged += UpdateBar;

        // Hide bar at full HP
        canvas.enabled = false;
    }

    private void LateUpdate()
    {
        // Always face the camera
        transform.rotation = Quaternion.LookRotation(
            transform.position - cam.position
        );
    }

    private void OnDestroy()
    {
        healthSystem.OnHealthChanged -= UpdateBar;
    }

    private void UpdateBar()
    {
        canvas.enabled = true;
        healthBarFill.transform.localScale = new Vector3(
        healthSystem.GetHealthNormalized(), 1f, 1f
    );
    }
}