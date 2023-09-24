using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    public EnvironmentChecker environmentChecker;

    void Update()
    {
        var hitData = environmentChecker.CheckObstacle();

        if(hitData.hitFound)
        {
            Debug.Log("Object Founded "+ hitData.hitInfo.transform.name);
        }
    }

    
}


