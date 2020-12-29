using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
  [SerializeField] private GameObject floorPrefab;
  [SerializeField] private GameObject wallPrefab;

  [SerializeField] private int levelSize = 10;
  [SerializeField] private int tileSize = 5;

  private Vector3 tileScaleVec;
  private bool testDone = false;

  // Start is called before the first frame update
  void Start()
  {
    tileScaleVec = new Vector3((float)tileSize, (float)tileSize, (float)tileSize);
  }

  // Update is called once per frame
  void Update()
  {
    if (Input.GetKeyDown(KeyCode.T) && !testDone)
    {
      for (int edgeIndex = -1; edgeIndex <= levelSize; edgeIndex++)
      {
        Vector3 posToptEdge = new Vector3(edgeIndex * tileSize, 0, levelSize * tileSize);
        GameObject tileTopEdge = Instantiate(wallPrefab, posToptEdge, Quaternion.identity, this.transform);
        tileTopEdge.transform.localScale = tileScaleVec;
        tileTopEdge.name = (int)posToptEdge.x + ", " + (int)posToptEdge.y;
        
        Vector3 posBottomEdge = new Vector3(edgeIndex * tileSize, 0, -tileSize);
        GameObject tileBottomEdge = Instantiate(wallPrefab, posBottomEdge, Quaternion.identity, this.transform);
        tileBottomEdge.transform.localScale = tileScaleVec;
        tileBottomEdge.name = (int)posBottomEdge.x + ", " + (int)posBottomEdge.y;
      }

      for (int i = 0; i < levelSize; i++)
      {
        Vector3 posLeftEdge = new Vector3(-tileSize, 0, i * tileSize);
        GameObject tileLeftEdge = Instantiate(wallPrefab, posLeftEdge, Quaternion.identity, this.transform);
        tileLeftEdge.transform.localScale = tileScaleVec;
        tileLeftEdge.name = (int)posLeftEdge.x + ", " + (int)posLeftEdge.y;
        
        Vector3 posRightEdge = new Vector3(levelSize * tileSize, 0, i * tileSize);
        GameObject tileRightEdge = Instantiate(wallPrefab, posRightEdge, Quaternion.identity, this.transform);
        tileRightEdge.transform.localScale = tileScaleVec;
        tileRightEdge.name = (int)posRightEdge.x + ", " + (int)posRightEdge.y;

        for (int j = 0; j < levelSize; j++)
        {
          Vector3 pos = new Vector3(i * tileSize, 0, j * tileSize);
          GameObject tileFloor = Instantiate(floorPrefab, pos, floorPrefab.transform.rotation, this.transform);
          tileFloor.transform.localScale = tileScaleVec;
          tileFloor.name = i + ", " + j;
        }
      }

      testDone = true;
    }
  }
}
