using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ModeSelectionPage : MonoBehaviour
{
  [SerializeField] private TMP_Text textModeDescription;
  [SerializeField] private Button btnStandardMode;
  [SerializeField] private Button btnLongPlayMode;
  [SerializeField] private Button btnEndlessMode;
  [SerializeField] private Button btnSpeedMode;
  
  private EventSystem eventSystem;
  private GameManager gameManager;

  private void Awake()
  {
    gameManager = FindObjectOfType<GameManager>();
    eventSystem = EventSystem.current;
  }

  void Update()
  {
    if (eventSystem.currentSelectedGameObject == btnStandardMode.gameObject)
    {
      textModeDescription.text = "Get the highest score in 5 levels under 5 minutes!";
    }
    else if (eventSystem.currentSelectedGameObject == btnLongPlayMode.gameObject)
    {
      textModeDescription.text = "Get the highest score in 30 levels under 30 minutes!";
    }
    else if (eventSystem.currentSelectedGameObject == btnEndlessMode.gameObject)
    {
      textModeDescription.text = "Get the highest score without any time limit!";
    }
    else if (eventSystem.currentSelectedGameObject == btnSpeedMode.gameObject)
    {
      textModeDescription.text = "Beat the highest amount of levels in 15 minutes!";
    }
    else
    {
      textModeDescription.text = "";
    }
  }
}
