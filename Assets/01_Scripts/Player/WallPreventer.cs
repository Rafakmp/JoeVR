using Unity.XR.CoreUtils;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(XROrigin))]
public class WallPreventer : MonoBehaviour
{
    private CharacterController m_CharacterController;
    private Transform m_LocalHeadTransform;

    void Awake()
    {
        if (!TryGetComponent(out m_CharacterController) ||
            !TryGetComponent(out XROrigin xrOrigin))
        {
            Debug.LogWarning("Faltan componentes. Desactivando WallPreventer.");
            this.enabled = false;
            return;
        }

        // Obtenemos la referencia a la cámara (la cabeza)
        m_LocalHeadTransform = xrOrigin.Camera.transform;
    }

    void Update()
    {
        // Sincronizamos el centro del Character Controller con la posición real de la cabeza.
        // Esto evita que al caminar físicamente, la cabeza atraviese las paredes.
        m_CharacterController.center = new Vector3(
            m_LocalHeadTransform.localPosition.x,
            m_CharacterController.center.y,
            m_LocalHeadTransform.localPosition.z
        );

        // Forzamos una actualización del Character Controller cada frame.
        m_CharacterController.SimpleMove(Vector3.zero);
    }
}