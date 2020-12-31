using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
  private enum TileType
  {
    Floor,
    Wall,
    Room,
    Corridor
  }

  private enum TileObj
  {
    Nothing,
    Enemy
  }

  private enum CorridorDirection
  {
    Up = 0,
    Right,
    Down,
    Left
  }

  private readonly List<CorridorDirection> directions = Enum.GetValues(typeof(CorridorDirection)).Cast<CorridorDirection>().ToList();

  private struct Room
  {
    public Vector2Int size;
    public Vector2Int posBottomLeft;
  }

  private struct CorridorCandidate
  {
    public CorridorDirection direction;
    public Vector2Int pos;
    public List<Vector2Int> tiles;
  }

  private struct Tile
  {
    public TileType tileType;
    public TileObj tileObj;
  }

  private List<Room> rooms = new List<Room>();

  [SerializeField] private GameObject floorPrefab;
  [SerializeField] private GameObject wallPrefab;
  [SerializeField] private GameObject roomPrefab;
  [SerializeField] private GameObject corridorPrefab;
  [SerializeField] private GameObject enemyPrefab;

  [SerializeField] [Min(1)] private int levelSize = 10;
  [SerializeField] [Min(1)] private int tileSize = 5;
  [SerializeField] private int prefferedMinNbrRoom = 1;
  [SerializeField] private int maxNbrRoom = 5;
  [SerializeField] private Vector2Int MinRoomSize;
  [SerializeField] private Vector2Int MaxRoomSize;
  [SerializeField] private int nbrEnemies = 3;

  private Vector3 tileScaleVec;

  private Tile[][] levelTiles;

  void Start()
  {
    InitialiseGenerator();
    GenerateRooms();
    GenerateCorridors();
    GenerateEnemies();
    GenerateLevelFromTiles();
  }

  private void GenerateEnemies()
  {
    for (int i = 0; i < nbrEnemies; i++)
    {
      if (!IsAnyTileInRoomsFree())
        break;

      bool tileSelected = false;
      int xPos = -1;
      int yPos = -1;
      while (!tileSelected)
      {
        Room room = rooms[Random.Range(0, rooms.Count)];
        xPos = Random.Range(room.posBottomLeft.x, room.posBottomLeft.x + room.size.x);
        yPos = Random.Range(room.posBottomLeft.y, room.posBottomLeft.y + room.size.y);
        
        if (levelTiles[xPos][yPos].tileObj == TileObj.Nothing)
          tileSelected = true;
      }
      levelTiles[xPos][yPos].tileObj = TileObj.Enemy;
    }
  }

  private bool IsAnyTileInRoomsFree()
  {
    foreach (var room in rooms)
    {
      for (int x = room.posBottomLeft.x; x < room.posBottomLeft.x + room.size.x; x++)
      {
        for (int y = room.posBottomLeft.y; y < room.posBottomLeft.y + room.size.y; y++)
        {
          if (levelTiles[x][y].tileObj == TileObj.Nothing)
            return true;
        }
      }
    }

    return false;
  }

  private void GenerateCorridors()
  {
    foreach (var room in rooms)
    {
      var possibleCorridors = FindAllPossibleCorridorsForRoom(room);
      var nbrTilesLongestCorridor = possibleCorridors.Max(x => x.tiles.Count);
      var longestCorridors = possibleCorridors.Where(x => x.tiles.Count == nbrTilesLongestCorridor).ToArray();
      CorridorCandidate chosenCorridor = longestCorridors[Random.Range(0, longestCorridors.Length)];

      foreach (var tilePos in chosenCorridor.tiles)
        levelTiles[tilePos.x][tilePos.y].tileType = TileType.Corridor;
    }
  }

  private List<CorridorCandidate> FindAllPossibleCorridorsForRoom(Room room)
  {
    List<CorridorCandidate> possibleCorridors = new List<CorridorCandidate>();
    foreach (var direction in directions)
    {
      switch (direction)
      {
        case CorridorDirection.Up:
          if (room.posBottomLeft.y + room.size.y > levelSize)
            break;

          for (int x = room.posBottomLeft.x; x < room.posBottomLeft.x + room.size.x; x++)
          {
            int y = room.posBottomLeft.y + room.size.y;
            CorridorCandidate candidate;
            candidate.direction = CorridorDirection.Up;
            candidate.pos = new Vector2Int(x, y);
            candidate.tiles = new List<Vector2Int>();
            while (y < levelSize)
            {
              if (levelTiles[x][y].tileType != TileType.Room)
                candidate.tiles.Add(new Vector2Int(x, y));
              else
                break;

              y++;
            }
            possibleCorridors.Add(candidate);
          }
          break;
        case CorridorDirection.Right:
          if (room.posBottomLeft.x + room.size.x >= levelSize)
            break;

          for (int y = room.posBottomLeft.y; y < room.posBottomLeft.y + room.size.y; y++)
          {
            int x = room.posBottomLeft.x + room.size.x;
            CorridorCandidate candidate;
            candidate.direction = CorridorDirection.Right;
            candidate.pos = new Vector2Int(x, y);
            candidate.tiles = new List<Vector2Int>();
            while (x < levelSize)
            {
              if (levelTiles[x][y].tileType != TileType.Room)
                candidate.tiles.Add(new Vector2Int(x, y));
              else
                break;

              x++;
            }
            possibleCorridors.Add(candidate);
          }
          break;
        case CorridorDirection.Down:
          if (room.posBottomLeft.y - 1 < 0)
            break;

          for (int x = room.posBottomLeft.x; x < room.posBottomLeft.x + room.size.x; x++)
          {
            int y = room.posBottomLeft.y - 1;
            CorridorCandidate candidate;
            candidate.direction = CorridorDirection.Down;
            candidate.pos = new Vector2Int(x, y);
            candidate.tiles = new List<Vector2Int>();
            while (y >= 0)
            {
              if (levelTiles[x][y].tileType != TileType.Room)
                candidate.tiles.Add(new Vector2Int(x, y));
              else
                break;

              y--;
            }
            possibleCorridors.Add(candidate);
          }
          break;
        case CorridorDirection.Left:
          if (room.posBottomLeft.x - 1 < 0)
            break;

          for (int y = room.posBottomLeft.y; y < room.posBottomLeft.y + room.size.y; y++)
          {
            int x = room.posBottomLeft.x - 1;
            CorridorCandidate candidate;
            candidate.direction = CorridorDirection.Left;
            candidate.pos = new Vector2Int(x, y);
            candidate.tiles = new List<Vector2Int>();
            while (x >= 0)
            {
              if (levelTiles[x][y].tileType != TileType.Room)
                candidate.tiles.Add(new Vector2Int(x, y));
              else
                break;

              x--;
            }
            possibleCorridors.Add(candidate);
          }
          break;
        default:
          Debug.LogError("Unsuported corridor direction");
          break;
      }
    }
    return possibleCorridors;
  }

  private void GenerateRooms()
  {
    int targetNbrRoom = Random.Range(prefferedMinNbrRoom, maxNbrRoom + 1);
    int nbrRoomGenerated = 0;
    bool freeRoomKnownToExists = false;
    while (nbrRoomGenerated < targetNbrRoom)
    {
      if (!freeRoomKnownToExists)
      {
        if (!CanAllocateRoomOfSize(MinRoomSize.x, MinRoomSize.y))
          break;
        else
          freeRoomKnownToExists = true;
      }

      int sizeX = Random.Range(MinRoomSize.x, MaxRoomSize.x + 1);
      int sizeY = Random.Range(MinRoomSize.y, MaxRoomSize.y + 1);
      int posBottomLeftX = Random.Range(0, levelSize - (sizeX - 1));
      int posBottomLeftY = Random.Range(0, levelSize - (sizeY - 1));

      if (!IsRoomValid(sizeX, sizeY, posBottomLeftX, posBottomLeftY))
        continue;

      for (int x = 0; x < sizeX; x++)
      {
        for (int y = 0; y < sizeY; y++)
        {
          if (x + posBottomLeftX >= levelSize || y + posBottomLeftY >= levelSize)
            continue;

          levelTiles[x + posBottomLeftX][y + posBottomLeftY].tileType = TileType.Room;
        }
      }

      PlaceWallsAroundRoom(sizeX, sizeY, posBottomLeftX, posBottomLeftY);
      Room room = new Room()
      {
        size = new Vector2Int(sizeX, sizeY),
        posBottomLeft = new Vector2Int(posBottomLeftX, posBottomLeftY)
      };
      rooms.Add(room);

      freeRoomKnownToExists = false;
      nbrRoomGenerated++;
    }
  }

  private void PlaceWallsAroundRoom(int sizeX, int sizeY, int posBottomLeftX, int posBottomLeftY)
  {
    for (int i = 0; i < sizeY; i++)
    {
      int rowPos = i + posBottomLeftY;
      if (posBottomLeftX - 1 >= 0)
        levelTiles[posBottomLeftX - 1][rowPos].tileType = TileType.Wall;

      if (posBottomLeftX + sizeX < levelSize)
        levelTiles[posBottomLeftX + sizeX][rowPos].tileType = TileType.Wall;
    }

    for (int j = 0; j < sizeX; j++)
    {
      int columnPos = j + posBottomLeftX;
      if (posBottomLeftY - 1 >= 0)
        levelTiles[columnPos][posBottomLeftY - 1].tileType = TileType.Wall;

      if (posBottomLeftY + sizeY < levelSize)
        levelTiles[columnPos][posBottomLeftY + sizeY].tileType = TileType.Wall;
    }
  }

  private bool CanAllocateRoomOfSize(int sizeX, int sizeY)
  {
    var listFreeRoomPos = new List<Vector2Int>();
    for (int x = 0; x < levelSize - (sizeX - 1); x++)
    {
      for (int y = 0; y < levelSize - (sizeY - 1); y++)
      {
        if (levelTiles[x][y].tileType == TileType.Floor)
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

  private bool IsRoomValid(int sizeX, int sizeY, int posBottomLeftX, int posBottomLeftY)
  {
    if (posBottomLeftX >= levelSize - (sizeX - 1))
      return false;
    if (posBottomLeftY >= levelSize - (sizeY - 1))
      return false;

    for (int x = 0; x < sizeX; x++)
    {
      for (int y = 0; y < sizeY; y++)
      {
        if (x + posBottomLeftX >= levelSize || y + posBottomLeftY >= levelSize)
          continue;

        Tile tile = levelTiles[x + posBottomLeftX][y + posBottomLeftY];
        if (tile.tileType == TileType.Room || tile.tileType == TileType.Wall)
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
        Vector3 pos = new Vector3(i * tileSize, 0, j * tileSize);
        TileType tile = levelTiles[i][j].tileType;
        GameObject prefabTile = null;
        switch (tile)
        {
          case TileType.Floor:
            prefabTile = floorPrefab;
            break;
          case TileType.Wall:
            prefabTile = wallPrefab;
            break;
          case TileType.Room:
            prefabTile = roomPrefab;
            break;
          case TileType.Corridor:
            prefabTile = corridorPrefab;
            break;
          default:
            Debug.LogError("Unhandled tile type " + tile.ToString());
            break;
        }
        GameObject objTile = Instantiate(prefabTile, pos, floorPrefab.transform.rotation, this.transform);
        objTile.transform.localScale = tileScaleVec;
        objTile.name = i + ", " + j;

        TileObj obj = levelTiles[i][j].tileObj;
        GameObject prefabObj = null;
        switch (obj)
        {
          case TileObj.Enemy:
            prefabObj = enemyPrefab;
            break;
          case TileObj.Nothing:
          default:
            break;
        }

        if (prefabObj != null)
        {
          Vector3 posCenter = new Vector3(i * tileSize + (float)tileSize / 2f, 0, j * tileSize + (float)tileSize / 2f);
          Instantiate(prefabObj, posCenter, Quaternion.identity, objTile.transform);
        }
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
      {
        levelTiles[j][k].tileType = TileType.Floor;
        levelTiles[j][k].tileObj = TileObj.Nothing;
      }
    }
  }
}
