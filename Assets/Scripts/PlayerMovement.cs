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
    {
      RaycastHit hit;
      if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, (float)lvlGenerator.TileSize))
        print(hit.collider.gameObject.name);
      else
        transform.position += transform.forward * lvlGenerator.TileSize;
    }
    else if (Input.GetKeyDown(KeyCode.DownArrow))
    {
      RaycastHit hit;
      if (Physics.Raycast(transform.position + Vector3.up, -transform.forward, out hit, (float)lvlGenerator.TileSize))
        print(hit.collider.gameObject.name);
      else
        transform.position -= transform.forward * lvlGenerator.TileSize;
    }
    else if (Input.GetKeyDown(KeyCode.RightArrow))
    {
      transform.Rotate(new Vector3(0, 90, 0));
    }
    else if (Input.GetKeyDown(KeyCode.LeftArrow))
    {
      transform.Rotate(new Vector3(0, -90, 0));
    }
  }
}
