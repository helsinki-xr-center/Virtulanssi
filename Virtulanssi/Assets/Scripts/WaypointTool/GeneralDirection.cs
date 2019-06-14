using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeneralDirection
{
    public static Vector3 DirectionVector(Directions dir)
    {
        Vector3 d = Vector3.zero;
        switch(dir)
        {
            case Directions.North:
                d = Vector3.forward;
                break;
            case Directions.West:
                d = Vector3.left;
                break;
            case Directions.South:
                d = Vector3.back;
                break;
            case Directions.East:
                d = Vector3.right;
                break;
            case Directions.NorthWest:
                d = new Vector3(-1f, 0f, 1f).normalized;
                break;
            case Directions.SouthWest:
                d = new Vector3(-1f, 0f, -1f).normalized;
                break;
            case Directions.SouthEast:
                d = new Vector3(1f, 0f, -1f).normalized;
                break;
            case Directions.NorthEast:
                d = new Vector3(1f, 0f, 1f).normalized;
                break;
        }
        return d;
    }

    public static Vector3  DirectionRight(Vector3 direction)
    {
        Vector3 right = new Vector3(direction.z, direction.y, -direction.x).normalized;
        return right;
    }

    public static Vector3 DirectionLeft(Vector3 direction)
    {
        Vector3 left = new Vector3(-direction.z, direction.y, direction.x).normalized;
        return left;
    }

}
