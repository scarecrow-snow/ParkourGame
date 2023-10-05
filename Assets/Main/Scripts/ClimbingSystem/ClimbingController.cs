using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Player;
using UnityEngine;

public class ClimbingController : MonoBehaviour
{
    EnvironmentChecker ec;
    PlayerScript ps;
    
    [SerializeField] ClimbingPoint currentClimbPoint;


    void Awake()
    {
        ec = GetComponent<EnvironmentChecker>();
        ps = GetComponent<PlayerScript>();
    }

    void Update()
    {
        if(ps.playerInAction) return;

        if (!ps.playerHanging)
        {
            if (Input.GetButton("Jump"))
            {
                if (ec.CheckClimbing(transform.forward, out RaycastHit climbInfo))
                {
                    currentClimbPoint = climbInfo.transform.GetComponent<ClimbingPoint>();

                    ps.SetControl(false);

                    StartCoroutine(ClimbToLedge("IdleToClimb", climbInfo.transform, 0.40f, 54f, playerHandOffset: new Vector3(-0.19f, -0.2f, 0.3f)));
                    return;
                }
            }

            if(Input.GetButton("Leave"))
            {
                if(ec.CheckDropClimbPoint(out RaycastHit DropHit))
                {
                    currentClimbPoint =  GetNearestClimbingPoint(DropHit.transform, DropHit.point);
                    if(currentClimbPoint != null)
                    {
                        ps.SetControl(false);
                        StartCoroutine(ClimbToLedge("DropToFreehang", currentClimbPoint.transform, 0.41f, 0.54f, playerHandOffset: new Vector3(-0.13f, -0.27f, 0.55f)));
                        return;
                    }
                }
            }
        }
        else
        {
            // 壁からのジャンプ
            if(Input.GetButton("Leave"))
            {
                StartCoroutine(JumpFromWall());
                return;
            }
            
            // 入力を受け取る
            float horizontal = Mathf.Round(Input.GetAxisRaw("Horizontal"));
            float vertical = Mathf.Round(Input.GetAxisRaw("Vertical"));

            var inputDirection = new Vector2(horizontal, vertical);

            if(ps.playerInAction || inputDirection == Vector2.zero) return;

            // 登頂部へ登る
            if(currentClimbPoint.mountPoint && Input.GetButton("Jump"))
            {
                if(inputDirection.y == 1)
                {
                    StartCoroutine(ClimbToTop());
                    return;
                }
            }


            // 接続しているneighbourへのクライミングジャンプでの移動または、シミーでの移動
            var neighbour = currentClimbPoint?.GetNeighbour(inputDirection);

            if(neighbour == null) return;

            // クライミングジャンプで移動
            if(neighbour.connectionType == ConnectionType.Jump && Input.GetButton("Jump"))
            {
                currentClimbPoint = neighbour.climbingPoint;

                if(neighbour.pointDirection.y == 1)
                {
                    StartCoroutine(ClimbToLedge("ClimbUp", currentClimbPoint.transform, 0.34f, 0.64f, playerHandOffset: new Vector3(0.22f, 0f, 0.309f)));
                }
                else if(neighbour.pointDirection.y == -1)
                {
                    StartCoroutine(ClimbToLedge("ClimbDown", currentClimbPoint.transform, 0.31f, 0.68f, playerHandOffset: new Vector3(0.12f, -0.07f, 0.29f)));
                }
                else if(neighbour.pointDirection.x == 1)
                {
                    StartCoroutine(ClimbToLedge("ClimbRight", currentClimbPoint.transform, 0.20f, 0.51f, playerHandOffset: new Vector3(0.11f, 0f, 0.262f)));
                }
                else if(neighbour.pointDirection.x == -1)
                {
                    // アニメーションの着地後の手の位置がおかしいため右ジャンプを反転して使用
                    //StartCoroutine(ClimbToLedge("ClimbLeft", currentClimbPoint.transform, 0.20f, 0.51f, playerHandOffset: new Vector3(0.08f, 0.04f, 0.33f)));
                    StartCoroutine(ClimbToLedge("ClimbLeft", currentClimbPoint.transform, 0.20f, 0.51f, AvatarTarget.LeftHand, playerHandOffset: new Vector3(0.11f, 0f, 0.262f)));
                
                }

                
            }
            // シミーで移動
            else if(neighbour.connectionType == ConnectionType.Move)
            {
                currentClimbPoint = neighbour.climbingPoint;

                if(neighbour.pointDirection.x == 1)
                {
                    StartCoroutine(ClimbToLedge("ShimmyRight", currentClimbPoint.transform, 0f, 0.30f, playerHandOffset: new Vector3(0.16f, 0.02f, 0.33f)));
                }
                else if(neighbour.pointDirection.x == -1)
                {
                    StartCoroutine(ClimbToLedge("ShimmyLeft", currentClimbPoint.transform, 0f, 0.30f, AvatarTarget.LeftHand, playerHandOffset: new Vector3(0.16f, 0.02f, 0.33f)));
                }
            }
        }


    }


    IEnumerator ClimbToLedge(string animationName, Transform ledgePoint, float compareStartTime, float compareEndTime,
     AvatarTarget hand = AvatarTarget.RightHand, Vector3? playerHandOffset = null)
    {
        var compareParams = new CompareTargetParameter()
        {
            position = SetHandPosition(ledgePoint, hand, playerHandOffset),
            bodyPart = hand,
            positionWeight = Vector3.one,
            startTime = compareStartTime,
            endTime = compareEndTime
        };

        var requireRot = Quaternion.LookRotation(-ledgePoint.forward);

        yield return ps.PerformAction(animationName, compareParams, requireRot, true);

        ps.playerHanging = true;
    }

    Vector3 SetHandPosition(Transform ledge, AvatarTarget hand, Vector3? playerhandOffset)
    {
        var offsetValue = playerhandOffset != null ? playerhandOffset.Value : Vector3.zero;
       
        var handDirection = hand == AvatarTarget.RightHand ? ledge.right : -ledge.right;        

        return ledge.position + ledge.forward * offsetValue.x + Vector3.up * offsetValue.y - handDirection * offsetValue.z;
    }

    IEnumerator JumpFromWall()
    {
        ps.playerHanging = false;

        yield return ps.PerformAction("JumpFromWall");

        
        ps.ResetRequiredRotation();
        ps.SetControl(true);
    }

    IEnumerator ClimbToTop()
    {
        ps.playerHanging = false;
        yield return ps.PerformAction("ClimbToTop");
        
        ps.EnableCC(true);

        yield return new WaitForSeconds(0.5f);

        ps.ResetRequiredRotation();
        ps.SetControl(true);
    }

    ClimbingPoint GetNearestClimbingPoint(Transform dropClimbPoint, Vector3 hitPoint)
    {
        var points = dropClimbPoint.GetComponentsInChildren<ClimbingPoint>();

        ClimbingPoint nearestPoint = null;
        float nearestPointDistance = Mathf.Infinity;

        foreach(var point in points)
        {
            float distance = Vector3.Distance(point.transform.position, hitPoint);
            if(distance < nearestPointDistance)
            {
                nearestPoint = point;
                nearestPointDistance = distance;
            }

        }

        return nearestPoint;
    }
}

