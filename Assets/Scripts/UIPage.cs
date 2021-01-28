using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIPage : MonoBehaviour
{
  [SerializeField] private GameObject defaultSelectedObject;

  private void OnEnable()
  {
    if (EventSystem.current.currentSelectedGameObject != null)
      EventSystem.current.SetSelectedGameObject(defaultSelectedObject);
  }
}
