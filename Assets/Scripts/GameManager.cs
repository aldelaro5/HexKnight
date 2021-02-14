using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class GameManager : MonoBehaviour
{
  public enum GameMode
  {
    Standard,
    LongPlay,
    Endless,
    Survival
  }

  [SerializeField] private LevelGeneratorParams[] standardModeLevels;
  [SerializeField] private GameObject levelPrefab;
  [SerializeField] private Camera mainCamera;
  [SerializeField] private Canvas mainMenu;
  [SerializeField] private UIManager uiManager;
  [SerializeField] private UIPage pausePage;
  [SerializeField] private UIPage mainMenuPage;
  [SerializeField] private UIPage endGamePage;
  [SerializeField] private UIPage gameOverPage;
  [SerializeField] private HUD hud;
  [SerializeField] private float timeLimitSeconds = 300;
  [SerializeField] private int pointsOnHit = 50;
  [SerializeField] private int pointsOnKill = 100;
  [SerializeField] private int pointsOnExitUnlock = 500;
  [SerializeField] private int pointsPerHPLeft = 300;
  [SerializeField] private int pointsPerSecondsLeft = 20;

  public Inputs Inputs { get; private set; }

  private LevelGenerator generator;
  public Settings Settings { get; private set; }
  public Player Player { get; private set; }
  public int Score { get; private set; }
  public float TimeLeft { get; private set; }
  public GameMode gameMode { get; private set; }
  public int currentLevelIndex { get; private set; } = 0;
  public string strTimeLeft
  {
    get => (int)Mathf.Floor(TimeLeft / 60) + ":" +
           (Mathf.Floor(TimeLeft) % 60).ToString().PadLeft(2, '0');
  }
  public int nbrEnemyKilled { get; private set; }

  private bool inGame = false;
  private AudioListener mainCameraAudioListener;

  private const string settingsFilename = "settings.json";

  private void Awake()
  {
    Inputs = new Inputs();
    Settings = new Settings();
    if (File.Exists(settingsFilename))
      LoadSettings();

    ApplyBindingsOverrides();
    Inputs.Player.Disable();
    Inputs.UI.Enable();
    if (mainCamera != null)
      mainCameraAudioListener = mainCamera.GetComponent<AudioListener>();
  }

  private void ApplyBindingsOverrides()
  {
    foreach (var item in Settings.bindingOverridesKb)
    {
      var binding = Inputs.SelectMany(x => x.bindings).FirstOrDefault(x => x.id.ToString() == item.id);
      if (binding != null)
      {
        var action = Inputs.FirstOrDefault(x => x.bindings.Count(x => x.id.ToString() == item.id) > 0);
        int index = action.bindings.IndexOf(x => x.id.ToString() == item.id);
        action.ApplyBindingOverride(index, item.overridePath);
      }
    }

    foreach (var item in Settings.bindingOverridesGamepad)
    {
      var binding = Inputs.SelectMany(x => x.bindings).FirstOrDefault(x => x.id.ToString() == item.id);
      if (binding != null)
      {
        var action = Inputs.FirstOrDefault(x => x.bindings.Count(x => x.id.ToString() == item.id) > 0);
        int index = action.bindings.IndexOf(x => x.id.ToString() == item.id);
        action.ApplyBindingOverride(index, item.overridePath);
      }
    }
  }

  private void LoadSettings()
  {
    string json = File.ReadAllText(settingsFilename);
    Settings = JsonUtility.FromJson<Settings>(json);
  }

  public void WriteSettings()
  {
    string json = JsonUtility.ToJson(Settings, true);
    File.WriteAllText(settingsFilename, json);
  }

  public void OnStartGame()
  {
    currentLevelIndex = 0;
    Score = 0;
    nbrEnemyKilled = 0;
    TimeLeft = timeLimitSeconds;
    GameObject go = Instantiate(levelPrefab, Vector3.zero, Quaternion.identity, transform);
    go.name = "Level";
    generator = go.GetComponent<LevelGenerator>();

    generator.GenerateLevel(standardModeLevels[0]);

    mainCameraAudioListener.enabled = false;
    mainCamera.enabled = false;
    hud.gameObject.SetActive(true);
    uiManager.ChangePage(null);
    Inputs.Player.Enable();
    Inputs.UI.Disable();
    Player = generator.Player.GetComponent<Player>();
    hud.UpdateDisplay();
    inGame = true;
  }

  private void Update()
  {
    if (inGame && mainCamera != null)
    {
      TimeLeft -= Time.deltaTime;
      if (TimeLeft <= 0)
      {
        TimeLeft = 0;
        StartCoroutine(GameOver());
      }
      hud.UpdateDisplay();
    }
  }

  public void HitEnemy()
  {
    AddScore(pointsOnHit);
  }

  public void KilledEnemy()
  {
    nbrEnemyKilled++;
    AddScore(pointsOnKill);
  }

  public void ExitUnlocked()
  {
    AddScore(pointsOnExitUnlock);
  }

  private void AddScore(int pointsToAdd)
  {
    if (hud != null)
    {
      Score += pointsToAdd;
      hud.UpdateDisplay();
    }
  }

  public void UpdateHUD()
  {
    if (hud != null)
      hud.UpdateDisplay();
  }

  public void GoToNextLevel()
  {
    foreach (Transform item in generator.gameObject.transform)
      Destroy(item.gameObject);

    currentLevelIndex++;
    if (currentLevelIndex < standardModeLevels.Length)
      generator.GenerateLevel(standardModeLevels[currentLevelIndex]);
    else
      StartCoroutine(EndGame());
  }

  private IEnumerator EndGame()
  {
    inGame = false;
    Inputs.Player.Disable();
    hud.gameObject.SetActive(false);
    StartCoroutine(uiManager.FadeOut(false));
    while (uiManager.Fading)
      yield return null;

    Destroy(generator.gameObject);
    ScorePage scorePage = endGamePage.GetComponent<ScorePage>();
    CalculateFinalScore();
    scorePage.ResetPage();
    uiManager.ChangePage(endGamePage);
    Player.MainCamera.gameObject.SetActive(false);
    mainCameraAudioListener.enabled = true;
    mainCamera.enabled = true;
    StartCoroutine(uiManager.FadeIn(true));

    StartCoroutine(scorePage.ShowPage());
    while (!scorePage.isDone)
      yield return null;

    Inputs.UI.Enable();
    while (Inputs.UI.Submit.ReadValue<float>() == 0)
      yield return null;

    StartCoroutine(uiManager.FadeOut(false));
    while (uiManager.Fading)
      yield return null;

    OnReturnToMainMenu();
    StartCoroutine(uiManager.FadeIn(true));
    yield break;
  }

  private void CalculateFinalScore()
  {
    Score += pointsPerHPLeft * Player.Hp;
    Score += pointsPerSecondsLeft * (int)Mathf.Floor(TimeLeft);
  }

  public IEnumerator GameOver()
  {
    inGame = false;
    Inputs.Player.Disable();
    hud.gameObject.SetActive(false);
    StartCoroutine(uiManager.FadeOut(false));
    while (uiManager.Fading)
      yield return null;
      
    Destroy(generator.gameObject);
    uiManager.ChangePage(gameOverPage);
    Player.MainCamera.gameObject.SetActive(false);
    mainCameraAudioListener.enabled = true;
    mainCamera.enabled = true;
    StartCoroutine(uiManager.FadeIn(true));
    yield return new WaitForSeconds(2);
    StartCoroutine(uiManager.FadeOut(false));
    while (uiManager.Fading)
      yield return null;
    OnReturnToMainMenu();
    StartCoroutine(uiManager.FadeIn(true));
    yield break;
  }

  public void Pause()
  {
    Time.timeScale = 0f;
    Inputs.Player.Disable();
    Inputs.UI.Enable();
    uiManager.ChangePage(pausePage);
  }

  public void OnUnpause()
  {
    uiManager.ChangePage(null);
    Inputs.Player.Enable();
    Inputs.UI.Disable();
    Time.timeScale = 1f;
  }

  public void OnReturnToMainMenu()
  {
    inGame = false;
    Inputs.Player.Disable();
    if (generator != null)
      Destroy(generator.gameObject);
    Player.UnhookInputEvents();
    Destroy(Player.gameObject);
    currentLevelIndex = 0;
    mainCameraAudioListener.enabled = true;
    mainCamera.enabled = true;
    hud.gameObject.SetActive(false);
    uiManager.ChangePage(mainMenuPage);
    Inputs.Player.Disable();
    Inputs.UI.Enable();
    Time.timeScale = 1f;
  }

  public void OnExitGame()
  {
    print("Quitting...");
    Application.Quit();
  }
}
