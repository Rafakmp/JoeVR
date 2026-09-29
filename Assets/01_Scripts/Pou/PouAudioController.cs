using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

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

    [SerializeField]
    private PouExpressionController expressionController;

    [SerializeField]
    private Transform mouthAudioPoint;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private XRGrabInteractable grabInteractable;

    // =========================================================
    // IDLE
    // =========================================================

    [Header("Sonidos casuales / Idle")]

    [SerializeField]
    private AudioClip[] idleSounds;

    [SerializeField]
    private float idleMinInterval = 15f;

    [SerializeField]
    private float idleMaxInterval = 30f;

    [Range(0f, 1f)]
    [SerializeField]
    private float idleChance = 0.75f;

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

    [SerializeField]
    private AudioClip[] energyRecoverySounds;

    [SerializeField]
    private float energyRecoveryStep = 8f;

    [SerializeField]
    private Vector2 energyRecoveryInterval =
        new Vector2(10f, 18f);

    // =========================================================
    // RECUPERACIÓN DE FELICIDAD
    // =========================================================

    [Header("Recuperación de felicidad")]

    [SerializeField]
    private AudioClip[] happinessRecoverySounds;

    [SerializeField]
    private float happinessRecoveryStep = 5f;

    [SerializeField]
    private Vector2 happinessRecoveryInterval =
        new Vector2(8f, 16f);

    // =========================================================
    // RECUPERACIÓN DE SALUD
    // =========================================================

    [Header("Recuperación de salud")]

    [SerializeField]
    private AudioClip[] healthRecoverySounds;

    [SerializeField]
    private float healthRecoveryStep = 5f;

    [SerializeField]
    private Vector2 healthRecoveryInterval =
        new Vector2(10f, 20f);

    // =========================================================
    // SUBIDA DE EDAD
    // =========================================================

    [Header("Subida de edad")]

    [SerializeField]
    private AudioClip[] ageUpSounds;

    [Tooltip(
        "Si está activo, el sonido de subir de edad " +
        "se reproduce aunque ya haya otro sonando."
    )]
    [SerializeField]
    private bool forceAgeUpSound = true;

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

    [SerializeField]
    private float pitchVariation = 0.05f;

    [SerializeField]
    private bool preventOverlappingSounds = true;

    // =========================================================
    // VOZ POR EDAD
    // =========================================================

    [Header("Voz por edad")]

    [Tooltip(
        "Pitch de Pou cuando tiene 1 año."
    )]
    [SerializeField]
    private float youngestVoicePitch = 1.35f;

    [Tooltip(
        "Pitch de Pou cuando tiene 100 años."
    )]
    [SerializeField]
    private float oldestVoicePitch = 0.80f;

    // =========================================================
    // AGARRAR
    // =========================================================

    [Header("Al agarrar")]

    [SerializeField]
    private bool playIdleSoundWhenGrabbed = true;

    [SerializeField]
    private float grabCooldown = 2f;

    private float lastGrabSoundTime =
        -Mathf.Infinity;

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
    // SUEÑO
    // =========================================================

    private bool wasSleeping = false;

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

        if (expressionController == null)
        {
            expressionController =
                GetComponent<PouExpressionController>();
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
            pouStats.OnFed +=
                HandleFed;

            pouStats.OnStateChanged +=
                HandleStateChanged;

            pouStats.OnAgeIncreased +=
                HandleAgeIncreased;
        }

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered
                .AddListener(
                    OnPouGrabbed
                );
        }
    }

    // =========================================================
    // DISABLE
    // =========================================================

    private void OnDisable()
    {
        if (pouStats != null)
        {
            pouStats.OnFed -=
                HandleFed;

            pouStats.OnStateChanged -=
                HandleStateChanged;

            pouStats.OnAgeIncreased -=
                HandleAgeIncreased;
        }

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered
                .RemoveListener(
                    OnPouGrabbed
                );
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

        // =====================================================
        // RECUPERACIONES
        // =====================================================

        UpdateEnergyRecoverySounds();

        UpdateHappinessRecoverySounds();

        UpdateHealthRecoverySounds();

        // =====================================================
        // DORMIR
        // =====================================================

        bool isSleeping =
            expressionController != null &&
            expressionController.IsSleeping;

        if (isSleeping)
        {
            // ---------------------------------------------
            // ACABA DE DORMIRSE
            // ---------------------------------------------

            if (!wasSleeping)
            {
                if (!preventOverlappingSounds ||
                    !audioSource.isPlaying)
                {
                    PlayRandom(
                        snoreSounds
                    );
                }

                nextSnoreTime =
                    Time.time +
                    Random.Range(
                        snoreInterval.x,
                        snoreInterval.y
                    );
            }

            // ---------------------------------------------
            // YA ESTÁ DORMIDO
            // ---------------------------------------------

            TryPlay(
                snoreSounds,
                snoreInterval,
                ref nextSnoreTime
            );

            wasSleeping = true;

            ScheduleNextIdle();

            return;
        }

        // -----------------------------------------------------
        // DESPERTÓ
        // -----------------------------------------------------

        wasSleeping = false;

        // =====================================================
        // QUEJAS
        // =====================================================

        bool madeComplaint =
            TryPlayContextualComplaint();

        // =====================================================
        // BOSTEZOS
        // =====================================================

        if (!madeComplaint &&
            pouStats.energy <= 35f)
        {
            TryPlay(
                yawnSounds,
                yawnInterval,
                ref nextYawnTime
            );
        }

        // =====================================================
        // IDLE
        // =====================================================

        TryPlayIdle();
    }

    // =========================================================
    // SETUP AUDIO
    // =========================================================

    private void SetupAudioSource()
    {
        if (audioSource == null)
        {
            return;
        }

        if (mouthAudioPoint != null &&
            audioSource.transform !=
            mouthAudioPoint)
        {
            audioSource.transform.SetParent(
                mouthAudioPoint,
                false
            );
        }

        audioSource.playOnAwake = false;

        audioSource.loop = false;

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
    // GRAB
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

        PlayRandomForced(
            idleSounds
        );

        ScheduleNextIdle();
    }

    // =========================================================
    // IDLE
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
    // CRITICAL
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
    // COMPLAINT
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

        PlayRandom(
            clips
        );

        ScheduleNextComplaint(
            interval
        );

        ScheduleNextIdle();

        return true;
    }

    // =========================================================
    // DETERMINE COMPLAINT
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
    // URGENCY
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
    // INTERVALS
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
                return new Vector2(
                    8f,
                    15f
                );
        }
    }

    // =========================================================
    // NEXT COMPLAINT
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
    // ENERGY RECOVERY
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

        if (expressionController == null ||
            !expressionController.IsSleeping)
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
    // HAPPINESS RECOVERY
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
    // HEALTH RECOVERY
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

    private void HandleFed(
        float amount)
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
    // SUBIR DE EDAD
    // =========================================================

    private void HandleAgeIncreased(
        float newAge)
    {
        if (ageUpSounds == null ||
            ageUpSounds.Length == 0)
        {
            return;
        }

        if (forceAgeUpSound)
        {
            PlayRandomForced(
                ageUpSounds
            );
        }
        else
        {
            if (!preventOverlappingSounds ||
                !audioSource.isPlaying)
            {
                PlayRandom(
                    ageUpSounds
                );
            }
        }

        ScheduleNextIdle();
    }

    // =========================================================
    // STATE
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
    // TRY PLAY
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

        if (Time.time < nextTime)
        {
            return;
        }

        if (preventOverlappingSounds &&
            audioSource.isPlaying)
        {
            return;
        }

        PlayRandom(
            clips
        );

        nextTime =
            Time.time +
            Random.Range(
                interval.x,
                interval.y
            );

        ScheduleNextIdle();
    }

    // =========================================================
    // AGE PITCH
    // =========================================================

    private float GetAgePitch()
    {
        if (pouStats == null)
        {
            return basePitch;
        }

        float age01 =
            pouStats.GetAgeNormalized();

        float agePitch =
            Mathf.Lerp(
                youngestVoicePitch,
                oldestVoicePitch,
                age01
            );

        return
            basePitch *
            agePitch;
    }

    // =========================================================
    // RANDOM SOUND
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

        float agePitch =
            GetAgePitch();

        audioSource.pitch =
            Mathf.Max(
                0.1f,
                agePitch +
                Random.Range(
                    -pitchVariation,
                    pitchVariation
                )
            );

        audioSource.PlayOneShot(
            clip,
            masterVolume
        );
    }

    // =========================================================
    // RANDOM FORCED
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

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        float agePitch =
            GetAgePitch();

        audioSource.pitch =
            Mathf.Max(
                0.1f,
                agePitch +
                Random.Range(
                    -pitchVariation,
                    pitchVariation
                )
            );

        audioSource.PlayOneShot(
            clip,
            masterVolume
        );
    }

    // =========================================================
    // API
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