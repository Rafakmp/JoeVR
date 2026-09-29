using UnityEngine;

public class MenuAnchor : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float distance = 1.5f;
    [SerializeField] private float heightOffset = -0.2f;
    private void Update()
    {
        PlaceInFrontOfPlayer();
    }
    public void PlaceInFrontOfPlayer()
    {
        if (playerCamera == null) return;

        Vector3 forward = playerCamera.forward;
        forward.y = 0f;
        forward.Normalize();

        transform.position = playerCamera.position + forward * distance + Vector3.up * heightOffset;

        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        transform.Rotate(0f, 180f, 0f); // para que mire al jugador
    }
}