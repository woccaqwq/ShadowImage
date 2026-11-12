using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(CharacterController), typeof(PlayerLook))]
public class FirstPersonPlayer : MonoBehaviour
{
    public Camera view;
    public PuzzleInteraction interaction;
    public float moveSpeed=2.5f, eyeHeight=1.65f, gravity=20f;
    public bool AcceptInput { get; private set; }
    CharacterController body;
    float verticalSpeed;
    public PlayerLook look;
    void Awake()
    {
        body=GetComponent<CharacterController>();
        if(!look) look=GetComponent<PlayerLook>();
        body.height=1.8f; body.radius=.3f; body.center=new Vector3(0,.9f,0);
        body.stepOffset=.25f; body.skinWidth=.03f;
        if(!view) view=Camera.main;
        if(!interaction) interaction=FindObjectOfType<PuzzleInteraction>();
        if(!view) { enabled=false; return; }
        view.transform.SetParent(transform,false);
        view.transform.localPosition=new Vector3(0,eyeHeight,0);
        view.transform.localRotation=Quaternion.identity;
        view.nearClipPlane=.05f; view.fieldOfView=60;
        if(interaction) { interaction.view=view; interaction.player=this; }
    }
    void Start() { look.Capture(); }
    void Update()
    {
        AcceptInput=false;
        bool holding=interaction && interaction.IsManipulating;
        if(!look.PrepareInput(holding)) return;
        AcceptInput=true;
        look.Tick(view);
        var input=Vector3.ClampMagnitude(new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical")),1);
        if(body.isGrounded && verticalSpeed<0) verticalSpeed=-2;
        verticalSpeed-=gravity*Time.deltaTime;
        body.Move((transform.TransformDirection(input)*moveSpeed+Vector3.up*verticalSpeed)*Time.deltaTime);
    }
    void OnApplicationFocus(bool focus)
    {
        if(!focus) { AcceptInput=false; if(look) look.Release(); }
    }
    void OnDisable() { AcceptInput=false; if(look) look.Release(); }
}
