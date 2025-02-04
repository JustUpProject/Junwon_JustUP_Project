using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.UI.Image;

public enum PlayerState
{
    Move,
    Jump,
    Attach,
    Skill,
    Item,
    Death
}

public class BasicControler : MonoBehaviour
{
    [SerializeField] private LayerMask floorMask;
    [SerializeField] private LayerMask wallMask;
    SlidingPartical partical;

    [SerializeField]
    private bool direction = false; //true = ���������� �̵�, false = �������� �̵�

    private GameData gameData;

    private Animator animator;
    private bool firstJumpAble = true; //�÷��̾��� ���� ���� ���� üũ
    private bool doubleJumpAble = true; //�÷��̾��� ���� ���� ���� ���� üũ
    private bool isSlidingOnWall = false; //�÷��̾ ���� ����ִ��� ���� üũ
    private bool velocityInit = true;

    public float rayLength;
    public float rayLengthFloor;
    public float moveSpeed;
    public float jumpPower;
    public float slidingSpeed; //�����̵����� �������� �ӵ�

    public PlayerState state;

    private BoxCollider2D PlayerCollider;

    private int playerHealth;
    public int PlayerHealth
    {
        get { return playerHealth; }
        set { playerHealth = value; }
    }

    private Vector3 wallPos; //�浹�� ���� ��ġ ����

    private static BasicControler instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static BasicControler Instance
    {
        get
        {
            return instance;
        }
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        state = PlayerState.Move;
        playerHealth = 3;
        gameData = Resources.Load<GameData>("ScriptableObject/Datas");
        partical = GetComponent<SlidingPartical>();
        PlayerCollider = GetComponent<BoxCollider2D>();

        //if(partical == null)
        //    partical = this.AddComponent<SlidingPartical>();

        transform.position = gameData.SavePoint;
    }
    

    void Update()
    {
        JumpPlayer();

        
        

        if (state == PlayerState.Move)
        {
            PlayerCollider.offset = new Vector2(0.1f, 0.0f);
            PlayerCollider.size = new Vector2(5.0f, 3.2f);
            state = PlayerState.Move;
            animator.SetBool("Jump", false);
            animator.SetBool("Attach", false);
            MovePlayer();
        }
        else if (state == PlayerState.Jump)
        {
            PlayerCollider.offset = new Vector2(0.0f, 1.6f);
            PlayerCollider.size = new Vector2(2.0f, 1.8f);
            animator.SetBool("Attach", false);
            animator.SetBool("Jump", true);
            MovePlayer();
        }
        else if (state == PlayerState.Attach)
        {
            PlayerCollider.offset = new Vector2(0.0f, 0.0f);
            PlayerCollider.size = new Vector2(2.5f, 4.8f);
            animator.SetBool("Attach", true);
            animator.SetBool("Jump", false);
            
        }
        else if (state == PlayerState.Death)
        {
            animator.SetBool("Death", true);
            StartCoroutine(DeadCount());
            transform.position = gameData.SavePoint;

            if (playerHealth == 0)
            {
                SceneManager.LoadScene("GameOver");
            }
        }

        isSlidingOnWall = false;

        if(state != PlayerState.Attach)
            GetComponent<Rigidbody2D>().gravityScale = 1.0f;

        if(ObjectCheck() == 2)
        {
            state = PlayerState.Move;
        }

        //WallCheck();
        //FloorCheck();

        Debug.Log(state);
        
        velocityInit = true;
        
    }

    IEnumerator DeadCount()
    {
        yield return new WaitForSeconds(1.0f);
        yield return null;
    }

    private void MovePlayer()
    {
        
        //if (direction == true)
        //{
        //    transform.position += new Vector3(moveSpeed * Time.deltaTime, 0, 0);
        //    //rigidbodyPlayer.MovePosition(rigidbodyPlayer.position + Direction * speed * Time.deltaTime);
        //}
        //else if (direction == false) //변경사항 &&FloorCheck()
        //{
        //    transform.position -= new Vector3(moveSpeed * Time.deltaTime, 0, 0);
        //    //rigidbodyPlayer.MovePosition(rigidbodyPlayer.position + Direction * speed * Time.deltaTime);
        //}
        if (transform.localScale.x > 0)
        {
            transform.position -= new Vector3(moveSpeed * Time.deltaTime, 0, 0);
            //rigidbodyPlayer.MovePosition(rigidbodyPlayer.position + Direction * speed * Time.deltaTime);
        }
        else if (transform.localScale.x < 0) //변경사항 &&FloorCheck()
        {
            transform.position += new Vector3(moveSpeed * Time.deltaTime, 0, 0);
            //rigidbodyPlayer.MovePosition(rigidbodyPlayer.position + Direction * speed * Time.deltaTime);
        }
    }

    private void JumpPlayer()
    {
        if ((Input.GetKeyDown(KeyCode.Space)) && doubleJumpAble == true)
        {
            state = PlayerState.Jump;
            this.direction = true;

            GetComponent<Rigidbody2D>().gravityScale = 1;
            isSlidingOnWall = false;
            GetComponent<Rigidbody2D>().velocity = new Vector3(0, jumpPower, 0);

            if (firstJumpAble == false)
            {
                doubleJumpAble = false;
            }
            firstJumpAble = false;


        }
    }

    private void WallSliding()
    {
        if(velocityInit) { GetComponent<Rigidbody2D>().velocity = Vector3.zero; }

        velocityInit = false;
        isSlidingOnWall = true;

        GetComponent<Rigidbody2D>().gravityScale = slidingSpeed;
        if (partical.isParticleCycle == true)
            partical.SpwanParticle();
        

    }

    public void InitJump()
    {
        firstJumpAble = true;
        doubleJumpAble = true;
    }


    //public bool FloorCheck()
    //{
    //    Vector2 origin = this.transform.position;
    //    Vector2 direction = Vector2.down;

    //    RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, new Vector3(0.3f, 0.01f, 0), 0.0f, direction, rayLengthFloor);
        

    //    foreach (RaycastHit2D hit in hits)
    //    {
            
    //        if (hit.collider.CompareTag("Floor"))
    //        {
    //            Debug.Log("바닥");
    //            InitJump();
    //            this.direction = true;
    //            state = PlayerState.Move;
    //            return true;
    //        }
    //    }

    //    return false;
    //}

    private void OnDrawGizmos()
    {
        Vector2 origin = this.transform.position;
        Vector2 direction = Vector2.left;

        Gizmos.color = Color.yellow;
        RaycastHit2D hits = Physics2D.Raycast(origin, direction, rayLength);

        if(hits.collider != null)
        {
            Gizmos.DrawCube(hits.point, new Vector3(0.5f, 0.01f, 0));
        }
    }

    //public int WallCheck()
    //{
    //    Vector2 origin = this.transform.position;

    //    Vector2 direction = Vector2.right;
    //    RaycastHit2D hits = Physics2D.Raycast(origin, direction, rayLength, wallMask);

    //    if(hits.collider != null)
    //    {
    //        if (hits.collider.CompareTag("Wall"))
    //        {
    //            InitJump();

    //            wallPos = hits.collider.transform.position;
    //            Turn(wallPos);
    //            WallSliding();

    //            state = PlayerState.Attach;

    //            return 1;

    //        }

    //    }
    //    direction = Vector2.left;

    //    hits = Physics2D.Raycast(origin, direction, rayLength, wallMask);

    //    if(hits.collider != null)
    //    {
    //        if (hits.collider.CompareTag("Wall"))
    //        {
    //            InitJump();

    //            Turn(wallPos);
    //            WallSliding();

    //            state = PlayerState.Attach;
    //        }
    //    }

    //    return 0;
    //}

    public int ObjectCheck()
    {
        int result = 0;

        Vector2 originF = this.transform.position;
        Vector2 directionF = Vector2.down;

        RaycastHit2D[] hits = Physics2D.BoxCastAll(originF, new Vector3(0.3f, 0.01f, 0), 0.0f, directionF, rayLengthFloor);


        foreach (RaycastHit2D hit in hits)
        {

            if (hit.collider.CompareTag("Object"))
            {
                Debug.Log("바닥");
                InitJump();
                this.direction = true;
                state = PlayerState.Move;
                result = 2;
            }
        }


        Vector2 origin = this.transform.position;

        Vector2 direction = Vector2.right;
        RaycastHit2D hitss = Physics2D.Raycast(origin, direction, rayLength, wallMask);

        if (hitss.collider != null)
        {
            if (hitss.collider.CompareTag("Object"))
            {
                InitJump();

                wallPos = hitss.collider.transform.position;
                Turn(wallPos);
                WallSliding();

                state = PlayerState.Attach;

                result = 1;

            }

        }
        direction = Vector2.left;

        hitss = Physics2D.Raycast(origin, direction, rayLength, wallMask);

        if (hitss.collider != null)
        {
            if (hitss.collider.CompareTag("Object"))
            {
                InitJump();

                Turn(wallPos);
                WallSliding();

                state = PlayerState.Attach;
                result = 1;

            }
        }

        return result;
    }

    private void Turn(Vector3 wallPos)
    {
        Debug.Log("턴");
        //transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        if (wallPos.x < transform.position.x && direction == true)
        {
            direction = false;
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
            //transform.rotation = new Quaternion(0, 180, 0, 0);
        }
        else if (wallPos.x > transform.position.x && direction == true)
        {
            direction = false;
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
            //transform.rotation = new Quaternion(0, 0, 0, 0);
        }
    }

    public void PlayerHit()
    {
        playerHealth -= 1;
        state = PlayerState.Death;
    }

    private void PlayerDie()
    {
        if(playerHealth == 0)
        {
            SceneManager.LoadScene("GameOver");
        }
    }
    public void SetDir()
    {
       direction = !direction;
    }
    public bool getPrivateDir()
    {
        return direction;
    }
}
