using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
  [SerializeField] private CanvasRenderer scorePanel;
  [SerializeField] private TMP_Text scoreText;
  [SerializeField] private TMP_Text hpText;
  [SerializeField] private TMP_Text maxHpText;
  [SerializeField] private TMP_Text timeLeftText;
  [SerializeField] private TMP_Text levelText;

  private GameManager gameManager;

  private void Awake()
  {
    gameManager = FindObjectOfType<GameManager>();
  }

  public void UpdateDisplay()
  {
    hpText.text = gameManager.Player.Hp.ToString();
    maxHpText.text = gameManager.Player.MaxHp.ToString();
    if (gameManager.gameMode == GameManager.GameMode.Speed)
      scorePanel.gameObject.SetActive(false);
    else
      scoreText.text = gameManager.Score.ToString().PadLeft(6, '0');
    if (gameManager.gameMode == GameManager.GameMode.Endless)
      timeLeftText.text = "Endless";
    else
      timeLeftText.text = gameManager.strTimeLeft;
    levelText.text = (gameManager.currentLevelIndex + 1).ToString();
  }
}
