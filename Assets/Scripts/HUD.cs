using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
  [SerializeField] private TMP_Text scoreText;
  [SerializeField] private TMP_Text hpText;
  [SerializeField] private TMP_Text maxHpText;

  private GameManager gameManager;

  private void Awake()
  {
    gameManager = FindObjectOfType<GameManager>();
  }

  public void UpdateDisplay()
  {
    hpText.text = gameManager.Player.Hp.ToString();
    maxHpText.text = gameManager.Player.MaxHp.ToString();
    scoreText.text = gameManager.Score.ToString().PadLeft(5, '0');
  }
}
