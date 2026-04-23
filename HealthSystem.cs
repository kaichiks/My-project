using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private float maxHP = 100f;
    private float currentHP;

    public event Action OnHealthChanged;

    private void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        OnHealthChanged?.Invoke();
        Debug.Log(gameObject.name + " HP: " + currentHP);

        if (currentHP <= 0)
        {
            Destroy(gameObject);
        }
    }

    public float GetHealthNormalized()
    {
        return currentHP / maxHP;
    }
}