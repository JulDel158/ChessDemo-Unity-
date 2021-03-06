using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    public enum TYPE {
        NONE = 0,
        PAWN,
        KNIGHT,
        BISHOP,
        ROOK,
        QUEEN,
        KING
    }

    [SerializeField]
    private bool isPlayer1 = true;


    public int value { get; private set; }

    public TYPE pieceType { set; get; } = TYPE.NONE;

    public void SetPlayer(bool player1) { isPlayer1 = player1; }

    public void SetValue(int v)
    {
        value = v;
    }

    public bool IsPlayer1 ()
    {
        return isPlayer1;
    }
}
