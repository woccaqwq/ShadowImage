using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public float sensitivity=2, pitchLimit=80;
    float pitch;
    public bool PrepareInput(bool holding)
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(holding) return true;
            Release(); return false;
        }
        if(Cursor.lockState==CursorLockMode.Locked) return true;
        if(Input.GetMouseButtonDown(0)) Capture();
        return false;
    }
    public void Tick(Camera camera)
    {
        transform.Rotate(0,Input.GetAxisRaw("Mouse X")*sensitivity,0);
        pitch=Mathf.Clamp(pitch-Input.GetAxisRaw("Mouse Y")*sensitivity,-pitchLimit,pitchLimit);
        camera.transform.localRotation=Quaternion.Euler(pitch,0,0);
    }
    public void Capture() { Cursor.lockState=CursorLockMode.Locked; Cursor.visible=false; }
    public void Release() { Cursor.lockState=CursorLockMode.None; Cursor.visible=true; }
    void OnApplicationFocus(bool focus) { if(!focus) Release(); }
    void OnDisable() { Release(); }
}
