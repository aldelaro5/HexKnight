using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class OptionsUI : MonoBehaviour
{
  [SerializeField] private Slider musicVolumeSlider;
  [SerializeField] private Slider sfxVolumeSlider;

  private GameManager gameManager;

  private void Awake()
  {
    gameManager = FindObjectOfType<GameManager>();
  }

  private void OnEnable()
  {
    musicVolumeSlider.value = gameManager.Settings.musicVolume;
    sfxVolumeSlider.value = gameManager.Settings.sfxVolume;
  }

  public void OnConfirmSettings()
  {
    gameManager.Settings.musicVolume = musicVolumeSlider.value;
    gameManager.Settings.sfxVolume = sfxVolumeSlider.value;
    gameManager.Settings.bindingOverridesKb.Clear();
    gameManager.Settings.bindingOverridesGamepad.Clear();
    
    var bindingsKb = gameManager.Inputs.SelectMany(x => x.bindings).Where(x => x.groups == "Keyboard" &&
                                                                               !string.IsNullOrEmpty(x.overridePath));
    foreach (var binding in bindingsKb)
    {
      bindingOverrideSetting newBinding = new bindingOverrideSetting();
      newBinding.overridePath = binding.overridePath;
      newBinding.id = binding.id.ToString();
      gameManager.Settings.bindingOverridesKb.Add(newBinding);
    }

    var bindingsGamepad = gameManager.Inputs.SelectMany(x => x.bindings).Where(x => x.groups == "Gamepad" &&
                                                                                    !string.IsNullOrEmpty(x.overridePath));
    foreach (var binding in bindingsGamepad)
    {
      var bindingOverride = gameManager.Settings.bindingOverridesGamepad.FirstOrDefault(x => x.id == binding.id.ToString());
      if (bindingOverride != null)
      {
        bindingOverride.overridePath = binding.overridePath;
      }
      else
      {
        bindingOverrideSetting newBinding = new bindingOverrideSetting();
        newBinding.id = binding.id.ToString();
        newBinding.overridePath = binding.overridePath;
        gameManager.Settings.bindingOverridesGamepad.Add(newBinding);
      }
    }

    gameManager.WriteSettings();
  }
}
