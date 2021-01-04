using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  [SerializeField] [Min(0.01f)] private float movementSpeed = 1f;
  [SerializeField] private float movementDelayInSeconds = 0.25f;
  private WaitForSeconds movementDelay;

  [SerializeField] [Min(0.01f)] private float turningSpeed = 1f;
  [SerializeField] private float turningDelayInSeconds = 0.25f;
  private WaitForSeconds turningDelay;

  private Vector2Int currentTile;
  public Vector2Int Tile { get => currentTile; }

  private LevelGenerator lvlGenerator;
  private bool isMoving = false;
  private bool isTurning = false;

  private void Start()
  {
    lvlGenerator = FindObjectOfType<LevelGenerator>();
    currentTile = lvlGenerator.Vec3CenterToTile(transform.position);
    movementDelay = new WaitForSeconds(movementDelayInSeconds);
    turningDelay = new WaitForSeconds(turningDelayInSeconds);
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
    yield return movementDelay;
    isMoving = false;
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
    if (isMoving || isTurning)
      return;

    if (Input.GetKey(KeyCode.UpArrow))
    {
      Vector3 destVector = transform.position + transform.forward * lvlGenerator.TileSize;
      Vector2Int destTile = lvlGenerator.Vec3CenterToTile(destVector);
      if (!lvlGenerator.IsTileReserved(destTile))
        StartCoroutine(MoveToDestinationTile(destTile));
    }
    else if (Input.GetKey(KeyCode.DownArrow))
    {
      Vector3 destVector = transform.position - transform.forward * lvlGenerator.TileSize;
      Vector2Int destTile = lvlGenerator.Vec3CenterToTile(destVector);
      if (!lvlGenerator.IsTileReserved(destTile))
        StartCoroutine(MoveToDestinationTile(destTile));
    }
    else if (Input.GetKey(KeyCode.X))
    {
      if (Input.GetKey(KeyCode.RightArrow))
      {
        Vector3 destVector = transform.position + transform.right * lvlGenerator.TileSize;
        Vector2Int destTile = lvlGenerator.Vec3CenterToTile(destVector);
        if (!lvlGenerator.IsTileReserved(destTile))
          StartCoroutine(MoveToDestinationTile(destTile));
      }
      else if (Input.GetKey(KeyCode.LeftArrow))
      {
        Vector3 destVector = transform.position + -transform.right * lvlGenerator.TileSize;
        Vector2Int destTile = lvlGenerator.Vec3CenterToTile(destVector);
        if (!lvlGenerator.IsTileReserved(destTile))
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
