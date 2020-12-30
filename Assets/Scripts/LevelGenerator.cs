using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
  private enum Tile
  {
    Floor,
    Wall,
    Room
  }

  private struct Room
  {
    public Vector2Int size;
    public Vector2Int posTopLeft;
  }

  [SerializeField] private GameObject floorPrefab;
  [SerializeField] private GameObject wallPrefab;
  [SerializeField] private GameObject roomPrefab;

  [SerializeField] [Min(1)] private int levelSize = 10;
  [SerializeField] [Min(1)] private int tileSize = 5;
  [SerializeField] private int prefferedMinNbrRoom = 1;
  [SerializeField] private int maxNbrRoom = 5;
  [SerializeField] private Vector2Int MinRoomSize;
  [SerializeField] private Vector2Int MaxRoomSize;

  private Vector3 tileScaleVec;

  private Tile[][] levelTiles;

  void Start()
  {
    InitialiseGenerator();
    GenerateRooms();
    GenerateLevelFromTiles();
  }

  private void GenerateRooms()
  {
    int targetNbrRoom = Random.Range(prefferedMinNbrRoom, maxNbrRoom + 1);
    Room[] rooms = new Room[targetNbrRoom];
    int nbrRoomGenerated = 0;
    bool freeRoomKnownToExists = false;
    while (nbrRoomGenerated < targetNbrRoom)
    {
      if (!freeRoomKnownToExists)
      {
        if (!IsRoomOfSizeFree(MinRoomSize.x, MinRoomSize.y))
          break;
        else
          freeRoomKnownToExists = true;
      }

      int sizeX = Random.Range(MinRoomSize.x, MaxRoomSize.x + 1);
      int sizeY = Random.Range(MinRoomSize.y, MaxRoomSize.y + 1);
      int posTopLeftX = Random.Range(0, levelSize - (sizeX - 1));
      int posTopLeftY = Random.Range(0, levelSize - (sizeY - 1));

      if (!IsRoomValid(sizeX, sizeY, posTopLeftX, posTopLeftY))
        continue;

      for (int x = 0; x < sizeX; x++)
      {
        for (int y = 0; y < sizeY; y++)
        {
          if (x + posTopLeftX >= levelSize || y + posTopLeftY >= levelSize)
            continue;

          levelTiles[x + posTopLeftX][y + posTopLeftY] = Tile.Room;
        }
      }

      for (int i = 0; i < sizeY; i++)
      {
        int rowPos = i + posTopLeftY;
        if (posTopLeftX - 1 >= 0)
          levelTiles[posTopLeftX - 1][rowPos] = Tile.Wall;

        if (posTopLeftX + sizeX < levelSize)
          levelTiles[posTopLeftX + sizeX][rowPos] = Tile.Wall;
      }

      for (int j = 0; j < sizeX; j++)
      {
        int columnPos = j + posTopLeftX;
        if (posTopLeftY - 1 >= 0)
          levelTiles[columnPos][posTopLeftY - 1] = Tile.Wall;

        if (posTopLeftY + sizeY < levelSize)
          levelTiles[columnPos][posTopLeftY + sizeY] = Tile.Wall;
      }

      rooms[nbrRoomGenerated].size.x = sizeX;
      rooms[nbrRoomGenerated].size.y = sizeY;
      rooms[nbrRoomGenerated].posTopLeft.x = posTopLeftX;
      rooms[nbrRoomGenerated].posTopLeft.y = posTopLeftY;

      freeRoomKnownToExists = false;
      nbrRoomGenerated++;
    }
  }

  private bool IsRoomOfSizeFree(int sizeX, int sizeY)
  {
    var listFreeRoomPos = new List<Vector2Int>();
    for (int x = 0; x < levelSize - (sizeX - 1); x++)
    {
      for (int y = 0; y < levelSize - (sizeY - 1); y++)
      {
        if (levelTiles[x][y] == Tile.Floor)
          listFreeRoomPos.Add(new Vector2Int(x, y));
      }
    }

    foreach (var roomPosCandidate in listFreeRoomPos)
    {
      if (IsRoomValid(sizeX, sizeY, roomPosCandidate.x, roomPosCandidate.y))
        return true;
    }

    return false;
  }

  private bool IsRoomValid(int sizeX, int sizeY, int posTopLeftX, int posTopLeftY)
  {
    if (posTopLeftX >= levelSize - (sizeX - 1))
      return false;
    if (posTopLeftY >= levelSize - (sizeY - 1))
      return false;

    for (int x = 0; x < sizeX; x++)
    {
      for (int y = 0; y < sizeY; y++)
      {
        if (x + posTopLeftX >= levelSize || y + posTopLeftY >= levelSize)
          continue;

        Tile tile = levelTiles[x + posTopLeftX][y + posTopLeftY];
        if (tile == Tile.Room || tile == Tile.Wall)
          return false;
      }
    }

    return true;
  }

  private void GenerateLevelFromTiles()
  {
    for (int edgeIndex = -1; edgeIndex <= levelSize; edgeIndex++)
    {
      Vector3 posToptEdge = new Vector3(edgeIndex * tileSize, 0, levelSize * tileSize);
      GameObject tileTopEdge = Instantiate(wallPrefab, posToptEdge, Quaternion.identity, this.transform);
      tileTopEdge.transform.localScale = tileScaleVec;
      tileTopEdge.name = (int)posToptEdge.x / tileSize + ", " + (int)posToptEdge.z / tileSize;

      Vector3 posBottomEdge = new Vector3(edgeIndex * tileSize, 0, -tileSize);
      GameObject tileBottomEdge = Instantiate(wallPrefab, posBottomEdge, Quaternion.identity, this.transform);
      tileBottomEdge.transform.localScale = tileScaleVec;
      tileBottomEdge.name = (int)posBottomEdge.x / tileSize + ", " + (int)posBottomEdge.z / tileSize;
    }

    for (int i = 0; i < levelSize; i++)
    {
      Vector3 posLeftEdge = new Vector3(-tileSize, 0, i * tileSize);
      GameObject tileLeftEdge = Instantiate(wallPrefab, posLeftEdge, Quaternion.identity, this.transform);
      tileLeftEdge.transform.localScale = tileScaleVec;
      tileLeftEdge.name = (int)posLeftEdge.x / tileSize + ", " + (int)posLeftEdge.z / tileSize;

      Vector3 posRightEdge = new Vector3(levelSize * tileSize, 0, i * tileSize);
      GameObject tileRightEdge = Instantiate(wallPrefab, posRightEdge, Quaternion.identity, this.transform);
      tileRightEdge.transform.localScale = tileScaleVec;
      tileRightEdge.name = (int)posRightEdge.x / tileSize + ", " + (int)posRightEdge.z / tileSize;

      for (int j = 0; j < levelSize; j++)
      {
        // The Z pos is inverted compared to the tile map
        Vector3 pos = new Vector3(i * tileSize, 0, (levelSize - 1) * tileSize - (j * tileSize));
        Tile tile = levelTiles[i][j];
        GameObject prefab = null;
        switch (tile)
        {
          case Tile.Floor:
            prefab = floorPrefab;
            break;
          case Tile.Wall:
            prefab = wallPrefab;
            break;
          case Tile.Room:
            prefab = roomPrefab;
            break;
          default:
            Debug.LogError("Unhandled tile type " + tile.ToString());
            break;
        }
        GameObject objTile = Instantiate(prefab, pos, floorPrefab.transform.rotation, this.transform);
        objTile.transform.localScale = tileScaleVec;
        objTile.name = i + ", " + j;
      }
    }
  }

  private void InitialiseGenerator()
  {
    tileScaleVec = new Vector3((float)tileSize, (float)tileSize, (float)tileSize);
    levelTiles = new Tile[levelSize][];
    for (int i = 0; i < levelSize; i++)
      levelTiles[i] = new Tile[levelSize];

    for (int j = 0; j < levelSize; j++)
    {
      for (int k = 0; k < levelSize; k++)
        levelTiles[j][k] = Tile.Floor;
    }
  }
}
