using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitUnlocker : MonoBehaviour
{
  private LevelGenerator lvlGenerator;
  private Animator animator;

  private void Start()
  {
    lvlGenerator = FindObjectOfType<LevelGenerator>();
    animator = GetComponent<Animator>();
  }

  private void OnButtonPressed()
  {
    lvlGenerator.UnlockExit();
  }

  public void PressButton()
  {
    animator.SetTrigger("ExitUnlocked");
  } 
}
