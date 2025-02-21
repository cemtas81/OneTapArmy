using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    public float speed = 3f;
    private Vector3 targetPosition;

    public void SetTarget(Vector3 newTarget)
    {
        targetPosition = newTarget;
    }

    void Update()
    {
        if (targetPosition != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        }
    }
}