using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System.Collections.Generic;

public class OptionsUI : MonoBehaviour
{
  [SerializeField] private Slider musicVolumeSlider;
  [SerializeField] private Slider sfxVolumeSlider;
  [SerializeField] private Toggle toggleFullScreen;
  [SerializeField] private TMP_Dropdown dropdResolutions;

  private GameManager gameManager;
  private Resolution[] resolutions;

  private void Awake()
  {
    gameManager = FindObjectOfType<GameManager>();

    List<string> strResolutions = new List<string>();
    resolutions = Screen.resolutions;
    int indexCurrentRes = 0;
    for (int i = 0; i < resolutions.Length; i++)
    {
      string str = resolutions[i].width + "x" + resolutions[i].height;
      strResolutions.Add(str);

      if (resolutions[i].width == Screen.currentResolution.width &&
          resolutions[i].height == Screen.currentResolution.height)
      {
        indexCurrentRes = i;
      }
    }
    dropdResolutions.AddOptions(strResolutions);
    dropdResolutions.value = indexCurrentRes;
    dropdResolutions.RefreshShownValue();
  }

  private void OnEnable()
  {
    musicVolumeSlider.value = gameManager.Settings.musicVolume;
    sfxVolumeSlider.value = gameManager.Settings.sfxVolume;
    toggleFullScreen.isOn = gameManager.Settings.fullScreen;
    Resolution res = resolutions.FirstOrDefault(x => x.height == gameManager.Settings.resHeight && 
                                                     x.width == gameManager.Settings.resWidth);
    if (res.height != 0)
      dropdResolutions.value = Array.IndexOf(resolutions, res);
  }

  public void OnConfirmSettings()
  {
    gameManager.Settings.musicVolume = musicVolumeSlider.value;
    gameManager.Settings.sfxVolume = sfxVolumeSlider.value;
    gameManager.Settings.fullScreen = toggleFullScreen.isOn;
    Resolution res = resolutions[dropdResolutions.value];
    gameManager.Settings.resHeight = res.height;
    gameManager.Settings.resWidth = res.width;
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

  public void OnToggleFullscreen()
  {
    Screen.fullScreen = toggleFullScreen.isOn;
    print("fullscreen: " + Screen.fullScreen);
  }

  public void OnResolutionChanged()
  {
    Resolution res = resolutions[dropdResolutions.value];
    Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    print("resolution: " + Screen.currentResolution);
  }
}
