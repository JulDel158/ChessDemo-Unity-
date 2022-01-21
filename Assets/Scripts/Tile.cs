using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField]
    private Color baseColor, offsetColor;
    [SerializeField]
    private SpriteRenderer srenderer;
    [SerializeField]
    private GameObject highlight;

    public List<Tile> neighbors;

    public ChessPiece piece;

    [SerializeField]
    private uint x, y;

    public void SetPosition(uint _x, uint _y) {
        x = _x;
        y = _y;
    }

    public uint GetXpos() { return x; }

    public uint GetYpos() { return y; }

    public void Init(bool isOffset)
    {
        srenderer.color = isOffset ? offsetColor : baseColor;
    }

    public bool HasPiece()
    {
        return (piece && piece.pieceType != ChessPiece.TYPE.NONE);
    }

    public void Highlight(bool h)
    {
        highlight.SetActive(h);
    }

    public void highlightNeighbors(bool h)
    {
        highlight.SetActive(h);
        foreach (var n in neighbors)
        {
            n.highlight.SetActive(h);
        }
    }

    public bool isHighligted()
    {
        if (highlight != null)
            return highlight.activeSelf;

        return false;
    }

    private void OnMouseEnter()
    {
        //highlight.SetActive(true);
        //highlightNeighbors(true);
    }

    private void OnMouseExit()
    {
        // highlight.SetActive(false);
        // highlightNeighbors(false);
    }
}
