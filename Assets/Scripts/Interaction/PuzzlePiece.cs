using System.Collections.Generic;
using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    public string pieceType;
    public PuzzleSlot CurrentSlot { get; internal set; }
    public Vector3 SavedPosition { get; private set; }
    public Quaternion SavedRotation { get; private set; }
    public Vector3 OriginalScale { get; private set; }
    public Vector3[] LocalVertices { get; private set; }
    Collider[] colliders;
    bool[] colliderStates;
    public void Capture()
    {
        SavedPosition=transform.position; SavedRotation=transform.rotation; OriginalScale=transform.localScale;
        // Cache root-local geometry before presentation changes size or position.
        var points=new List<Vector3>();
        foreach(var filter in GetComponentsInChildren<MeshFilter>())
        {
            if(!filter.sharedMesh || !filter.sharedMesh.isReadable) { LocalVertices=null; return; }
            var matrix=transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;
            foreach(var v in filter.sharedMesh.vertices) points.Add(matrix.MultiplyPoint3x4(v));
        }
        LocalVertices=points.ToArray();
    }
    public void DisableCollision()
    {
        colliders=GetComponentsInChildren<Collider>(); colliderStates=new bool[colliders.Length];
        for(int i=0;i<colliders.Length;i++) { colliderStates[i]=colliders[i].enabled; colliders[i].enabled=false; }
    }
    public void RestoreCollision()
    {
        if(colliders!=null) for(int i=0;i<colliders.Length;i++) if(colliders[i]) colliders[i].enabled=colliderStates[i];
        colliders=null;
    }
    public void RestorePose() { transform.SetPositionAndRotation(SavedPosition,SavedRotation); transform.localScale=OriginalScale; }
    public Matrix4x4 DestinationMatrix(Vector3 position,Quaternion rotation)
    {
        var parent=transform.parent;
        return parent ? parent.localToWorldMatrix*Matrix4x4.TRS(parent.InverseTransformPoint(position),Quaternion.Inverse(parent.rotation)*rotation,OriginalScale)
            : Matrix4x4.TRS(position,rotation,OriginalScale);
    }
}
