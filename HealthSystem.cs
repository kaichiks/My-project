using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour
{
    public event EventHandler OnHealthChanged;
    public event EventHandler OnDeath;

    [SerializeField] private int maxHP = 100;
    [SerializeField] private float healthDecreaseRate = 0f;
    [SerializeField] private bool destroyOnDeath = false;

    private float currentHP;
    private bool isDead = false;

    private void Awake()
    {
        currentHP = maxHP;
    }

    private void Update()
    {
        if (isDead || healthDecreaseRate <= 0f) return;

        currentHP -= healthDecreaseRate * Time.deltaTime;
        currentHP = Mathf.Max(currentHP, 0);
        OnHealthChanged?.Invoke(this, EventArgs.Empty);

        if (currentHP <= 0)
            HandleDeath();
    }
 
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHP = Mathf.Clamp(currentHP - amount, 0, maxHP);
        OnHealthChanged?.Invoke(this, EventArgs.Empty);

        if (currentHP <= 0)
            HandleDeath();
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        OnHealthChanged?.Invoke(this, EventArgs.Empty);
    }

    private void HandleDeath()
    {
        if (isDead) return;
        isDead = true;

        OnDeath?.Invoke(this, EventArgs.Empty);

        if (destroyOnDeath)
            Destroy(gameObject);
    }

    public float GetHealthNormalized() => currentHP / maxHP;
    public bool IsDead() => isDead;
}
