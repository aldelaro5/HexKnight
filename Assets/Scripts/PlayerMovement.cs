using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  private LevelGenerator lvlGenerator;

  [SerializeField] [Min(0.01f)] private float movementSpeed = 1f;
  [SerializeField] private float movementDelayInSeconds = 0.25f;
  private WaitForSeconds movementDelay;

  [SerializeField] [Min(0.01f)] private float turningSpeed = 1f;
  [SerializeField] private float turningDelayInSeconds = 0.25f;
  private WaitForSeconds turningDelay;

  private bool isMoving = false;
  private bool isTurning = false;

  private void Start()
  {
    lvlGenerator = FindObjectOfType<LevelGenerator>();
    movementDelay = new WaitForSeconds(movementDelayInSeconds);
    turningDelay = new WaitForSeconds(turningDelayInSeconds);
  }

  private IEnumerator MoveToDestination(Vector3 destination)
  {
    isMoving = true;
    while (transform.position != destination)
    {
      transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * movementSpeed);
      yield return null;
    }
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
      RaycastHit hit;
      if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, (float)lvlGenerator.TileSize))
        print(hit.collider.gameObject.name);
      else
        StartCoroutine(MoveToDestination(transform.position + transform.forward * lvlGenerator.TileSize));
    }
    else if (Input.GetKey(KeyCode.DownArrow))
    {
      RaycastHit hit;
      if (Physics.Raycast(transform.position + Vector3.up, -transform.forward, out hit, (float)lvlGenerator.TileSize))
        print(hit.collider.gameObject.name);
      else
        StartCoroutine(MoveToDestination(transform.position - transform.forward * lvlGenerator.TileSize));
    }
    else if (Input.GetKey(KeyCode.X))
    {
      if (Input.GetKey(KeyCode.RightArrow))
      {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, transform.right, out hit, (float)lvlGenerator.TileSize))
          print(hit.collider.gameObject.name);
        else
          StartCoroutine(MoveToDestination(transform.position + transform.right * lvlGenerator.TileSize));
      }
      else if (Input.GetKey(KeyCode.LeftArrow))
      {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, -transform.right, out hit, (float)lvlGenerator.TileSize))
          print(hit.collider.gameObject.name);
        else
          StartCoroutine(MoveToDestination(transform.position + -transform.right * lvlGenerator.TileSize));
      }
    }
    else
    {
      if (Input.GetKey(KeyCode.RightArrow))
      {
        StartCoroutine(TurnToAngleAroundY(90f));
      }
      else if (Input.GetKey(KeyCode.LeftArrow))
      {
        StartCoroutine(TurnToAngleAroundY(-90f));
      }
    }
  }
}
