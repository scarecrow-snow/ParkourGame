using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [Header("Player Movement")]
    public float movementSpeed = 5f;

    public MainCameraController MCC;

    public float rotSpeed = 600f;
    Quaternion requiredRotation;
    bool playerControl = true;

    [Header("Player Animator")]
    public Animator animator;

    [Header("Player Collision & Gravity")]
    public CharacterController CC;
    public float surfaceCheckRadius = 0.3f;
    public Vector3 surfaceChakeOffset;
    public LayerMask surfaceLayer;
    bool onSurface;
    [SerializeField] float fallingSpeed;
    [SerializeField] Vector3 moveDir;


    void Update()
    {
        PlayerMovement();

        if(!playerControl) return;

        if(onSurface)
        {
            fallingSpeed = 0f;
        }
        else
        {
            fallingSpeed += Physics.gravity.y * Time.deltaTime;
        }

        var velocity = moveDir * movementSpeed;
        velocity.y = fallingSpeed;

        
        SurfaceCheck();
        Debug.Log("Player on Surface = " + onSurface);
    }

    void PlayerMovement()
    {
        // 入力処理
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        float movementAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
        
        var movementInput = (new Vector3(horizontal, 0, vertical)).normalized;

        var movementDirection = MCC.flatRotation * movementInput;

        CC.Move(movementDirection * movementSpeed * Time.deltaTime);
            
        // 移動の値を更新
        if(movementAmount > 0)
        {
            requiredRotation = Quaternion.LookRotation(movementDirection);
        }
        
        movementDirection = moveDir;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, requiredRotation, rotSpeed * Time.deltaTime);

       
        // Animation
        animator.SetFloat("movementValue", movementAmount, 0.2f, Time.deltaTime);
    }

    void SurfaceCheck()
    {
        onSurface = Physics.CheckSphere(transform.TransformPoint(surfaceChakeOffset), surfaceCheckRadius, surfaceLayer);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.TransformPoint(surfaceChakeOffset), surfaceCheckRadius);
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
}