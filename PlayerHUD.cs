using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private Transform healthBarFill;
    private HealthSystem healthSystem;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        healthSystem.OnHealthChanged += UpdateHUD;
    }

    private void OnDestroy()
    {
        healthSystem.OnHealthChanged -= UpdateHUD;
    }

    private void UpdateHUD()
    {
        healthBarFill.transform.localScale = new Vector3(
         healthSystem.GetHealthNormalized(), 1f, 1f
     );
    }
}