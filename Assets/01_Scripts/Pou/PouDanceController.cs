using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PouDanceController : MonoBehaviour
{
    [Header("Referencias")]

    [SerializeField]
    private PouExpressionController expressionController;

    [SerializeField]
    private SkinnedMeshRenderer faceRenderer;

    [SerializeField]
    private PouStats pouStats;

    [Tooltip("XRGrabInteractable de Pou (para detectar cuando lo agarran).")]
    [SerializeField]
    private XRGrabInteractable grabInteractable;

    [Header("BlendShape Names")]

    [SerializeField]
    private string breathShape = "breath";

    [SerializeField]
    private string eatShape = "eat";

    [Header("Baile")]

    [SerializeField]
    private float danceSpeed = 8f;

    [SerializeField]
    private float breathDanceAmount = 60f;

    [SerializeField]
    private float eatDanceAmount = 70f;

    [Header("Salto")]

    [SerializeField]
    private float jumpHeight = 0.08f;

    [SerializeField]
    private float jumpSpeedMultiplier = 1.6f;

    [Header("Felicidad")]

    [SerializeField]
    private float happinessPerSecond = 4f;

    // ---------------------------------------------------------

    private bool isDancing = false;
    private bool isGrabbed = false;

    private int breathIndex = -1;
    private int eatIndex = -1;

    private Vector3 baseLocalPos;

    public bool IsDancing => isDancing;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (expressionController == null)
            expressionController = GetComponent<PouExpressionController>();

        if (pouStats == null)
            pouStats = GetComponent<PouStats>();

        if (faceRenderer == null)
            faceRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        if (grabInteractable == null)
            grabInteractable = GetComponent<XRGrabInteractable>();

        baseLocalPos = transform.localPosition;

        if (faceRenderer != null && faceRenderer.sharedMesh != null)
        {
            breathIndex =
                faceRenderer.sharedMesh.GetBlendShapeIndex(breathShape);

            eatIndex =
                faceRenderer.sharedMesh.GetBlendShapeIndex(eatShape);
        }
    }

    // =========================================================
    // ENABLE / DISABLE
    // =========================================================

    private void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;

        // Al soltar, recalculamos la posición base para no saltar
        baseLocalPos = transform.localPosition;
    }

    // =========================================================
    // LATE UPDATE
    // =========================================================

    private void LateUpdate()
    {
        if (!isDancing) return;

        float t = Time.time * danceSpeed;

        // -----------------------------------------
        // Blendshapes de baile (siempre, aunque esté agarrado)
        // -----------------------------------------

        if (faceRenderer != null)
        {
            if (breathIndex != -1)
            {
                float breathVal =
                    (Mathf.Sin(t) + 1f) * 0.5f * breathDanceAmount;

                faceRenderer.SetBlendShapeWeight(breathIndex, breathVal);
            }

            if (eatIndex != -1)
            {
                float eatVal =
                    (Mathf.Sin(t * 1.3f + 1f) + 1f) * 0.5f * eatDanceAmount;

                faceRenderer.SetBlendShapeWeight(eatIndex, eatVal);
            }
        }

        // -----------------------------------------
        // Salto SOLO si NO está agarrado
        // -----------------------------------------

        if (!isGrabbed)
        {
            float jumpOffset =
                Mathf.Abs(Mathf.Sin(t * jumpSpeedMultiplier)) * jumpHeight;

            transform.localPosition =
                baseLocalPos + Vector3.up * jumpOffset;
        }

        // -----------------------------------------
        // Felicidad
        // -----------------------------------------

        if (pouStats != null)
        {
            pouStats.AddHappiness(happinessPerSecond * Time.deltaTime);
        }
    }

    // =========================================================
    // API
    // =========================================================

    public void StartDancing()
    {
        if (isDancing) return;

        baseLocalPos = transform.localPosition;

        isDancing = true;

        Debug.Log("PouDanceController: empieza a bailar.");
    }

    public void StopDancing()
    {
        if (!isDancing) return;

        isDancing = false;

        // Devolver a su posición original
        if (!isGrabbed)
            transform.localPosition = baseLocalPos;

        // Resetear blendshapes de baile
        if (faceRenderer != null)
        {
            if (breathIndex != -1)
                faceRenderer.SetBlendShapeWeight(breathIndex, 0f);

            if (eatIndex != -1)
                faceRenderer.SetBlendShapeWeight(eatIndex, 0f);
        }

        Debug.Log("PouDanceController: deja de bailar.");
    }

    public void SetDancing(bool dancing)
    {
        if (dancing) StartDancing();
        else StopDancing();
    }
}