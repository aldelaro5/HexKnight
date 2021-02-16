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
  private GameManager gameManager;

  private void Start()
  {
    gameManager = FindObjectOfType<GameManager>();
    lvlGenerator = FindObjectOfType<LevelGenerator>();
    animator = GetComponent<Animator>();
    audioSource = GetComponent<AudioSource>();
  }

  private void OnButtonPressed()
  {
    gameManager.Player.audioSource.PlayOneShot(exitUnlockSfx, gameManager.Settings.sfxVolume);
    lvlGenerator.UnlockExit();
  }

  public void PressButton()
  {
    audioSource.PlayOneShot(buttonPressedSfx, gameManager.Settings.sfxVolume);
    animator.SetTrigger("ExitUnlocked");
  } 
}
