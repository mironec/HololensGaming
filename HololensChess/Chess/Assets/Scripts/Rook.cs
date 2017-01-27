using System.Collections;
using UnityEngine;

public class Rook : Chessmove
{
    public override bool[,] PossibleMove()
    {
        bool[,] r = new bool[8, 8];

        Chessmove c;
        int i;

        //Right 
        i = CurrentX;
        while(true)
        {
            i++;
            if (i >= 8)
                break;

            c = BoardManager.Instance.Chessmoves[i, CurrentY];
            if (c == null)
                r[i, CurrentY] = true;
            else
            {
                if (c.isWhite != isWhite)
                    r[i, CurrentY] = true;

                break;
            }
        }

        //Left 
        i = CurrentX;
        while (true)
        {
            i--;
            if (i <0)
                break;

            c = BoardManager.Instance.Chessmoves[i, CurrentY];
            if (c == null)
                r[i, CurrentY] = true;
            else
            {
                if (c.isWhite != isWhite)
                    r[i, CurrentY] = true;

                break;
            }
        }

        //Up 
        i = CurrentY;
        while (true)
        {
            i++;
            if (i >= 8)
                break;

            c = BoardManager.Instance.Chessmoves[CurrentX, i];
            if (c == null)
                r[CurrentX, i] = true;
            else
            {
                if (c.isWhite != isWhite)
                    r[CurrentX, i] = true;

                break;
            }
        }

        //Down 
        i = CurrentY;
        while (true)
        {
            i--;
            if (i < 0)
                break;

            c = BoardManager.Instance.Chessmoves[CurrentX, i];
            if (c == null)
                r[CurrentX, i] = true;
            else
            {
                if (c.isWhite != isWhite)
                    r[CurrentX, i] = true;

                break;
            }
        }

        return r;
    }
}
