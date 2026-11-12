using System.Collections;
using UnityEngine;
public class PuzzleInteraction : MonoBehaviour
{
    public Camera view;
    public FirstPersonPlayer player;
    public PuzzlePlacementRules rules;
    public HeldItemPresentation presentation;
    public float rotationStep=90, rotationSpeed=360, reach=20;
    public enum InteractionState { Empty, PickingUp, Holding, Placing }
    public InteractionState State { get; private set; }
    public bool IsManipulating => State!=InteractionState.Empty;
    public Vector3 Pointer => player?new Vector3(Screen.width*.5f,Screen.height*.5f,0):Input.mousePosition;
    public PuzzlePiece Selected { get; private set; }
    public string Message { get; private set; }
    PuzzlePiece held;
    PuzzleSlot originalSlot, reservedSlot;
    Quaternion desiredRotation;
    Coroutine action;
    void Start() {
        if(!view || !rules || !rules.Ready || !presentation) { Debug.LogError("Assign camera, placement rules and held-item presentation.",this); enabled=false; return; }
        presentation.view=view;
        if(!player) { Cursor.lockState=CursorLockMode.None; Cursor.visible=true; }
    }
    void Update()
    {
        if(player && !player.AcceptInput) return;
        if(Input.GetKeyDown(KeyCode.Escape) && held) {
            if(State==InteractionState.Holding) BeginPlace(held.SavedPosition,held.SavedRotation,true);
            else CancelImmediate();
            return;
        }
        if(State==InteractionState.PickingUp || State==InteractionState.Placing) return;
        if(State==InteractionState.Empty) {
            Selected=null;
            if(Physics.Raycast(view.ScreenPointToRay(Pointer),out var hit,reach,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore)) Selected=hit.transform.GetComponentInParent<PuzzlePiece>();
            if(Input.GetMouseButtonDown(0) && Selected) BeginPickup(); return;
        }
        if(Input.GetKeyDown(KeyCode.Q)) Rotate(rules.operationPlane.up,-rotationStep);
        if(Input.GetKeyDown(KeyCode.E)) Rotate(rules.operationPlane.up,rotationStep);
        float scroll=Input.mouseScrollDelta.y;
        if(Mathf.Abs(scroll)>.01f) Rotate(rules.operationPlane.right,Mathf.Sign(scroll)*rotationStep);
        presentation.PlacementRotation=Quaternion.RotateTowards(presentation.PlacementRotation,desiredRotation,Mathf.Max(1,rotationSpeed)*Time.deltaTime);
        if(!rules.PlanePoint(view.ScreenPointToRay(Pointer),reach,out var point)) {
            Message="Look back at the operation plane; click to return the piece";
            if(Input.GetMouseButtonDown(0)) BeginPlace(held.SavedPosition,held.SavedRotation,true); return;
        }
        var result=rules.Evaluate(held,point,presentation.PlacementRotation,out var slot);
        Message=result==PuzzlePlacementRules.Placement.Outside?"Outside frame - click to return":slot?"Click to put into slot":"Click to put at crosshair";
        if(!Input.GetMouseButtonDown(0)) return;
        if(result==PuzzlePlacementRules.Placement.Outside) BeginPlace(held.SavedPosition,held.SavedRotation,true);
        else {
            if(slot && !slot.Reserve(held)) return;
            reservedSlot=slot;
            BeginPlace(slot?slot.transform.position:point,slot?slot.transform.rotation:presentation.PlacementRotation,false);
        }
    }
    void Rotate(Vector3 axis,float degrees) { desiredRotation=Quaternion.AngleAxis(degrees,axis)*desiredRotation; }
    void BeginPickup() {
        held=Selected; held.Capture(); originalSlot=held.CurrentSlot;
        if(originalSlot) originalSlot.Release(held);
        held.DisableCollision(); desiredRotation=held.SavedRotation; presentation.Begin(held);
        State=InteractionState.PickingUp; Message="Picking up"; action=StartCoroutine(Pickup());
    }
    IEnumerator Pickup() { yield return presentation.Pickup(); State=InteractionState.Holding; action=null; }
    void BeginPlace(Vector3 destination,Quaternion rotation,bool restore) {
        State=InteractionState.Placing; Message=restore?"Returning":"Putting down"; action=StartCoroutine(Place(destination,rotation,restore));
    }
    IEnumerator Place(Vector3 destination,Quaternion rotation,bool restore) {
        yield return presentation.Place(destination,rotation);
        if(restore && originalSlot && originalSlot.Reserve(held)) originalSlot.Commit(held);
        else if(!restore && reservedSlot) reservedSlot.Commit(held);
        Finish();
    }
    public void CancelImmediate() {
        if(action!=null) StopCoroutine(action);
        if(presentation) presentation.End();
        if(held) {
            if(reservedSlot) reservedSlot.Release(held);
            held.RestorePose();
            if(originalSlot && originalSlot.Reserve(held)) originalSlot.Commit(held);
        }
        Finish();
    }
    void Finish() {
        if(held) held.RestoreCollision(); held=null; Selected=null; originalSlot=null; reservedSlot=null;
        State=InteractionState.Empty; Message=""; action=null;
    }
    void OnDisable() { CancelImmediate(); }
}
