using System.Collections;
using UnityEngine;
public class HeldItemPresentation : MonoBehaviour
{
    public Camera view;
    public Transform arm;
    public float pickupDuration=.3f, placementDuration=.3f, carriedScale=.28f;
    public Vector3 handOffset=new Vector3(.28f,.05f,.71f);
    [Min(0)] public float hoverAmplitude=.015f, hoverFrequency=.8f;
    PuzzlePiece piece;
    Vector3 armRest;
    float startedAt;
    bool animating;
    public Quaternion PlacementRotation { get; set; }
    Vector3 HandPosition => view.transform.TransformPoint(handOffset+Vector3.up*(Mathf.Sin((Time.time-startedAt)*Mathf.PI*2*Mathf.Max(0,hoverFrequency))*Mathf.Max(0,hoverAmplitude)));
    Quaternion HandRotation => view.transform.rotation*PlacementRotation;
    public void Begin(PuzzlePiece value) { piece=value; PlacementRotation=value.SavedRotation; startedAt=Time.time; if(arm) armRest=arm.localPosition; }
    void LateUpdate() { if(piece && !animating) { piece.transform.SetPositionAndRotation(HandPosition,HandRotation); piece.transform.localScale=piece.OriginalScale*Mathf.Max(.05f,carriedScale); } }
    void AnimateArm(float t) { if(arm) arm.localPosition=armRest+new Vector3(0,.035f,.14f)*Mathf.Sin(t*Mathf.PI); }
    public IEnumerator Pickup()
    {
        animating=true; var from=piece.transform.position; var rotation=piece.transform.rotation;
        float duration=Mathf.Max(.01f,pickupDuration);
        for(float t=0;t<duration;t+=Time.deltaTime) {
            float f=Mathf.SmoothStep(0,1,t/duration);
            piece.transform.SetPositionAndRotation(Vector3.Lerp(from,HandPosition,f),Quaternion.Slerp(rotation,HandRotation,f));
            piece.transform.localScale=Vector3.Lerp(piece.OriginalScale,piece.OriginalScale*Mathf.Max(.05f,carriedScale),f);
            AnimateArm(t/duration); yield return null;
        }
        if(arm) arm.localPosition=armRest; animating=false;
    }
    public IEnumerator Place(Vector3 destination,Quaternion rotation)
    {
        animating=true; var from=piece.transform.position; var r=piece.transform.rotation; var scale=piece.transform.localScale;
        float duration=Mathf.Max(.01f,placementDuration);
        for(float t=0;t<duration;t+=Time.deltaTime) {
            float f=Mathf.SmoothStep(0,1,t/duration);
            piece.transform.SetPositionAndRotation(Vector3.Lerp(from,destination,f),Quaternion.Slerp(r,rotation,f));
            piece.transform.localScale=Vector3.Lerp(scale,piece.OriginalScale,f); AnimateArm(t/duration); yield return null;
        }
        piece.transform.SetPositionAndRotation(destination,rotation); piece.transform.localScale=piece.OriginalScale; End();
    }
    public void End() { if(piece && arm) arm.localPosition=armRest; piece=null; animating=false; }
}
