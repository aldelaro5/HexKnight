using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  private LevelGenerator lvlGenerator;

  [SerializeField]
  [Min(0.01f)]
  private float movementSpeed = 1f;

  [SerializeField]
  [Min(0.01f)]
  private float turningSpeed = 1f;

  private bool isMoving = false;
  private bool isTurning = false;

  private void Start()
  {
    lvlGenerator = FindObjectOfType<LevelGenerator>();
  }

  private IEnumerator MoveToDestination(Vector3 destination)
  {
    isMoving = true;
    while (transform.position != destination)
    {
      transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * movementSpeed);
      yield return null;
    }
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
    isTurning = false;
    yield break;
  }

  private void Update()
  {
    if (isMoving || isTurning)
      return;

    if (Input.GetKeyDown(KeyCode.UpArrow))
    {
      RaycastHit hit;
      if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, (float)lvlGenerator.TileSize))
        print(hit.collider.gameObject.name);
      else
        StartCoroutine(MoveToDestination(transform.position + transform.forward * lvlGenerator.TileSize));
    }
    else if (Input.GetKeyDown(KeyCode.DownArrow))
    {
      RaycastHit hit;
      if (Physics.Raycast(transform.position + Vector3.up, -transform.forward, out hit, (float)lvlGenerator.TileSize))
        print(hit.collider.gameObject.name);
      else
        StartCoroutine(MoveToDestination(transform.position - transform.forward * lvlGenerator.TileSize));
    }
    else if (Input.GetKeyDown(KeyCode.RightArrow))
    {
      StartCoroutine(TurnToAngleAroundY(90f));
    }
    else if (Input.GetKeyDown(KeyCode.LeftArrow))
    {
      StartCoroutine(TurnToAngleAroundY(-90f));
    }
  }
}
