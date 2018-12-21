using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(Animator))]
public class Player : MonoBehaviour
{
    //[SerializeField] [Range(0.1f, 2f)] float rotateSpeed = 1.0f;
    [SerializeField] [Range(0.1f, 20f)] float walkSpeed = 10.0f;
    [SerializeField] [Range(0.1f, 10f)] float jumpVelocity = 6.0f;
    [SerializeField] [Range(0.1f, 10f)] float avoidVelocity = 6.0f;
    public LayerMask groundLayer = 1;

    float m_avoidVelocity = 6.0f;
    float angulerVelocity = 1.0f;
    Vector2 horizontalVelocity;
    Vector3 targetDirection;
    bool isJumping = false;
    bool isAttack = false;
    bool isAvoiding = false;

    //private bool isTurbo = false;

    void Start()
    {
        InputManager.Instance.OnActionEvent.AddListener(delegate (ActionType type) {
            //print("OnActionEvent: " + type.ToString());
            switch (type)
            {
                case ActionType.Attack:
                    Attack();
                    break;
                case ActionType.Avoid:
                    Avoid();
                    break;
                case ActionType.Jump:
                    Jump();
                    break;
                default:
                    break;
            }
        });
    }

    void Update()
    {
        horizontalVelocity.x = Input.GetAxis("Horizontal");
        horizontalVelocity.y = Input.GetAxis("Vertical");

        //if (isTurbo == false)
        //{
        //    if (Input.GetKeyDown(KeyCode.T))
        //    {
        //        Turbo();
        //    }
        //}
    }

    void FixedUpdate()
    {
        //float speedScale = 1;
        //if (isTurbo == true)
        //{
        //    speedScale = 2;
        //}

        JumpUpdate();
        AvoidUpdate();

        UpdateDirection();
        UpdateLocomotion();
        UpdateAnimation();
    }

    //移動
	private void UpdateLocomotion()
    {
        if (isAttack) return;
        if (isAvoiding) return;

        float speed = Mathf.Abs(horizontalVelocity.x) + Mathf.Abs(horizontalVelocity.y);
        speed = Mathf.Clamp01(speed);

        if (isJumping)
        {
            speed *= 0.5f;
        }

        if (speed > 0f)
        {
            Vector3 velocity = transform.forward * walkSpeed * speed;
            velocity.y = _rigidbody.velocity.y;
            _rigidbody.velocity = velocity;
        }
    }

    //回転
    private void UpdateDirection()
    {
        if (!Camera.main.transform) return;
        if (isAvoiding) return;
        if (isJumping) return;
        if (isAttack) return;

        var forward = Camera.main.transform.forward;
        var right = Camera.main.transform.right;
        forward.y = 0;
        targetDirection = horizontalVelocity.x * right + horizontalVelocity.y * forward;

        if (horizontalVelocity != Vector2.zero && targetDirection.magnitude > 0.1f)
        {
            var lookDirection = targetDirection.normalized;
            var targetRotation = Quaternion.LookRotation(lookDirection, transform.up);
            var euler = transform.eulerAngles;
            euler.y = targetRotation.eulerAngles.y;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(euler), angulerVelocity * Time.fixedDeltaTime);
        }
    }

    //アニメーションの切り替え
    private void UpdateAnimation()
    {
        if (isJumping)
        {
            animator.SetFloat("VerticalVelocity", _rigidbody.velocity.y);
        }
        else
        {
            float speed = Mathf.Abs(horizontalVelocity.x) + Mathf.Abs(horizontalVelocity.y);
            speed = Mathf.Clamp01(speed);
            animator.SetFloat("Speed", speed, 0.01f, Time.deltaTime);
        }
    }

    //private void Turbo()
    //{
    //    isTurbo = true;
    //    StartCoroutine(DelayMethod(2.0f, () => {
    //        isTurbo = false;
    //    }));
    //}

    //--------------------------------------------------
    //回避

    private void Avoid()
    {
        if (isJumping) return;
        if (isAttack) return;
        if (isAvoiding) return;

        isAvoiding = true;
        m_avoidVelocity = avoidVelocity;
        animator.SetBool("IsAvoiding", isAvoiding);

        if (horizontalVelocity.magnitude == 0)
        {
            m_avoidVelocity = -avoidVelocity/2;
            animator.SetBool("IsBackStep", true);
        }

        StartCoroutine(DelayMethod(0.4f, () => {
            AvoidEnd();
        }));
    }

    public void AvoidUpdate()
    {
        if (isAvoiding) {
            Vector3 force = transform.forward * m_avoidVelocity;
            force.y = _rigidbody.velocity.y;
            _rigidbody.AddForce(force, ForceMode.Impulse);
        }
    }

    public void FootR()
    {
        print("FootR");
    }

    public void FootL()
    {
        print("FootL");
    }

    public void AvoidEnd()
    {
        isAvoiding = false;
        animator.SetBool("IsBackStep", false);
        animator.SetBool("IsAvoiding", isAvoiding);
    }

    //--------------------------------------------------
    //攻撃

    private void Attack()
    {
        if (isJumping) return;
        if (isAttack) return;
        if (isAvoiding) return;

        isAttack = true;
        animator.SetBool("IsAttack", isAttack);
    }

    public void Hit()
    {
        print("Attack -> Hit");
        AttackEnd();
    }

    public void AttackCancel()
    {
        print("Attack -> Cancel");
        AttackEnd();
    }

    public void AttackEnd()
    {
        isAttack = false;
        animator.SetBool("IsAttack", isAttack);
    }

    //--------------------------------------------------
    //ジャンプ

    private void Jump()
    {
        if (isJumping) return;
        if (isAttack) return;
        if (isAvoiding) return;

        //ジャンプアクション
        _rigidbody.AddForce(Vector3.up * jumpVelocity, ForceMode.Impulse);
        isJumping = true;
        animator.SetBool("IsJumping", isJumping);
    }

   private void JumpUpdate()
    {
        if (!isJumping) return;

        //地面に近づいたらジャンプ終了
        if (_rigidbody.velocity.y < 0 && IsGround())
        {
            isJumping = false;
            animator.SetBool("IsJumping", isJumping);
        }
    }

    //--------------------------------------------------

    //地面と接しているかどうか
    bool IsGround()
    {
        if (GetGroundDistance() > 0.05f)
        {
            return false;
        } else {
            return true;
        }
    }

    //地面との距離を取得する
    float GetGroundDistance()
    {
        var distance = 10f;
        var layLength = 10f;
        RaycastHit groundHit;

        // スフィアを飛ばして足場との距離を取る
        Ray ray = new Ray(transform.position + Vector3.up * (_capsuleCollider.radius), Vector3.down);
        if (Physics.SphereCast(ray, _capsuleCollider.radius * 0.9f, out groundHit, _capsuleCollider.radius + layLength, groundLayer))
        {
            distance = (groundHit.distance - _capsuleCollider.radius * 0.1f);
        }

        return (float)System.Math.Round(distance, 2);
    }

    //--------------------------------------------------

    private CapsuleCollider m_CapsuleCollider;
    public CapsuleCollider _capsuleCollider
    {
        get
        {
            if (m_CapsuleCollider == null) m_CapsuleCollider = GetComponent<CapsuleCollider>();
            return m_CapsuleCollider;
        }
    }

    private Rigidbody m_Rigidbody;
    public Rigidbody _rigidbody
    {
        get
        {
            if (m_Rigidbody == null) m_Rigidbody = GetComponent<Rigidbody>();
            return m_Rigidbody;
        }
    }

    private Animator m_Animator;
    public Animator animator
    {
        get
        {
            if (m_Animator == null) m_Animator = GetComponent<Animator>();
            return m_Animator;
        }
    }

    //--------------------------------------------------

    private IEnumerator DelayMethod(float waitTime, Action action)
    {
        yield return new WaitForSeconds(waitTime);
        action();
    }
}