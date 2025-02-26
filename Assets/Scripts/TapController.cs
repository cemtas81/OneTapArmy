
using UnityEngine;
using UnityEngine.Events;


public class TapController : MonoBehaviour
{
    public LayerMask groundLayer;
    public UnityEvent<Vector3> move;
    private void Start()
    {
        // Ensure the move event is initialized
        if (move == null)
        {
            move = new UnityEvent<Vector3>();
        }
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                Vector3 targetPosition = hit.point;
                move?.Invoke(targetPosition);
            }
        }
    }
}
