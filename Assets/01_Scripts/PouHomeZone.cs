 
using UnityEngine;

/// <summary>
/// Poné uno de estos por cuarto. El Collider de este mismo objeto tiene que
/// ser un Trigger que cubra todo el cuarto (o el área donde querés que
/// "cuente" como ese cuarto).
///
/// El Pou solo tendrá un punto de regreso mientras esté dentro de esta zona.
/// Al salir, se elimina ese punto de regreso.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PouHomeZone : MonoBehaviour
{
    public bool isRoom=false;
    [Tooltip("Dónde se acomoda el Pou exactamente dentro de este cuarto. Si lo dejás vacío, usa la posición de este mismo objeto.")]
    [SerializeField] private Transform restPoint;
    
    public Transform RestPoint => restPoint != null ? restPoint : transform;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PouGrabAndReturn pou = other.GetComponentInParent<PouGrabAndReturn>();

        if (pou != null)
        {
            pou.SetCurrentRoom(RestPoint);
            pou.isInRoom=isRoom;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PouGrabAndReturn pou = other.GetComponentInParent<PouGrabAndReturn>();

        if (pou != null)
        {
            pou.ClearCurrentRoom(RestPoint);
            pou.isInRoom =false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(RestPoint.position, 0.1f);
        Gizmos.DrawLine(
            RestPoint.position,
            RestPoint.position + RestPoint.up * 0.2f
        );
    }
}
 