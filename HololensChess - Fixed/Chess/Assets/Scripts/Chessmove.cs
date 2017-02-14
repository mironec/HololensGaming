using System;
using System.Collections;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

public abstract class Chessmove : MonoBehaviour
{
    public int CurrentX { set; get; }
    public int CurrentY { set; get; }
    public bool isWhite;

    public void SetPosition(int x, int y)
    {
        CurrentX = x;
        CurrentY = y;
    }

    public virtual bool[,] PossibleMove()
    {
        return new bool[8,8]; 
    }
}
