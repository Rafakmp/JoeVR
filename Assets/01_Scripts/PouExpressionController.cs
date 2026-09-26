using UnityEngine;

public class PouExpressionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PouStats pouStats;
    [SerializeField] private SkinnedMeshRenderer faceRenderer;

    [Header("BlendShape Names")]
    [SerializeField] private string blinkShape = "blink";
    [SerializeField] private string breathShape = "breath";
    [SerializeField] private string eatShape = "eat";
    [SerializeField] private string sleepShape = "sleep";

    // =========================================================
    // BLINK
    // =========================================================

    [Header("Blink")]
    [SerializeField] private bool enableBlink = true;

    [Tooltip("Tiempo antes del primer parpadeo.")]
    [SerializeField] private float firstBlinkDelay = 2f;

    [Tooltip("Tiempo mínimo entre parpadeos.")]
    [SerializeField] private float minBlinkInterval = 2f;

    [Tooltip("Tiempo máximo entre parpadeos.")]
    [SerializeField] private float maxBlinkInterval = 5f;

    [Tooltip("Velocidad de cerrar y abrir los ojos.")]
    [SerializeField] private float blinkSpeed = 500f;

    [Tooltip("Parpadeo normal.")]
    [SerializeField] private float normalBlinkAmount = 0f;

    [Tooltip("Parpadeo cuando está cansado.")]
    [SerializeField] private float tiredBlinkAmount = 30f;

    [Tooltip("Parpadeo cuando está muy cansado.")]
    [SerializeField] private float veryTiredBlinkAmount = 60f;

    // =========================================================
    // BREATH
    // =========================================================

    [Header("Breath")]
    [SerializeField] private bool enableBreathing = true;

    [SerializeField] private float breathSpeed = 2f;

    [SerializeField] private float breathAmount = 35f;

    // =========================================================
    // EAT
    // =========================================================

    [Header("Eat")]
    [SerializeField] private float hungryEatAmount = 30f;

    [SerializeField] private float veryHungryEatAmount = 70f;

    // =========================================================
    // SLEEP
    // =========================================================

    [Header("Sleep")]
    [SerializeField] private float tiredSleepAmount = 20f;

    [SerializeField] private float veryTiredSleepAmount = 70f;

    // =========================================================
    // POSITION / ROTATION
    // =========================================================

    [Header("Fixed Position")]
    [SerializeField] private bool keepFixedPosition = true;

    // Posición original del Pou.
    private Vector3 originalPosition;

    // Rotación original del Pou.
    private Quaternion originalRotation;

    // =========================================================
    // INTERNAL VARIABLES
    // =========================================================

    private int blinkIndex = -1;
    private int breathIndex = -1;
    private int eatIndex = -1;
    private int sleepIndex = -1;

    // Blink
    private float nextBlinkTime;
    private float blinkValue;

    private bool isBlinking;
    private bool closingEyes;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        // Buscar PouStats automáticamente.
        if (pouStats == null)
        {
            pouStats = GetComponent<PouStats>();
        }

        // Guardar posición EXACTA.
        originalPosition = transform.localPosition;

        // Guardar rotación EXACTA.
        originalRotation = transform.localRotation;

        // Buscar los BlendShapes.
        FindBlendShapes();

        // Preparar primer parpadeo.
        nextBlinkTime =
            Time.time + firstBlinkDelay;
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // =====================================================
        // MANTENER POSICIÓN
        // =====================================================

        if (keepFixedPosition)
        {
            transform.localPosition = originalPosition;
        }

        // =====================================================
        // MANTENER ROTACIÓN
        // =====================================================

        transform.localRotation = originalRotation;

        // =====================================================
        // ACCIONES DEL POU
        // =====================================================

        UpdateBreath();

        UpdateBlink();

        UpdateEating();

        UpdateSleeping();
    }

    // =========================================================
    // FIND BLENDSHAPES
    // =========================================================

    private void FindBlendShapes()
    {
        if (faceRenderer == null)
        {
            Debug.LogError(
                "PouExpressionController: " +
                "Face Renderer no está asignado."
            );

            return;
        }

        Mesh mesh = faceRenderer.sharedMesh;

        if (mesh == null)
        {
            Debug.LogError(
                "PouExpressionController: " +
                "El SkinnedMeshRenderer no tiene Mesh."
            );

            return;
        }

        // IMPORTANTE:
        // Los nombres están en MINÚSCULA.

        blinkIndex =
            mesh.GetBlendShapeIndex("blink");

        breathIndex =
            mesh.GetBlendShapeIndex("breath");

        eatIndex =
            mesh.GetBlendShapeIndex("eat");

        sleepIndex =
            mesh.GetBlendShapeIndex("sleep");

        // Mostrar información en Console.

        CheckBlendShape(
            "blink",
            blinkIndex
        );

        CheckBlendShape(
            "breath",
            breathIndex
        );

        CheckBlendShape(
            "eat",
            eatIndex
        );

        CheckBlendShape(
            "sleep",
            sleepIndex
        );
    }

    // =========================================================
    // CHECK BLENDSHAPE
    // =========================================================

    private void CheckBlendShape(
        string shapeName,
        int index
    )
    {
        if (index == -1)
        {
            Debug.LogWarning(
                "PouExpressionController: " +
                "No se encontró el BlendShape '" +
                shapeName +
                "'."
            );
        }
        else
        {
            Debug.Log(
                "PouExpressionController: " +
                "BlendShape encontrado: " +
                shapeName
            );
        }
    }

    // =========================================================
    // BREATH
    // =========================================================

    private void UpdateBreath()
    {
        if (!enableBreathing)
            return;

        if (breathIndex == -1)
            return;

        // Oscilación suave.
        float value =
            (Mathf.Sin(
                Time.time * breathSpeed
            ) + 1f) * 0.5f;

        float finalValue =
            value * breathAmount;

        faceRenderer.SetBlendShapeWeight(
            breathIndex,
            finalValue
        );
    }

    // =========================================================
    // BLINK
    // =========================================================

    private void UpdateBlink()
    {
        if (!enableBlink)
            return;

        if (blinkIndex == -1)
            return;

        float tiredAmount =
            CalculateTiredBlinkAmount();

        // =====================================================
        // INICIAR PARPADEO
        // =====================================================

        if (!isBlinking &&
            Time.time >= nextBlinkTime)
        {
            isBlinking = true;
            closingEyes = true;
        }

        // =====================================================
        // CERRAR / ABRIR OJOS
        // =====================================================

        if (isBlinking)
        {
            // -------------------------------------------------
            // CERRAR
            // -------------------------------------------------

            if (closingEyes)
            {
                blinkValue =
                    Mathf.MoveTowards(
                        blinkValue,
                        100f,
                        blinkSpeed *
                        Time.deltaTime
                    );

                if (blinkValue >= 100f)
                {
                    closingEyes = false;
                }
            }

            // -------------------------------------------------
            // ABRIR
            // -------------------------------------------------

            else
            {
                blinkValue =
                    Mathf.MoveTowards(
                        blinkValue,
                        tiredAmount,
                        blinkSpeed *
                        Time.deltaTime
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

    // =========================================================
    // TIRED BLINK
    // =========================================================

    private float CalculateTiredBlinkAmount()
    {
        if (pouStats == null)
            return normalBlinkAmount;

        float energy =
            pouStats.energy;

        // =====================================================
        // ENERGÍA NORMAL
        // =====================================================

        if (energy >= 60f)
        {
            return normalBlinkAmount;
        }

        // =====================================================
        // CANSANCIO MEDIO
        // =====================================================

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

        // =====================================================
        // MUY CANSADO
        // =====================================================

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
    // EAT
    // =========================================================

    private void UpdateEating()
    {
        if (eatIndex == -1)
            return;

        if (pouStats == null)
            return;

        float hunger =
            pouStats.hunger;

        float targetAmount = 0f;

        // =====================================================
        // HAMBRE
        // =====================================================

        if (hunger < 50f)
        {
            float percentage =
                Mathf.InverseLerp(
                    50f,
                    0f,
                    hunger
                );

            targetAmount =
                Mathf.Lerp(
                    hungryEatAmount,
                    veryHungryEatAmount,
                    percentage
                );
        }

        // =====================================================
        // SUAVIZAR
        // =====================================================

        float current =
            faceRenderer.GetBlendShapeWeight(
                eatIndex
            );

        current =
            Mathf.MoveTowards(
                current,
                targetAmount,
                100f *
                Time.deltaTime
            );

        faceRenderer.SetBlendShapeWeight(
            eatIndex,
            current
        );
    }

    // =========================================================
    // SLEEP
    // =========================================================

    private void UpdateSleeping()
    {
        if (sleepIndex == -1)
            return;

        if (pouStats == null)
            return;

        float energy =
            pouStats.energy;

        float targetAmount = 0f;

        // =====================================================
        // CANSANCIO
        // =====================================================

        if (energy < 40f)
        {
            float percentage =
                Mathf.InverseLerp(
                    40f,
                    0f,
                    energy
                );

            targetAmount =
                Mathf.Lerp(
                    tiredSleepAmount,
                    veryTiredSleepAmount,
                    percentage
                );
        }

        // =====================================================
        // SUAVIZAR
        // =====================================================

        float current =
            faceRenderer.GetBlendShapeWeight(
                sleepIndex
            );

        current =
            Mathf.MoveTowards(
                current,
                targetAmount,
                80f *
                Time.deltaTime
            );

        faceRenderer.SetBlendShapeWeight(
            sleepIndex,
            current
        );
    }
}