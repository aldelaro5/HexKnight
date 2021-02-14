using System;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
  Up = 0,
  Right,
  Down,
  Left,
  NONE
}

[Serializable]
public class LevelGeneratorParams
{
  [Min(6)] public int levelSize = 10;
  [Min(1)] public int tileSize = 5;
  public int prefferedMinNbrRoom = 1;
  public int maxNbrRoom = 5;
  public Vector2Int MinRoomSize;
  public Vector2Int MaxRoomSize;
  public Vector2Int MinStartRoomSize;
  public Vector2Int MaxStartRoomSize;
  public Vector2Int MinEndRoomSize;
  public Vector2Int MaxEndRoomSize;
  public int nbrEnemies = 3;
  public float likelyhoodTurret = 0.25f;
  public int nbrHealthDrops = 5;
}

[Serializable]
public class bindingOverrideSetting
{
  public string id;
  public string overridePath;
}

[Serializable]
public class Settings
{
  public float musicVolume = 1f;
  public float sfxVolume = 1f;
  public List<bindingOverrideSetting> bindingOverridesKb = new List<bindingOverrideSetting>();
  public List<bindingOverrideSetting> bindingOverridesGamepad = new List<bindingOverrideSetting>();
}
