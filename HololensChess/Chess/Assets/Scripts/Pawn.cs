using System.Collections;
using UnityEngine;

public class Pawn : Chessmove
{
    public override bool[,] PossibleMove()
    {
        bool[,] r = new bool[8, 8];
        Chessmove c, c2;
        int[] e = BoardManager.Instance.EnPassantMove;
        //white team move
        if (isWhite)
        {
            //Diagonal Left
            if(CurrentX !=0 && CurrentY !=7)
            {
                
                if (e[0] == CurrentX - 1 && e[1] == CurrentY + 1)
                    r[CurrentX - 1, CurrentY + 1] = true;


                c = BoardManager.Instance.Chessmoves[CurrentX - 1, CurrentY + 1];
                if (c != null && !c.isWhite)
                    r[CurrentX - 1, CurrentY + 1] = true;
            }

            //Diagonal Right
            if (CurrentX != 7 && CurrentY != 7)
            {
                if (e[0] == CurrentX + 1 && e[1] == CurrentY + 1)
                    r[CurrentX + 1, CurrentY + 1] = true;

                c = BoardManager.Instance.Chessmoves[CurrentX + 1, CurrentY + 1];
                if (c != null && !c.isWhite)
                    r[CurrentX + 1, CurrentY + 1] = true;
            }
            //Middle
            if(CurrentY !=7)
            {
                c = BoardManager.Instance.Chessmoves[CurrentX, CurrentY + 1];
                if (c == null)
                    r[CurrentX, CurrentY + 1] = true; 
            }
            //Middle on first move 
            if (CurrentY == 1)
            {
                c = BoardManager.Instance.Chessmoves[CurrentX, CurrentY + 1];
                c2 = BoardManager.Instance.Chessmoves[CurrentX, CurrentY + 2];
                if (c == null & c2 == null)
                    r[CurrentX, CurrentY + 2] = true;
            }
        } else
        {
            //Diagonal Left
            if (CurrentX != 0 && CurrentY != 0)
            {
                if (e[0] == CurrentX - 1 && e[1] == CurrentY - 1)
                    r[CurrentX - 1, CurrentY - 1] = true;

                c = BoardManager.Instance.Chessmoves[CurrentX - 1, CurrentY - 1];
                if (c != null && c.isWhite)
                    r[CurrentX - 1, CurrentY - 1] = true;
            }

            //Diagonal Right
            if (CurrentX != 7 && CurrentY != 0)
            {
                if (e[0] == CurrentX + 1 && e[1] == CurrentY - 1)
                    r[CurrentX + 1, CurrentY - 1] = true;

                c = BoardManager.Instance.Chessmoves[CurrentX + 1, CurrentY - 1];
                if (c != null && c.isWhite)
                    r[CurrentX + 1, CurrentY - 1] = true;
            }
            //Middle
            if (CurrentY != 0)
            {
                c = BoardManager.Instance.Chessmoves[CurrentX, CurrentY - 1];
                if (c == null)
                    r[CurrentX, CurrentY - 1] = true;
            }
            //Middle on first move 
            if (CurrentY == 6)
            {
                c = BoardManager.Instance.Chessmoves[CurrentX, CurrentY - 1];
                c2 = BoardManager.Instance.Chessmoves[CurrentX, CurrentY - 2];
                if (c == null & c2 == null)
                    r[CurrentX, CurrentY - 2] = true;
            }

        }
        return r; 
    }


}
