using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  [SerializeField] private LevelGeneratorParams[] levels;
  [SerializeField] private GameObject levelPrefab;
  [SerializeField] private Camera mainCamera;
  [SerializeField] private Canvas mainMenu;
  
  private LevelGenerator generator;
  private int currentLevelIndex = 0;

  public void OnStartGame()
  {
    currentLevelIndex = 0;
    GameObject go = Instantiate(levelPrefab, Vector3.zero, Quaternion.identity, transform);
    go.name = "Level";
    generator = go.GetComponent<LevelGenerator>();

    generator.GenerateLevel(levels[0]);
    mainCamera.enabled = false;
    mainMenu.enabled = false;
  }

  public void GoToNextLevel()
  {
    foreach (Transform item in generator.gameObject.transform)
      Destroy(item.gameObject);

    currentLevelIndex++;
    if (currentLevelIndex < levels.Length)
      generator.GenerateLevel(levels[currentLevelIndex]);
  }

  public void OnExitGame()
  {
    print("Quitting...");
    Application.Quit();
  }
}
