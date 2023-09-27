using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    public EnvironmentChecker environmentChecker;
    bool playerInAction;
    public Animator animator;
    public PlayerScript playerScript;

    [Header("Parkour Action Area")]
    public List<NewParkourAction> newParkourAction;

    void Update()
    {
        if (Input.GetButton("Jump") && !playerInAction)
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
                        break;
                    }
                }
            }
        }
    }
    IEnumerator PerformParkourAction(NewParkourAction action)
    {
        playerInAction = true;
        playerScript.SetControl(false);


        animator.CrossFade(action.AnimationName, 0.2f);
        yield return null;

        var animationState = animator.GetNextAnimatorStateInfo(0);
        if(!animationState.IsName(action.AnimationName))
            Debug.Log("Animation Name is Incorrect");

        float timerCounter = 0f;
        while(timerCounter <= animationState.length)
        {
            timerCounter += Time.deltaTime;

            if(action.LookAtObstacle)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, action.RequireRotation, playerScript.rotSpeed * Time.deltaTime);
            }

            if(action.AllowTargetMatching)
            {
                CompareTarget(action);
            }

            yield return null;
        }
        
        playerScript.SetControl(true);
        playerInAction = false;
    }
    
    void CompareTarget(NewParkourAction action)
    {
        animator.MatchTarget(action.ComparePosition, transform.rotation, action.CompareBodyPart,
            new MatchTargetWeightMask(new Vector3(0, 1, 0), 0), action.CompareStartTime, action.CompareEndTime);
    }
    /*
    async UniTaskVoid PerformParkour(NewParkourAction action)
    {
        playerInAction = true;
        playerScript.SetControl(false);
        animator.CrossFade(action.AnimationName, 0.2f);
        await UniTask.Yield();

        var animationState = animator.GetNextAnimatorStateInfo(0);
        if(!animationState.IsName(action.AnimationName))
            Debug.Log("Animation Name is Incorrect");

        float timerCounter = 0f;
        while(timerCounter <= animationState.length)
        {
            timerCounter += Time.deltaTime;
            await UniTask.Yield();
        }

        playerScript.SetControl(true);
        playerInAction = false;
    }
    */
    



}


