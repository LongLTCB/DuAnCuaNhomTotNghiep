using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class TilemapVisualizer : MonoBehaviour
{
    [SerializeField]
    private Tilemap floorTilemap, wallTilemap;
    [SerializeField]
    private TileBase floorTile, wallTop, wallSideRight, wallSiderLeft, wallBottom, wallFull,
    wallInnerCornerDownLeft, wallInnerCornerDownRight,
        wallDiagonalCornerDownRight, wallDiagonalCornerDownLeft, wallDiagonalCornerUpRight, wallDiagonalCornerUpLeft;

    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
        PaintTiles(floorPositions, floorTilemap, floorTile);
    }

    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (var position in positions)
        {
            PaintSingleTile(tilemap, tile, position);
        }
    }

    internal void PaintSingleBasicWall(Vector2Int position, string binaryType)
    {
        int typeAsInt = Convert.ToInt32(binaryType, 2);
        if (!ThuatToanMaNhiPhan.TryResolveBasicShape(typeAsInt, out var wallShape))
        {
            return;
        }

        var tile = ResolveWallTile(wallShape);
        if (tile != null)
        {
            PaintSingleTile(wallTilemap, tile, position);
        }
    }

    private void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2Int position)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tilePosition, tile);
    }

    private TileBase ResolveWallTile(WallShape wallShape)
    {
        switch (wallShape)
        {
            case WallShape.Top:
                return wallTop;
            case WallShape.Left:
                return wallSiderLeft;
            case WallShape.Right:
                return wallSideRight;
            case WallShape.Bottom:
                return wallBottom;
            default:
                return wallFull;
        }
    }

    public void Clear()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
    }

    internal void PaintSingleCornerWall(Vector2Int position, string binaryType)
    {
        int typeAsInt = Convert.ToInt32(binaryType, 2);
        TileBase tile = null;
        if (!ThuatToanMaNhiPhan.wallInnerCornerDownLeft.Contains(typeAsInt))
        {
            tile = wallInnerCornerDownLeft;
       }
        else if (!ThuatToanMaNhiPhan.wallInnerCornerDownRight.Contains(typeAsInt))
        {
            tile = wallInnerCornerDownRight;
        }
        else if (!ThuatToanMaNhiPhan.wallDiagonalCornerDownLeft.Contains(typeAsInt))
        {
            tile = wallDiagonalCornerDownLeft;
        }
        else if (!ThuatToanMaNhiPhan.wallDiagonalCornerDownRight.Contains(typeAsInt))
        {
            tile = wallDiagonalCornerDownRight; // Đã sửa lại cho đúng biến DownRight thay vì DownLeft như video
        }
        else if (!ThuatToanMaNhiPhan.wallDiagonalCornerUpRight.Contains(typeAsInt))
        {
            tile = wallDiagonalCornerUpRight;
        }
        else if (!ThuatToanMaNhiPhan.wallDiagonalCornerUpLeft.Contains(typeAsInt))
        {
            tile = wallDiagonalCornerUpLeft;
        }
        else if (!ThuatToanMaNhiPhan.wallFullEightDirections.Contains(typeAsInt))
        {
            tile = wallFull;
        }
        else if (!ThuatToanMaNhiPhan.wallBottmEightDirections.Contains(typeAsInt))
        {
            tile = wallBottom;
        }

        if (tile != null)
        
            PaintSingleTile(wallTilemap, tile, position);
        
    }
}