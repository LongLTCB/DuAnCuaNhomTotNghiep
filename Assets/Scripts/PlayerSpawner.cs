using UnityEngine;
using Photon.Pun;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
    IEnumerator Start()
    {
        while (!PhotonNetwork.InRoom)
        {
            yield return null;
        }

        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        Vector3 spawnPos =
            GroundPositionManager.GetRandomGroundPosition();

        string selectedClass =
            PlayerPrefs.GetString(
                "MySelectedClass",
                "Class_Warrior");

        PhotonNetwork.Instantiate(
            selectedClass,
            spawnPos,
            Quaternion.identity);
    }
}