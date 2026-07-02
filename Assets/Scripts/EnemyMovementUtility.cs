using UnityEngine;

public static class EnemyMovementUtility
{
    private const float skinWidth = 0.05f;

    public static Vector3 MoveTowardsWithoutPassingThroughWalls(Vector3 currentPosition, Vector3 targetPosition, float moveSpeed, float deltaTime)
    {
        Vector3 step = targetPosition - currentPosition;
        if (step.sqrMagnitude <= Mathf.Epsilon)
        {
            return currentPosition;
        }

        step = step.normalized * moveSpeed * deltaTime;
        Vector3 desiredPosition = currentPosition + step;

        RaycastHit2D hit = Physics2D.Linecast(currentPosition, desiredPosition);
        if (hit.collider != null && !ShouldIgnoreHit(hit.collider))
        {
            if (hit.distance > skinWidth)
            {
                return currentPosition + (Vector3)(step.normalized * (hit.distance - skinWidth));
            }

            return currentPosition;
        }

        return desiredPosition;
    }

    private static bool ShouldIgnoreHit(Collider2D hitCollider)
    {
        if (hitCollider.isTrigger)
        {
            return true;
        }

        if (hitCollider.CompareTag("Player"))
        {
            return true;
        }

        if (hitCollider.CompareTag("Enemy"))
        {
            return true;
        }

        return false;
    }
}