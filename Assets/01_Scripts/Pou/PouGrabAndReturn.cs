using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Va en el mismo GameObject que el XR Grab Interactable.
/// Mientras el jugador lo agarra, avisa a PouExpressionController para que
/// deje de girar solo. Al soltarlo, el Pou desliza suavemente hasta el
/// punto de descanso del cuarto donde esté en ese momento (ver PouHomeZone).
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class PouGrabAndReturn : MonoBehaviour
{
    public bool isInRoom = false;
    [Header("Punto por defecto")]
    [Tooltip("Se usa solo si el Pou todavía no entró a ningún PouHomeZone (por ejemplo, al arrancar el juego fuera de cualquier cuarto). Los puntos reales de cada cuarto se definen con PouHomeZone.")]
    [SerializeField] private Transform defaultHomePoint;
  
    [Header("Animación de regreso")]
    [SerializeField] private float returnDuration = 0.6f;
    [SerializeField] private AnimationCurve returnEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Sonido al acomodarse (opcional)")]
    [SerializeField] private AudioClip placementSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Referencias (se auto-completan si las dejás vacías)")]
    [SerializeField] private XRGrabInteractable grabInteractable;
    [SerializeField] private PouExpressionController expressionController;
    [SerializeField] private Rigidbody body;

    private Collider[] ownColliders;
    private Coroutine returnRoutine;

    // Se actualiza solo, cada vez que el Pou entra a un PouHomeZone
    // (ya sea llevado en la mano o volviendo solo). Nunca se limpia al
    // salir de un cuarto: si lo soltás en un pasillo sin zona propia,
    // vuelve al último cuarto conocido.
    private Transform currentRoomHomePoint;

    private Transform EffectiveHomePoint => currentRoomHomePoint != null ? currentRoomHomePoint : defaultHomePoint;

    private void Awake()
    {
        if (grabInteractable == null) grabInteractable = GetComponent<XRGrabInteractable>();
        if (expressionController == null) expressionController = GetComponent<PouExpressionController>();
        if (body == null) body = GetComponent<Rigidbody>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        ownColliders = GetComponentsInChildren<Collider>();

        // Si al arrancar el juego ya estamos parados dentro de algún
        // PouHomeZone, lo detectamos por posición (no dependemos de que
        // OnTriggerEnter dispare justo en el primer frame).
        InitializeCurrentRoomFromOverlap();

        Transform startPoint = EffectiveHomePoint;

        if (startPoint != null)
        {
            transform.SetPositionAndRotation(startPoint.position, startPoint.rotation);

            if (body != null)
            {
                body.isKinematic = true;
            }
        }
    }

    private void InitializeCurrentRoomFromOverlap()
    {
        PouHomeZone[] zones = FindObjectsByType<PouHomeZone>(FindObjectsSortMode.None);

        foreach (PouHomeZone zone in zones)
        {
            Collider zoneCollider = zone.GetComponent<Collider>();

            if (zoneCollider != null && zoneCollider.bounds.Contains(transform.position))
            {
                currentRoomHomePoint = zone.RestPoint;
                break;
            }
        }
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    /// <summary>
    /// Llamado por PouHomeZone cuando el Pou entra a su trigger.
    /// Así siempre sabemos a qué cuarto (y a qué punto exacto) volver.
    /// </summary>
  
     
public void SetCurrentRoom(Transform roomRestPoint)
    {
        currentRoomHomePoint = roomRestPoint;
    }

    /// <summary>
    /// Se llama cuando el Pou sale de la zona.
    /// Solo limpia el punto si corresponde a la zona de la que realmente salió.
    /// </summary>
    public void ClearCurrentRoom(Transform roomRestPoint)
    {
        if (currentRoomHomePoint == roomRestPoint)
        {
            currentRoomHomePoint = null;
        }
    }
 

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        SetCollidersEnabled(true);

        if (expressionController != null)
        {
            expressionController.SetGrabbed(true);
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        Transform target = EffectiveHomePoint;

        if (target == null)
        {
            // No hay ningún punto disponible todavía: lo dejamos donde cayó
            // y reactivamos el "mirar al jugador".
            if (expressionController != null)
            {
                expressionController.SetGrabbed(false);
                body.isKinematic = false;
            }
            return;
        }

        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
        }

        returnRoutine = StartCoroutine(ReturnHomeRoutine(target));
    }

    private IEnumerator ReturnHomeRoutine(Transform target)
    {
        if (body != null)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
        }

        // Desactivamos los colliders para que no vaya empujando muebles
        // (ni dispare otros PouHomeZone) mientras "flota" de regreso.
        SetCollidersEnabled(false);

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        float elapsed = 0f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float eased = returnEase.Evaluate(Mathf.Clamp01(elapsed / returnDuration));

            transform.SetPositionAndRotation(
                Vector3.Lerp(startPosition, target.position, eased),
                Quaternion.Slerp(startRotation, target.rotation, eased)
            );

            yield return null;
        }

        transform.SetPositionAndRotation(target.position, target.rotation);

        SetCollidersEnabled(true);

        if (expressionController != null)
        {
            expressionController.SetGrabbed(false);
            expressionController.TriggerLandingBounce();
        }

        if (placementSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(placementSound);
        }

        returnRoutine = null;
    }

    private void SetCollidersEnabled(bool value)
    {
        if (ownColliders == null)
            return;

        foreach (Collider col in ownColliders)
        {
            if (col != null)
            {
                col.enabled = value;
            }
        }
    }
}