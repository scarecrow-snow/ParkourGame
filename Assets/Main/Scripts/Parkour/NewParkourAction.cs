using System.Collections;
using System.Collections.Generic;

using UnityEngine;


[CreateAssetMenu(menuName = "Parkour Menu/Create New Parkour Action")]
public class NewParkourAction : ScriptableObject
{
    [Header("Checking Obstacle height")]
    [SerializeField] string animationName;
    [SerializeField] float minimumHeight;
    [SerializeField] float maximumHeight;

    [Header("Rotating Player towards Obstacle")]
    [SerializeField] bool lookAtObstacle;
    public Quaternion RequireRotation {get; set;}

    [Header("Target Matching")]
    [SerializeField] bool allowTargetMatching = true;
    [SerializeField] AvatarTarget compareBodyPart;
    [SerializeField] float compareStartTime;
    [SerializeField] float compareEndTime;

    public Vector3 ComparePosition {get; set;}

    /// <summary>
    /// 対象の物体の高さがパルクールアクションを発動するための範囲内にあるかどうかを判定する
    /// </summary>
    /// <param name="hitData">対象物の情報</param>
    /// <param name="player">自身のTransform</param>
    /// <returns>範囲内の場合にtrueを返す</returns>
    public bool ChekIfAvailable(obstacleInfo hitData, Transform player)
    {
        float checkHeight = hitData.heightInfo.point.y - player.position.y;

        if(checkHeight < minimumHeight || checkHeight > maximumHeight)
            return false;

        if(lookAtObstacle)
        {
            RequireRotation = Quaternion.LookRotation(-hitData.hitInfo.normal);
        }

        if(allowTargetMatching)
        {
            ComparePosition = hitData.heightInfo.point;
        }

        return true;
    }

    public string AnimationName => animationName;
    public bool LookAtObstacle => lookAtObstacle;

    public bool AllowTargetMatching => allowTargetMatching;
    public AvatarTarget CompareBodyPart => compareBodyPart;
    public float CompareStartTime => compareStartTime;
    public float CompareEndTime => compareEndTime;
}
