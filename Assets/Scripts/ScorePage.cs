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
  [SerializeField] private TMP_Text textHealthLeft;
  [SerializeField] private TMP_Text textEnemiesKilled;
  [SerializeField] private TMP_Text textTimeLeft;
  [SerializeField] private TMP_Text textScore;

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
    scorePanel.gameObject.SetActive(false);
  }

  public IEnumerator ShowPage()
  {
    isDone = false;
    yield return new WaitForSeconds(0.5f);
    textHealthLeft.text = gameManager.Player.Hp + " / " + gameManager.Player.MaxHp;
    healthLeftPanel.gameObject.SetActive(true);
    yield return new WaitForSeconds(0.5f);
    textEnemiesKilled.text = gameManager.nbrEnemyKilled.ToString();
    EnemiesKilledPanel.gameObject.SetActive(true);
    yield return new WaitForSeconds(0.5f);
    textTimeLeft.text = gameManager.strTimeLeft;
    timeLeftPanel.gameObject.SetActive(true);
    yield return new WaitForSeconds(0.5f);
    textScore.text = gameManager.Score.ToString().PadLeft(6, '0');
    scorePanel.gameObject.SetActive(true);
    isDone = true;
    yield break;
  }
}
