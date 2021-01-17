using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitUnlocker : MonoBehaviour
{
  [SerializeField] AudioClip buttonPressedSfx;
  [SerializeField] AudioClip exitUnlockSfx;

  private LevelGenerator lvlGenerator;
  private Animator animator;
  private AudioSource audioSource;

  private void Start()
  {
    lvlGenerator = FindObjectOfType<LevelGenerator>();
    animator = GetComponent<Animator>();
    audioSource = GetComponent<AudioSource>();
  }

  private void OnButtonPressed()
  {
    audioSource.PlayOneShot(exitUnlockSfx);
    lvlGenerator.UnlockExit();
  }

  public void PressButton()
  {
    audioSource.PlayOneShot(buttonPressedSfx);
    animator.SetTrigger("ExitUnlocked");
  } 
}
