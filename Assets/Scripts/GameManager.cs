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
    Standard = 0,
    LongPlay,
    Endless,
    Speed
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
  [SerializeField] private float timeLimitStandardSeconds = 300;
  [SerializeField] private float timeLimitSpeedSeconds = 900;
  [SerializeField] private float timeLimitLongPlaySeconds = 1800;
  [SerializeField] private int pointsOnHit = 50;
  [SerializeField] private int pointsOnKill = 100;
  [SerializeField] private int pointsOnExitUnlock = 500;
  [SerializeField] private int pointsPerHPLeft = 300;
  [SerializeField] private int pointsPerSecondsLeft = 20;
  [SerializeField] private int nbrLevelLongPlay = 50;
  [SerializeField] private MusicPlayer musicPlayer;
  [SerializeField] private LoopableAudio musicStandardMode;
  [SerializeField] private LoopableAudio musicLongPlayMode;
  [SerializeField] private LoopableAudio musicEndlessMode;
  [SerializeField] private LoopableAudio musicSpeedMode;

  public Inputs Inputs { get; private set; }

  private LevelGenerator generator;
  public Settings Settings { get; private set; }
  public MusicPlayer MusicPlayer { get => musicPlayer; }
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
  private LevelGeneratorParams[] currentLevelsParams;
  private LoopableAudio currentMusic;
  private Coroutine musicPlaying;

  private const string settingsFilename = "settings.json";

  private void Awake()
  {
    Inputs = new Inputs();
    Settings = new Settings();
    if (File.Exists(settingsFilename))
      LoadSettings();

    Screen.SetResolution(Settings.resWidth, Settings.resHeight, Settings.fullScreen);
    ApplyBindingsOverrides();
    Inputs.Player.Disable();
    Inputs.UI.Enable();
    Inputs.UI.Cancel.performed += OnUICancel;
    if (mainCamera != null)
      mainCameraAudioListener = mainCamera.GetComponent<AudioListener>();
  }

  private void OnUICancel(InputAction.CallbackContext ctx)
  {
    if (uiManager.CurrentPage.btnBack != null)
      uiManager.CurrentPage.btnBack.onClick.Invoke();
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

  public void OnStartGame(int mode)
  {
    currentLevelIndex = 0;
    Score = 0;
    nbrEnemyKilled = 0;

    GameObject go = Instantiate(levelPrefab, Vector3.zero, Quaternion.identity, transform);
    go.name = "Level";
    generator = go.GetComponent<LevelGenerator>();

    gameMode = (GameMode)mode;
    switch (gameMode)
    {
      case GameMode.Standard:
        StartStandardMode();
        break;
      case GameMode.LongPlay:
        StartLongPlay();
        break;
      case GameMode.Endless:
        StartEndless();
        break;
      case GameMode.Speed:
        StartSpeed();
        break;
    }

    mainCameraAudioListener.enabled = false;
    mainCamera.enabled = false;
    hud.gameObject.SetActive(true);
    uiManager.ChangePage(null);
    Inputs.Player.Enable();
    Inputs.UI.Disable();
    Player = generator.Player.GetComponent<Player>();
    hud.UpdateDisplay();
    musicPlaying = StartCoroutine(musicPlayer.PlayLoopableMusic(currentMusic));
    inGame = true;
  }

  private void StartSpeed()
  {
    TimeLeft = timeLimitSpeedSeconds;
    currentMusic = musicSpeedMode;

    generator.GenerateLevel(GetLevelParamsForLevelIndexInCurve(0));
  }

  private void StartEndless()
  {
    currentMusic = musicEndlessMode;

    generator.GenerateLevel(GetLevelParamsForLevelIndexInCurve(0));
  }

  private void StartLongPlay()
  {
    TimeLeft = timeLimitLongPlaySeconds;
    currentMusic = musicLongPlayMode;

    currentLevelsParams = new LevelGeneratorParams[nbrLevelLongPlay];
    for (int i = 0; i < nbrLevelLongPlay; i++)
    {
      LevelGeneratorParams levelParams = GetLevelParamsForLevelIndexInCurve(i);

      currentLevelsParams[i] = levelParams;
    }
    generator.GenerateLevel(currentLevelsParams[0]);
  }

  private LevelGeneratorParams GetLevelParamsForLevelIndexInCurve(int index)
  {
    LevelGeneratorParams levelParams = new LevelGeneratorParams();
    levelParams.tileSize = 3;
    levelParams.MinStartRoomSize = new Vector2Int(2, 2);
    levelParams.MaxStartRoomSize = new Vector2Int(2, 2);
    levelParams.MinEndRoomSize = new Vector2Int(2, 2);
    levelParams.MaxEndRoomSize = new Vector2Int(2, 2);

    levelParams.nbrEnemies = 3 + (index / 3);
    levelParams.nbrHealthDrops = 2 + (int)Mathf.Round(Mathf.Sqrt((float)index));
    levelParams.prefferedMinNbrRoom = (int)((9f / 12f) * (float)index + 3);
    levelParams.maxNbrRoom = levelParams.prefferedMinNbrRoom;
    int minRoomSize = (int)(Mathf.Round(Mathf.Sqrt((float)index / 5f)) + 2f);
    levelParams.MinRoomSize = new Vector2Int(minRoomSize, minRoomSize);
    levelParams.MaxRoomSize = new Vector2Int(minRoomSize + 1, minRoomSize + 1);
    levelParams.levelSize = 10 + index / 2;
    if (index == 0)
      levelParams.likelyhoodTurret = 0f;
    else
      levelParams.likelyhoodTurret = Mathf.Clamp((index + 1) * (0.5f / (float)nbrLevelLongPlay), 0f, 0.5f);
    return levelParams;
  }

  private void StartStandardMode()
  {
    TimeLeft = timeLimitStandardSeconds;
    currentMusic = musicStandardMode;

    currentLevelsParams = standardModeLevels;
    generator.GenerateLevel(currentLevelsParams[0]);
  }

  private void Update()
  {
    if (inGame && mainCamera != null)
    {
      if (gameMode != GameMode.Endless)
      {
        TimeLeft -= Time.deltaTime;
        if (TimeLeft <= 0)
        {
          TimeLeft = 0;
          musicPlayer.StopPlaying();
          StartCoroutine(GameOver());
        }
      }
      hud.UpdateDisplay();
    }
  }

  public void HitEnemy()
  {
    if (gameMode != GameMode.Speed)
      AddScore(pointsOnHit);
  }

  public void KilledEnemy()
  {
    if (gameMode != GameMode.Speed)
    {
      nbrEnemyKilled++;
      AddScore(pointsOnKill);
    }
  }

  public void ExitUnlocked()
  {
    if (gameMode != GameMode.Speed)
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

    if (gameMode == GameMode.Endless || gameMode == GameMode.Speed)
      generator.GenerateLevel(GetLevelParamsForLevelIndexInCurve(currentLevelIndex));
    else if (currentLevelIndex < currentLevelsParams.Length)
      generator.GenerateLevel(currentLevelsParams[currentLevelIndex]);
    else
      StartCoroutine(EndGame());
  }

  private IEnumerator EndGame()
  {
    inGame = false;
    musicPlayer.StopPlaying();
    Inputs.Player.Disable();
    hud.gameObject.SetActive(false);
    StartCoroutine(uiManager.FadeOut(false));
    while (uiManager.Fading)
      yield return null;

    Destroy(generator.gameObject);
    ScorePage scorePage = endGamePage.GetComponent<ScorePage>();
    if (gameMode != GameMode.Endless && gameMode != GameMode.Speed)
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
    if (gameMode == GameMode.Endless || gameMode == GameMode.Speed)
    {
      StartCoroutine(EndGame());
      yield break;
    }
    inGame = false;
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
    StopCoroutine(musicPlaying);
    musicPlayer.StopPlaying();
    Inputs.Player.Disable();
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
