using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitUnlocker : MonoBehaviour
{
  private LevelGenerator lvlGenerator;

  private void Start()
  {
    lvlGenerator = FindObjectOfType<LevelGenerator>();
  }

  private void OnTriggerEnter(Collider other) 
  {
    if (other.CompareTag("Player"))
    {
      lvlGenerator.UnlockExit();
      Destroy(gameObject);
    }
  }
}
