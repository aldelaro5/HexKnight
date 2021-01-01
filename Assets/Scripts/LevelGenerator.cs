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
    ExitRoomWall,
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
  [SerializeField] private GameObject exitWallPrefab;
  [SerializeField] private GameObject borderWallPrefab;
  [SerializeField] private GameObject roomPrefab;
  [SerializeField] private GameObject corridorPrefab;
  [SerializeField] private GameObject enemyPrefab;

  [SerializeField] [Min(6)] private int levelSize = 10;
  [SerializeField] [Min(1)] private int tileSize = 5;
  [SerializeField] private int prefferedMinNbrRoom = 1;
  [SerializeField] private int maxNbrRoom = 5;
  [SerializeField] private Vector2Int MinRoomSize;
  [SerializeField] private Vector2Int MaxRoomSize;
  [SerializeField] private Vector2Int MinStartRoomSize;
  [SerializeField] private Vector2Int MaxStartRoomSize;
  [SerializeField] private Vector2Int MinEndRoomSize;
  [SerializeField] private Vector2Int MaxEndRoomSize;
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
        // Never spawn in the start and end room
        Room room = rooms[Random.Range(2, rooms.Count)];
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
    for (int i = 0; i < rooms.Count; i++)
    {
      // Do not consider the start and end room room as free for objects to spawn
      if (i <= 1)
        continue;

      for (int x = rooms[i].posBottomLeft.x; x < rooms[i].posBottomLeft.x + rooms[i].size.x; x++)
      {
        for (int y = rooms[i].posBottomLeft.y; y < rooms[i].posBottomLeft.y + rooms[i].size.y; y++)
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
    for (int i = 0; i < rooms.Count; i++)
    {
      List<CorridorCandidate> possibleCorridors = null;
      if (i == 1)
      {
        Room exitRoom = rooms[1];
        // Act as if the exit room included the walls and get the corridors that leads to them
        Room endRoomWithWall = new Room()
        {
          posBottomLeft = new Vector2Int(exitRoom.posBottomLeft.x - 1, exitRoom.posBottomLeft.y - 1),
          size = new Vector2Int(exitRoom.size.x + 1, exitRoom.size.y + 1)
        };
        possibleCorridors = FindAllPossibleCorridorsForRoom(endRoomWithWall);
      }
      else
      {
        possibleCorridors = FindAllPossibleCorridorsForRoom(rooms[i]);
      }

      var nbrTilesLongestCorridor = possibleCorridors.Max(x => x.tiles.Count);
      var longestCorridors = possibleCorridors.Where(x => x.tiles.Count == nbrTilesLongestCorridor).ToArray();
      CorridorCandidate chosenCorridor = longestCorridors[Random.Range(0, longestCorridors.Length)];

      foreach (var tilePos in chosenCorridor.tiles)
        levelTiles[tilePos.x][tilePos.y].tileType = TileType.Corridor;
    }
  }

  private bool CanTileBeACorridor(int x, int y)
  {
    return levelTiles[x][y].tileType != TileType.Room && levelTiles[x][y].tileType != TileType.ExitRoomWall;
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
              if (CanTileBeACorridor(x, y))
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
              if (CanTileBeACorridor(x, y))
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
              if (CanTileBeACorridor(x, y))
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
              if (CanTileBeACorridor(x, y))
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
    GenerateStartAndEndRoom();
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

      PlaceWallsAroundRoom(sizeX, sizeY, posBottomLeftX, posBottomLeftY, false);
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

  private void GenerateStartAndEndRoom()
  {
    int sizeStartX = Random.Range(MinStartRoomSize.x, MaxStartRoomSize.x + 1);
    int sizeStartY = Random.Range(MinStartRoomSize.y, MaxStartRoomSize.y + 1);
    for (int x = 0; x < sizeStartX; x++)
    {
      for (int y = 0; y < sizeStartY; y++)
        levelTiles[x][y].tileType = TileType.Room;
    }
    PlaceWallsAroundRoom(sizeStartX, sizeStartY, 0, 0, false);
    Room startRoom = new Room()
    {
      posBottomLeft = new Vector2Int(0, 0),
      size = new Vector2Int(sizeStartX, sizeStartY)
    };
    rooms.Add(startRoom);

    int sizeEndX = Random.Range(MinEndRoomSize.x, MaxEndRoomSize.x + 1);
    int sizeEndY = Random.Range(MinEndRoomSize.y, MaxEndRoomSize.y + 1);
    int posXEnd = levelSize - sizeEndX;
    int posYEnd = levelSize - sizeEndY;
    for (int x = posXEnd; x < posXEnd + sizeEndX; x++)
    {
      for (int y = posYEnd; y < posYEnd + sizeEndY; y++)
        levelTiles[x][y].tileType = TileType.Room;
    }
    PlaceWallsAroundRoom(sizeEndX, sizeEndY, posXEnd, posYEnd, true);
    // Corner of the exit room
    levelTiles[posXEnd - 1][posYEnd - 1].tileType = TileType.ExitRoomWall;
    Room endRoom = new Room()
    {
      posBottomLeft = new Vector2Int(posXEnd, posYEnd),
      size = new Vector2Int(sizeEndX, sizeEndY)
    };
    rooms.Add(endRoom);
  }

  private void PlaceWallsAroundRoom(int sizeX, int sizeY, int posBottomLeftX, int posBottomLeftY, bool exitRoom)
  {
    TileType wallType = exitRoom ? TileType.ExitRoomWall : TileType.Wall;
    for (int i = 0; i < sizeY; i++)
    {
      int rowPos = i + posBottomLeftY;
      if (posBottomLeftX - 1 >= 0)
      {
        if (levelTiles[posBottomLeftX - 1][rowPos].tileType != TileType.ExitRoomWall)
          levelTiles[posBottomLeftX - 1][rowPos].tileType = wallType;
      }

      if (posBottomLeftX + sizeX < levelSize)
      {
        if (levelTiles[posBottomLeftX + sizeX][rowPos].tileType != TileType.ExitRoomWall)
          levelTiles[posBottomLeftX + sizeX][rowPos].tileType = wallType;
      }
    }

    for (int j = 0; j < sizeX; j++)
    {
      int columnPos = j + posBottomLeftX;
      if (posBottomLeftY - 1 >= 0)
      {
        if (levelTiles[columnPos][posBottomLeftY - 1].tileType != TileType.ExitRoomWall)
          levelTiles[columnPos][posBottomLeftY - 1].tileType = wallType;
      }

      if (posBottomLeftY + sizeY < levelSize)
      {
        if (levelTiles[columnPos][posBottomLeftY + sizeY].tileType != TileType.ExitRoomWall)
          levelTiles[columnPos][posBottomLeftY + sizeY].tileType = wallType;
      }
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

        if (!CanTileBeInARoom(x + posBottomLeftX, y + posBottomLeftY))
          return false;
      }
    }

    return true;
  }

  private bool CanTileBeInARoom(int x, int y)
  {
    TileType tileType = levelTiles[x][y].tileType;
    return tileType != TileType.Room && tileType != TileType.Wall && tileType != TileType.ExitRoomWall;
  }

  private void GenerateLevelFromTiles()
  {
    for (int edgeIndex = -1; edgeIndex <= levelSize; edgeIndex++)
    {
      Vector3 posToptEdge = new Vector3(edgeIndex * tileSize, 0, levelSize * tileSize);
      GameObject tileTopEdge = Instantiate(borderWallPrefab, posToptEdge, Quaternion.identity, this.transform);
      tileTopEdge.transform.localScale = tileScaleVec;
      tileTopEdge.name = (int)posToptEdge.x / tileSize + ", " + (int)posToptEdge.z / tileSize;

      Vector3 posBottomEdge = new Vector3(edgeIndex * tileSize, 0, -tileSize);
      GameObject tileBottomEdge = Instantiate(borderWallPrefab, posBottomEdge, Quaternion.identity, this.transform);
      tileBottomEdge.transform.localScale = tileScaleVec;
      tileBottomEdge.name = (int)posBottomEdge.x / tileSize + ", " + (int)posBottomEdge.z / tileSize;
    }

    for (int i = 0; i < levelSize; i++)
    {
      Vector3 posLeftEdge = new Vector3(-tileSize, 0, i * tileSize);
      GameObject tileLeftEdge = Instantiate(borderWallPrefab, posLeftEdge, Quaternion.identity, this.transform);
      tileLeftEdge.transform.localScale = tileScaleVec;
      tileLeftEdge.name = (int)posLeftEdge.x / tileSize + ", " + (int)posLeftEdge.z / tileSize;

      Vector3 posRightEdge = new Vector3(levelSize * tileSize, 0, i * tileSize);
      GameObject tileRightEdge = Instantiate(borderWallPrefab, posRightEdge, Quaternion.identity, this.transform);
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
          case TileType.ExitRoomWall:
            prefabTile = exitWallPrefab;
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
