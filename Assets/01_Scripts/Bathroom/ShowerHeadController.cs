using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Makes a shower head grabbable with XRI and sprays water while its selected
/// interactor holds the Activate (Trigger) action.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class ShowerHeadController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Point at the outlet of the shower. Its forward axis must point in the water direction.")]
    [SerializeField] private Transform nozzle;

    [Tooltip("Optional explicit target. When assigned, only this Joe can be cleaned.")]
    [SerializeField] private PouStats joe;

    [Tooltip("Optional particle system. A lightweight default is created at runtime when left empty.")]
    [SerializeField] private ParticleSystem waterParticles;

    [Tooltip("Optional line renderer for the continuous water stream. A default is created at runtime when left empty.")]
    [SerializeField] private LineRenderer waterStream;

    [Header("Water")]
    [SerializeField, Min(0.1f)] private float maxDistance = 4f;
    [SerializeField] private LayerMask hitLayers = ~0;

    private XRGrabInteractable grabInteractable;
    private bool triggerHeld;

    public void Configure(Transform nozzleTransform, PouStats joeStats)
    {
        nozzle = nozzleTransform;
        joe = joeStats;
    }
    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (nozzle == null)
            nozzle = transform;

        CreateDefaultVisualsIfNeeded();
        SetWaterActive(false);
    }

    private void OnEnable()
    {
        if (grabInteractable == null)
            grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.activated.AddListener(OnActivated);
        grabInteractable.deactivated.AddListener(OnDeactivated);
        grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.activated.RemoveListener(OnActivated);
            grabInteractable.deactivated.RemoveListener(OnDeactivated);
            grabInteractable.selectExited.RemoveListener(OnSelectExited);
        }

        triggerHeld = false;
        SetWaterActive(false);
    }

    private void Update()
    {
        bool spraying = triggerHeld && grabInteractable != null && grabInteractable.isSelected;
        SetWaterActive(spraying);

        if (!spraying)
            return;

        UpdateWaterImpact();
    }

    private void OnActivated(ActivateEventArgs args)
    {
        if (grabInteractable.isSelected)
            triggerHeld = true;
    }

    private void OnDeactivated(DeactivateEventArgs args)
    {
        triggerHeld = false;
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        triggerHeld = false;
        SetWaterActive(false);
    }

    private void UpdateWaterImpact()
    {
        Vector3 origin = nozzle.position;
        Vector3 direction = nozzle.forward;
        float visualEnd = maxDistance;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, hitLayers, QueryTriggerInteraction.Collide))
        {
            visualEnd = hit.distance;
            PouStats hitPou = hit.collider.GetComponentInParent<PouStats>();

            if (hitPou != null && (joe == null || hitPou == joe))
                hitPou.ApplyWaterCleaning(Time.deltaTime);
        }

        if (waterStream != null)
        {
            waterStream.SetPosition(0, origin);
            waterStream.SetPosition(1, origin + direction * visualEnd);
        }
    }

    private void SetWaterActive(bool active)
    {
        if (waterParticles != null)
        {
            if (active && !waterParticles.isPlaying)
                waterParticles.Play(true);
            else if (!active && waterParticles.isPlaying)
                waterParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (waterStream != null)
            waterStream.enabled = active;
    }

    private void CreateDefaultVisualsIfNeeded()
    {
        if (waterParticles == null)
        {
            GameObject particlesObject = new GameObject("Water Particles");
            particlesObject.transform.SetParent(nozzle, false);
            waterParticles = particlesObject.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = waterParticles.main;
            main.playOnAwake = false;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startLifetime = 0.35f;
            main.startSpeed = 5f;
            main.startSize = 0.025f;
            main.maxParticles = 100;

            ParticleSystem.EmissionModule emission = waterParticles.emission;
            emission.rateOverTime = 90f;

            ParticleSystem.ShapeModule shape = waterParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 4f;
            shape.radius = 0.015f;
        }

        if (waterStream == null)
        {
            waterStream = gameObject.AddComponent<LineRenderer>();
            waterStream.useWorldSpace = true;
            waterStream.positionCount = 2;
            waterStream.startWidth = 0.025f;
            waterStream.endWidth = 0.012f;
            waterStream.startColor = new Color(0.55f, 0.85f, 1f, 0.8f);
            waterStream.endColor = new Color(0.55f, 0.85f, 1f, 0.15f);
            waterStream.numCapVertices = 3;

            Shader lineShader = Shader.Find("Sprites/Default");
            if (lineShader != null)
                waterStream.material = new Material(lineShader);
        }
    }
}