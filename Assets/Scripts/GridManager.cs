using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    GridManager(bool n, uint _width, uint _height)
    {
        setNeighbors = n;
        width = _width;
        height = _height;
    }

    [SerializeField]
    private bool setNeighbors;
    [SerializeField]
    private uint width, height;
    [SerializeField]
    private Tile tilePrefab;

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private Tile current;

    [SerializeField]
    private ChessPiece piecePrefab;

    private bool liftedFinger;

    // dynamic list that highlights and contains the tiles the currently selected piece can move to
    [SerializeField]
    private List<Tile> movableSpaces;

    Tile[,] tiles;

    void Start() {
        // todo, move this outside of this class
        Input.multiTouchEnabled = false;

        InitGrid();
        InitBoard();

        // moving camera position to match grid's dimmensions
        cam.transform.position = new Vector3((float)width / 2.0f - 0.5f, (float)height / 2.0f - 0.5f, -10.0f);

        liftedFinger = true;
    }

    private void InitBoard()
    {
        // temporary spawning a chess piece at (0, 0) tiles
        //tiles[0, 0].piece = Instantiate(piecePrefab, tiles[0, 0].transform);
        //tiles[0, 0].piece.pieceType = ChessPiece.TYPE.KING;
        //tiles[0, 0].piece.name = "KING1";

        //tiles[0, 1].piece = Instantiate(piecePrefab, tiles[0, 1].transform);
        //tiles[0, 1].piece.pieceType = ChessPiece.TYPE.PAWN;
        //tiles[0, 1].piece.name = "PAWN1";

        //tiles[6, 2].piece = Instantiate(piecePrefab, tiles[6, 2].transform);
        //tiles[6, 2].piece.pieceType = ChessPiece.TYPE.PAWN;
        //tiles[6, 2].piece.name = "PAWN2";
        //tiles[6, 2].piece.SetPlayer(false);

        List<ChessPiece.TYPE> set = new List<ChessPiece.TYPE>
        {
            ChessPiece.TYPE.ROOK, 
            ChessPiece.TYPE.KNIGHT,
            ChessPiece.TYPE.BISHOP,
            ChessPiece.TYPE.QUEEN,
            ChessPiece.TYPE.KING,
            ChessPiece.TYPE.BISHOP,
            ChessPiece.TYPE.KNIGHT,
            ChessPiece.TYPE.ROOK
        };

        int[] values = { 5, 3, 3, 9, 0, 3, 3, 5 };

        // Player 1
        uint y1 = 0, y2 = 1, y3 = height - 2, y4 = height - 1;
        for (uint x = 0; x < (uint)set.Count; ++x)
        {
            var pawn1 = getTileAt(x, y2);
            pawn1.piece = Instantiate(piecePrefab, pawn1.transform);
            pawn1.piece.SetPlayer(true);

            var pawn2 = getTileAt(x, y3);
            pawn2.piece = Instantiate(piecePrefab, pawn2.transform);
            pawn2.piece.SetPlayer(false);

            pawn1.piece.SetValue(1);
            pawn2.piece.SetValue(1);
            pawn1.piece.pieceType = pawn2.piece.pieceType = ChessPiece.TYPE.PAWN;

            var piece1 = getTileAt(x, y1);
            piece1.piece = Instantiate(piecePrefab, piece1.transform);
            piece1.piece.SetPlayer(true);

            var piece2 = getTileAt(x, y4);
            piece2.piece = Instantiate(piecePrefab, piece2.transform);
            piece2.piece.SetPlayer(false);

            piece1.piece.SetValue(values[x]);
            piece2.piece.SetValue(values[x]);
            piece1.piece.pieceType = piece2.piece.pieceType = set[(int)x];
        }
    }

    private void Update() {
        if (Input.touchCount > 0 && liftedFinger)
        {
            SelectPiece();

            liftedFinger = false;
        }
        else if (Input.touchCount == 0)
        {
            liftedFinger = true;
        }
    }

    // handles chess piece selection and deselection upon touch
    private void SelectPiece() {
        var touch = Input.GetTouch(0);

        Vector3 pos = Camera.main.ScreenToWorldPoint(touch.position);

        // de-selecting the last piece
        if (current) { 
            current.Highlight(false);
            foreach (var p in movableSpaces)
            {
                p.Highlight(false);
            }

            // before we clear we must move our piece if selecting a target distination
            // movableSpaces.Clear();
        } 

        var selected = getTileAt((uint)(pos.x + 0.5f), (uint)(pos.y + 0.5f)); // retriving the piece being touched
        if (movableSpaces.Contains(selected) && current.HasPiece()) {
            current.MovePieceTo(ref selected);
            current = null;
            selected = null;
        }
        movableSpaces.Clear();
        current = (selected == current) ? null : selected;

        // if this tile has a valid piece, highlight it
        if (current != null && current.HasPiece()) {
            current.Highlight(true);
        }

        HighlightPossibleMove();
    }

    // highlights all possible moves for a piece
    private void HighlightPossibleMove()
    {
        if (current == null || !current.isHighligted() || !current.HasPiece()) { return; }

        uint px = current.GetXpos();
        uint py = current.GetYpos();

        switch (current.piece.pieceType)
        {
            case ChessPiece.TYPE.PAWN:
                {
                    PawnMoveSet(px, py);
                    break;
                }
            case ChessPiece.TYPE.KNIGHT:
                {
                    KnightMoveSet(px, py);
                    break;
                }
            case ChessPiece.TYPE.BISHOP:
                {
                    BishopMoveSet(px, py);
                    break;
                }
            case ChessPiece.TYPE.ROOK:
                {
                    RookMoveSet(px, py);
                    break;
                }
            case ChessPiece.TYPE.QUEEN:
                {
                    // must move like the bishop and the rook
                    BishopMoveSet(px, py);
                    RookMoveSet(px, py);
                    break;
                }
            case ChessPiece.TYPE.KING:
                {
                    KingMoveSet(px, py);
                }
                break;
            default:
                break;
        }

        // highlighting all posible target and removing any invalid targets
        for (int i = 0; i < movableSpaces.Count; ++i)
        {
            var p = movableSpaces[i];
            if (p.HasPiece() && p.piece.IsPlayer1() == current.piece.IsPlayer1())
            {
                movableSpaces.RemoveAt(i--);
                continue;
            }

            p.Highlight(true);
        }
    }

    private void PawnMoveSet(in uint px, in uint py)
    {
        uint y = current.piece.IsPlayer1() ? py + 1 : py - 1;
        for (int x = (int)px - 1; x <= px + 1; ++x)
        {
            var curr = getTileAt((uint)x, y);
            if (curr == null) { continue; }

            if (x != px && curr.HasPiece())
            {
                movableSpaces.Add(curr);
            }
            else if (x == px)
            {
                movableSpaces.Add(curr);
            }
        }
    }

    private void KnightMoveSet(in uint px, in uint py)
    {
        var curr = getTileAt(px - 2, py - 1);
        if (curr) { movableSpaces.Add(curr); }
        curr = getTileAt(px - 2, py + 1);
        if (curr) { movableSpaces.Add(curr); }

        curr = getTileAt(px + 2, py - 1);
        if (curr) { movableSpaces.Add(curr); }
        curr = getTileAt(px + 2, py + 1);
        if (curr) { movableSpaces.Add(curr); }

        curr = getTileAt(px - 1, py - 2);
        if (curr) { movableSpaces.Add(curr); }
        curr = getTileAt(px + 1, py - 2);
        if (curr) { movableSpaces.Add(curr); }

        curr = getTileAt(px - 1, py + 2);
        if (curr) { movableSpaces.Add(curr); }
        curr = getTileAt(px + 1, py + 2);
        if (curr) { movableSpaces.Add(curr); }
    }

    private void BishopMoveSet(in uint px, in uint py)
    {
        Tile curr = null;
        // moving (/)
        for (uint y = py + 1, x = px + 1; y < height && x < width; ++y, ++x)
        {
            curr = getTileAt(x, y);
            movableSpaces.Add(curr);
            if (curr.HasPiece()) { break; }
        }

        for (int y = (int)py - 1, x = (int)px - 1; (y >= 0 && x >= 0); --y, --x)
        {
            curr = getTileAt((uint)x, (uint)y);
            movableSpaces.Add(curr);
            if (curr.HasPiece()) { break; }
        }

        // moving (\)
        for (int y = (int)py - 1, x = (int)px + 1; y >= 0 && x < width; --y, ++x)
        {
            curr = getTileAt((uint)x, (uint)y);
            movableSpaces.Add(curr);
            if (curr.HasPiece()) { break; }
        }

        for (int y = (int)py + 1, x = (int)px - 1; y < height && x >= 0; ++y, --x)
        {
            curr = getTileAt((uint)x, (uint)y);
            movableSpaces.Add(curr);
            if (curr.HasPiece()) { break; }
        }
    }

    private void RookMoveSet(in uint px, in uint py)
    {
        Tile curr = null;

        // moving along the x axist const y value
        for (uint x1 = px + 1; x1 < width; ++x1)
        {
            curr = getTileAt(x1, py);
            movableSpaces.Add(curr);
            if (curr.HasPiece()) { break; }
        }

        for (int x1 = (int)px - 1; x1 >= 0; --x1)
        {
            curr = getTileAt((uint)x1, py);
            movableSpaces.Add(curr);
            if (curr.HasPiece()) { break; }
        }

        // moving along the y axist const x value
        for (uint y1 = py + 1; y1 < height; ++y1)
        {
            curr = getTileAt(px, y1);
            movableSpaces.Add(curr);
            if (curr.HasPiece()) { break; }
        }

        for (int y1 = (int)py - 1; y1 >= 0; --y1)
        {
            curr = getTileAt(px, (uint)y1);
            movableSpaces.Add(curr);
            if (curr.HasPiece()) { break; }
        }
    }

    private void KingMoveSet(in uint px, in uint py)
    {
        for (uint y = (py > 0) ? py - 1 : py; y <= py + 1; ++y)
        {
            for (uint x = (px > 0) ? px - 1 : px; x <= px + 1; ++x)
            {
                if (x == px && y == py) { continue; }

                var curr = getTileAt(x, y);

                if (curr == null) { continue; }

                movableSpaces.Add(curr);
            }
        }
    }

    // sets all variables in the grid to false
    private void ClearGrid() {
        // instantiating tiles objects
        for (int y = 0; y < height; ++y) {
            for (int x = 0; x < width; ++x) {
                tiles[y, x].Highlight(false);
            }
        }
    }

    // initializes the grid
    private void InitGrid() {
        // allocating tiles
        tiles = new Tile[height, width];

        // instantiating tiles objects
        for (uint y = 0; y < height; ++y) {
            for (uint x = 0; x < width; ++x) {
                var curr = Instantiate(tilePrefab, new Vector3(x, y), Quaternion.identity);

                curr.name = $"Tile {x} {y}";
                curr.SetPosition(x, y);

                bool isOffset = (x + y) % 2 == 0;
                curr.Init(isOffset);

                tiles[y, x] = curr;
            }
        }

        // if this option is enabled we will create neighbor linkage between the tiles
        if (setNeighbors) { InitNeightbors(); }
    }

    // creates the neighboring dependencies for each tile
    private void InitNeightbors() {
        // linking neighbors
        int total = (int)(width * height);
        for (int n = 0; n < total; ++n) {
            int x = (int)(n % (int)width);
            int y = (int)(n / (int)width);

            for (int _y = (y > 0) ? y - 1 : y; _y <= y + 1; ++_y) {
                for (int _x = (x > 0) ? x - 1 : x; _x <= x + 1; ++_x) {
                    if ((_x < width && _y < height) && !(_x == x && _y == y)) {
                        tiles[y, x].neighbors.Add(tiles[_y, _x]);
                    }
                }
            }
        }
    }

    // returns tile at position x, y or null if invalid
    public Tile getTileAt(uint x = 0, uint y = 0) {
        return (x >= width || y >= height) ? null : tiles[y, x];
    }
}
