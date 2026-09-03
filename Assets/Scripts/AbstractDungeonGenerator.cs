using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
    [SerializeField]
    protected TilemapVisualizer tilemapVisualizer = null;
    [SerializeField]
    protected Vector2Int startPosition = Vector2Int.zero;
    // Danh sách các ô nền của dungeon
    public static HashSet<Vector2Int> FloorPositions =
        new HashSet<Vector2Int>();

// Báo hiệu dungeon đã sinh xong
 public static bool GenerationFinished = false;
    public void GenerateDungeon()
    {
         GenerationFinished = false;

    tilemapVisualizer.Clear();

    RunProceduralGeneration();

    GenerationFinished = true;
    }

    protected abstract void RunProceduralGeneration();
}