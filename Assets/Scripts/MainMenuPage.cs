using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainMenuPage : MonoBehaviour
{
  [SerializeField] private TMP_Text txtVersion;

  private void Awake()
  {
    txtVersion.text = "v" + Application.version;
  }
}
