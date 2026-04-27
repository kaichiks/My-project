using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour
{
    public event EventHandler OnHealthChanged;
    public event EventHandler OnDeath;

    [SerializeField] private int maxHP = 100;
    [SerializeField] private float healthDecreaseRate = 5f; // HP lost per second

    private float currentHP;

    private void Awake()
    {
        currentHP = maxHP;
    }

    private void Update()
    {
        if (currentHP <= 0) return;

        currentHP -= healthDecreaseRate * Time.deltaTime;
        currentHP = Mathf.Max(currentHP, 0);
        OnHealthChanged?.Invoke(this, EventArgs.Empty);

        if (currentHP <= 0)
            OnDeath?.Invoke(this, EventArgs.Empty);
    }

    public void TakeDamage(int amount)
    {
        if (currentHP <= 0) return;

        currentHP = Mathf.Clamp(currentHP - amount, 0, maxHP);
        OnHealthChanged?.Invoke(this, EventArgs.Empty);

        if (currentHP <= 0)
            OnDeath?.Invoke(this, EventArgs.Empty);
    }

    public void Heal(float amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        OnHealthChanged?.Invoke(this, EventArgs.Empty);
    }

    public float GetHealthNormalized() => currentHP / maxHP;

    public bool IsDead() => currentHP <= 0;
}
