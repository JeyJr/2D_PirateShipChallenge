using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BoatStats : MonoBehaviour
{
    public SpawnEnemies SpawnEnemies { get; set; }
    public GameManager GameManager { get; set; }

    [Header("BOAT OBJ")]
    [SerializeField] private bool player;

    [Header("LIFE")]
    [SerializeField] private float maxLife;
    [SerializeField] private float currentLife;
    [SerializeField] private Slider lifeBar;
    [SerializeField] private float lifeComparisonValue;
    public float CurrentLife { get => currentLife;}

    [Space(5)]
    [Header("SPRITES")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    public Sprite spriteNoDamage, spriteLightDamage, spriteHeavyDamage, spriteDead;

    [SerializeField] private int points = 1;

    [Header("HIT and DEATH")]
    public GameObject explosion;
    public UnityEvent onDeath, onHit;
    private bool _isDead;
    public bool IsDead
    {
        get { return _isDead; }
        private set
        {
            if (_isDead == value)
                return;
            _isDead = value;

            if (player)
            {
                GetComponent<PlayerMovement>().IsDead = value;
                GetComponent<PlayerBulletsPooling>().IsDead = value;
            }
            else
            {
                GetComponent<EnemyCheckDistance>().IsDead = value;
                GetComponent<EnemyMovement>().IsDead = value;
            }
        }
    }
    private BoatsSFX boatsSFX;

    private void OnEnable()
    {
        boatsSFX = GetComponent<BoatsSFX>();
    }

    public void SetInitialValues(float maxLife)
    {
        lifeComparisonValue = maxLife / 3f;

        this.maxLife = maxLife;
        currentLife = maxLife;
        lifeBar.maxValue = maxLife;
        lifeBar.value = maxLife;

        if (!player)
        {
            lifeBar.gameObject.SetActive(false);
        }

        IsDead = false;
        UpdateSprite(spriteNoDamage);
    }

    public void TakeDamage(float damage)
    {
        boatsSFX.PlayClip(BoatSFXClip.takeDMG);

        if (damage <= 0) return;

        currentLife -= damage;
        onHit.Invoke();

        if (currentLife <= 0)
        {
            onDeath.Invoke();
        }
    }

    public void OnHit()
    {
        if(currentLife <= maxLife - (lifeComparisonValue * 2))
        {
            UpdateSprite(spriteHeavyDamage);
        }
        else if (currentLife <= maxLife - lifeComparisonValue)
        {
            UpdateSprite(spriteLightDamage);
        }

        if (!player)
        {
            lifeBar.gameObject.SetActive(true);
        }

        lifeBar.value = currentLife;
    }
    
    public void OnDeath()
    {
        Instantiate(explosion, transform.position, Quaternion.identity);
        UpdateSprite(spriteDead);
        IsDead = true;

        if (player)
        {
            StartCoroutine(EndGamePlayer());
        }
        else{
            StartCoroutine(DisableBoat());
        }

        //SFX?
    }

    IEnumerator EndGamePlayer()
    {
        yield return new WaitForSeconds(2);

        if(GameManager.GameState != GameState.EndGame)
            GameManager.SetGameState(GameState.EndGame);
    }

    IEnumerator DisableBoat()
    {
        GameManager.Points += points;

        yield return new WaitForSeconds(1);
        SpawnEnemies.Enqueue(gameObject);
    }

    private void UpdateSprite(Sprite sprite)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        spriteRenderer.sprite = sprite;
    }
}
