using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    public EnvironmentChecker environmentChecker;
    
    public Animator animator;
    public PlayerScript playerScript;
    [SerializeField] NewParkourAction jumpDownParkourAction;

    [Header("Parkour Action Area")]
    public List<NewParkourAction> newParkourAction;

    // 連続でジャンプできないようにタイマーで制御する
    private float JumpTimer;

    void Start()
    {
        JumpTimer = Time.time;
    }

    void Update()
    {
        if(playerScript.playerInAction || playerScript.playerHanging) return;


        if(Input.GetButton("Jump"))
        {
            var hitData = environmentChecker.CheckObstacle();

            if (hitData.hitFound)
            {
                foreach (var action in newParkourAction)
                {
                    if (action.ChekIfAvailable(hitData, transform))
                    {
                        // perform parkour action
                        StartCoroutine(PerformParkourAction(action));
                        return;
                    }
                }
            }
        }

        if (playerScript.playerOnLedge && Input.GetButtonDown("Jump"))
        {
            if(playerScript.LedgeInfo.angle <= 50 && Time.time - JumpTimer > 2f)
            {
                playerScript.playerOnLedge = false;
                JumpTimer = Time.time;
                StartCoroutine(PerformParkourAction(jumpDownParkourAction));
                return;
            }
        }

    }
    IEnumerator PerformParkourAction(NewParkourAction action)
    {
        playerScript.SetControl(false);

        CompareTargetParameter compareTargetParameter = null;
        if(action.AllowTargetMatching)
        {
            compareTargetParameter = new CompareTargetParameter()
            {
                position = action.ComparePosition,
                bodyPart = action.CompareBodyPart,
                positionWeight = action.ComparePositionWeight,
                startTime = action.CompareStartTime,
                endTime = action.CompareEndTime
            };
            
        }

        yield return playerScript.PerformAction(action.AnimationName, compareTargetParameter, action.RequireRotation, action.LookAtObstacle, action.ParkourActionDelay);

        playerScript.SetControl(true);
    }

}


