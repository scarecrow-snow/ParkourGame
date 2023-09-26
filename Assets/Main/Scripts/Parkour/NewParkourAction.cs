using System.Collections;
using System.Collections.Generic;

using UnityEngine;


[CreateAssetMenu(menuName = "Parkour Menu/Create New Parkour Action")]
public class NewParkourAction : ScriptableObject
{
    [SerializeField] string animationName;
    [SerializeField] float minimumHeight;
    [SerializeField] float maximumHeight;
    [SerializeField] bool lookAtObstacle;

    public Quaternion RequireRotation {get; set;}

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

        return true;
    }

    public string AnimationName => animationName;
    public bool LookAtObstacle => lookAtObstacle;
}
