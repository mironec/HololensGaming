using System.Collections;
using UnityEngine;

public class King : Chessmove
{
    public override bool[,] PossibleMove()
    {
        bool[,] r = new bool[8, 8];

        Chessmove c;
        int i, j;

        //top side 
        i = CurrentX - 1;
        j = CurrentY + 1;
        if (CurrentY !=7)
        {
            for(int k=0; k<3; k++)
            {
                if (i>=0 || i< 8)
                {
                    c = BoardManager.Instance.Chessmoves[i, j];
                    if (c == null)
                        r[i, j] = true;
                    else if (isWhite != c.isWhite)
                        r[i, j] = true; 
                }
                i++;
            }
        }

        //down side 
        i = CurrentX - 1;
        j = CurrentY - 1;
        if (CurrentY != 0)
        {
            for (int k = 0; k < 3; k++)
            {
                if (i >= 0 || i < 8)
                {
                    c = BoardManager.Instance.Chessmoves[i, j];
                    if (c == null)
                        r[i, j] = true;
                    else if (isWhite != c.isWhite)
                        r[i, j] = true;
                }
                i++;
            }
        }

        //middleleft
        if(CurrentX !=0)
        {
            c = BoardManager.Instance.Chessmoves[CurrentX - 1, CurrentY];
            if (c == null)
                r[CurrentX - 1, CurrentY] = true;
            else if (isWhite != c.isWhite)
                r[CurrentX - 1, CurrentY] = true;
        }

        //middleright
        if (CurrentX != 7)
        {
            c = BoardManager.Instance.Chessmoves[CurrentX + 1, CurrentY];
            if (c == null)
                r[CurrentX + 1, CurrentY] = true;
            else if (isWhite != c.isWhite)
                r[CurrentX + 1, CurrentY] = true;
        }

        return r;
    }

}
