using System;
using UnityEngine;

public enum PouState
{
    Normal,
    Hungry,
    Tired,
    Sad,
    Sick,
    Dead
}

public class PouStats : MonoBehaviour
{
    [Header("Stats")]
    [Range(0f, 100f)]
    public float hunger = 100f;

    [Range(0f, 100f)]
    public float health = 100f;

    [Range(0f, 100f)]
    public float energy = 100f;

    [Range(0f, 100f)]
    public float happiness = 100f;

    [Header("Decay Per Minute")]
    [SerializeField] private float hungerDecay = 2f;
    [SerializeField] private float energyDecay = 1.5f;
    [SerializeField] private float happinessDecay = 0.8f;

    [Header("Health")]
    [SerializeField] private float healthDecayWhenStarving = 1.5f;
    [SerializeField] private float healthDecayWhenExhausted = 1f;
    [SerializeField] private float healthRecovery = 0.25f;

    public PouState CurrentState { get; private set; } = PouState.Normal;

    public event Action<PouState> OnStateChanged;

    private void Update()
    {
        UpdateStats();
        UpdateState();
    }

    private void UpdateStats()
    {
        float multiplier = Time.deltaTime / 60f;

        hunger -= hungerDecay * multiplier;
        energy -= energyDecay * multiplier;
        happiness -= happinessDecay * multiplier;

        if (hunger <= 15f)
        {
            health -= healthDecayWhenStarving * multiplier;
        }

        if (energy <= 10f)
        {
            health -= healthDecayWhenExhausted * multiplier;
        }

        if (hunger > 60f && energy > 40f && happiness > 40f)
        {
            health += healthRecovery * multiplier;
        }

        hunger = Mathf.Clamp(hunger, 0f, 100f);
        health = Mathf.Clamp(health, 0f, 100f);
        energy = Mathf.Clamp(energy, 0f, 100f);
        happiness = Mathf.Clamp(happiness, 0f, 100f);
    }

    private void UpdateState()
    {
        PouState newState;

        if (health <= 0f)
        {
            newState = PouState.Dead;
        }
        else if (health <= 25f)
        {
            newState = PouState.Sick;
        }
        else if (hunger <= 15f)
        {
            newState = PouState.Hungry;
        }
        else if (energy <= 15f)
        {
            newState = PouState.Tired;
        }
        else if (happiness <= 20f)
        {
            newState = PouState.Sad;
        }
        else
        {
            newState = PouState.Normal;
        }

        if (newState != CurrentState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(CurrentState);
        }
    }

    public void Feed(float amount)
    {
        hunger += amount;
        hunger = Mathf.Clamp(hunger, 0f, 100f);
    }

    public void RestoreHealth(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0f, 100f);
    }

    public void RestoreEnergy(float amount)
    {
        energy += amount;
        energy = Mathf.Clamp(energy, 0f, 100f);
    }

    public void AddHappiness(float amount)
    {
        happiness += amount;
        happiness = Mathf.Clamp(happiness, 0f, 100f);
    }

    public void Play(float happinessAmount, float energyCost)
    {
        happiness += happinessAmount;
        energy -= energyCost;

        happiness = Mathf.Clamp(happiness, 0f, 100f);
        energy = Mathf.Clamp(energy, 0f, 100f);
    }

    public void Sleep(float energyAmount)
    {
        energy += energyAmount;
        energy = Mathf.Clamp(energy, 0f, 100f);
    }
}