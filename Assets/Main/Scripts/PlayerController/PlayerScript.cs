using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [Header("Player Movement")]
    public float movementSpeed = 5f;
    public float movementAmount;
    public MainCameraController MCC;
    public EnvironmentChecker environmentChecker;
    public float rotSpeed = 600f;
    Quaternion requiredRotation;
    bool playerControl = true;
    public bool playerInAction {get; private set;}

    [Header("Player Animator")]
    public Animator animator;

    [Header("Player Collision & Gravity")]
    public CharacterController CC;
    public float surfaceCheckRadius = 0.3f;
    public Vector3 surfaceCheckeOffset;
    public LayerMask surfaceLayer;
    public bool onSurface;
    public bool playerOnLedge {get; set;}
    public bool playerHanging {get; set;}

    public LedgeInfo LedgeInfo {get; set;}
    [SerializeField] float fallingSpeed;
    [SerializeField] Vector3 moveDir;
    [SerializeField] Vector3 requiredMoveDir; 
    Vector3 velocity;


    void Update()
    {
        if(!playerControl) return;

        if(playerHanging) return;

        velocity = Vector3.zero;

        if(onSurface)
        {
            fallingSpeed = 0f;
            velocity = moveDir * movementSpeed;

            playerOnLedge = environmentChecker.CheckLedge(moveDir, out LedgeInfo ledgeInfo);

            if(playerOnLedge)
            {
                LedgeInfo = ledgeInfo;
                PlayerLedgeMovement();
                
                Debug.Log("Player is on ledge");
            }

            animator.SetFloat("movementValue", velocity.magnitude / movementSpeed, 0.2f, Time.deltaTime);
        }
        else
        {
            fallingSpeed += Physics.gravity.y * Time.deltaTime;

            velocity = transform.forward * movementSpeed / 2;
        }

        
        velocity.y = fallingSpeed;

        PlayerMovement();

        PlayerRotation();

        SurfaceCheck();

        animator.SetBool("onSurface" , onSurface);
        Debug.Log("Player on Surface = " + onSurface);
    }

    void PlayerMovement()
    {
        // 入力処理
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        var movementInput = new Vector3(horizontal, 0, vertical).normalized;

        movementAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));

        // 入力とカメラの向きからプレイヤーの向きを算出
        requiredMoveDir = MCC.flatRotation * movementInput;

        // プレイヤーの移動
        CC.Move(velocity * Time.deltaTime);
       
        // locomotion用のアニメーションパラメータ
        animator.SetFloat("movementValue", movementAmount, 0.2f, Time.deltaTime);
    }

    void PlayerRotation()
    {
        // プレイヤーの回転角度を算出
        if(movementAmount > 0 && moveDir.magnitude > 0.2f)
        {
            requiredRotation = Quaternion.LookRotation(moveDir);
        }
        
        moveDir = requiredMoveDir;

        // プレイヤーを回転
        transform.rotation = Quaternion.RotateTowards(transform.rotation, requiredRotation, rotSpeed * Time.deltaTime);

    }

    void SurfaceCheck()
    {
        onSurface = Physics.CheckSphere(transform.TransformPoint(surfaceCheckeOffset), surfaceCheckRadius, surfaceLayer);
    }

    

     /// <summary>
    /// 端にいる場合に進行方向との角度が90度未満の場合は移動できないようにする
    /// </summary>
    void PlayerLedgeMovement()
    {
        float angle = Vector3.Angle(LedgeInfo.surfaceHit.normal, requiredMoveDir);

        if(angle < 90)
        {
            velocity = Vector3.zero;
            moveDir = Vector3.zero;
            
        }
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.TransformPoint(surfaceCheckeOffset), surfaceCheckRadius);
    }

    public IEnumerator PerformAction(string AnimationName, CompareTargetParameter ctp = null, Quaternion RequireRotation = new Quaternion(),
     bool LookAtObstacle = false, float ParkourActionDelay = 0f)
    {
        playerInAction = true;
        
        animator.CrossFadeInFixedTime(AnimationName, 0.2f);
        yield return null;

        var animationState = animator.GetNextAnimatorStateInfo(0);
        if (!animationState.IsName(AnimationName))
            Debug.Log("Animation Name is Incorrect");


        float rotateStartTime = ctp != null ? ctp.startTime : 0f;
        float timerCounter = 0f;
        while (timerCounter <= animationState.length)
        {
            timerCounter += Time.deltaTime;

            float normalizedTimeCounter = timerCounter / animationState.length;

            if (LookAtObstacle && normalizedTimeCounter > rotateStartTime)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, RequireRotation, rotSpeed * Time.deltaTime);
            }

            if (ctp != null)
            {
                CompareTarget(ctp);
            }

            if (animator.IsInTransition(0) && timerCounter > 0.5f)
            {
                break;
            }

            yield return null;
        }

        yield return new WaitForSeconds(ParkourActionDelay);

        
        playerInAction = false;
    }

    void CompareTarget(CompareTargetParameter compareTargetParameter)
    {
        animator.MatchTarget(compareTargetParameter.position, transform.rotation, compareTargetParameter.bodyPart,
            new MatchTargetWeightMask(compareTargetParameter.positionWeight, 0), compareTargetParameter.startTime, compareTargetParameter.endTime);
    }

    public void SetControl(bool hasControl)
    {
        this.playerControl = hasControl;
        CC.enabled = playerControl;

        if(!playerControl)
        {
            animator.SetFloat("movementValue", 0f);
            requiredRotation = transform.rotation;
        }

    }

    public void EnableCC(bool enabled)
    {
        CC.enabled = enabled;
    }

    public void ResetRequiredRotation()
    {
        requiredRotation = transform.rotation;
    }

    public bool HasPlayerControl
    {
        get => playerControl;
        set => playerControl = value;
    }

   
}

public class CompareTargetParameter
{
    public Vector3 position;
    public AvatarTarget bodyPart;
    public Vector3 positionWeight;
    public float startTime;
    public float endTime;

}