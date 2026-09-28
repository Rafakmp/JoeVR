using UnityEngine;

public class PouExpressionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PouStats pouStats;
    [SerializeField] private SkinnedMeshRenderer faceRenderer;
    [SerializeField] private PouGrabAndReturn pouGrabAndReturn;
    [SerializeField] private bool light;

    [Tooltip("AudioSource que reproduce los sonidos de Pou.")]
    [SerializeField] private AudioSource voiceAudioSource;

    [Header("BlendShape Names")]
    [SerializeField] private string blinkShape = "blink";
    [SerializeField] private string breathShape = "breath";
    [SerializeField] private string eatShape = "eat";
    [SerializeField] private string sleepShape = "sleep";
    // =========================================================
    // MIRAR AL JUGADOR
    // =========================================================

    [Header("Mirar al jugador")]
    [SerializeField] private bool enableLookAtPlayer = true;

    [SerializeField] private Transform playerTarget;

    [SerializeField] private float turnSpeed = 180f;

    [SerializeField] private float minDistanceToLook = 0.2f;

    [SerializeField] private float minAngleToTurn = 2f;

    // =========================================================
    // BLINK
    // =========================================================

    [Header("Blink")]
    [SerializeField] private bool enableBlink = true;

    [SerializeField] private float firstBlinkDelay = 2f;

    [SerializeField] private float minBlinkInterval = 2f;

    [SerializeField] private float maxBlinkInterval = 5f;

    [SerializeField] private float blinkSpeed = 500f;

    [SerializeField] private float normalBlinkAmount = 0f;

    [SerializeField] private float tiredBlinkAmount = 30f;

    [SerializeField] private float veryTiredBlinkAmount = 65f;

    // =========================================================
    // BREATH
    // =========================================================

    [Header("Breath")]
    [SerializeField] private bool enableBreathing = true;

    [SerializeField] private float breathSpeed = 2f;

    [SerializeField] private float breathMouthAmount = 8f;

    [SerializeField]
    [Range(0f, 0.2f)]
    private float breathScaleAmount = 0.03f;

    // =========================================================
    // HABLAR / HACER RUIDOS
    // =========================================================

    [Header("Hablar / sonidos")]
    [Tooltip("Cuánto abre la boca cuando hace un sonido.")]
    [SerializeField] private float talkingMouthAmount = 70f;

    [Tooltip("Velocidad con la que la boca pulsa mientras habla.")]
    [SerializeField] private float talkingPulseSpeed = 12f;

    [Tooltip("Velocidad para abrir/cerrar suavemente la boca.")]
    [SerializeField] private float talkingSmoothSpeed = 250f;

    [Tooltip("Mínima apertura incluso en sonidos suaves.")]
    [SerializeField] private float talkingMinimumAmount = 25f;

    // =========================================================
    // HAMBRE
    // =========================================================

    [Header("Hambre")]
    [SerializeField] private float hungryMouthAmount = 20f;

    [SerializeField] private float veryHungryMouthAmount = 50f;

    [SerializeField] private float hungryHeightStretch = 0.06f;

    [SerializeField] private float veryHungryHeightStretch = 0.16f;

    // =========================================================
    // CANSANCIO
    // =========================================================

    [Header("Cansancio")]
    [SerializeField] private float tiredEnergyStart = 40f;

    [SerializeField] private float tiredHeightShrink = 0.08f;

    [SerializeField] private float veryTiredHeightShrink = 0.25f;

    // =========================================================
    // SUAVIZADO
    // =========================================================

    [Header("Suavizado")]
    [SerializeField] private float mouthSmoothSpeed = 180f;

    [SerializeField] private float scaleSmoothSpeed = 4f;

    // =========================================================
    // REBOTE
    // =========================================================

    [Header("Rebote al aterrizar")]
    [SerializeField] private float landingBounceAmount = 0.12f;

    [SerializeField] private float landingBounceSpeed = 10f;

    [SerializeField] private float landingBounceDamping = 4f;

    // =========================================================
    // INTERNAL
    // =========================================================

    private int blinkIndex = -1;
    private int breathIndex = -1;
    private int eatIndex = -1;

    private float nextBlinkTime;
    private float blinkValue;

    private bool isBlinking;
    private bool closingEyes;

    private Vector3 baseScale;

    private bool isReturningHome;

    private float landingBounceTimer = -1f;

    private float currentMouthValue;

    private int sleepIndex = -1;

    private bool isSleeping = false;

    // Velocidad del loop del blend shape "sleep"
    [Header("Sleep")]
    [SerializeField] private float sleepLoopSpeed = 60f;
    [SerializeField] private float sleepEnergyPerSecond = 8f;

    private float sleepBlendValue = 0f;
    private bool sleepGoingUp = true;

    // Se usa para no recalcular la energía con stats.Sleep() cada frame
    private float sleepEnergyTimer = 0f;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (pouStats == null)
        {
            pouStats = GetComponent<PouStats>();
        }

        baseScale = transform.localScale;

        if (playerTarget == null && Camera.main != null)
        {
            playerTarget = Camera.main.transform;
        }

        FindBlendShapes();

        nextBlinkTime =
            Time.time + firstBlinkDelay;
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (playerTarget == null && Camera.main != null)
        {
            playerTarget = Camera.main.transform;
        }

        // Mientras duerme, no gira, no parpadea normal, no respira animado
        if (pouGrabAndReturn.isInRoom && !light)
        {
            isSleeping = true;
        }
        else
        {
            isSleeping = false;
        }

        
        if (isSleeping)
        {
            UpdateSleep();
            return;
        }

        UpdateLookAtPlayer();
        UpdateBlink();

        float breathWave = enableBreathing
            ? (Mathf.Sin(Time.time * breathSpeed) + 1f) * 0.5f
            : 0f;

        UpdateBreathMouth(breathWave);
        UpdateMouthFromAudio();
        UpdateBodyScale(breathWave);
    }
    private void UpdateSleep()
    {
        // -----------------------------------------
        // Ojos cerrados al 100%
        // -----------------------------------------
        if (blinkIndex != -1)
        {
            faceRenderer.SetBlendShapeWeight(blinkIndex, 100f);
        }

        // -----------------------------------------
        // Boca cerrada
        // -----------------------------------------
        if (eatIndex != -1)
        {
            faceRenderer.SetBlendShapeWeight(eatIndex, 0f);
        }

        // -----------------------------------------
        // Blend shape "sleep" en loop 0 -> 100 -> 0
        // -----------------------------------------
        if (sleepIndex != -1)
        {
            if (sleepGoingUp)
            {
                sleepBlendValue += sleepLoopSpeed * Time.deltaTime;
                if (sleepBlendValue >= 100f)
                {
                    sleepBlendValue = 100f;
                    sleepGoingUp = false;
                }
            }
            else
            {
                sleepBlendValue -= sleepLoopSpeed * Time.deltaTime;
                if (sleepBlendValue <= 0f)
                {
                    sleepBlendValue = 0f;
                    sleepGoingUp = true;
                }
            }

            faceRenderer.SetBlendShapeWeight(sleepIndex, sleepBlendValue);
        }

        // -----------------------------------------
        // Recuperar energía mientras duerme
        // -----------------------------------------
        if (pouStats != null)
        {
            sleepEnergyTimer += Time.deltaTime;

            // Cada 1 segundo le damos energía
            if (sleepEnergyTimer >= 1f)
            {
                pouStats.Sleep(sleepEnergyPerSecond);
                sleepEnergyTimer = 0f;
            }
        }
    }
    // =========================================================
    // API
    // =========================================================

    public void SetGrabbed(bool grabbed)
    {
    }

    public void SetReturningHome(bool returning)
    {
        isReturningHome = returning;
    }

    public void TriggerLandingBounce()
    {
        landingBounceTimer = 0f;
    }

    public void SetSleeping(bool sleeping)
    {
        isSleeping = sleeping;
        
        if (sleeping)
        {
            // Forzar ojos cerrados
            blinkValue = 100f;
            isBlinking = false;
            closingEyes = false;

            // Cerrar boca
            currentMouthValue = 0f;

            // Reiniciar loop del blend shape "sleep"
            sleepBlendValue = 0f;
            sleepGoingUp = true;

            // Apagar luz del cuarto
          
        }
        else
        {
            // Al despertar, parpadear normal
            nextBlinkTime = Time.time + 1f;

            

            // Resetear blend shape de dormir
            if (sleepIndex != -1)
            {
                faceRenderer.SetBlendShapeWeight(sleepIndex, 0f);
            }
        }
    }

    public bool IsSleeping => isSleeping;

    // =========================================================
    // BLENDSHAPES
    // =========================================================

    private void FindBlendShapes()
    {
        if (faceRenderer == null)
        {
            Debug.LogError(
                "PouExpressionController: Face Renderer no asignado."
            );

            return;
        }

        Mesh mesh = faceRenderer.sharedMesh;

        if (mesh == null)
        {
            Debug.LogError(
                "PouExpressionController: SkinnedMeshRenderer no tiene Mesh."
            );

            return;
        }

        blinkIndex =
            mesh.GetBlendShapeIndex(blinkShape);

        breathIndex =
            mesh.GetBlendShapeIndex(breathShape);

        eatIndex =
            mesh.GetBlendShapeIndex(eatShape);
        sleepIndex = mesh.GetBlendShapeIndex(sleepShape);

        CheckBlendShape(blinkShape, blinkIndex);
        CheckBlendShape(breathShape, breathIndex);
        CheckBlendShape(eatShape, eatIndex);
     
        CheckBlendShape(sleepShape, sleepIndex);
    }

    private void CheckBlendShape(
        string shapeName,
        int index)
    {
        if (index == -1)
        {
            Debug.LogWarning(
                $"PouExpressionController: No se encontró '{shapeName}'."
            );
        }
    }

    // =========================================================
    // MIRAR
    // =========================================================

    private void UpdateLookAtPlayer()
    {
        if (!enableLookAtPlayer ||
            isReturningHome ||
            playerTarget == null)
        {
            return;
        }

        Vector3 toPlayer =
            playerTarget.position -
            transform.position;

        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude <
            minDistanceToLook * minDistanceToLook)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(
                toPlayer.normalized,
                Vector3.up
            );

        if (Quaternion.Angle(
                transform.rotation,
                targetRotation
            ) < minAngleToTurn)
        {
            return;
        }

        transform.rotation =
            Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
    }

    // =========================================================
    // BLINK
    // =========================================================

    private void UpdateBlink()
    {
        if (!enableBlink ||
            blinkIndex == -1)
        {
            return;
        }

        float tiredAmount =
            CalculateTiredBlinkAmount();

        if (!isBlinking &&
            Time.time >= nextBlinkTime)
        {
            isBlinking = true;
            closingEyes = true;
        }

        if (isBlinking)
        {
            if (closingEyes)
            {
                blinkValue =
                    Mathf.MoveTowards(
                        blinkValue,
                        100f,
                        blinkSpeed * Time.deltaTime
                    );

                if (blinkValue >= 100f)
                {
                    closingEyes = false;
                }
            }
            else
            {
                blinkValue =
                    Mathf.MoveTowards(
                        blinkValue,
                        tiredAmount,
                        blinkSpeed * Time.deltaTime
                    );

                if (blinkValue <= tiredAmount)
                {
                    isBlinking = false;

                    nextBlinkTime =
                        Time.time +
                        Random.Range(
                            minBlinkInterval,
                            maxBlinkInterval
                        );
                }
            }
        }

        float finalBlink =
            Mathf.Max(
                blinkValue,
                tiredAmount
            );

        faceRenderer.SetBlendShapeWeight(
            blinkIndex,
            finalBlink
        );
    }

    private float CalculateTiredBlinkAmount()
    {
        if (pouStats == null)
        {
            return normalBlinkAmount;
        }

        float energy =
            pouStats.energy;

        if (energy >= 60f)
        {
            return normalBlinkAmount;
        }

        if (energy >= 30f)
        {
            float percentage =
                Mathf.InverseLerp(
                    60f,
                    30f,
                    energy
                );

            return Mathf.Lerp(
                normalBlinkAmount,
                tiredBlinkAmount,
                percentage
            );
        }

        float veryTiredPercentage =
            Mathf.InverseLerp(
                30f,
                0f,
                energy
            );

        return Mathf.Lerp(
            tiredBlinkAmount,
            veryTiredBlinkAmount,
            veryTiredPercentage
        );
    }

    // =========================================================
    // RESPIRACIÓN
    // =========================================================

    private void UpdateBreathMouth(
        float breathWave)
    {
        if (breathIndex == -1)
        {
            return;
        }

        float amount =
            breathWave *
            breathMouthAmount;

        faceRenderer.SetBlendShapeWeight(
            breathIndex,
            amount
        );
    }

    // =========================================================
    // BOCA AL HACER RUIDOS
    // =========================================================

    private void UpdateMouthFromAudio()
    {
        if (eatIndex == -1)
        {
            return;
        }

        bool isMakingSound =
            voiceAudioSource != null &&
            voiceAudioSource.isPlaying;

        float targetMouth = 0f;

        // -----------------------------------------------------
        // HACIENDO UN SONIDO
        // -----------------------------------------------------

        if (isMakingSound)
        {
            float pulse =
                (Mathf.Sin(
                    Time.time *
                    talkingPulseSpeed
                ) + 1f) * 0.5f;

            // Variación entre apertura mínima y máxima.
            targetMouth =
                Mathf.Lerp(
                    talkingMinimumAmount,
                    talkingMouthAmount,
                    pulse
                );
        }

        // -----------------------------------------------------
        // ESTÁ HAMBRIENTO PERO NO ESTÁ HACIENDO SONIDO
        // -----------------------------------------------------

        else if (pouStats != null &&
                 pouStats.hunger < 50f)
        {
            float percentage =
                Mathf.InverseLerp(
                    50f,
                    0f,
                    pouStats.hunger
                );

            targetMouth =
                Mathf.Lerp(
                    hungryMouthAmount,
                    veryHungryMouthAmount,
                    percentage
                );
        }

        // -----------------------------------------------------
        // SUAVIZAR
        // -----------------------------------------------------

        currentMouthValue =
            Mathf.MoveTowards(
                currentMouthValue,
                targetMouth,
                mouthSmoothSpeed *
                Time.deltaTime
            );

        faceRenderer.SetBlendShapeWeight(
            eatIndex,
            currentMouthValue
        );
    }

    // =========================================================
    // CUERPO
    // =========================================================

    private void UpdateBodyScale(
        float breathWave)
    {
        float breathPulse =
            enableBreathing
                ? breathWave * breathScaleAmount
                : 0f;

        float heightOffset = 0f;

        // -----------------------------------------------------
        // HAMBRE
        // -----------------------------------------------------

        if (pouStats != null &&
            pouStats.hunger < 50f)
        {
            float percentage =
                Mathf.InverseLerp(
                    50f,
                    0f,
                    pouStats.hunger
                );

            heightOffset +=
                Mathf.Lerp(
                    hungryHeightStretch,
                    veryHungryHeightStretch,
                    percentage
                );
        }

        // -----------------------------------------------------
        // CANSANCIO
        // -----------------------------------------------------

        if (pouStats != null &&
            pouStats.energy < tiredEnergyStart)
        {
            float percentage =
                Mathf.InverseLerp(
                    tiredEnergyStart,
                    0f,
                    pouStats.energy
                );

            heightOffset -=
                Mathf.Lerp(
                    tiredHeightShrink,
                    veryTiredHeightShrink,
                    percentage
                );
        }

        float bounce =
            UpdateLandingBounce();

        float horizontalAdjustment =
            1f -
            heightOffset * 0.35f;

        horizontalAdjustment =
            Mathf.Max(
                0.8f,
                horizontalAdjustment
            );

        Vector3 targetScale =
            new Vector3(
                baseScale.x *
                (horizontalAdjustment +
                 breathPulse),

                baseScale.y *
                (1f +
                 breathPulse +
                 heightOffset +
                 bounce),

                baseScale.z *
                (horizontalAdjustment +
                 breathPulse)
            );

        transform.localScale =
            Vector3.MoveTowards(
                transform.localScale,
                targetScale,
                scaleSmoothSpeed *
                Time.deltaTime
            );
    }

    // =========================================================
    // REBOTE
    // =========================================================

    private float UpdateLandingBounce()
    {
        if (landingBounceTimer < 0f)
        {
            return 0f;
        }

        landingBounceTimer +=
            Time.deltaTime;

        float damped =
            Mathf.Exp(
                -landingBounceDamping *
                landingBounceTimer
            );

        float bounce =
            Mathf.Sin(
                landingBounceTimer *
                landingBounceSpeed
            ) *
            landingBounceAmount *
            damped;

        if (damped < 0.02f)
        {
            landingBounceTimer = -1f;
        }

        return bounce;
    }
    public void SetLight(bool isLightOn)
    {
        light = isLightOn;
    }
    public void PlayEatAnimation(float duration = 1f)
    {
        if (eatIndex == -1)
            return;

        StartCoroutine(EatAnimationCoroutine(duration));
    }

    private System.Collections.IEnumerator EatAnimationCoroutine(float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float value = Mathf.Sin(
                (timer / duration) * Mathf.PI
            ) * 100f;

            faceRenderer.SetBlendShapeWeight(
                eatIndex,
                value
            );

            yield return null;
        }

        faceRenderer.SetBlendShapeWeight(
            eatIndex,
            0f
        );
    }
}