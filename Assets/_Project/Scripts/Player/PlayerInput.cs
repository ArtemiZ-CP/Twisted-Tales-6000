using UnityEngine;

public static class PlayerInput
{
    public static float GetMouseScrollDelta()
    {
        return Input.mouseScrollDelta.y;
    }

    public static bool SwitchUI()
    {
        return Input.GetKeyDown(KeyCode.H);
    }
}
