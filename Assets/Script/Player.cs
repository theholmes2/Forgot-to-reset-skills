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

    private void Awake()
    {
      rigid = GetComponent<Rigidbody2D>(); //리지드 바디 초기화
    }

    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>(); //wasd 받아서 방향전달
       

    }

    void FixedUpdate()
    {

        if (inputVec.y < 0) //아래 누르면
        { 
            rigid.linearVelocity = new Vector2(inputVec.x * speed, inputVec.y * speed * 2); //아래로도 이동
        }
        else
        {
            rigid.linearVelocity = new Vector2(inputVec.x * speed, rigid.linearVelocity.y); //좌우이동,위아래는 변동없음
        }

    }

    void OnJump()
    {
        if (!canJump) //점프 가 안되면 false  -> return 한다.
            return;

        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, jumpPower); //점프
        canJump = false; //점프 못하게

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Floor")) //바닥검사
        {
            canJump = true; //점프가능
        }
    }
    
}
