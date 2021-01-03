using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour
{
  [SerializeField] [Min(0.01f)] private float movementSpeed = 1f;
  [SerializeField] private float idleTimeInSeconds = 5f;
  private WaitForSeconds idleDelay;

  private readonly Direction[] directions = Enum.GetValues(typeof(Direction)).Cast<Direction>().ToArray();
  private LevelGenerator lvlGenerator;
  private bool isMoving = false;
  private bool isIdling = false;
  private Vector2Int currentTile;

  void Start()
  {
    lvlGenerator = FindObjectOfType<LevelGenerator>();
    idleDelay = new WaitForSeconds(idleTimeInSeconds);
    currentTile = lvlGenerator.Vec3CenterToTile(transform.position);
    lvlGenerator.ReserveTile(currentTile);
  }

  private IEnumerator MoveToDestinationTile(Vector2Int destinationTile)
  {
    isMoving = true;
    lvlGenerator.ReserveTile(destinationTile);
    Vector3 destVec = lvlGenerator.TileToVec3Center(destinationTile);
    while (transform.position != destVec)
    {
      transform.position = Vector3.MoveTowards(transform.position, destVec, Time.deltaTime * movementSpeed);
      yield return null;
    }
    lvlGenerator.FreeTile(currentTile);
    currentTile = destinationTile;
    StartCoroutine(Idle());
    isMoving = false;
    yield break;
  }

  private IEnumerator Idle()
  {
    isIdling = true;
    yield return idleDelay;
    isIdling = false;
    yield break;
  }

  void Update()
  {
    if (isMoving)
      return;

    if (isIdling)
      return;

    Direction dir = directions[Random.Range(0, directions.Length)];

    Vector2Int destTile = Vector2Int.zero;
    Vector3 vec3Direction = Vector3.zero;
    switch (dir)
    {
      case Direction.Up:
        vec3Direction = Vector3.forward;
        destTile = new Vector2Int(currentTile.x, currentTile.y + 1);
        break;
      case Direction.Right:
        vec3Direction = Vector3.right;
        destTile = new Vector2Int(currentTile.x + 1, currentTile.y);
        break;
      case Direction.Down:
        vec3Direction = Vector3.back;
        destTile = new Vector2Int(currentTile.x, currentTile.y - 1);
        break;
      case Direction.Left:
        vec3Direction = Vector3.left;
        destTile = new Vector2Int(currentTile.x - 1, currentTile.y);
        break;
    }

    transform.LookAt(transform.position + vec3Direction);

    if (!lvlGenerator.IsTileReserved(destTile))
      StartCoroutine(MoveToDestinationTile(destTile));
    else
      StartCoroutine(Idle());
  }
}
