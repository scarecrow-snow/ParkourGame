using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbingController : MonoBehaviour
{
    EnvironmentChecker ec;
    PlayerScript ps;

    public float InOutValue;
    public float UpDownValue;
    public float LeftRightValue;

    void Awake()
    {
        ec = GetComponent<EnvironmentChecker>();
        ps = GetComponent<PlayerScript>();
    }

    void Update()
    {
        if (!ps.playerHanging)
        {
            if (Input.GetButton("Jump"))
            {
                if (ec.CheckClimbing(transform.forward, out RaycastHit climbInfo))
                {
                    ps.SetControl(false);
                    StartCoroutine(ClimbToLedge("IdleToClimb", climbInfo.transform, 0.40f, 54f));
                }
            }
        }
        else
        {
            // Ledge to Ledge ParkourAction
        }


    }


    IEnumerator ClimbToLedge(string animationName, Transform ledgePoint, float compareStartTime, float compareEndTime)
    {
        var compareParams = new CompareTargetParameter()
        {
            position = SetHandPosition(ledgePoint),
            bodyPart = AvatarTarget.RightHand,
            positionWeight = Vector3.one,
            startTime = compareStartTime,
            endTime = compareEndTime
        };

        var requireRot = Quaternion.LookRotation(-ledgePoint.forward);

        yield return ps.PerformAction(animationName, compareParams, requireRot, true);

        ps.playerHanging = true;
    }

    Vector3 SetHandPosition(Transform ledge)
    {
        InOutValue = -0.2f;
        UpDownValue = -0.09f;
        LeftRightValue = 0.15f;

        return ledge.position + ledge.forward * InOutValue + Vector3.up * UpDownValue - ledge.right * LeftRightValue;
    }
}
