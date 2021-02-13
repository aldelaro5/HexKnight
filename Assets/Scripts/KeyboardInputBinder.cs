using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardInputBinder : MonoBehaviour
{
  [Serializable]
  private class Binding
  {
    public InputActionReference actionRef;
    public string bindingName;
  }

  [SerializeField] private TMP_Text bindingNameText;
  [SerializeField] private List<Binding> bindings;

  private GameManager gameManager;

  private void Awake()
  {
    gameManager = FindObjectOfType<GameManager>();
  }

  private void OnEnable()
  {
    if (bindings[0].actionRef == null)
      return;

    InputAction action;
    int bindingIndex;
    ResolveBindingInfoFromIndex(0, out action, out bindingIndex);
    bindingNameText.text = action.GetBindingDisplayString(bindingIndex);
  }

  private void ResolveBindingInfoFromIndex(int i, out InputAction action, out int bindingIndex)
  {
    action = gameManager.Inputs.FirstOrDefault(x => x.id == bindings[i].actionRef.ToInputAction().id);

    if (string.IsNullOrEmpty(action.name))
      throw new Exception("Cannot find action for name " + bindings[i].actionRef.ToInputAction().name);

    List<InputBinding> inputBindings = action.bindings.ToList();
    InputBinding theBinding;
    if (string.IsNullOrWhiteSpace(bindings[i].bindingName))
    {
      theBinding = inputBindings.FirstOrDefault(x => (x.groups == "Keyboard"));
    }
    else
    {
      theBinding = inputBindings.FirstOrDefault(x => (x.name == bindings[i].bindingName) &&
                                                        x.groups == "Keyboard");
    }

    if (theBinding.id == Guid.Empty)
    {
      throw new Exception("Cannot find binding for name " + bindings[i].bindingName + " for action " +
                          bindings[i].actionRef.ToInputAction().name);
    }

    bindingIndex = inputBindings.FindIndex(x => x.id == theBinding.id);
  }

  public void InteractiveRebind()
  {
    InputAction action;
    int bindingIndex;
    ResolveBindingInfoFromIndex(0, out action, out bindingIndex);
    var rebindingOp = action.PerformInteractiveRebinding(bindingIndex);
    rebindingOp.WithCancelingThrough("<Keyboard>/escape");
    rebindingOp.OnCancel(x =>
    {
      rebindingOp.Dispose();
      bindingNameText.text = action.GetBindingDisplayString(bindingIndex);
    });
    rebindingOp.OnComplete(x =>
    {
      print("rebound " + action.name + " - " + bindings[0].bindingName +
            " to " + action.bindings[bindingIndex].effectivePath);
      bindingNameText.text = action.GetBindingDisplayString(bindingIndex);
      rebindingOp.Dispose();

      string overridePath = action.bindings[bindingIndex].overridePath;
      for (int i = 1; i < bindings.Count; i++)
      {
        ResolveBindingInfoFromIndex(i, out action, out bindingIndex);
        action.ApplyBindingOverride(bindingIndex, overridePath);
        print("rebound " + action.name + " - " + bindings[i].bindingName +
              " to " + action.bindings[bindingIndex].effectivePath);
      }
    });
    bindingNameText.text = "Enter key, ESC to cancel";
    rebindingOp.Start();
  }

  public void ResetToDefault()
  {
    for (int i = 0; i < bindings.Count; i++)
    {
      InputAction action;
      int bindingIndex;
      ResolveBindingInfoFromIndex(i, out action, out bindingIndex);
      action.RemoveBindingOverride(bindingIndex);
      bindingNameText.text = action.GetBindingDisplayString(bindingIndex);
      print("rebound " + action.name + " - " + bindings[i].bindingName +
            " to " + action.bindings[bindingIndex].effectivePath);
    }
  }
}
