using UnityEngine;

public class InteractionHUD : MonoBehaviour
{
    public PuzzleInteraction interaction;
    void OnGUI()
    {
        if(!interaction || !interaction.isActiveAndEnabled) return;
        var p=interaction.Pointer; var old=GUI.color;
        GUI.color=interaction.IsManipulating?Color.yellow:interaction.Selected?Color.green:Color.white;
        GUI.Label(new Rect(p.x-6,Screen.height-p.y-10,24,24),"+"); GUI.color=old;
        string message=interaction.player && !interaction.player.AcceptInput?"Click to capture mouse. Esc releases mouse when empty-handed.":interaction.Message;
        GUI.Box(new Rect(12,12,620,55),"WASD: move  Mouse: look  Click: pick up / put down  Q/E & wheel: rotate\n"+message);
    }
}
