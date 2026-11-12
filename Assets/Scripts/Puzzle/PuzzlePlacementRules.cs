using UnityEngine;

public class PuzzlePlacementRules : MonoBehaviour
{
    public Transform operationPlane;
    public Light puzzleLight;
    public Vector2 frameHalfSize=new Vector2(2,1.25f);
    public float positionTolerance=.2f, angleTolerance=15;
    public PuzzleSlot[] slots;
    public enum Placement { Outside, Free, Snap }
    public bool Ready => operationPlane && puzzleLight;
    public bool PlanePoint(Ray ray,float reach,out Vector3 point)
    {
        point=default;
        if(!Ready) return false;
        if(new Plane(operationPlane.forward,operationPlane.position).Raycast(ray,out float d) && d<=reach) { point=ray.GetPoint(d); return true; }
        return false;
    }
    public Placement Evaluate(PuzzlePiece piece,Vector3 position,Quaternion rotation,out PuzzleSlot candidate)
    {
        candidate=null;
        if(!WithinProjection(piece,position,rotation)) return Placement.Outside;
        float best=float.PositiveInfinity;
        if(slots!=null) foreach(var slot in slots)
        {
            if(!slot || !slot.Accepts(piece)||!slot.Available(piece)) continue;
            float distance=Vector3.ProjectOnPlane(position-slot.transform.position,operationPlane.forward).magnitude;
            if(distance<=positionTolerance && Quaternion.Angle(rotation,slot.transform.rotation)<=angleTolerance && distance<best)
            { best=distance; candidate=slot; }
        }
        return candidate?Placement.Snap:Placement.Free;
    }
    public bool WithinProjection(PuzzlePiece piece,Vector3 position,Quaternion rotation)
    {
        if(!Ready || piece.LocalVertices==null || piece.LocalVertices.Length==0) return false;
        var matrix=piece.DestinationMatrix(position,rotation);
        Vector3 origin=puzzleLight.transform.position,normal=operationPlane.forward;
        float numerator=Vector3.Dot(operationPlane.position-origin,normal);
        foreach(var vertex in piece.LocalVertices)
        {
            Vector3 ray=matrix.MultiplyPoint3x4(vertex)-origin;
            float denominator=Vector3.Dot(ray,normal);
            if(Mathf.Abs(denominator)<.00001f || numerator/denominator<=0) return false;
            Vector3 projected=origin+ray*(numerator/denominator)-operationPlane.position;
            if(Mathf.Abs(Vector3.Dot(projected,operationPlane.right))>frameHalfSize.x+.0001f || Mathf.Abs(Vector3.Dot(projected,operationPlane.up))>frameHalfSize.y+.0001f) return false;
        }
        return true;
    }
}
