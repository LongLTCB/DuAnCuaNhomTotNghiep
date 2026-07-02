using UnityEngine;
using Photon.Pun;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]
    private int minRoomScore = 8;

    [SerializeField]
    private float retryDelay = 0.15f;

    [SerializeField]
    private float npcSearchRadius = 8f;

    IEnumerator Start()
    {
        while (!PhotonNetwork.InRoom)
        {
            yield return null;
        }

        while (GroundPositionManager.groundPositions.Count == 0)
        {
            GroundPositionManager manager = FindObjectOfType<GroundPositionManager>();
            if (manager != null)
            {
                manager.RefreshGroundPositions();
            }

            if (GroundPositionManager.groundPositions.Count == 0)
            {
                yield return new WaitForSeconds(retryDelay);
            }
        }

        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        Vector3 spawnPos = GetPreferredSpawnPosition();

        if (spawnPos == Vector3.zero)
        {
            spawnPos = GroundPositionManager.GetRandomGroundPosition();
        }

        if (spawnPos == Vector3.zero)
        {
            Debug.LogError("PlayerSpawner: Khong tim duoc vi tri spawn hop le tren map.");
            return;
        }

        GroundPositionManager.RegisterProtectedGroundArea(spawnPos, 7f);

        string selectedClass =
            PlayerPrefs.GetString(
                "MySelectedClass",
                "Class_Warrior");

        PhotonNetwork.Instantiate(
            selectedClass,
            spawnPos,
            Quaternion.identity);
    }

    private Vector3 GetPreferredSpawnPosition()
    {
        Vector3 groundCenter = GetGroundCenterPosition();
        Vector3 npcPosition = GetNpcPosition();

        if (npcPosition != Vector3.zero)
        {
            Vector3 nearNpc = GetClosestValidGroundPosition(npcPosition, npcSearchRadius);
            if (nearNpc != Vector3.zero)
            {
                return nearNpc;
            }
        }

        if (groundCenter != Vector3.zero)
        {
            Vector3 centerSpawn = GetClosestValidGroundPosition(groundCenter, float.MaxValue);
            if (centerSpawn != Vector3.zero)
            {
                return centerSpawn;
            }
        }

        return GroundPositionManager.GetRandomGroundPosition(minRoomScore, true);
    }

    private Vector3 GetNpcPosition()
    {
        GameObject npc = GameObject.Find("NPC");
        if (npc != null)
        {
            return npc.transform.position;
        }

        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("NPC"))
            {
                return obj.transform.position;
            }
        }

        return Vector3.zero;
    }

    private Vector3 GetGroundCenterPosition()
    {
        if (GroundPositionManager.groundPositions.Count == 0)
        {
            return Vector3.zero;
        }

        Vector3 sum = Vector3.zero;
        foreach (Vector3 position in GroundPositionManager.groundPositions)
        {
            sum += position;
        }

        return sum / GroundPositionManager.groundPositions.Count;
    }

    private Vector3 GetClosestValidGroundPosition(Vector3 referencePosition, float maxDistance)
    {
        Vector3 bestPosition = Vector3.zero;
        float bestDistance = float.MaxValue;
        float maxDistanceSqr = maxDistance * maxDistance;

        foreach (Vector3 groundPosition in GroundPositionManager.groundPositions)
        {
            float distanceToReference = (groundPosition - referencePosition).sqrMagnitude;
            if (distanceToReference > maxDistanceSqr)
            {
                continue;
            }

            if (minRoomScore > 0)
            {
                Vector3 current = groundPosition;
                Vector3 searchCenter = referencePosition;
                if ((current - searchCenter).sqrMagnitude > maxDistanceSqr)
                {
                    continue;
                }
            }

            if (distanceToReference < bestDistance)
            {
                bestDistance = distanceToReference;
                bestPosition = groundPosition;
            }
        }

        return bestPosition;
    }
}