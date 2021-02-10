using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
  [SerializeField] [Min(0.01f)] private float movementSpeed = 1f;
  [SerializeField] [Min(0.01f)] private float movementTransitionSpeed = 1f;
  [SerializeField] private float movementDelayInSeconds = 0.25f;
  [SerializeField] [Min(0.01f)] private float turningSpeed = 1f;
  [SerializeField] private float turningDelayInSeconds = 0.25f;
  [SerializeField] private int nbrIFrames = 60;
  [SerializeField] private int nbrShieldFrames = 120;
  [SerializeField] private float attackCooldownInSeconds = 1f;
  [SerializeField] private int hp;
  [SerializeField] private int maxHp = 5;
  [SerializeField] private AudioClip moveSfx;
  [SerializeField] private AudioClip attackSfx;
  [SerializeField] private AudioClip dataLinesSfx;
  [SerializeField] private AudioClip tookDamageSfx;
  [SerializeField] private AudioClip deathSfx;
  [SerializeField] private ParticleSystem atkVFX;
  [SerializeField] private ParticleSystem shieldVFX;
  [SerializeField] private ParticleSystem deathVFX;
  [SerializeField] private GameObject shieldObject;
  [SerializeField] private GameObject hoverObject;
  [SerializeField] private Camera mainCamera;
  [SerializeField] private GameObject knightObj;

  public Camera MainCamera { get => mainCamera; }
  public int Hp { get => hp; }
  public int MaxHp { get => maxHp; }

  private AudioSource audioSource;

  private WaitForSeconds turningDelay;
  private WaitForSeconds movementDelay;
  private WaitForSeconds attackShieldDelay;

  private Action<InputAction.CallbackContext> attackCallback;
  private Action<InputAction.CallbackContext> shieldCallback;
  private Action<InputAction.CallbackContext> pauseCallback;

  private Vector2Int currentTile;
  public Vector2Int Tile { get => currentTile; }

  private GameManager gameManager;
  private LevelGenerator lvlGenerator;
  private PlayerInput playerInput;
  private Animator animator;
  private bool isMoving = false;
  private bool isTurning = false;
  private bool isTakingDamage = false;
  private bool isAttacking = false;
  private bool isShielding = false;
  private bool triggerStopShield = false;

  private void Awake()
  {
    hp = maxHp;
    attackCallback = x => OnAttackInput(x);
    shieldCallback = x => OnShieldInput(x);
    pauseCallback = x => OnPauseInput(x);

    gameManager = FindObjectOfType<GameManager>();
    // If we are testing
    if (gameManager == null)
    {
      gameManager = gameObject.AddComponent<GameManager>();
      gameManager.Inputs.Player.Enable();
    }
    gameManager.Inputs.Player.Attack.performed += attackCallback;
    gameManager.Inputs.Player.Shield.performed += shieldCallback;
    gameManager.Inputs.Player.Pause.performed += pauseCallback;
    animator = GetComponent<Animator>();
    audioSource = GetComponent<AudioSource>();
    lvlGenerator = FindObjectOfType<LevelGenerator>();
    movementDelay = new WaitForSeconds(movementDelayInSeconds);
    turningDelay = new WaitForSeconds(turningDelayInSeconds);
    attackShieldDelay = new WaitForSeconds(attackCooldownInSeconds);
  }

  public void UnhookInputEvents()
  {
    gameManager.Inputs.Player.Attack.performed -= attackCallback;
    gameManager.Inputs.Player.Shield.performed -= shieldCallback;
    gameManager.Inputs.Player.Pause.performed -= pauseCallback;
  }

  private void OnPauseInput(InputAction.CallbackContext ctx)
  {
    gameManager.Pause();
  }

  private bool IsBusy()
  {
    return (isMoving || isTurning || isAttacking || isShielding);
  }

  private void Update()
  {
    if (IsBusy())
      return;

    hoverObject.transform.localRotation = Quaternion.identity;

    ProcessMovementInputs();
  }

  private void ProcessMovementInputs()
  {
    if (gameManager.Inputs.Player.ForwardBackward.ReadValue<float>() == 1)
    {
      Vector3 destVector = transform.position + transform.forward * lvlGenerator.TileSize;
      Vector2Int destTile = lvlGenerator.Vec3CenterToTile(destVector);
      if (lvlGenerator.GetTileInfo(destTile).state == LevelGenerator.TileState.Free ||
          lvlGenerator.IsTileExitTile(destTile))
      {
        hoverObject.transform.localRotation = Quaternion.Euler(45, 0, 0);
        StartCoroutine(MoveToDestinationTile(destTile));
      }
    }
    else if (gameManager.Inputs.Player.ForwardBackward.ReadValue<float>() == -1)
    {
      Vector3 destVector = transform.position - transform.forward * lvlGenerator.TileSize;
      Vector2Int destTile = lvlGenerator.Vec3CenterToTile(destVector);
      if (lvlGenerator.GetTileInfo(destTile).state == LevelGenerator.TileState.Free)
      {
        hoverObject.transform.localRotation = Quaternion.Euler(-45, 0, 0);
        StartCoroutine(MoveToDestinationTile(destTile));
      }
    }
    else if (gameManager.Inputs.Player.LockRotation.ReadValue<float>() == 1)
    {
      if (gameManager.Inputs.Player.LeftRight.ReadValue<float>() == 1)
      {
        Vector3 destVector = transform.position + transform.right * lvlGenerator.TileSize;
        Vector2Int destTile = lvlGenerator.Vec3CenterToTile(destVector);
        if (lvlGenerator.GetTileInfo(destTile).state == LevelGenerator.TileState.Free)
        {
          hoverObject.transform.localRotation = Quaternion.Euler(0, 0, -45);
          StartCoroutine(MoveToDestinationTile(destTile));
        }
      }
      else if (gameManager.Inputs.Player.LeftRight.ReadValue<float>() == -1)
      {
        Vector3 destVector = transform.position + -transform.right * lvlGenerator.TileSize;
        Vector2Int destTile = lvlGenerator.Vec3CenterToTile(destVector);
        if (lvlGenerator.GetTileInfo(destTile).state == LevelGenerator.TileState.Free)
        {
          hoverObject.transform.localRotation = Quaternion.Euler(0, 0, 45);
          StartCoroutine(MoveToDestinationTile(destTile));
        }
      }
    }
    else
    {
      if (gameManager.Inputs.Player.LeftRight.ReadValue<float>() == 1)
        StartCoroutine(TurnToAngleAroundY(90f));
      else if (gameManager.Inputs.Player.LeftRight.ReadValue<float>() == -1)
        StartCoroutine(TurnToAngleAroundY(-90f));
    }
  }

  public void OnShieldInput(InputAction.CallbackContext ctx)
  {
    if (IsBusy())
      return;

    StartCoroutine(Shield());
  }

  public void OnAttackInput(InputAction.CallbackContext ctx)
  {
    if (IsBusy())
      return;

    Vector3 destVector = transform.position + transform.forward * lvlGenerator.TileSize;
    Vector2Int destTile = lvlGenerator.Vec3CenterToTile(destVector);
    LevelGenerator.TileInfo tileInfo = lvlGenerator.GetTileInfo(destTile);
    if (tileInfo.state == LevelGenerator.TileState.ExitUnlockerBlocked)
    {
      ExitUnlocker unlocker = tileInfo.obj.GetComponent<ExitUnlocker>();
      unlocker.PressButton();
    }
    else
    {
      StartCoroutine(Attack(destTile));
    }
  }

  public void ResetTile(Vector3 startCenter)
  {
    Vector2Int startTile = lvlGenerator.Vec3CenterToTile(startCenter);
    currentTile = startTile;
    lvlGenerator.ReserveTileAsPlayer(startTile, gameObject);
    transform.position = lvlGenerator.TileToVec3Center(new Vector2Int(startTile.x, startTile.y - 3));
    StartCoroutine(MoveIntoStart(startCenter));
  }

  private IEnumerator MoveIntoStart(Vector3 start)
  {
    isMoving = true;
    audioSource.PlayOneShot(dataLinesSfx);
    while (transform.position != start)
    {
      transform.position = Vector3.MoveTowards(transform.position, start, movementTransitionSpeed);
      yield return null;
    }
    isMoving = false;
    yield break;
  }

  private IEnumerator Die()
  {
    gameManager.Inputs.Player.Disable();
    knightObj.SetActive(false);
    deathVFX.Play();
    audioSource.PlayOneShot(deathSfx);
    yield return new WaitForSeconds(2);
    StartCoroutine(gameManager.GameOver());
    yield break;
  }

  private IEnumerator ReceiveHit(int dmg)
  {
    isTakingDamage = true;

    animator.SetTrigger("TakeDamage");
    hp -= dmg;
    gameManager.UpdateHUD();
    if (hp <= 0)
    {
      audioSource.PlayOneShot(deathSfx);
      StartCoroutine(Die());
    }
    else
    {
      audioSource.PlayOneShot(tookDamageSfx);
      for (int i = 0; i < nbrIFrames; i++)
        yield return null;

      isTakingDamage = false;
    }
    yield break;
  }

  private void OnParticleCollision(GameObject other)
  {
    GotAttacked(1);
  }

  public void GotAttacked(int dmg)
  {
    triggerStopShield = true;

    if (!isTakingDamage && !isShielding)
      StartCoroutine(ReceiveHit(dmg));
  }

  private IEnumerator MoveToDestinationTile(Vector2Int destinationTile)
  {
    isMoving = true;
    bool toExit = lvlGenerator.IsTileExitTile(destinationTile);
    if (!toExit)
      lvlGenerator.ReserveTileAsPlayer(destinationTile, gameObject);

    Vector3 destVec = lvlGenerator.TileToVec3Center(destinationTile);
    audioSource.PlayOneShot(moveSfx);
    while (transform.position != destVec)
    {
      transform.position = Vector3.MoveTowards(transform.position, destVec, Time.deltaTime * movementSpeed);
      yield return null;
    }

    if (!toExit)
      lvlGenerator.FreeTile(currentTile);
    currentTile = destinationTile;
    yield return movementDelay;
    if (toExit)
    {
      destVec = lvlGenerator.TileToVec3Center(new Vector2Int(destinationTile.x, destinationTile.y + 3));
      audioSource.PlayOneShot(dataLinesSfx);
      while (transform.position != destVec)
      {
        transform.position = Vector3.MoveTowards(transform.position, destVec, movementTransitionSpeed);
        yield return null;
      }
      lvlGenerator.ExitReached();
    }
    else
    {
      isMoving = false;
    }
    yield break;
  }

  private IEnumerator Attack(Vector2Int destinationTile)
  {
    isAttacking = true;
    atkVFX.Play();
    audioSource.PlayOneShot(attackSfx);
    if (lvlGenerator.GetTileInfo(destinationTile).state == LevelGenerator.TileState.Enemy)
    {
      GameObject objEnemy = lvlGenerator.GetTileInfo(destinationTile).obj;
      objEnemy.SendMessage("GotAttacked", 1);
    }
    yield return attackShieldDelay;
    atkVFX.Stop();
    isAttacking = false;
    yield break;
  }

  private IEnumerator Shield()
  {
    isShielding = true;
    shieldVFX.Play();
    shieldObject.SetActive(true);
    int frames = 0;
    triggerStopShield = false;
    while (frames < nbrShieldFrames && !triggerStopShield)
    {
      yield return null;
      frames++;
    }
    shieldVFX.Stop();
    shieldObject.SetActive(false);
    isShielding = false;
    yield break;
  }

  private IEnumerator TurnToAngleAroundY(float angle)
  {
    isTurning = true;
    Vector3 angles = transform.rotation.eulerAngles;
    Quaternion destAngle = Quaternion.Euler(angles.x, angles.y + angle, angles.z);
    while (transform.rotation.eulerAngles != destAngle.eulerAngles)
    {
      transform.rotation = Quaternion.RotateTowards(transform.rotation, destAngle, Time.deltaTime * turningSpeed);
      yield return null;
    }
    yield return turningDelay;
    isTurning = false;
    yield break;
  }
}
