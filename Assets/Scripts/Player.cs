using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float moveForce = 10f;
    
    [SerializeField]
    private float jumpForce = 11f;

    private float movementX;
    private Rigidbody2D myBody;
    private Animator anim;
    private SpriteRenderer sr;
    private string WALK_ANIMATION = "Walk";
    private string GROUND_TAG = "Ground";
    private string ENEMY_TAG = "Enemy";

    private bool isGrounded = true;

    public Text Text;

    private void Awake()
    {
        myBody = GetComponent<Rigidbody2D>();    
        anim = GetComponent<Animator>();    
        sr = GetComponent<SpriteRenderer>();    
    }

    void Start()
    {
        
    }

    void Update()
    {
        PLayerMoveKeyboard();
        AnimatePlayer();
        PLayerJump();
    }


    void PLayerMoveKeyboard()
    {
        movementX = ControlFreak2.CF2Input.GetAxisRaw("Horizontal"); //IN raw there are only -1 on left and +1 on right

        transform.position += new Vector3(movementX, 0f, 0f) * Time.deltaTime * moveForce;

    }

    void AnimatePlayer()
    {
        // movementX =  -1 0 1

        if (movementX > 0)
        {
            sr.flipX = false;   // flip the player is false
            anim.SetBool(WALK_ANIMATION, true); // set animator true
        }
        else if(movementX < 0)
        {
            sr.flipX = true;
            anim.SetBool(WALK_ANIMATION, true); // set animator true
        }
        else
        {   
            anim.SetBool(WALK_ANIMATION, false); // set animator false
        }
    }


    void PLayerJump()
    {
        if (ControlFreak2.CF2Input.GetButtonDown("Jump") && isGrounded)
        {
            isGrounded = false;
            myBody.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(GROUND_TAG))
        {
            isGrounded = true;
        }

        if (collision.gameObject.CompareTag(ENEMY_TAG))
        {
            Destroy(gameObject);
            

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(ENEMY_TAG))
        {
            Destroy(gameObject);
            
        }
        
    }

    
   
    
}

