using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthDrop : MonoBehaviour
{
  [SerializeField] private AudioClip collectSfx;

  private GameManager gameManager;
  private bool alreadyHealed = false;

  private void Awake()
  {
    gameManager = FindObjectOfType<GameManager>();
  }

  private void OnTouched(Collider other)
  {
    if (other.CompareTag("Player") && !alreadyHealed)
    {
      if (gameManager.Player.Hp < gameManager.Player.MaxHp)
      {
        gameManager.Player.Heal(1);
        gameManager.Player.audioSource.PlayOneShot(collectSfx, gameManager.Settings.sfxVolume);
        alreadyHealed = true;
        Destroy(gameObject);
      }
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    OnTouched(other);
  }

  private void OnTriggerStay(Collider other)
  {
    OnTouched(other);
  }
}
