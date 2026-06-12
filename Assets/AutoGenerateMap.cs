using UnityEngine;

public class AutoGenerateMap : MonoBehaviour
{
    public AbstractDungeonGenerator dungeon;

    void Start()
    {
        dungeon.GenerateDungeon();
    }
}