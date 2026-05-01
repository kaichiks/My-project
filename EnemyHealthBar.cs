using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBarFill;  

    private Transform cam;
    private HealthSystem healthSystem;
    private Canvas canvas;

    private void Awake()
    {
        healthSystem = GetComponentInParent<HealthSystem>();
        canvas = GetComponent<Canvas>();
        cam = Camera.main.transform;

        healthSystem.OnHealthChanged += UpdateBar;

        UpdateBar(this, System.EventArgs.Empty);
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.LookRotation(
            transform.position - cam.position
        );
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
            healthSystem.OnHealthChanged -= UpdateBar;
    }

    private void UpdateBar(object sender, System.EventArgs e)
    {
        if (healthBarFill != null && healthSystem != null)
            healthBarFill.fillAmount = healthSystem.GetHealthNormalized();
    }
}
