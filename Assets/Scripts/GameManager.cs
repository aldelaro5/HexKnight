using System;
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
  private Player player;
  public Player Player { get => player; }
  private int currentLevelIndex = 0;
  private int score = 0;
  public int Score { get => score; }
  private AudioListener mainCameraAudioListener;
  private HUD hud;

  private void Awake()
  {
    inputs = new Inputs();
    inputs.Player.Disable();
    inputs.UI.Enable();
    if (mainCamera != null)
      mainCameraAudioListener = mainCamera.GetComponent<AudioListener>();
    if (mainMenu != null)
      hud = mainMenu.transform.Find("HUD").GetComponent<HUD>();
  }

  public void OnStartGame()
  {
    currentLevelIndex = 0;
    score = 0;
    GameObject go = Instantiate(levelPrefab, Vector3.zero, Quaternion.identity, transform);
    go.name = "Level";
    generator = go.GetComponent<LevelGenerator>();

    generator.GenerateLevel(levels[0]);

    mainCameraAudioListener.enabled = false;
    mainCamera.enabled = false;
    hud.gameObject.SetActive(true);
    uiManager.ChangePage(null);
    inputs.Player.Enable();
    inputs.UI.Disable();
    player = generator.Player.GetComponent<Player>();
    hud.UpdateDisplay();
  }

  public void AddScore(int pointsToAdd)
  {
    score += pointsToAdd;
    hud.UpdateDisplay();
  }

  public void UpdateHUD()
  {
    hud.UpdateDisplay();
  }

  public void GoToNextLevel()
  {
    foreach (Transform item in generator.gameObject.transform)
      Destroy(item.gameObject);

    currentLevelIndex++;
    if (currentLevelIndex < levels.Length)
      generator.GenerateLevel(levels[currentLevelIndex]);
    else
      StartCoroutine(EndGame());
  }

  private IEnumerator EndGame()
  {
    hud.gameObject.SetActive(false);
    StartCoroutine(uiManager.FadeOut(false));
    while (uiManager.Fading)
      yield return null;
    StartCoroutine(uiManager.FadeIn(true));
    OnReturnToMainMenu();
    yield break;
  }

  public IEnumerator GameOver()
  {
    hud.gameObject.SetActive(false);
    StartCoroutine(uiManager.FadeOut(false));
    while (uiManager.Fading)
      yield return null;
    StartCoroutine(uiManager.FadeIn(true));
    OnReturnToMainMenu();
    yield break;
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
    player.UnhookInputEvents();
    Destroy(player.gameObject);
    currentLevelIndex = 0;
    mainCameraAudioListener.enabled = true;
    mainCamera.enabled = true;
    hud.gameObject.SetActive(false);
    uiManager.ChangePage(mainMenuPage);
    inputs.Player.Disable();
    inputs.UI.Enable();
    Time.timeScale = 1f;
  }

  public void OnExitGame()
  {
    print("Quitting...");
    Application.Quit();
  }
}
