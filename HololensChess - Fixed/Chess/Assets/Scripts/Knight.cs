using System.Collections;
using UnityEngine;

public class Knight : Chessmove
{
    public override bool[,] PossibleMove()
    {
        bool[,] r = new bool[8, 8];
        //Upleft 
        KnightMove(CurrentX - 1, CurrentY + 2, ref r);

        //Upright
        KnightMove(CurrentX + 1, CurrentY + 2, ref r);

        //Rightup
        KnightMove(CurrentX + 2, CurrentY + 1, ref r);

        //Rightdown
        KnightMove(CurrentX + 2, CurrentY -1, ref r);

        //Downleft 
        KnightMove(CurrentX - 1, CurrentY - 2, ref r);

        //Downright
        KnightMove(CurrentX + 1, CurrentY - 2, ref r);
        
        //Leftup
        KnightMove(CurrentX - 2, CurrentY + 1, ref r);

        //Leftdown
        KnightMove(CurrentX - 2, CurrentY - 1, ref r);


        return r;
    }

    public void KnightMove(int x, int y, ref bool[,] r)
    {
        Chessmove c;
        if(x>=0 && x<8 && y>=0 && y<8)
        {
            c = BoardManager.Instance.Chessmoves[x, y];
            if (c == null)
                r[x, y] = true;
            else if (isWhite != c.isWhite)
                r[x, y] = true;
        }

    }

}
