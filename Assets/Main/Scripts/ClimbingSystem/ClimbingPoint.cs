using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class ClimbingPoint : MonoBehaviour
{
    public List<Neighbour> neighbours;

    void Awake()
    {
        var twoWayClimbNeighbour = neighbours.Where(n => n.isPointTwoWay);
        foreach(var neighbour in twoWayClimbNeighbour)
        {
            neighbour.climbingPoint?.CreatePointConnection(this, -neighbour.pointDirection, neighbour.connectionType, neighbour.isPointTwoWay);
        }
    }

    public void CreatePointConnection(ClimbingPoint climbingPoint, Vector2 pointDirection, ConnectionType connectionType, bool isPointTwoWay)
    {
        var neighbour = new Neighbour()
        {
            climbingPoint = climbingPoint,
            pointDirection = pointDirection,
            connectionType = connectionType,
            isPointTwoWay = isPointTwoWay
        };

        if(!neighbours.Contains(neighbour))
        {
            neighbours.Add(neighbour);
        }
        
    }

    public Neighbour GetNeighbour(Vector2 climbDirection)
    {
        Neighbour neighbour = null;

        if(climbDirection.y != 0)
        {
            neighbour = neighbours.FirstOrDefault(n => n.pointDirection.y == climbDirection.y);
        }

        if(climbDirection.x != 0)
        {
            neighbour = neighbours.FirstOrDefault(n => n.pointDirection.x == climbDirection.x);
        }

        return neighbour;
    }

    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, transform.forward, Color.blue);

        foreach(var neighbour in neighbours)
        {
            if(neighbour.climbingPoint != null)
            {
                Debug.DrawLine(transform.position, neighbour.climbingPoint.transform.position, neighbour.isPointTwoWay ? Color.green : Color.black);
            }
        }
    }
}

[System.Serializable]
public class Neighbour
{
    public ClimbingPoint climbingPoint;
    public Vector2 pointDirection;

    public ConnectionType connectionType;
    public bool isPointTwoWay = true;

    // override object.Equals
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        
        var temp = (Neighbour)obj;
        return climbingPoint.Equals(temp.climbingPoint) && pointDirection == temp.pointDirection && connectionType == temp.connectionType && isPointTwoWay == temp.isPointTwoWay;
        
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public enum ConnectionType
{
    Jump,
    Move
}