using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScorePage : MonoBehaviour
{
  [SerializeField] private CanvasRenderer healthLeftPanel;
  [SerializeField] private CanvasRenderer EnemiesKilledPanel;
  [SerializeField] private CanvasRenderer timeLeftPanel;
  [SerializeField] private CanvasRenderer scorePanel;
  [SerializeField] private CanvasRenderer levelsPanel;
  [SerializeField] private CanvasRenderer promptPanel;
  [SerializeField] private TMP_Text textHealthLeft;
  [SerializeField] private TMP_Text textEnemiesKilled;
  [SerializeField] private TMP_Text textTimeLeft;
  [SerializeField] private TMP_Text textScore;
  [SerializeField] private TMP_Text textLevels;
  [SerializeField] private TMP_Text textMode;
  
  public bool isDone { get; private set; } = false;

  private GameManager gameManager;

  private void Awake()
  {
    gameManager = FindObjectOfType<GameManager>();
  }

  public void ResetPage()
  {
    healthLeftPanel.gameObject.SetActive(false);
    EnemiesKilledPanel.gameObject.SetActive(false);
    timeLeftPanel.gameObject.SetActive(false);
    levelsPanel.gameObject.SetActive(false);
    scorePanel.gameObject.SetActive(false);
    promptPanel.gameObject.SetActive(false);
  }

  public IEnumerator ShowPage()
  {
    isDone = false;
    switch (gameManager.gameMode)
    {
      case GameManager.GameMode.Standard:
        textMode.text = "Standard";
        break;
      case GameManager.GameMode.LongPlay:
        textMode.text = "Challenge";
        break;
      case GameManager.GameMode.Endless:
        textMode.text = "Endless";
        break;
      case GameManager.GameMode.Speed:
        textMode.text = "Speed";
        break;
    }

    yield return new WaitForSeconds(0.5f);
    if (gameManager.gameMode != GameManager.GameMode.Endless && gameManager.gameMode != GameManager.GameMode.Speed)
    {
      textHealthLeft.text = gameManager.Player.Hp + " / " + gameManager.Player.MaxHp;
      healthLeftPanel.gameObject.SetActive(true);
      yield return new WaitForSeconds(0.5f);
    }
    if (gameManager.gameMode != GameManager.GameMode.Speed)
    {
      textEnemiesKilled.text = gameManager.nbrEnemyKilled.ToString();
      EnemiesKilledPanel.gameObject.SetActive(true);
      yield return new WaitForSeconds(0.5f);
    }

    textLevels.text = gameManager.currentLevelIndex.ToString();
    levelsPanel.gameObject.SetActive(true);
    yield return new WaitForSeconds(0.5f);

    if (gameManager.gameMode != GameManager.GameMode.Endless && gameManager.gameMode != GameManager.GameMode.Speed)
    {
      textTimeLeft.text = gameManager.strTimeLeft;
      timeLeftPanel.gameObject.SetActive(true);
      yield return new WaitForSeconds(0.5f);
    }
    if (gameManager.gameMode != GameManager.GameMode.Speed)
    {
      textScore.text = gameManager.Score.ToString().PadLeft(6, '0');
      scorePanel.gameObject.SetActive(true);
    }
    promptPanel.gameObject.SetActive(true);
    isDone = true;
    yield break;
  }
}
