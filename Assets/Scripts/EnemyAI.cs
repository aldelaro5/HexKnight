using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour
{
  [SerializeField] [Min(0.01f)] private float movementSpeed = 1f;
  [SerializeField] private int tilesAttackRange = 3;
  [SerializeField] private float idleTimeInSeconds = 5f;
  private WaitForSeconds idleDelay;
  [SerializeField] private float attackTimeInSeconds = 1f;
  private WaitForSeconds attackDelay;

  private readonly Direction[] directions = Enum.GetValues(typeof(Direction)).Cast<Direction>().ToArray();
  private LevelGenerator lvlGenerator;
  private Player playerMovement;
  private bool isMoving = false;
  private bool isIdling = false;
  private Coroutine idlingCoroutine;
  private Vector2Int currentTile;

  void Start()
  {
    lvlGenerator = FindObjectOfType<LevelGenerator>();
    playerMovement = FindObjectOfType<Player>();
    idleDelay = new WaitForSeconds(idleTimeInSeconds);
    attackDelay = new WaitForSeconds(attackTimeInSeconds);
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
    idlingCoroutine = StartCoroutine(Idle());
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

  private IEnumerator WaitToAttack()
  {
    isIdling = true;
    yield return attackDelay;
    isIdling = false;
    yield break;
  }

  void Update()
  {
    if (isMoving)
      return;

    Direction dir = Direction.NONE;
    if (Math.Abs(currentTile.x - playerMovement.Tile.x) +
        Math.Abs(currentTile.y - playerMovement.Tile.y) == 1)
    {
      transform.LookAt(playerMovement.transform);
      if (isIdling)
        return;
      
      AIAttack();
      return;
    }
    else if (Math.Abs(currentTile.x - playerMovement.Tile.x) +
             Math.Abs(currentTile.y - playerMovement.Tile.y) <= tilesAttackRange)
    {
      if (isIdling)
      {
        StopCoroutine(idlingCoroutine);
        isIdling = false;
      }

      dir = DetermineDirectionToApproachPlayer();
      transform.LookAt(playerMovement.transform);
      AIMove(dir, false);
    }
    else
    {
      if (isIdling)
        return;

      dir = directions[Random.Range(0, directions.Length)];
      AIMove(dir, true);
    }
  }

  private void AIAttack()
  {
    playerMovement.GotAttacked();
    idlingCoroutine = StartCoroutine(WaitToAttack());
  }

  private Direction DetermineDirectionToApproachPlayer()
  {
    if (currentTile.x < playerMovement.Tile.x)
    {
      if (!lvlGenerator.IsTileReserved(new Vector2Int(currentTile.x + 1, currentTile.y)))
        return Direction.Right;
    }
    if (currentTile.x > playerMovement.Tile.x)
    {
      if (!lvlGenerator.IsTileReserved(new Vector2Int(currentTile.x - 1, currentTile.y)))
        return Direction.Left;
    }
    if (currentTile.y < playerMovement.Tile.y)
    {
      if (!lvlGenerator.IsTileReserved(new Vector2Int(currentTile.x, currentTile.y + 1)))
        return Direction.Up;
    }
    if (currentTile.y > playerMovement.Tile.y)
    {
      if (!lvlGenerator.IsTileReserved(new Vector2Int(currentTile.x, currentTile.y - 1)))
        return Direction.Down;
    }

    return Direction.NONE;
  }

  private void AIMove(Direction dir, bool lookAt)
  {
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
      case Direction.NONE:
        vec3Direction = transform.forward;
        destTile = currentTile;
        break;
    }

    if (lookAt)
      transform.LookAt(transform.position + vec3Direction);

    if (!lvlGenerator.IsTileReserved(destTile))
      StartCoroutine(MoveToDestinationTile(destTile));
    else
      idlingCoroutine = StartCoroutine(Idle());
  }
}
