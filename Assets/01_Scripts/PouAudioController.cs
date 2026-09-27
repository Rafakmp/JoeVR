using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Control de sonidos del Pou.
///
/// Categorías:
/// - Sonidos casuales / Idle
/// - Sonidos al ser agarrado
/// - Hambre
/// - Comer
/// - Cansancio
/// - Bostezos
/// - Ronquidos
/// - Tristeza
/// - Enfermedad
/// - Recuperación de energía
/// - Recuperación de felicidad
/// - Recuperación de salud
///
/// Todos los sonidos utilizan el mismo AudioSource.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PouAudioController : MonoBehaviour
{
    private enum ComplaintType
    {
        None,
        Hungry,
        Tired,
        Sad,
        Sick
    }

    // =========================================================
    // REFERENCIAS
    // =========================================================

    [Header("Referencias")]

    [SerializeField]
    private PouStats pouStats;

    [Tooltip("Punto ubicado en la boca de Pou.")]
    [SerializeField]
    private Transform mouthAudioPoint;

    [Tooltip("AudioSource que reproduce todos los sonidos.")]
    [SerializeField]
    private AudioSource audioSource;

    [Tooltip("XR Grab Interactable del Pou.")]
    [SerializeField]
    private XRGrabInteractable grabInteractable;

    // =========================================================
    // SONIDOS CASUALES / IDLE
    // =========================================================

    [Header("Sonidos casuales / Idle")]

    [Tooltip(
        "Sonidos que Pou puede hacer de vez en cuando " +
        "y también puede decir al ser agarrado."
    )]
    [SerializeField]
    private AudioClip[] idleSounds;

    [Tooltip("Tiempo mínimo entre sonidos casuales.")]
    [SerializeField]
    private float idleMinInterval = 15f;

    [Tooltip("Tiempo máximo entre sonidos casuales.")]
    [SerializeField]
    private float idleMaxInterval = 30f;

    [Tooltip(
        "Probabilidad de que haga un sonido casual " +
        "cuando llegue el momento."
    )]
    [Range(0f, 1f)]
    [SerializeField]
    private float idleChance = 0.75f;

    [Tooltip(
        "Desactiva los sonidos casuales cuando las necesidades " +
        "son demasiado bajas."
    )]
    [SerializeField]
    private bool disableIdleWhenNeedsAreCritical = true;

    // =========================================================
    // HAMBRE
    // =========================================================

    [Header("Hambre")]

    [SerializeField]
    private AudioClip[] hungrySounds;

    [SerializeField]
    private Vector2 hungryInterval =
        new Vector2(6f, 13f);

    [SerializeField]
    private float hungerComplaintStart = 45f;

    // =========================================================
    // COMER
    // =========================================================

    [Header("Al comer")]

    [SerializeField]
    private AudioClip[] eatSounds;

    // =========================================================
    // CANSANCIO
    // =========================================================

    [Header("Cansancio")]

    [SerializeField]
    private AudioClip[] tiredSounds;

    [SerializeField]
    private Vector2 tiredInterval =
        new Vector2(8f, 16f);

    [SerializeField]
    private float tiredComplaintStart = 40f;

    // =========================================================
    // BOSTEZOS
    // =========================================================

    [Header("Bostezos")]

    [SerializeField]
    private AudioClip[] yawnSounds;

    [SerializeField]
    private Vector2 yawnInterval =
        new Vector2(12f, 24f);

    // =========================================================
    // RONQUIDOS
    // =========================================================

    [Header("Ronquidos")]

    [SerializeField]
    private AudioClip[] snoreSounds;

    [SerializeField]
    private float asleepEnergyThreshold = 8f;

    [SerializeField]
    private Vector2 snoreInterval =
        new Vector2(4f, 9f);

    // =========================================================
    // TRISTEZA
    // =========================================================

    [Header("Tristeza")]

    [SerializeField]
    private AudioClip[] sadSounds;

    [SerializeField]
    private Vector2 sadInterval =
        new Vector2(10f, 20f);

    [SerializeField]
    private float sadComplaintStart = 40f;

    // =========================================================
    // ENFERMEDAD
    // =========================================================

    [Header("Enfermedad")]

    [SerializeField]
    private AudioClip[] sickSounds;

    [SerializeField]
    private Vector2 sickInterval =
        new Vector2(8f, 15f);

    [SerializeField]
    private float sickComplaintStart = 60f;

    // =========================================================
    // RECUPERACIÓN DE ENERGÍA
    // =========================================================

    [Header("Recuperación de energía")]

    [Tooltip(
        "Sonidos cuando la energía de Pou aumenta."
    )]
    [SerializeField]
    private AudioClip[] energyRecoverySounds;

    [Tooltip(
        "Cantidad de energía que debe recuperar " +
        "antes de reproducir un sonido."
    )]
    [SerializeField]
    private float energyRecoveryStep = 8f;

    [SerializeField]
    private Vector2 energyRecoveryInterval =
        new Vector2(10f, 18f);

    // =========================================================
    // RECUPERACIÓN DE FELICIDAD
    // =========================================================

    [Header("Recuperación de felicidad")]

    [Tooltip(
        "Sonidos cuando la felicidad de Pou aumenta."
    )]
    [SerializeField]
    private AudioClip[] happinessRecoverySounds;

    [Tooltip(
        "Cantidad de felicidad que debe aumentar " +
        "antes de reproducir un sonido."
    )]
    [SerializeField]
    private float happinessRecoveryStep = 5f;

    [SerializeField]
    private Vector2 happinessRecoveryInterval =
        new Vector2(8f, 16f);

    // =========================================================
    // RECUPERACIÓN DE SALUD
    // =========================================================

    [Header("Recuperación de salud")]

    [Tooltip(
        "Sonidos cuando la salud de Pou aumenta."
    )]
    [SerializeField]
    private AudioClip[] healthRecoverySounds;

    [Tooltip(
        "Cantidad de salud que debe recuperar " +
        "antes de reproducir un sonido."
    )]
    [SerializeField]
    private float healthRecoveryStep = 5f;

    [SerializeField]
    private Vector2 healthRecoveryInterval =
        new Vector2(10f, 20f);

    // =========================================================
    // AUDIO
    // =========================================================

    [Header("Audio")]

    [Range(0f, 1f)]
    [SerializeField]
    private float masterVolume = 1f;

    [SerializeField]
    private float minDistance = 0.7f;

    [SerializeField]
    private float maxDistance = 12f;

    [SerializeField]
    private float basePitch = 1f;

    [Tooltip(
        "Variación aleatoria de tono."
    )]
    [SerializeField]
    private float pitchVariation = 0.05f;

    [Tooltip(
        "Evita que varios sonidos se reproduzcan al mismo tiempo."
    )]
    [SerializeField]
    private bool preventOverlappingSounds = true;

    // =========================================================
    // AGARRAR
    // =========================================================

    [Header("Al agarrar")]

    [SerializeField]
    private bool playIdleSoundWhenGrabbed = true;

    [Tooltip(
        "Tiempo mínimo entre sonidos causados por agarrar a Pou."
    )]
    [SerializeField]
    private float grabCooldown = 2f;

    private float lastGrabSoundTime = -Mathf.Infinity;

    // =========================================================
    // TIEMPOS
    // =========================================================

    private float nextIdleTime;
    private float nextComplaintTime;
    private float nextSnoreTime;
    private float nextYawnTime;

    private float nextEnergyRecoveryTime;
    private float nextHappinessRecoveryTime;
    private float nextHealthRecoveryTime;

    // =========================================================
    // ENERGÍA
    // =========================================================

    private float previousEnergy;
    private float accumulatedEnergyRecovery;

    // =========================================================
    // FELICIDAD
    // =========================================================

    private float previousHappiness;
    private float accumulatedHappinessRecovery;

    // =========================================================
    // SALUD
    // =========================================================

    private float previousHealth;
    private float accumulatedHealthRecovery;

    // =========================================================
    // ESTADO
    // =========================================================

    private PouState previousState;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (pouStats == null)
        {
            pouStats =
                GetComponent<PouStats>();
        }

        if (audioSource == null)
        {
            audioSource =
                GetComponent<AudioSource>();
        }

        if (grabInteractable == null)
        {
            grabInteractable =
                GetComponent<XRGrabInteractable>();
        }

        SetupAudioSource();

        if (pouStats != null)
        {
            previousEnergy =
                pouStats.energy;

            previousHappiness =
                pouStats.happiness;

            previousHealth =
                pouStats.health;

            previousState =
                pouStats.CurrentState;
        }
    }

    // =========================================================
    // ENABLE
    // =========================================================

    private void OnEnable()
    {
        if (pouStats != null)
        {
            pouStats.OnFed += HandleFed;

            pouStats.OnWentToSleep +=
                HandleWentToSleep;

            pouStats.OnStateChanged +=
                HandleStateChanged;
        }

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered
                .AddListener(OnPouGrabbed);
        }
    }

    // =========================================================
    // DISABLE
    // =========================================================

    private void OnDisable()
    {
        if (pouStats != null)
        {
            pouStats.OnFed -= HandleFed;

            pouStats.OnWentToSleep -=
                HandleWentToSleep;

            pouStats.OnStateChanged -=
                HandleStateChanged;
        }

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered
                .RemoveListener(OnPouGrabbed);
        }
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        ScheduleNextIdle();

        nextComplaintTime =
            Time.time +
            Random.Range(3f, 7f);

        nextSnoreTime =
            Time.time +
            Random.Range(3f, 6f);

        nextYawnTime =
            Time.time +
            Random.Range(6f, 12f);

        nextEnergyRecoveryTime =
            Time.time +
            Random.Range(6f, 12f);

        nextHappinessRecoveryTime =
            Time.time +
            Random.Range(6f, 12f);

        nextHealthRecoveryTime =
            Time.time +
            Random.Range(8f, 15f);
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (pouStats == null ||
            audioSource == null)
        {
            return;
        }

        if (pouStats.CurrentState ==
            PouState.Dead)
        {
            return;
        }

        // -----------------------------------------------------
        // RECUPERACIONES
        // -----------------------------------------------------

        UpdateEnergyRecoverySounds();

        UpdateHappinessRecoverySounds();

        UpdateHealthRecoverySounds();

        // -----------------------------------------------------
        // RONQUIDOS
        // -----------------------------------------------------

        if (pouStats.energy <=
            asleepEnergyThreshold)
        {
            TryPlay(
                snoreSounds,
                snoreInterval,
                ref nextSnoreTime
            );

            ScheduleNextIdle();

            return;
        }

        // -----------------------------------------------------
        // QUEJAS
        // -----------------------------------------------------

        bool madeComplaint =
            TryPlayContextualComplaint();

        // -----------------------------------------------------
        // BOSTEZOS
        // -----------------------------------------------------

        if (!madeComplaint &&
            pouStats.energy <= 35f)
        {
            TryPlay(
                yawnSounds,
                yawnInterval,
                ref nextYawnTime
            );
        }

        // -----------------------------------------------------
        // SONIDOS IDLE
        // -----------------------------------------------------

        TryPlayIdle();
    }

    // =========================================================
    // CONFIGURACIÓN DEL AUDIO
    // =========================================================

    private void SetupAudioSource()
    {
        if (audioSource == null)
        {
            return;
        }

        // Mover el AudioSource al punto de la boca.
        if (mouthAudioPoint != null &&
            audioSource.transform != mouthAudioPoint)
        {
            audioSource.transform.SetParent(
                mouthAudioPoint,
                false
            );
        }

        audioSource.playOnAwake = false;

        audioSource.loop = false;

        // Audio 3D.
        audioSource.spatialBlend = 1f;

        audioSource.spatialize = true;

        audioSource.dopplerLevel = 0f;

        audioSource.minDistance =
            minDistance;

        audioSource.maxDistance =
            maxDistance;

        audioSource.rolloffMode =
            AudioRolloffMode.Logarithmic;

        audioSource.volume =
            masterVolume;

        audioSource.pitch =
            basePitch;
    }

    // =========================================================
    // AL AGARRAR
    // =========================================================

    private void OnPouGrabbed(
        SelectEnterEventArgs args)
    {
        if (!playIdleSoundWhenGrabbed)
        {
            return;
        }

        if (idleSounds == null ||
            idleSounds.Length == 0)
        {
            return;
        }

        if (Time.time -
            lastGrabSoundTime <
            grabCooldown)
        {
            return;
        }

        lastGrabSoundTime =
            Time.time;

        // Al agarrarlo, utiliza uno de los mismos
        // sonidos casuales.
        PlayRandomForced(
            idleSounds
        );

        ScheduleNextIdle();
    }

    // =========================================================
    // SONIDOS IDLE
    // =========================================================

    private void TryPlayIdle()
    {
        if (Time.time <
            nextIdleTime)
        {
            return;
        }

        ScheduleNextIdle();

        if (idleSounds == null ||
            idleSounds.Length == 0)
        {
            return;
        }

        if (disableIdleWhenNeedsAreCritical &&
            HasCriticalNeed())
        {
            return;
        }

        if (preventOverlappingSounds &&
            audioSource.isPlaying)
        {
            return;
        }

        if (Random.value >
            idleChance)
        {
            return;
        }

        PlayRandom(
            idleSounds
        );
    }

    private void ScheduleNextIdle()
    {
        nextIdleTime =
            Time.time +
            Random.Range(
                idleMinInterval,
                idleMaxInterval
            );
    }

    // =========================================================
    // NECESIDADES CRÍTICAS
    // =========================================================

    private bool HasCriticalNeed()
    {
        if (pouStats.hunger <= 20f)
        {
            return true;
        }

        if (pouStats.energy <= 20f)
        {
            return true;
        }

        if (pouStats.happiness <= 20f)
        {
            return true;
        }

        if (pouStats.health <= 25f)
        {
            return true;
        }

        return false;
    }

    // =========================================================
    // QUEJAS CONTEXTUALES
    // =========================================================

    private bool TryPlayContextualComplaint()
    {
        if (Time.time <
            nextComplaintTime)
        {
            return false;
        }

        ComplaintType complaint =
            DetermineComplaintType();

        if (complaint ==
            ComplaintType.None)
        {
            ScheduleNextComplaint(
                new Vector2(8f, 15f)
            );

            return false;
        }

        AudioClip[] clips =
            GetClipsForComplaint(
                complaint
            );

        Vector2 interval =
            GetIntervalForComplaint(
                complaint
            );

        if (clips == null ||
            clips.Length == 0)
        {
            ScheduleNextComplaint(
                interval
            );

            return false;
        }

        if (preventOverlappingSounds &&
            audioSource.isPlaying)
        {
            return false;
        }

        PlayRandom(clips);

        ScheduleNextComplaint(
            interval
        );

        ScheduleNextIdle();

        return true;
    }

    // =========================================================
    // DECIDIR QUEJA
    // =========================================================

    private ComplaintType DetermineComplaintType()
    {
        float healthUrgency =
            CalculateUrgency(
                pouStats.health,
                sickComplaintStart
            );

        float hungerUrgency =
            CalculateUrgency(
                pouStats.hunger,
                hungerComplaintStart
            );

        float energyUrgency =
            CalculateUrgency(
                pouStats.energy,
                tiredComplaintStart
            );

        float happinessUrgency =
            CalculateUrgency(
                pouStats.happiness,
                sadComplaintStart
            );

        healthUrgency *= 1.35f;

        if (pouStats.CurrentState ==
            PouState.Sick)
        {
            healthUrgency *= 1.25f;
        }

        if (pouStats.CurrentState ==
            PouState.Hungry)
        {
            hungerUrgency *= 1.2f;
        }

        if (pouStats.CurrentState ==
            PouState.Tired)
        {
            energyUrgency *= 1.2f;
        }

        if (pouStats.CurrentState ==
            PouState.Sad)
        {
            happinessUrgency *= 1.2f;
        }

        float total =
            healthUrgency +
            hungerUrgency +
            energyUrgency +
            happinessUrgency;

        if (total < 0.2f)
        {
            return ComplaintType.None;
        }

        float roll =
            Random.Range(
                0f,
                total
            );

        if ((roll -= healthUrgency) <= 0f)
        {
            return ComplaintType.Sick;
        }

        if ((roll -= hungerUrgency) <= 0f)
        {
            return ComplaintType.Hungry;
        }

        if ((roll -= energyUrgency) <= 0f)
        {
            return ComplaintType.Tired;
        }

        return ComplaintType.Sad;
    }

    // =========================================================
    // INTENSIDAD
    // =========================================================

    private float CalculateUrgency(
        float value,
        float startThreshold)
    {
        if (value >= startThreshold)
        {
            return 0f;
        }

        return Mathf.Clamp01(
            Mathf.InverseLerp(
                startThreshold,
                0f,
                value
            )
        );
    }

    // =========================================================
    // CLIPS
    // =========================================================

    private AudioClip[] GetClipsForComplaint(
        ComplaintType type)
    {
        switch (type)
        {
            case ComplaintType.Hungry:
                return hungrySounds;

            case ComplaintType.Tired:
                return tiredSounds;

            case ComplaintType.Sad:
                return sadSounds;

            case ComplaintType.Sick:
                return sickSounds;

            default:
                return null;
        }
    }

    // =========================================================
    // INTERVALOS
    // =========================================================

    private Vector2 GetIntervalForComplaint(
        ComplaintType type)
    {
        switch (type)
        {
            case ComplaintType.Hungry:
                return hungryInterval;

            case ComplaintType.Tired:
                return tiredInterval;

            case ComplaintType.Sad:
                return sadInterval;

            case ComplaintType.Sick:
                return sickInterval;

            default:
                return new Vector2(8f, 15f);
        }
    }

    // =========================================================
    // PRÓXIMA QUEJA
    // =========================================================

    private void ScheduleNextComplaint(
        Vector2 interval)
    {
        nextComplaintTime =
            Time.time +
            Random.Range(
                interval.x,
                interval.y
            );
    }

    // =========================================================
    // RECUPERACIÓN DE ENERGÍA
    // =========================================================

    private void UpdateEnergyRecoverySounds()
    {
        float current =
            pouStats.energy;

        float difference =
            current -
            previousEnergy;

        if (difference > 0f)
        {
            accumulatedEnergyRecovery +=
                difference;
        }

        previousEnergy =
            current;

        if (accumulatedEnergyRecovery <
            energyRecoveryStep)
        {
            return;
        }

        if (Time.time <
            nextEnergyRecoveryTime)
        {
            return;
        }

        if (energyRecoverySounds == null ||
            energyRecoverySounds.Length == 0)
        {
            accumulatedEnergyRecovery = 0f;

            ScheduleNextEnergyRecovery();

            return;
        }

        if (pouStats.energy >
            asleepEnergyThreshold)
        {
            if (!preventOverlappingSounds ||
                !audioSource.isPlaying)
            {
                PlayRandom(
                    energyRecoverySounds
                );

                ScheduleNextIdle();
            }
        }

        accumulatedEnergyRecovery = 0f;

        ScheduleNextEnergyRecovery();
    }

    private void ScheduleNextEnergyRecovery()
    {
        nextEnergyRecoveryTime =
            Time.time +
            Random.Range(
                energyRecoveryInterval.x,
                energyRecoveryInterval.y
            );
    }

    // =========================================================
    // RECUPERACIÓN DE FELICIDAD
    // =========================================================

    private void UpdateHappinessRecoverySounds()
    {
        float current =
            pouStats.happiness;

        float difference =
            current -
            previousHappiness;

        if (difference > 0f)
        {
            accumulatedHappinessRecovery +=
                difference;
        }

        previousHappiness =
            current;

        if (accumulatedHappinessRecovery <
            happinessRecoveryStep)
        {
            return;
        }

        if (Time.time <
            nextHappinessRecoveryTime)
        {
            return;
        }

        if (happinessRecoverySounds == null ||
            happinessRecoverySounds.Length == 0)
        {
            accumulatedHappinessRecovery = 0f;

            ScheduleNextHappinessRecovery();

            return;
        }

        if (!preventOverlappingSounds ||
            !audioSource.isPlaying)
        {
            PlayRandom(
                happinessRecoverySounds
            );

            ScheduleNextIdle();
        }

        accumulatedHappinessRecovery = 0f;

        ScheduleNextHappinessRecovery();
    }

    private void ScheduleNextHappinessRecovery()
    {
        nextHappinessRecoveryTime =
            Time.time +
            Random.Range(
                happinessRecoveryInterval.x,
                happinessRecoveryInterval.y
            );
    }

    // =========================================================
    // RECUPERACIÓN DE SALUD
    // =========================================================

    private void UpdateHealthRecoverySounds()
    {
        float current =
            pouStats.health;

        float difference =
            current -
            previousHealth;

        if (difference > 0f)
        {
            accumulatedHealthRecovery +=
                difference;
        }

        previousHealth =
            current;

        if (accumulatedHealthRecovery <
            healthRecoveryStep)
        {
            return;
        }

        if (Time.time <
            nextHealthRecoveryTime)
        {
            return;
        }

        if (healthRecoverySounds == null ||
            healthRecoverySounds.Length == 0)
        {
            accumulatedHealthRecovery = 0f;

            ScheduleNextHealthRecovery();

            return;
        }

        if (!preventOverlappingSounds ||
            !audioSource.isPlaying)
        {
            PlayRandom(
                healthRecoverySounds
            );

            ScheduleNextIdle();
        }

        accumulatedHealthRecovery = 0f;

        ScheduleNextHealthRecovery();
    }

    private void ScheduleNextHealthRecovery()
    {
        nextHealthRecoveryTime =
            Time.time +
            Random.Range(
                healthRecoveryInterval.x,
                healthRecoveryInterval.y
            );
    }

    // =========================================================
    // COMER
    // =========================================================

    private void HandleFed(float amount)
    {
        if (!preventOverlappingSounds ||
            !audioSource.isPlaying)
        {
            PlayRandom(
                eatSounds
            );
        }

        accumulatedEnergyRecovery = 0f;

        nextEnergyRecoveryTime =
            Time.time +
            Random.Range(
                4f,
                8f
            );

        nextComplaintTime =
            Time.time +
            Random.Range(
                3f,
                6f
            );

        ScheduleNextIdle();
    }

    // =========================================================
    // DORMIR
    // =========================================================

    private void HandleWentToSleep()
    {
        if (!preventOverlappingSounds ||
            !audioSource.isPlaying)
        {
            PlayRandom(
                yawnSounds
            );
        }

        nextComplaintTime =
            Time.time +
            Random.Range(
                6f,
                10f
            );

        ScheduleNextIdle();
    }

    // =========================================================
    // CAMBIO DE ESTADO
    // =========================================================

    private void HandleStateChanged(
        PouState newState)
    {
        previousState =
            newState;

        if (newState ==
            PouState.Dead)
        {
            return;
        }

        if (newState !=
            PouState.Normal)
        {
            nextComplaintTime =
                Mathf.Min(
                    nextComplaintTime,
                    Time.time +
                    Random.Range(
                        0.8f,
                        2f
                    )
                );

            ScheduleNextIdle();
        }
    }

    // =========================================================
    // REPRODUCCIÓN POR INTERVALO
    // =========================================================

    private void TryPlay(
        AudioClip[] clips,
        Vector2 interval,
        ref float nextTime)
    {
        if (clips == null ||
            clips.Length == 0)
        {
            return;
        }

        if (Time.time <
            nextTime)
        {
            return;
        }

        if (preventOverlappingSounds &&
            audioSource.isPlaying)
        {
            return;
        }

        PlayRandom(clips);

        nextTime =
            Time.time +
            Random.Range(
                interval.x,
                interval.y
            );

        ScheduleNextIdle();
    }

    // =========================================================
    // SONIDO ALEATORIO
    // =========================================================

    private void PlayRandom(
        AudioClip[] clips)
    {
        if (audioSource == null ||
            clips == null ||
            clips.Length == 0)
        {
            return;
        }

        AudioClip clip =
            clips[
                Random.Range(
                    0,
                    clips.Length
                )
            ];

        if (clip == null)
        {
            return;
        }

        audioSource.pitch =
            basePitch +
            Random.Range(
                -pitchVariation,
                pitchVariation
            );

        audioSource.PlayOneShot(
            clip,
            masterVolume
        );

        audioSource.pitch =
            basePitch;
    }

    // =========================================================
    // SONIDO FORZADO
    // =========================================================

    private void PlayRandomForced(
        AudioClip[] clips)
    {
        if (audioSource == null ||
            clips == null ||
            clips.Length == 0)
        {
            return;
        }

        AudioClip clip =
            clips[
                Random.Range(
                    0,
                    clips.Length
                )
            ];

        if (clip == null)
        {
            return;
        }

        // El sonido al agarrar tiene prioridad.
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.pitch =
            basePitch +
            Random.Range(
                -pitchVariation,
                pitchVariation
            );

        audioSource.PlayOneShot(
            clip,
            masterVolume
        );

        audioSource.pitch =
            basePitch;
    }

    // =========================================================
    // API PÚBLICA
    // =========================================================

    public void PlayRecoverySound()
    {
        if (!preventOverlappingSounds ||
            !audioSource.isPlaying)
        {
            PlayRandom(
                energyRecoverySounds
            );

            ScheduleNextIdle();
        }
    }

    public void PlayIdleSound()
    {
        if (!preventOverlappingSounds ||
            !audioSource.isPlaying)
        {
            PlayRandom(
                idleSounds
            );

            ScheduleNextIdle();
        }
    }

    public void PlayHappinessRecoverySound()
    {
        if (!preventOverlappingSounds ||
            !audioSource.isPlaying)
        {
            PlayRandom(
                happinessRecoverySounds
            );

            ScheduleNextIdle();
        }
    }

    public void PlayHealthRecoverySound()
    {
        if (!preventOverlappingSounds ||
            !audioSource.isPlaying)
        {
            PlayRandom(
                healthRecoverySounds
            );

            ScheduleNextIdle();
        }
    }
}