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

    [Header("Edad")]

    [Range(1f, 100f)]
    public float Age = 1f;

    [Range(0f, 100f)]
    public float AgeProgress = 0f;

    private const float MIN_AGE = 1f;
    private const float MAX_AGE = 100f;
    private const float AGE_PROGRESS_REQUIRED = 100f;
    private const float AGE_INCREASE = 5f;

    [Header("Decay Per Minute")]

    [SerializeField]
    private float hungerDecay = 2f;

    [SerializeField]
    private float energyDecay = 1.5f;

    [SerializeField]
    private float happinessDecay = 0.8f;

    [Header("Health")]

    [SerializeField]
    private float healthDecayWhenStarving = 1.5f;

    [SerializeField]
    private float healthDecayWhenExhausted = 1f;

    [SerializeField]
    private float healthRecovery = 0.25f;

    [Header("Hygiene")]

    [Range(0f, 100f)]
    public float cleanliness = 100f;

    [Min(0f)]
    public float dirtyAfterSeconds = 300f;

    [Min(0f)]
    public float cleanRate = 12f;

    public PouState CurrentState { get; private set; } = PouState.Normal;

    // =========================================================
    // EVENTOS
    // =========================================================

    public event Action<PouState> OnStateChanged;

    public event Action<float> OnFed;

    public event Action OnWentToSleep;

    public event Action<float> OnAgeChanged;

    public event Action<float> OnAgeIncreased;

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        UpdateStats();
        UpdateState();
    }

    // =========================================================
    // DECAY
    // =========================================================

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

        if (hunger > 60f &&
            energy > 40f &&
            happiness > 40f)
        {
            health += healthRecovery * multiplier;
        }

        hunger = Mathf.Clamp(hunger, 0f, 100f);
        health = Mathf.Clamp(health, 0f, 100f);
        energy = Mathf.Clamp(energy, 0f, 100f);
        happiness = Mathf.Clamp(happiness, 0f, 100f);
        cleanliness = Mathf.Clamp(cleanliness, 0f, 100f);
    }

    // =========================================================
    // STATE
    // =========================================================

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

    // =========================================================
    // COMIDA
    // =========================================================

    public void Feed(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        float previousHunger = hunger;

        hunger += amount;
        hunger = Mathf.Clamp(hunger, 0f, 100f);

        float realIncrease = hunger - previousHunger;

        AddAgeProgress(realIncrease);

        OnFed?.Invoke(realIncrease);
    }

    // =========================================================
    // EDAD
    // =========================================================

    public void AddAgeProgress(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        if (Age >= MAX_AGE)
        {
            Age = MAX_AGE;
            AgeProgress = 0f;
            return;
        }

        AgeProgress += amount;

        bool ageIncreased = false;

        while (AgeProgress >= AGE_PROGRESS_REQUIRED &&
               Age < MAX_AGE)
        {
            AgeProgress -= AGE_PROGRESS_REQUIRED;

            // -----------------------------------------------
            // SALTO AL SIGUIENTE MÚLTIPLO DE 5
            //
            // Ejemplos:
            //   Age = 1  -> Age = 5
            //   Age = 5  -> Age = 10
            //   Age = 10 -> Age = 15
            //   Age = 95 -> Age = 100
            // -----------------------------------------------

            float nextAge =
                Mathf.Floor(Age / AGE_INCREASE + 1f) *
                AGE_INCREASE;

            Age = Mathf.Min(nextAge, MAX_AGE);

            ageIncreased = true;

            OnAgeChanged?.Invoke(Age);
        }

        Age = Mathf.Clamp(Age, MIN_AGE, MAX_AGE);
        AgeProgress = Mathf.Clamp(AgeProgress, 0f, 100f);

        if (ageIncreased)
        {
            OnAgeIncreased?.Invoke(Age);
        }

        if (Age >= MAX_AGE)
        {
            Age = MAX_AGE;
            AgeProgress = 0f;
        }
    }

    public float GetAgeNormalized()
    {
        return Mathf.InverseLerp(MIN_AGE, MAX_AGE, Age);
    }

    // =========================================================
    // RECUPERACIONES (TAMBIÉN ENVEJECEN)
    // =========================================================

    public void RestoreHealth(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        float previousHealth = health;

        health += amount;
        health = Mathf.Clamp(health, 0f, 100f);

        float realIncrease = health - previousHealth;

        AddAgeProgress(realIncrease);
    }

    public void RestoreEnergy(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        float previousEnergy = energy;

        energy += amount;
        energy = Mathf.Clamp(energy, 0f, 100f);

        float realIncrease = energy - previousEnergy;

        AddAgeProgress(realIncrease);
    }

    public void AddHappiness(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        float previousHappiness = happiness;

        happiness += amount;
        happiness = Mathf.Clamp(happiness, 0f, 100f);

        float realIncrease = happiness - previousHappiness;

        AddAgeProgress(realIncrease);
    }

    public void Play(
        float happinessAmount,
        float energyCost)
    {
        float previousHappiness = happiness;

        happiness += happinessAmount;
        happiness = Mathf.Clamp(happiness, 0f, 100f);

        energy -= energyCost;
        energy = Mathf.Clamp(energy, 0f, 100f);

        float realHappinessIncrease = happiness - previousHappiness;

        AddAgeProgress(realHappinessIncrease);
    }

    public void Sleep(float energyAmount)
    {
        if (energyAmount <= 0f)
        {
            return;
        }

        float previousEnergy = energy;

        energy += energyAmount;
        energy = Mathf.Clamp(energy, 0f, 100f);

        float realIncrease = energy - previousEnergy;

        AddAgeProgress(realIncrease);

        OnWentToSleep?.Invoke();
    }

    // =========================================================
    // LIMPIEZA
    // =========================================================

    public void ApplyWaterCleaning(
        float secondsOfWaterImpact)
    {
        float previousCleanliness = cleanliness;

        cleanliness = Mathf.MoveTowards(
            cleanliness,
            100f,
            cleanRate * Mathf.Max(0f, secondsOfWaterImpact)
        );

        float realIncrease = cleanliness - previousCleanliness;

        AddAgeProgress(realIncrease);
    }

    // =========================================================
    // CARGAR PARTIDA
    // =========================================================

    public void ApplyLoadedData(
        float loadedHunger,
        float loadedHealth,
        float loadedEnergy,
        float loadedHappiness,
        float loadedCleanliness,
        float loadedAge,
        float loadedAgeProgress)
    {
        float previousAge = Age;

        hunger = Mathf.Clamp(loadedHunger, 0f, 100f);
        health = Mathf.Clamp(loadedHealth, 0f, 100f);
        energy = Mathf.Clamp(loadedEnergy, 0f, 100f);
        happiness = Mathf.Clamp(loadedHappiness, 0f, 100f);
        cleanliness = Mathf.Clamp(loadedCleanliness, 0f, 100f);

        Age = Mathf.Clamp(loadedAge, MIN_AGE, MAX_AGE);
        AgeProgress = Mathf.Clamp(loadedAgeProgress, 0f, 100f);

        if (Age >= MAX_AGE)
        {
            Age = MAX_AGE;
            AgeProgress = 0f;
        }

        UpdateState();

        if (!Mathf.Approximately(previousAge, Age))
        {
            OnAgeChanged?.Invoke(Age);
        }
    }
}