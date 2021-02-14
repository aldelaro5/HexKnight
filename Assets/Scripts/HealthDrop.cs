using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthDrop : MonoBehaviour
{
  private GameManager gameManager;

  private void Awake()
  {
    gameManager = FindObjectOfType<GameManager>();
  }

  private void OnTouched(Collider other)
  {
    if (other.CompareTag("Player"))
    {
      if (gameManager.Player.Hp < gameManager.Player.MaxHp)
      {
        gameManager.Player.Heal(1);
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
