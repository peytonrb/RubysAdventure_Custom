using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StrongEnemyController : MonoBehaviour
{
    public float speed;
    public bool vertical;
    public float changeTime = 3.0f;
    public ParticleSystem smokeEffect;
    public int damage;
    public bool takesThree;
    private int beenHit = 0;
    Rigidbody2D rigidbody2D;
    float timer;
    int direction = 1;
    bool broken = true;
    Animator animator;
    private RubyController rubyController;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        timer = changeTime;
        animator = GetComponent<Animator>();
        GameObject rubyControllerObject = GameObject.FindWithTag("RubyController");
        rubyController = rubyControllerObject.GetComponent<RubyController>();
    }

    void Update()
    {
        if (!broken)
        {
            return;
        }

        timer -= Time.deltaTime;

        if (timer < 0)
        {
            direction = -direction;
            timer = changeTime;
        }
    }

    void FixedUpdate()
    {
        if (!broken)
        {
            return;
        }

        Vector2 position = rigidbody2D.position;

        if (vertical)
        {
            position.y = position.y + Time.deltaTime * speed * direction;
            animator.SetFloat("MoveX", 0);
            animator.SetFloat("MoveY", direction);
        }
        else
        {
            position.x = position.x + Time.deltaTime * speed * direction;
            animator.SetFloat("MoveX", direction);
            animator.SetFloat("MoveY", 0);
        }

        rigidbody2D.MovePosition(position);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        RubyController player = other.gameObject.GetComponent<RubyController>();

        if (player != null)
        {
            player.ChangeHealth(-(damage));
        } 
    }

    //Public because we want to call it from elsewhere like the projectile script
    public void Fix()
    {
        if (beenHit != 2) {
            beenHit++;
            
        } else {
            if (rubyController != null) {
                rubyController.ChangeScore(1);
            }

            broken = false;
            rigidbody2D.simulated = false;
            animator.SetTrigger("Fixed");
            smokeEffect.Stop();
        }
    }
}
