using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TurretAI : MonoBehaviour
{
  [SerializeField] ParticleSystem deathVFX;
  [SerializeField] ParticleSystem atkVFX;
  [SerializeField] AudioClip laserSfx;
  [SerializeField] AudioClip tookDamageSfx;
  [SerializeField] AudioClip deathSfx;
  [SerializeField] GameObject MainMesh;
  [SerializeField] private int tilesAttackRange = 3;
  [SerializeField] private float idleTimeInSeconds = 5f;
  [SerializeField] private float attackCooldownInSeconds = 1f;
  [SerializeField] private int nbrIFrames = 60;
  [SerializeField] private int hp = 2;

  private Animator animator;
  private AudioSource audioSource;
  private GameManager gameManager;

  private WaitForSeconds idleDelay;
  private WaitForSeconds attackDelay;

  private readonly Direction[] directions = Enum.GetValues(typeof(Direction)).Cast<Direction>().ToArray();
  private LevelGenerator lvlGenerator;
  private Player player;
  private bool isIdling = false;
  private bool onAttackCooldown = false;
  private bool isTakingDamage = false;
  private bool isDying;
  private Coroutine idlingCoroutine;
  private Vector2Int currentTile;

  void Start()
  {
    animator = GetComponent<Animator>();
    audioSource = GetComponent<AudioSource>();
    lvlGenerator = FindObjectOfType<LevelGenerator>();
    gameManager = FindObjectOfType<GameManager>();
    player = FindObjectOfType<Player>();
    idleDelay = new WaitForSeconds(idleTimeInSeconds);
    attackDelay = new WaitForSeconds(attackCooldownInSeconds);
    currentTile = lvlGenerator.Vec3CenterToTile(transform.position);
    lvlGenerator.ReserveTileAsEnemy(currentTile, gameObject);
  }

  private IEnumerator Idle()
  {
    isIdling = true;
    yield return idleDelay;
    isIdling = false;
    yield break;
  }

  private IEnumerator WaitToAttack()
  {
    onAttackCooldown = true;
    yield return attackDelay;
    onAttackCooldown = false;
    yield break;
  }

  void Update()
  {
    if (onAttackCooldown || isDying)
      return;

    if (Math.Abs(currentTile.x - player.Tile.x) +
        Math.Abs(currentTile.y - player.Tile.y) <= tilesAttackRange)
    {
      if (isIdling)
      {
        StopCoroutine(Idle());
        isIdling = false;
      }

      transform.LookAt(player.transform);
      AIAttack();
    }
    else
    {
      if (isIdling)
        return;

      Direction randomDir = (Direction)Random.Range(0, (int)Direction.NONE);
      FaceDirection(randomDir);
      StartCoroutine(Idle());
    }
  }

  private void FaceDirection(Direction dir)
  {
    Vector3 vec3Direction = Vector3.zero;
    switch (dir)
    {
      case Direction.Up:
        vec3Direction = Vector3.forward;
        break;
      case Direction.Right:
        vec3Direction = Vector3.right;
        break;
      case Direction.Down:
        vec3Direction = Vector3.back;
        break;
      case Direction.Left:
        vec3Direction = Vector3.left;
        break;
      case Direction.NONE:
        vec3Direction = transform.forward;
        break;
    }

    transform.LookAt(transform.position + vec3Direction);
  }

  private void AIAttack()
  {
    audioSource.PlayOneShot(laserSfx, gameManager.Settings.sfxVolume);
    atkVFX.Play();
    idlingCoroutine = StartCoroutine(WaitToAttack());
  }

  private IEnumerator ReceiveHit(int dmg)
  {
    isTakingDamage = true;

    animator.SetTrigger("TakeDamage");
    hp -= dmg;
    gameManager.HitEnemy();
    if (hp <= 0)
    {
      audioSource.PlayOneShot(deathSfx, gameManager.Settings.sfxVolume);
      gameManager.KilledEnemy();
      Die();
    }
    else
    {
      for (int i = 0; i < nbrIFrames; i++)
        yield return null;

      isTakingDamage = false;
    }
    yield break;
  }

  private void Die()
  {
    Destroy(MainMesh);
    deathVFX.Play();
    Destroy(gameObject, deathVFX.main.duration);
    Destroy(this, deathVFX.main.duration);
    lvlGenerator.FreeTile(currentTile);
    isDying = true;
  }

  private void GotAttacked(int dmg)
  {
    if (!isTakingDamage)
      StartCoroutine(ReceiveHit(dmg));
  }
}
