using UnityEngine;
using System.Collections.Generic;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance;

    public List<Vector2> floorPositions =
        new List<Vector2>();

    private void Awake()
    {
        Instance = this;
    }
}