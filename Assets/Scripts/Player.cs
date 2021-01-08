using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
  [SerializeField] [Min(0.01f)] private float movementSpeed = 1f;
  [SerializeField] private float movementDelayInSeconds = 0.25f;
  [SerializeField] [Min(0.01f)] private float turningSpeed = 1f;
  [SerializeField] private float turningDelayInSeconds = 0.25f;
  [SerializeField] private int nbrIFrames = 60;
  [SerializeField] private float attackCooldownInSeconds = 1f;
  [SerializeField] private int hp = 5;

  private WaitForSeconds turningDelay;
  private WaitForSeconds movementDelay;


  private Vector2Int currentTile;
  public Vector2Int Tile { get => currentTile; }

  private LevelGenerator lvlGenerator;
  private Animator animator;
  private bool isMoving = false;
  private bool isTurning = false;
  private bool isTakingDamage = false;
  private bool isAttacking = false;

  private void Start()
  {
    animator = GetComponent<Animator>();
    lvlGenerator = FindObjectOfType<LevelGenerator>();
    movementDelay = new WaitForSeconds(movementDelayInSeconds);
    turningDelay = new WaitForSeconds(turningDelayInSeconds);
    ResetTile();
  }

  public void ResetTile()
  {
    currentTile = lvlGenerator.Vec3CenterToTile(transform.position);
    lvlGenerator.ReserveTileAsPlayer(currentTile, gameObject);
  }

  private void Die()
  {
    Destroy(gameObject);
  }

  private IEnumerator ReceiveHit(int dmg)
  {
    isTakingDamage = true;

    animator.SetTrigger("TakeDamage");
    hp -= dmg;
    if (hp <= 0)
    {
      Die();
    }
    else
    {
      for (int i = 0; i < nbrIFrames; i++)
        yield return null;

      isTakingDamage = false;
    }
    yield break;
  }

  public void GotAttacked(int dmg)
  {
    if (!isTakingDamage)
      StartCoroutine(ReceiveHit(dmg));
  }

  private IEnumerator MoveToDestinationTile(Vector2Int destinationTile)
  {
    isMoving = true;
    bool toExit = lvlGenerator.IsTileExitTile(destinationTile);
    if (!toExit)
      lvlGenerator.ReserveTileAsPlayer(destinationTile, gameObject);

    Vector3 destVec = lvlGenerator.TileToVec3Center(destinationTile);
    while (transform.position != destVec)
    {
      transform.position = Vector3.MoveTowards(transform.position, destVec, Time.deltaTime * movementSpeed);
      yield return null;
    }

    if (!toExit)
      lvlGenerator.FreeTile(currentTile);
    currentTile = destinationTile;
    yield return movementDelay;
    if (toExit)
    {
      lvlGenerator.ExitReached();
    }
    isMoving = false;
    yield break;
  }

  private IEnumerator Attack(Vector2Int destinationTile)
  {
    isAttacking = true;
    GameObject objEnemy = lvlGenerator.GetTileInfo(destinationTile).obj;
    objEnemy.SendMessage("GotAttacked", 1);
    yield return attackCooldownInSeconds;
    isAttacking = false;
    yield break;
  }

  private IEnumerator TurnToAngleAroundY(float angle)
  {
    isTurning = true;
    Vector3 angles = transform.rotation.eulerAngles;
    Quaternion destAngle = Quaternion.Euler(angles.x, angles.y + angle, angles.z);
    while (transform.rotation.eulerAngles != destAngle.eulerAngles)
    {
      transform.rotation = Quaternion.RotateTowards(transform.rotation, destAngle, Time.deltaTime * turningSpeed);
      yield return null;
    }
    yield return turningDelay;
    isTurning = false;
    yield break;
  }

  private void Update()
  {
    if (isMoving || isTurning || isAttacking)
      return;

    if (Input.GetKey(KeyCode.Z))
    {
      Vector3 destVector = transform.position + transform.forward * lvlGenerator.TileSize;
      Vector2Int destTile = lvlGenerator.Vec3CenterToTile(destVector);
      if (lvlGenerator.GetTileInfo(destTile).state == LevelGenerator.TileState.Enemy)
        StartCoroutine(Attack(destTile));
    }
    else if (Input.GetKey(KeyCode.UpArrow))
    {
      Vector3 destVector = transform.position + transform.forward * lvlGenerator.TileSize;
      Vector2Int destTile = lvlGenerator.Vec3CenterToTile(destVector);
      if (lvlGenerator.GetTileInfo(destTile).state == LevelGenerator.TileState.Free ||
          lvlGenerator.IsTileExitTile(destTile))
        StartCoroutine(MoveToDestinationTile(destTile));
    }
    else if (Input.GetKey(KeyCode.DownArrow))
    {
      Vector3 destVector = transform.position - transform.forward * lvlGenerator.TileSize;
      Vector2Int destTile = lvlGenerator.Vec3CenterToTile(destVector);
      if (lvlGenerator.GetTileInfo(destTile).state == LevelGenerator.TileState.Free)
        StartCoroutine(MoveToDestinationTile(destTile));
    }
    else if (Input.GetKey(KeyCode.X))
    {
      if (Input.GetKey(KeyCode.RightArrow))
      {
        Vector3 destVector = transform.position + transform.right * lvlGenerator.TileSize;
        Vector2Int destTile = lvlGenerator.Vec3CenterToTile(destVector);
        if (lvlGenerator.GetTileInfo(destTile).state == LevelGenerator.TileState.Free)
          StartCoroutine(MoveToDestinationTile(destTile));
      }
      else if (Input.GetKey(KeyCode.LeftArrow))
      {
        Vector3 destVector = transform.position + -transform.right * lvlGenerator.TileSize;
        Vector2Int destTile = lvlGenerator.Vec3CenterToTile(destVector);
        if (lvlGenerator.GetTileInfo(destTile).state == LevelGenerator.TileState.Free)
          StartCoroutine(MoveToDestinationTile(destTile));
      }
    }
    else
    {
      if (Input.GetKey(KeyCode.RightArrow))
        StartCoroutine(TurnToAngleAroundY(90f));
      else if (Input.GetKey(KeyCode.LeftArrow))
        StartCoroutine(TurnToAngleAroundY(-90f));
    }
  }
}
