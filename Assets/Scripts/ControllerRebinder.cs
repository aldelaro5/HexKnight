using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class ControllerRebinder : MonoBehaviour
{
  [Serializable]
  private class BindingGroup
  {
    public List<Binding> bindings;
    public string displayName;
  }

  [Serializable]
  private class Binding
  {
    public InputActionReference actionRef;
    public string compositeName;
    public string bindingName;
  }

  [SerializeField] private CanvasRenderer bindingOverlay;
  [SerializeField] private TMP_Text bindingNameText;
  [SerializeField] private List<BindingGroup> bindingsGroups;
  private GameManager gameManager;
  private WaitForSeconds inputRebindWait = new WaitForSeconds(0.25f);

  private void Awake()
  {
    gameManager = FindObjectOfType<GameManager>();
  }

  private void ResolveBindingInfo(int groupIndex, int index, out InputAction action, out int bindingIndex)
  {
    Binding binding = bindingsGroups[groupIndex].bindings[index];
    if (binding.actionRef.ToInputAction().actionMap.name == "UI")
      action = binding.actionRef.ToInputAction();
    else
      action = gameManager.Inputs.FirstOrDefault(x => x.id == binding.actionRef.ToInputAction().id);
    
    if (string.IsNullOrEmpty(action.name))
      throw new Exception("Cannot find action for name " + binding.actionRef.ToInputAction().name);

    List<InputBinding> inputBindings = action.bindings.ToList();
    InputBinding theBinding;
    if (string.IsNullOrWhiteSpace(binding.bindingName))
    {
      theBinding = inputBindings.FirstOrDefault(x => (x.groups == "Gamepad"));
    }
    else
    {
      var theComposite = inputBindings.FirstOrDefault(x => x.name == binding.compositeName && x.isComposite);
      if (theComposite.id == Guid.Empty)
        throw new Exception("Cannot find composite binding for name " + binding.compositeName);

      int indexPart = inputBindings.FindIndex(x => x.id == theComposite.id) + 1;
      theBinding = inputBindings[indexPart];
      while (theBinding.isPartOfComposite && theBinding.name != binding.bindingName &&
             theBinding.groups == "Gamepad")
      {
        indexPart++;
        theBinding = inputBindings[indexPart];
      }

      if (theBinding.name != binding.bindingName)
        theBinding.id = Guid.Empty;
    }

    if (theBinding.id == Guid.Empty)
    {
      throw new Exception("Cannot find binding for name " + binding.bindingName + " for action " +
                          binding.actionRef.ToInputAction().name + " and compositive " +
                          binding.compositeName);
    }

    bindingIndex = inputBindings.FindIndex(x => x.id == theBinding.id);
  }

  public void InteractiveRebind()
  {
    bindingOverlay.gameObject.SetActive(true);
    InteractiveRebind(0);
  }

  private void InteractiveRebind(int groupIndex)
  {
    if (groupIndex >= bindingsGroups.Count)
    {
      bindingOverlay.gameObject.SetActive(false);
      return;
    }

    InputAction action;
    int bindingIndex;
    ResolveBindingInfo(groupIndex, 0, out action, out bindingIndex);
    var rebindingOp = action.PerformInteractiveRebinding(bindingIndex);
    rebindingOp.WithCancelingThrough("<Keyboard>/escape");
    rebindingOp.OnCancel(x =>
    {
      rebindingOp.Dispose();

      for (int i = 0; i < bindingsGroups[groupIndex].bindings.Count; i++)
      {
        ResolveBindingInfo(groupIndex, i, out action, out bindingIndex);
        action.RemoveBindingOverride(bindingIndex);
        
        Binding binding = bindingsGroups[groupIndex].bindings[i];
        print("reset " + action.name + " - " + binding.compositeName +
            " - " + binding.bindingName + " to " + action.bindings[bindingIndex].effectivePath);
      }

      InteractiveRebind(groupIndex + 1);
    });
    rebindingOp.OnComplete(x =>
    {
      rebindingOp.Dispose();

      Binding binding = bindingsGroups[groupIndex].bindings[0];
      print("rebound " + action.name + " - " + binding.compositeName +
            " - " + binding.bindingName + " to " + action.bindings[bindingIndex].effectivePath);

      string overridePath = action.bindings[bindingIndex].overridePath;
      for (int i = 1; i < bindingsGroups[groupIndex].bindings.Count; i++)
      {
        ResolveBindingInfo(groupIndex, i, out action, out bindingIndex);
        action.ApplyBindingOverride(bindingIndex, overridePath);

        binding = bindingsGroups[groupIndex].bindings[i];
        print("rebound " + action.name + " - " + binding.compositeName +
            " - " + binding.bindingName + " to " + action.bindings[bindingIndex].effectivePath);
      }

      StartCoroutine(WaitAndProcessRebind(groupIndex + 1));
    });
    bindingNameText.text = bindingsGroups[groupIndex].displayName;
    rebindingOp.Start();
  }

  private IEnumerator WaitAndProcessRebind(int groupIndex)
  {
    yield return inputRebindWait;
    InteractiveRebind(groupIndex);
  }
}
