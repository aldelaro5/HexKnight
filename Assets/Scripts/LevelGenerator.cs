using System;
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
    Enemy,
    ExitUnlocker,
    HealthDrop
  }

  public enum TileState
  {
    Free,
    Blocked,
    ExitUnlockerBlocked,
    ExitBlocked,
    Enemy,
    Player
  }

  public struct TileInfo
  {
    public TileState state;
    public GameObject obj;
  }

  private readonly Direction[] directions = Enum.GetValues(typeof(Direction)).Cast<Direction>()
                                                .Where(x => x != Direction.NONE).ToArray();

  private struct Room
  {
    public Vector2Int size;
    public Vector2Int posBottomLeft;
  }

  private struct CorridorCandidate
  {
    public Direction direction;
    public Vector2Int pos;
    public List<Vector2Int> tiles;
  }

  private struct Tile
  {
    public TileType tileType;
    public TileObj tileObj;
  }

  private readonly float[] tileRotations = new float[] { 0, 90, 180, 270 };

  [SerializeField] private LevelGeneratorParams levelParams;

  [SerializeField] private Material floorMaterial;
  [SerializeField] private Material corridorMaterial;
  [SerializeField] private Material roomMaterial;
  [SerializeField] private GameObject floorPrefab;
  [SerializeField] private GameObject wallPrefab;
  [SerializeField] private GameObject exitWallPrefab;
  [SerializeField] private GameObject roomPrefab;
  [SerializeField] private GameObject corridorPrefab;
  [SerializeField] private GameObject enemyPrefab;
  [SerializeField] private GameObject healthDropPrefab;
  [SerializeField] private GameObject turretPrefab;
  [SerializeField] private GameObject playerPrefab;
  [SerializeField] private GameObject collectiblePrefab;
  [SerializeField] private GameObject dataLinePrefab;

  private GameObject player = null;
  public GameObject Player { get => player; }
  private GameManager game;

  private Vector3 tileScaleVec;
  private float halfTileSize;

  private List<Room> rooms = new List<Room>();
  private Tile[][] levelTiles;
  private TileInfo[][] tilesInfo;

  public int TileSize { get => levelParams.tileSize; }

  public TileInfo GetTileInfo(Vector2Int tile)
  {
    if (tile.x < 0 || tile.y < 0 || tile.x >= levelParams.levelSize || tile.y >= levelParams.levelSize)
      return new TileInfo() { state = TileState.Blocked, obj = null };
    return tilesInfo[tile.x][tile.y];
  }

  public void ReserveTileAsEnemy(Vector2Int tile, GameObject enemy)
  {
    tilesInfo[tile.x][tile.y].state = TileState.Enemy;
    tilesInfo[tile.x][tile.y].obj = enemy;
  }

  public void ReserveTileAsPlayer(Vector2Int tile, GameObject player)
  {
    tilesInfo[tile.x][tile.y].state = TileState.Player;
    tilesInfo[tile.x][tile.y].obj = player;
  }

  public void FreeTile(Vector2Int tile)
  {
    tilesInfo[tile.x][tile.y].state = TileState.Free;
    tilesInfo[tile.x][tile.y].obj = null;
  }

  public bool IsTileExitTile(Vector2Int tile)
  {
    return tile.x == levelParams.levelSize - 1 && tile.y == levelParams.levelSize;
  }

  public void UnlockExit()
  {
    for (int x = 0; x < levelParams.levelSize; x++)
    {
      for (int y = 0; y < levelParams.levelSize; y++)
      {
        if (tilesInfo[x][y].state == TileState.ExitBlocked)
        {
          Vector3 exitPos = tilesInfo[x][y].obj.transform.position;
          exitPos.y -= levelParams.tileSize;
          tilesInfo[x][y].obj.transform.position = exitPos;
          tilesInfo[x][y].state = TileState.Free;
        }
      }
    }
    game.ExitUnlocked();
  }

  public void ExitReached()
  {
    // If we are debugging the generator, just reload
    if (game == null && Application.isEditor)
    {
      foreach (Transform item in gameObject.transform)
        Destroy(item.gameObject);

      GenerateLevel(levelParams);
    }
    else
    {
      game.GoToNextLevel();
    }
  }

  public Vector2Int Vec3CenterToTile(Vector3 vec3)
  {
    return new Vector2Int(Mathf.RoundToInt((vec3.x - halfTileSize)) / levelParams.tileSize,
                          Mathf.RoundToInt((vec3.z - halfTileSize)) / levelParams.tileSize);
  }

  public Vector3 TileToVec3Center(Vector2Int tile)
  {
    return new Vector3((float)tile.x * (float)levelParams.tileSize + halfTileSize, 0,
                       (float)tile.y * (float)levelParams.tileSize + halfTileSize);
  }

  private void Start()
  {
    game = FindObjectOfType<GameManager>();
    // If we are debugging the generator
    if (game == null && Application.isEditor)
    {
      GenerateLevel(levelParams);
    }
  }

  private void InitialiseFloorMaterial()
  {
    int nbrFloorTextures = ((Texture2DArray)floorMaterial.GetTexture("_Textures")).depth;
    floorMaterial.SetInt("_NbrTextures", nbrFloorTextures);
    int nbrCorridorTextures = ((Texture2DArray)corridorMaterial.GetTexture("_Textures")).depth;
    corridorMaterial.SetInt("_NbrTextures", nbrCorridorTextures);
    int nbrRoomTextures = ((Texture2DArray)roomMaterial.GetTexture("_Textures")).depth;
    roomMaterial.SetInt("_NbrTextures", nbrRoomTextures);

    floorMaterial.SetFloat("_Seed", Random.value);
    corridorMaterial.SetFloat("_Seed", Random.value);
    roomMaterial.SetFloat("_Seed", Random.value);
  }

  public void GenerateLevel(LevelGeneratorParams genParams)
  {
    if (game == null)
      game = FindObjectOfType<GameManager>();

    InitialiseFloorMaterial();

    levelParams = genParams;
    halfTileSize = (float)levelParams.tileSize / 2f;
    InitialiseGenerator();
    GenerateRooms();
    GenerateCorridors();
    GenerateExitUnlocker();
    GenerateEnemies();
    GenerateHealthDrops();
    GenerateLevelFromTiles();
    SetupPlayer();
  }

  private void GenerateHealthDrops()
  {
    for (int i = 0; i < levelParams.nbrHealthDrops; i++)
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
      levelTiles[xPos][yPos].tileObj = TileObj.HealthDrop;
    }
  }

  private void SetupPlayer()
  {
    Vector3 startCenter = new Vector3(halfTileSize, 0, halfTileSize);
    if (player == null)
    {
      if (game != null)
        player = Instantiate(playerPrefab, startCenter, Quaternion.identity, game.transform);
      else
        player = Instantiate(playerPrefab, startCenter, Quaternion.identity);
      player.name = "Player";
    }
    Player playerComponent = player.GetComponent<Player>();
    playerComponent.ResetTile(startCenter);
  }

  private void GenerateExitUnlocker()
  {
    // Never spawn in the start and end room
    Room room = rooms[Random.Range(2, rooms.Count)];
    int xPos = Random.Range(room.posBottomLeft.x, room.posBottomLeft.x + room.size.x);
    int yPos = Random.Range(room.posBottomLeft.y, room.posBottomLeft.y + room.size.y);
    levelTiles[xPos][yPos].tileObj = TileObj.ExitUnlocker;
  }

  private void GenerateEnemies()
  {
    for (int i = 0; i < levelParams.nbrEnemies; i++)
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
        case Direction.Up:
          if (room.posBottomLeft.y + room.size.y > levelParams.levelSize)
            break;

          for (int x = room.posBottomLeft.x; x < room.posBottomLeft.x + room.size.x; x++)
          {
            int y = room.posBottomLeft.y + room.size.y;
            CorridorCandidate candidate;
            candidate.direction = Direction.Up;
            candidate.pos = new Vector2Int(x, y);
            candidate.tiles = new List<Vector2Int>();
            while (y < levelParams.levelSize)
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
        case Direction.Right:
          if (room.posBottomLeft.x + room.size.x >= levelParams.levelSize)
            break;

          for (int y = room.posBottomLeft.y; y < room.posBottomLeft.y + room.size.y; y++)
          {
            int x = room.posBottomLeft.x + room.size.x;
            CorridorCandidate candidate;
            candidate.direction = Direction.Right;
            candidate.pos = new Vector2Int(x, y);
            candidate.tiles = new List<Vector2Int>();
            while (x < levelParams.levelSize)
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
        case Direction.Down:
          if (room.posBottomLeft.y - 1 < 0)
            break;

          for (int x = room.posBottomLeft.x; x < room.posBottomLeft.x + room.size.x; x++)
          {
            int y = room.posBottomLeft.y - 1;
            CorridorCandidate candidate;
            candidate.direction = Direction.Down;
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
        case Direction.Left:
          if (room.posBottomLeft.x - 1 < 0)
            break;

          for (int y = room.posBottomLeft.y; y < room.posBottomLeft.y + room.size.y; y++)
          {
            int x = room.posBottomLeft.x - 1;
            CorridorCandidate candidate;
            candidate.direction = Direction.Left;
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
    int targetNbrRoom = Random.Range(levelParams.prefferedMinNbrRoom, levelParams.maxNbrRoom + 1);
    int nbrRoomGenerated = 0;
    bool freeRoomKnownToExists = false;
    while (nbrRoomGenerated < targetNbrRoom)
    {
      if (!freeRoomKnownToExists)
      {
        if (!CanAllocateRoomOfSize(levelParams.MinRoomSize.x, levelParams.MinRoomSize.y))
          break;
        else
          freeRoomKnownToExists = true;
      }

      int sizeX = Random.Range(levelParams.MinRoomSize.x, levelParams.MaxRoomSize.x + 1);
      int sizeY = Random.Range(levelParams.MinRoomSize.y, levelParams.MaxRoomSize.y + 1);
      int posBottomLeftX = Random.Range(0, levelParams.levelSize - (sizeX - 1));
      int posBottomLeftY = Random.Range(0, levelParams.levelSize - (sizeY - 1));

      if (!IsRoomValid(sizeX, sizeY, posBottomLeftX, posBottomLeftY))
        continue;

      for (int x = 0; x < sizeX; x++)
      {
        for (int y = 0; y < sizeY; y++)
        {
          if (x + posBottomLeftX >= levelParams.levelSize || y + posBottomLeftY >= levelParams.levelSize)
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
    int sizeStartX = Random.Range(levelParams.MinStartRoomSize.x, levelParams.MaxStartRoomSize.x + 1);
    int sizeStartY = Random.Range(levelParams.MinStartRoomSize.y, levelParams.MaxStartRoomSize.y + 1);
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

    int sizeEndX = Random.Range(levelParams.MinEndRoomSize.x, levelParams.MaxEndRoomSize.x + 1);
    int sizeEndY = Random.Range(levelParams.MinEndRoomSize.y, levelParams.MaxEndRoomSize.y + 1);
    int posXEnd = levelParams.levelSize - sizeEndX;
    int posYEnd = levelParams.levelSize - sizeEndY;
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

      if (posBottomLeftX + sizeX < levelParams.levelSize)
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

      if (posBottomLeftY + sizeY < levelParams.levelSize)
      {
        if (levelTiles[columnPos][posBottomLeftY + sizeY].tileType != TileType.ExitRoomWall)
          levelTiles[columnPos][posBottomLeftY + sizeY].tileType = wallType;
      }
    }
  }

  private bool CanAllocateRoomOfSize(int sizeX, int sizeY)
  {
    var listFreeRoomPos = new List<Vector2Int>();
    for (int x = 0; x < levelParams.levelSize - (sizeX - 1); x++)
    {
      for (int y = 0; y < levelParams.levelSize - (sizeY - 1); y++)
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
    if (posBottomLeftX >= levelParams.levelSize - (sizeX - 1))
      return false;
    if (posBottomLeftY >= levelParams.levelSize - (sizeY - 1))
      return false;

    for (int x = 0; x < sizeX; x++)
    {
      for (int y = 0; y < sizeY; y++)
      {
        if (x + posBottomLeftX >= levelParams.levelSize || y + posBottomLeftY >= levelParams.levelSize)
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
    for (int i = 0; i < levelParams.levelSize; i++)
    {
      for (int j = 0; j < levelParams.levelSize; j++)
      {
        Vector3 pos = new Vector3(i * levelParams.tileSize, 0, j * levelParams.tileSize);
        TileType tile = levelTiles[i][j].tileType;
        GameObject prefabTile = null;
        bool randomRotation = false;
        switch (tile)
        {
          case TileType.Floor:
            prefabTile = floorPrefab;
            randomRotation = true;
            break;
          case TileType.Wall:
            prefabTile = wallPrefab;
            tilesInfo[i][j].state = TileState.Blocked;
            randomRotation = true;
            break;
          case TileType.ExitRoomWall:
            prefabTile = exitWallPrefab;
            tilesInfo[i][j].state = TileState.ExitBlocked;
            break;
          case TileType.Room:
            prefabTile = roomPrefab;
            randomRotation = true;
            break;
          case TileType.Corridor:
            prefabTile = corridorPrefab;
            randomRotation = true;
            break;
        }
        GameObject objTile = Instantiate(prefabTile, pos, Quaternion.identity, this.transform);
        if (randomRotation)
        {
          Vector3 center = objTile.transform.position + new Vector3(halfTileSize, 0, halfTileSize);
          objTile.transform.RotateAround(center, Vector3.up, tileRotations[Random.Range(0, tileRotations.Length)]);
        }
        objTile.transform.localScale = tileScaleVec;
        objTile.name = i + ", " + j;

        if (tile == TileType.ExitRoomWall)
          tilesInfo[i][j].obj = objTile;

        TileObj obj = levelTiles[i][j].tileObj;
        GameObject prefabObj = null;
        switch (obj)
        {
          case TileObj.Enemy:
            if (Random.Range(0f, 1f - Mathf.Epsilon) < levelParams.likelyhoodTurret)
              prefabObj = turretPrefab;
            else
              prefabObj = enemyPrefab;
            break;
          case TileObj.ExitUnlocker:
            prefabObj = collectiblePrefab;
            tilesInfo[i][j].state = TileState.ExitUnlockerBlocked;
            break;
          case TileObj.HealthDrop:
            prefabObj = healthDropPrefab;
            break;
          case TileObj.Nothing:
            break;
        }

        if (prefabObj != null)
        {
          Vector3 posCenter = new Vector3(i * levelParams.tileSize + halfTileSize, 0, j * levelParams.tileSize + halfTileSize);
          GameObject go = Instantiate(prefabObj, posCenter, Quaternion.identity, this.transform);
          go.transform.localScale = tileScaleVec;
          tilesInfo[i][j].obj = go;
        }
      }
    }

    for (int i = 0; i < 5; i++)
    {
      float YEnd = levelParams.levelSize + i;
      float YStart = 0 - (i + 1);

      Vector3 dataLineEndPos = new Vector3((levelParams.levelSize - 1) * levelParams.tileSize, 0, YEnd * levelParams.tileSize);
      GameObject dataLineEndObj = Instantiate(dataLinePrefab, dataLineEndPos, Quaternion.identity, this.transform);
      Vector3 centerDataLineEndPos = dataLineEndObj.transform.position + new Vector3(halfTileSize, 0, halfTileSize);
      dataLineEndObj.transform.RotateAround(centerDataLineEndPos, Vector3.up, 90f);
      dataLineEndObj.transform.localScale = tileScaleVec;

      Vector3 dataLinePos = new Vector3(0, 0, YStart * levelParams.tileSize);
      GameObject dataLineObj = Instantiate(dataLinePrefab, dataLinePos, Quaternion.identity, this.transform);
      Vector3 centerDataLinePos = dataLineObj.transform.position + new Vector3(halfTileSize, 0, halfTileSize);
      dataLineObj.transform.RotateAround(centerDataLinePos, Vector3.up, 90f);
      dataLineObj.transform.localScale = tileScaleVec;
    }
  }

  private void InitialiseGenerator()
  {
    tileScaleVec = new Vector3((float)levelParams.tileSize, (float)levelParams.tileSize, (float)levelParams.tileSize);
    levelTiles = new Tile[levelParams.levelSize][];
    tilesInfo = new TileInfo[levelParams.levelSize][];
    rooms.Clear();
    for (int i = 0; i < levelParams.levelSize; i++)
    {
      levelTiles[i] = new Tile[levelParams.levelSize];
      tilesInfo[i] = new TileInfo[levelParams.levelSize];
    }

    for (int j = 0; j < levelParams.levelSize; j++)
    {
      for (int k = 0; k < levelParams.levelSize; k++)
      {
        levelTiles[j][k].tileType = TileType.Floor;
        levelTiles[j][k].tileObj = TileObj.Nothing;
        tilesInfo[j][k].state = TileState.Free;
        tilesInfo[j][k].obj = null;
      }
    }
  }
}
