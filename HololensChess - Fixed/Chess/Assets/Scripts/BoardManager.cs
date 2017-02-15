using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour, IInputClickHandler {
    public static BoardManager Instance {set; get;}
    private bool[,] allowedMoves { set; get; }

    public Chessmove [,] Chessmoves {set; get;}
    private Chessmove selectedChessman;

    private const float TILE_SIZE = 1.0f;
    private const float PAWN_OFFSET = 0.246f;
    private const float TILE_OFFSET = 0.5f;


    public int selectionX = -1;
    public int selectionY = -1;

    public List<GameObject> chessmanPrefab;
    private List<GameObject> activeChessman;

    private Material previousMat;
    public Material selectedMat;

    public int[] EnPassantMove { set; get; }

    public bool isWhiteTurn = true; 

    private void Start()
    {
        Instance = this;
        SpawnAllChess();
    }

    private void Update()
    {
        DrawChessBoard();
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        Debug.Log("Clicked.");
        UpdateSelection();
        if (selectionX >= 0 && selectionY >= 0)
        {
            if (selectedChessman == null)
            {
                SelectChessman(selectionX, selectionY);
            }
            else
            {
                MoveChessman(selectionX, selectionY);
            }
        }
    }

    private void SelectChessman (int x, int y)
    {
        if (Chessmoves[x, y] == null)
            return;
        if (Chessmoves[x, y].isWhite != isWhiteTurn)
            return;

        bool hasAtleastOneMove = false;
        allowedMoves = Chessmoves[x, y].PossibleMove();
        for(int i=0; i<8; i++)
        {
            for(int j=0; j<8; j++)
            {
                if (allowedMoves[i, j])
                    hasAtleastOneMove = true; 
            }
        }

        allowedMoves = Chessmoves[x, y].PossibleMove();
        selectedChessman = Chessmoves[x, y];
        previousMat = selectedChessman.GetComponent<MeshRenderer>().material;
        selectedMat.mainTexture = previousMat.mainTexture;
        selectedChessman.GetComponent<MeshRenderer>().material = selectedMat;
        BoardHighlights.Instance.HighlightAllowedMoves(allowedMoves);
    }

    private void MoveChessman(int x, int y)
    {
        if(allowedMoves[x,y])
        {
            Chessmove c = Chessmoves[x, y];
            if(c !=null && c.isWhite != isWhiteTurn)
            {
                
                //if its the king 
                if(c.GetType() == typeof(King))
                {
                    EndGame();
                    return;
                }
                //Destroyed A peice 
                activeChessman.Remove(c.gameObject);
                Destroy(c.gameObject);
            }
            if(x == EnPassantMove[0] && y == EnPassantMove[1])
            {
                if (isWhiteTurn)
                {
                    c = Chessmoves[x, y-1];
                    activeChessman.Remove(c.gameObject);
                    Destroy(c.gameObject);

                }
                else
                {
                    c = Chessmoves[x, y + 1];
                    activeChessman.Remove(c.gameObject);
                    Destroy(c.gameObject);
                }
            }
            EnPassantMove[0] = -1;
            EnPassantMove[1] = -1;
            if (selectedChessman.GetType() == typeof(Pawn))
            {
                if(y == 7)
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    SpawnChess(9, x, y);
                    selectedChessman = Chessmoves[x, y];
                }
                else if (y == 0)
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    SpawnChess(3, x, y);
                    selectedChessman = Chessmoves[x, y];
                }
                if (selectedChessman.CurrentY == 1 && y == 3)
                {
                    EnPassantMove[0] = x;
                    EnPassantMove[1] = y -1;
                }
                else if (selectedChessman.CurrentY == 6 && y== 4)
                {
                    EnPassantMove[0] = x;
                    EnPassantMove[1] = y +1;
                }
    
            }

            Debug.Log("Moving");
            Chessmoves[selectedChessman.CurrentX, selectedChessman.CurrentY] = null;
            selectedChessman.transform.position = GetTileCenter(x, y);
            selectedChessman.SetPosition(x, y);
            Chessmoves[x, y] = selectedChessman;
            isWhiteTurn = !isWhiteTurn; 
        }

        selectedChessman.GetComponent<MeshRenderer>().material = previousMat;
        BoardHighlights.Instance.Hidehighlights();
        selectedChessman = null;

    } 

    private void UpdateSelection()
    {
        if (!Camera.main)
            return;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 25.0f, LayerMask.GetMask("ChessPlane"))) {
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
        }
        else
        {
            selectionX = -1;
            selectionY = -1;
        }

    }

    private void SpawnChess(int index, int x, int y)
    {
        GameObject go = Instantiate(chessmanPrefab[index], GetTileCenter(x,y), Quaternion.Euler(-90, -90, 0)) as GameObject;
        go.transform.SetParent(transform);
        Chessmoves[x, y] = go.GetComponent<Chessmove>();
        Chessmoves[x, y].SetPosition(x, y);
        activeChessman.Add(go);
    }

    private void SpawnChess2(int index, int x, int y)
    {
        GameObject go = Instantiate(chessmanPrefab[index], GetTileCenter(x, y), Quaternion.Euler(-90, 90, 0)) as GameObject;
        go.transform.SetParent(transform);
        Chessmoves[x, y] = go.GetComponent<Chessmove>();
        Chessmoves[x, y].SetPosition(x, y);
        activeChessman.Add(go);
    }

    private void SpawnAllChess()
    {
        activeChessman = new List<GameObject>();
        Chessmoves = new Chessmove[8,8];
        EnPassantMove = new int[2] { -1, -1 };

        //BlackTeam
        //Rooks
        SpawnChess(6, 0, 0);
        SpawnChess(6, 7, 0);
        //Knight
        SpawnChess(7, 1, 0);
        SpawnChess(7, 6, 0);
        //Bishop
        SpawnChess(8, 2, 0);
        SpawnChess(8, 5, 0);
        //Queen
        SpawnChess(9, 3, 0);
        //King
        SpawnChess(10, 4, 0);
        //Pawn
        for (int i= 0; i< 8; i++)
        {
            SpawnChess(11, i, 1);
        }

        //WhiteTeam
        //Rooks
        SpawnChess(0, 0, 7);
        SpawnChess(0, 7, 7);
        //Knight
        SpawnChess2(1, 1, 7);
        SpawnChess2(1, 6, 7);
        //Bishop
        SpawnChess2(2, 2, 7);
        SpawnChess2(2, 5, 7);
        //Queen
        SpawnChess(3, 3, 7);
        //King
        SpawnChess(4, 4, 7);
        //Pawn
        for (int i = 0; i < 8; i++)
        {
            SpawnChess(5, i, 6);
        }

    }

    private Vector3 GetTileCenter(int x, int y)
    {
        Vector3 origin = Vector3.zero;
        origin.x += (TILE_SIZE * x) + TILE_OFFSET;
        origin.z += (TILE_SIZE * y) + TILE_OFFSET;
        return origin;
    }

    private Vector3 GetTileCenterPawn(int x, int y)
    {
        Vector3 origin = Vector3.zero;
        origin.x += (TILE_SIZE * x) + TILE_OFFSET;
        origin.z += (TILE_SIZE * y) + TILE_OFFSET;
        origin.y += PAWN_OFFSET;
        return origin;
    }

    private void DrawChessBoard()
    {
        Vector3 widthLine = Vector3.right * 8;
        Vector3 heightLine = Vector3.forward * 8;

        for(int i=0; i<=8; i++)
        {
            Vector3 start = Vector3.forward * i;
            Debug.DrawLine(start, start + widthLine);
            for (int j=0; j<=8; j++)
            {
                start = Vector3.right * j;
                Debug.DrawLine(start, start + heightLine);
            }
        }

        //Draw the selction 
        if (selectionX >=0 && selectionY >=0)
        {
            Debug.DrawLine(
                Vector3.forward * selectionY + Vector3.right * selectionX,
                Vector3.forward * (selectionY + 1) + Vector3.right * (selectionX + 1));

            Debug.DrawLine(
                Vector3.forward * (selectionY +1) + Vector3.right * selectionX,
                Vector3.forward * selectionY  + Vector3.right * (selectionX + 1));
        }

    }

    private void EndGame()
    {
        if (isWhiteTurn)
            Debug.Log("White Team Wins");
        else
            Debug.Log("Black Team Wins");

        foreach (GameObject go in activeChessman)
            Destroy(go);

        isWhiteTurn = true;
        BoardHighlights.Instance.Hidehighlights();
        SpawnAllChess();
        
    }
}
