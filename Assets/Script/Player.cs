using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    public Vector2 inputVec; //OnMove 에서 받은 벡터
    public bool canJump = true; //점프 가능여부
    public float jumpPower = 10f;//점프힘
    Rigidbody2D rigid;
    public int speed = 10; //이동속도
    bool isRun;
    bool isJumping;
    bool isRight;

    public int JumpCount;
    public int MaxJumpCount;

    public SPUM_Prefabs PrefabsController;
    public PlayerSkillController playerSkillController;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>(); //리지드 바디 초기화
        JumpCount = MaxJumpCount; //초기 점프 초기화
        playerSkillController = GetComponent<PlayerSkillController>();

        PrefabsController.PopulateAnimationLists();
        PrefabsController.OverrideControllerInit();

    }
    void FixedUpdate()
    {
        Move();
        Jump();
        SetIdle();
    }
    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>(); //wasd 받아서 방향전달

        if (inputVec.x > 0 && !isRight) //오른쪽이동 + 오른쪽안보면
        {
            transform.Rotate(0, 180, 0); //방향돌리기 (오른쪽)
            isRight = true; //오른쪽 보는중
        }
        else if (inputVec.x < 0 && isRight) //왼쪽이동과 오른쪽보면
        {
            transform.Rotate(0, 180, 0);  //방향돌리기 (왼쪽)
            isRight = false; //왼쪽보는중
        }
    }

    void Move()
    {

        if (inputVec.y < 0) //아래 누르면
        {
            rigid.linearVelocity = new Vector2(inputVec.x * speed, inputVec.y * speed * 2); //아래로도 이동
            PrefabsController.PlayAnimation(PlayerState.MOVE, 0);

        }
        else
        {
            rigid.linearVelocity = new Vector2(inputVec.x * speed, rigid.linearVelocity.y); //좌우이동,위아래는 변동없음

            PrefabsController.PlayAnimation(PlayerState.MOVE, 0);

        }



    }
    void SetIdle()
    {
        if (inputVec.x == 0)
        { //좌우이동이 없다면 Idel로
            PrefabsController.GoIdleAnimation();
        }

    }

    void OnJump()
    {
        if ((inputVec.y < 0))
            return;
        if (!canJump) //점프 가 안되면 false  -> return 한다.
            return;

        isJumping = true; //점프해도돼



    }
    void Jump()
    {

        if (isJumping)
        { //ㅇㅋ 점프함
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, jumpPower); //점프
            JumpCount--; //점프 횟수 차감
            isJumping = false; //점프 끝
        }

        if (JumpCount == 0) //점프 횟수 소진
        {
            canJump = false; //점프 못하게
        }

    }

  

    void OnJSkill() //J키 입력
    {
         playerSkillController.UseJSkill();

      
    }
    void OnLSkill()
    {
       playerSkillController.UseLSkill();

        
    }
    void OnUSkill()
    {
      playerSkillController.UseUSkill();
      
      
    }
    void OnISkill()
    {
      playerSkillController.UseISkill();
       
        
    }
    void OnOSkill()
    {
     playerSkillController.UseOSkill();
   
        
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Floor")) //바닥검사
        {
            canJump = true; //점프가능
            JumpCount = MaxJumpCount;
        }
    }

}
