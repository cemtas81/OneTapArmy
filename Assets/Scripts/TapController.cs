using UnityEngine;

public class TapController : MonoBehaviour
{
    public LayerMask groundLayer; // Layer for the ground
    public UnitMovement[] units; // Array of all units

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Detect tap
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                Vector3 targetPosition = hit.point;
                DirectUnits(targetPosition);
            }
        }
    }

    void DirectUnits(Vector3 targetPosition)
    {
        foreach (UnitMovement unit in units)
        {
            unit.SetTarget(targetPosition);
        }
    }
}