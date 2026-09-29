using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class LightSwitchVR : MonoBehaviour
{
    [Header("Estado")]
    [SerializeField] private bool isOn = true;

    [Header("Visual del interruptor")]
    [Tooltip("El objeto que se mueve/rota al presionar (opcional).")]
    [SerializeField] private Transform switchVisual;
    [SerializeField] private float onRotationX = -15f;
    [SerializeField] private float offRotationX = 15f;
    [SerializeField] private float moveSpeed = 10f;

    [Header("Luces del cuarto")]
    [Tooltip("Todas las luces que se apagan/encienden con este interruptor.")]
    [SerializeField] private Light[] roomLights;

    [Header("Eventos")]
    [Tooltip("Se dispara cuando el interruptor se enciende.")]
    public UnityEvent OnTurnedOn;

    [Tooltip("Se dispara cuando el interruptor se apaga.")]
    public UnityEvent OnTurnedOff;

    private XRSimpleInteractable interactable;
    private Quaternion targetRotation;

    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnSwitchPressed);
        targetRotation = Quaternion.Euler(isOn ? onRotationX : offRotationX, 0f, 0f);
    }

    private void OnDestroy()
    {
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnSwitchPressed);
    }

    private void Update()
    {
        if (switchVisual != null)
        {
            switchVisual.localRotation = Quaternion.Lerp(
                switchVisual.localRotation,
                targetRotation,
                Time.deltaTime * moveSpeed
            );
        }
    }

    private void OnSwitchPressed(SelectEnterEventArgs args)
    {
        Toggle();
    }

    public void Toggle()
    {
        isOn = !isOn;
        ApplyState();
    }

    public void SetState(bool state)
    {
        isOn = state;
        ApplyState();
    }

    private void ApplyState()
    {
        // Actualiza rotación visual
        targetRotation = Quaternion.Euler(isOn ? onRotationX : offRotationX, 0f, 0f);

        // Enciende/apaga luces
        if (roomLights != null)
        {
            foreach (Light l in roomLights)
            {
                if (l != null) l.enabled = isOn;
            }
        }

        // Dispara eventos
        if (isOn)
            OnTurnedOn?.Invoke();
        else
            OnTurnedOff?.Invoke();
    }
}