using System.Collections;
using UnityEngine;

public class Bishop : Chessmove
{
    public override bool[,] PossibleMove()
    {
        bool[,] r = new bool[8, 8];

        Chessmove c;
        int i, j;

        //Top left 
        i = CurrentX;
        j = CurrentY;
        while(true)
        {
            i--;
            j++;
            if(i<0 || j>=8)
            {
                break;
            }
            c = BoardManager.Instance.Chessmoves[i, j];
            if (c == null)
                r[i, j] = true;
            else
            {
                if(isWhite != c.isWhite)
                {
                    r[i, j] = true;
                }
                break;
            }
        }

        //Top Right 
        i = CurrentX;
        j = CurrentY;
        while (true)
        {
            i++;
            j++;
            if (i >= 8 || j >= 8)
            {
                break;
            }
            c = BoardManager.Instance.Chessmoves[i, j];
            if (c == null)
                r[i, j] = true;
            else
            {
                if (isWhite != c.isWhite)
                {
                    r[i, j] = true;
                }
                break;
            }
        }

        //Bottom Left 
        i = CurrentX;
        j = CurrentY;
        while (true)
        {
            i--;
            j--;
            if (i < 0 || j < 0)
            {
                break;
            }
            c = BoardManager.Instance.Chessmoves[i, j];
            if (c == null)
                r[i, j] = true;
            else
            {
                if (isWhite != c.isWhite)
                {
                    r[i, j] = true;
                }
                break;
            }
        }

        //Bottom Right 
        i = CurrentX;
        j = CurrentY;
        while (true)
        {
            i++;
            j--;
            if (i >= 8 || j < 0)
            {
                break;
            }
            c = BoardManager.Instance.Chessmoves[i, j];
            if (c == null)
                r[i, j] = true;
            else
            {
                if (isWhite != c.isWhite)
                {
                    r[i, j] = true;
                }
                break;
            }
        }
        return r;
    }

}
