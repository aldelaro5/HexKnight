using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  [SerializeField] private LevelGeneratorParams[] levels;
  [SerializeField] private GameObject levelPrefab;
  [SerializeField] private Camera mainCamera;
  [SerializeField] private Canvas mainMenu;
  [SerializeField] private UIManager uiManager;
  [SerializeField] private UIPage pausePage;
  [SerializeField] private UIPage mainMenuPage;

  public Inputs Inputs { get => inputs; }
  private Inputs inputs;

  private LevelGenerator generator;
  private int currentLevelIndex = 0;
  private AudioListener mainCameraAudioListener;

  private void Awake()
  {
    inputs = new Inputs();
    inputs.Player.Disable();
    inputs.UI.Enable();
    if (mainCamera != null)
    {
      mainCameraAudioListener = mainCamera.GetComponent<AudioListener>();
    }
  }

  public void OnStartGame()
  {
    currentLevelIndex = 0;
    GameObject go = Instantiate(levelPrefab, Vector3.zero, Quaternion.identity, transform);
    go.name = "Level";
    generator = go.GetComponent<LevelGenerator>();

    generator.GenerateLevel(levels[0]);

    mainCameraAudioListener.enabled = false;
    mainCamera.enabled = false;
    uiManager.ChangePage(null);
    inputs.Player.Enable();
    inputs.UI.Disable();
  }

  public void GoToNextLevel()
  {
    foreach (Transform item in generator.gameObject.transform)
      Destroy(item.gameObject);

    currentLevelIndex++;
    if (currentLevelIndex < levels.Length)
      generator.GenerateLevel(levels[currentLevelIndex]);
  }

  public void Pause()
  {
    Time.timeScale = 0f;
    inputs.Player.Disable();
    inputs.UI.Enable();
    uiManager.ChangePage(pausePage);
  }

  public void OnUnpause()
  {
    uiManager.ChangePage(null);
    inputs.Player.Enable();
    inputs.UI.Disable();
    Time.timeScale = 1f;
  }

  public void OnReturnToMainMenu()
  {
    Destroy(generator.gameObject);
    Player player = FindObjectOfType<Player>();
    player.UnhookInputEvents();
    Destroy(player.gameObject);
    currentLevelIndex = 0;
    mainCameraAudioListener.enabled = true;
    mainCamera.enabled = true;
    uiManager.ChangePage(mainMenuPage);
    Time.timeScale = 1f;
  }

  public void OnExitGame()
  {
    print("Quitting...");
    Application.Quit();
  }
}
