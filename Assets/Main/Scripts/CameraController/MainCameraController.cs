using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;


public class MainCameraController : MonoBehaviour
{
    public CinemachineVirtualCamera vcam;
    public float rotationY;
    
    void Update()
    {
        // カメラのstateを取得
        var state = vcam.State;

        // stateからquatarnionを取得
        var rotation = state.FinalOrientation;

        // オイラー角を取得
        var eular = rotation.eulerAngles;

        // Y-axis のオイラー角を取得
        rotationY = eular.y;

        // 
        var roundedRotationY = Mathf.RoundToInt(rotationY);
    }

    public Quaternion flatRotation => Quaternion.Euler(0, rotationY, 0);
    
}
