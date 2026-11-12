using UnityEngine;

public class PuzzleSlot : MonoBehaviour
{
    public PuzzlePiece allowedPiece;
    [Tooltip("Optional shared type for interchangeable pieces; empty uses allowedPiece.")]
    public string allowedType;
    public PuzzlePiece Occupant { get; private set; }
    public PuzzlePiece ReservedBy { get; private set; }
    public bool Accepts(PuzzlePiece piece) => string.IsNullOrEmpty(allowedType)?piece==allowedPiece:piece.pieceType==allowedType;
    public bool Available(PuzzlePiece piece) => (!Occupant || Occupant==piece) && (!ReservedBy || ReservedBy==piece);
    public bool Reserve(PuzzlePiece piece) { if(!Accepts(piece)||!Available(piece)) return false; ReservedBy=piece; return true; }
    public void Commit(PuzzlePiece piece)
    {
        if(ReservedBy!=piece) return;
        ReservedBy=null; Occupant=piece; piece.CurrentSlot=this;
    }
    public void Release(PuzzlePiece piece)
    {
        if(ReservedBy==piece) ReservedBy=null;
        if(Occupant==piece) Occupant=null;
        if(piece && piece.CurrentSlot==this) piece.CurrentSlot=null;
    }
    void OnDisable() { if(Occupant) Release(Occupant); ReservedBy=null; }
}
