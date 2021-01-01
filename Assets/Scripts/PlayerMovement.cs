using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  private LevelGenerator lvlGenerator;

  private void Start()
  {
    lvlGenerator = FindObjectOfType<LevelGenerator>();
  }

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.UpArrow))
      transform.position += transform.forward * lvlGenerator.TileSize;
    else if (Input.GetKeyDown(KeyCode.DownArrow))
      transform.position -= transform.forward * lvlGenerator.TileSize;
    else if (Input.GetKeyDown(KeyCode.RightArrow))
      transform.Rotate(new Vector3(0, 90, 0));
    else if (Input.GetKeyDown(KeyCode.LeftArrow))
      transform.Rotate(new Vector3(0, -90, 0));
  }
}
