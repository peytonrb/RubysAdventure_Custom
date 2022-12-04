using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class RubyController : MonoBehaviour
{
    public float speed = 3.0f;
    public int maxHealth = 5;
    public float timeInvincible = 2.0f;
    public int health { get { return currentHealth; } }
    private bool isInvincible;
    private float invincibleTimer;
    private int currentHealth;
    private Rigidbody2D rigidbody2d;
    private float horizontal;
    private float vertical;
    private bool gameOver = false;
    private int cogs;
    public static int level = 1;

    // Animation
    Animator animator;
    Vector2 lookDirection = new Vector2(1, 0);

    // Projectile
    public GameObject projectilePrefab;

    // Audio
    private AudioSource audioSource;
    public AudioClip throwSound;
    public AudioClip hitSound;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip background;
    public AudioClip pickupSound;

    // Paricle System
    public ParticleSystem healthIncrease;
    public ParticleSystem healthDecrease;

    // Text
    public TextMeshProUGUI scoreText;
    public GameObject winText;
    public GameObject loseText;
    public GameObject newScene;
    public TextMeshProUGUI cogText;
    private int totalScore = 0;

    // Speed pickup
    private bool isSpeeding;
    private float speedTimer;
    private float timeSpeeding = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        // QualitySettings.vSyncCount = 0;
        // Application.targetFrameRate = 10; // declares Unity to render 10 fps - declaring this makes it the same on every machine
        winText.SetActive(false);
        loseText.SetActive(false);
        newScene.SetActive(false);
        rigidbody2d = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = background;
        audioSource.Play();
        audioSource.loop = true;
        ChangeScore(totalScore);
        cogs = 4;
        ChangeCogs(cogs);
    }

    // Update is called once per frame
    void Update()
    {
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
        }

        if (isSpeeding)
        {
            speed = 6.0f;
            speedTimer -= Time.deltaTime; 

            if (speedTimer <= 0)
                isSpeeding = false;
        }

        if (!isSpeeding) {
            speed = 3.0f;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            Launch();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("NPC"));

            if (hit.collider != null)
            {
                NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();

                if (character != null)
                {
                    if (totalScore == 6)
                    {
                        SceneManager.LoadScene("Level2");
                        level = 2;
                    }
                    else
                    {
                        character.DisplayDialog();
                    }
                }
            }
        }

        if (Input.GetKey(KeyCode.R))
        {
            if (gameOver == true)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // this loads the currently active scene
                speed = 3.0f;
                gameOver = false;
                audioSource.Stop();
                audioSource.loop = false;
                audioSource.clip = winSound;
                audioSource.Play();
                audioSource.loop = true;
            }

        }
    }

    // now that Ruby has physics applied, use FixedUpdate for movement
    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector2 move = new Vector2(horizontal, vertical);

        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);

        Vector2 position = transform.position;
        position.x = position.x + speed * horizontal * Time.deltaTime; // frame independent
        position.y = position.y + speed * vertical * Time.deltaTime;
        transform.position = position;

        rigidbody2d.MovePosition(position);
    }

    public void ChangeSpeed(int velocity) {
        if (velocity > 0)
        {
            isSpeeding = true;
            speedTimer = timeSpeeding;
        }
    }

    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            if (isInvincible)
                return;

            isInvincible = true;
            invincibleTimer = timeInvincible;
            PlaySound(hitSound);
            Instantiate(healthDecrease, rigidbody2d.position + Vector2.up * 1.5f, Quaternion.identity);
        }

        if (amount > 0)
        {
            Instantiate(healthIncrease, rigidbody2d.position + Vector2.up * 1.5f, Quaternion.identity);
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        if (currentHealth == 0)
        {
            loseText.SetActive(true);
            speed = 0.0f;
            gameOver = true;
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = loseSound;
            audioSource.Play();
            audioSource.loop = true;
        }

        UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);
    }

    public void ChangeScore(int score)
    {
        totalScore += score;
        scoreText.text = "Robots Fixed: " + totalScore.ToString() + "/6";

        if (totalScore == 6 && level == 1)
        {
            newScene.SetActive(true);
        } else if (totalScore == 6 && level == 2) {
            winText.SetActive(true);
            gameOver = true;
            audioSource.clip = winSound;
            audioSource.Play();
            audioSource.loop = true;
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        RubyController player = other.gameObject.GetComponent<RubyController>();

        if (player != null)
        {
            player.ChangeHealth(-1);
        }

        if (other.collider.tag == "Cogs")
        {
            PlaySound(pickupSound);
            cogs += 4;
            ChangeCogs(cogs);
            Destroy(other.collider.gameObject);
        }
    }

    // when you want to launch a projectile
    void Launch()
    {
        if (cogs > 0)
        {
            GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

            Projectile projectile = projectileObject.GetComponent<Projectile>();
            projectile.Launch(lookDirection, 300);

            animator.SetTrigger("Launch");
            PlaySound(throwSound);
            cogs -= 1;
            ChangeCogs(cogs);
        }
    }

    void ChangeCogs(int cogs)
    {
        cogText.text = "Cogs Remaining: " + cogs.ToString();
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
