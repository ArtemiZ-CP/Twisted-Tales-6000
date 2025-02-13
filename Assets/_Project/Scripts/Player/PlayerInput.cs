using UnityEngine;

public static class PlayerInput
{
    public static float GetMouseScrollDelta()
    {
        return Input.mouseScrollDelta.y;
    }
}
