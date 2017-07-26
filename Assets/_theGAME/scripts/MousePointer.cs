using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// A Wrapper API for UnityEngine.Cursor.
/// <para>Use this the same as you would the UnityEngine.Cursor API, but enjoy the fact that locking
/// and unlocking the cursor will restore the mouse pointer to the position before it was locked.</para>
/// <para>Also provides a static property for reading and writing the mouse position.</para>
/// <para>See MSDN docs' GetCursorPos and SetCursorPos, as well as Unity3D docs' UnityEngine.Cursor</para>
/// </summary>
/// <example>
///   void Update(){
///       if(Input.GetMouseButtonDown(0)){ // if the left mouse button was just pressed, then:
///           // show fake cursor on screen where lock will be initiated, 
///           //   using Input.mousePosition to determine the placement in the window.
///           // *poof* -- example not shown.
///       } else if(Input.GetMouseButtonUp(0)) { // else if the left mouse button was just released, then:
///           // hide fake cursor on the screen.
///           // *poof* -- example not shown.
///       }
///   
///       bool lockMouse = Input.GetMouseButton(0); // is the left mouse button held?
///       
///       // initiate a mouse lock if left mouse button is held, release otherwise:
///       MousePointer.lockMode = lockMouse ? CursorLockMode.Locked : CursorLockMode.None; 
///       // releasing after locking using MousePointer.lockMode instead of Unity's Cursor.lockMode will
///       //   automatically move the cursor back to where it was just before the lock was initiated.
///       
///       // hide cursor because Unity will technically center the cursor on the screen while locked:
///       MousePointer.visible = !lockMouse;
///   }
/// </example>
internal class MousePointer {

    #region User32 Cursor Position Manipulation

    /// <summary>
    /// (Private) A structure that defines an x- and y- coordinate on the screen.
    /// Used exclusively to store a mouse's position coordinates.
    /// <para>See MSDN docs' POINT structure</para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct MousePosition {
        public int x;
        public int y;

        public MousePosition(int x, int y) {
            this.x = x;
            this.y = y;
        }
    }

    /// <summary>
    /// (Private) Moves the cursor to the specified screen coordinates.
    /// <para>See MSDN docs' SetCursorPos()</para>
    /// </summary>
    /// <param name="x">The new x-coordinate of the cursor.</param>
    /// <param name="y">The new y-coordinate of the cursor.</param>
    /// <returns>Returns true if successful, false otherwise.</returns>
    [DllImport("User32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    /// <summary>
    /// (Private) Moves the cursor to the specified screen coordinates.
    /// </summary>
    /// <param name="position">The new position of the cursor.</param>
    /// <returns>Returns true if successful, false otherwise.</returns>
    private static bool SetCursorPos(MousePosition position) {
        return SetCursorPos(position.x, position.y);
    }

    /// <summary>
    /// Retrieves the position of the mouse cursor, in screen coordinates.
    /// <para>See MSDN docs' GetCursorPos()</para>
    /// </summary>
    /// <param name="position">A pointer to a POINT structure that receives the screen coordinates of the cursor.</param>
    /// <returns>Returns true if successful or false otherwise.</returns>
    [DllImport("User32.dll")]
    private static extern bool GetCursorPos(out MousePosition position);

    #endregion User32 Cursor Position Manipulation

    #region Wrapper API for UnityEngine.Cursor

    /// <summary>
    /// (Private) Represents the last position the mouse was at before the cursor was locked.
    /// <para>Used for restoring the mouse to its former position when unlocking the cursor.</para>
    /// </summary>
    private static MousePosition lastMousePosition = new MousePosition { x = 0, y = 0 };

    /// <summary>Represents and controls the mouse's position on the screen as a Vector2.</summary>
    public static Vector2 position {
        get {
            MousePosition p;
            if (GetCursorPos(out p)) return new Vector2(p.x, p.y);
            return Vector2.zero;
        }
        set {
            MousePosition p = new MousePosition((int)value.x, (int)value.y);
            SetCursorPos(p);
        }
    }

    /// <summary>
    /// Determines whether the hardware pointer is visible or not.
    /// <para>See Unity3D docs' UnityEngine.Cursor.visible</para>
    /// </summary>
    public static bool visible {
        get { return Cursor.visible; }
        set { Cursor.visible = value; }
    }

    /// <summary>
    /// Determines whether the hardware pointer is locked to the center of the view, constrained to the window, or not constrained at all.
    /// <para>Restores the cursor to its position prior to being locked when you set the lockState to CursorLockMode.None.</para>
    /// <para>See Unity3D docs' UnityEngine.Cursor.lockState</para>
    /// </summary>
    public static CursorLockMode lockState {
        get { return Cursor.lockState; }
        set {
            switch (value) {
                case CursorLockMode.Locked:
                    if (Cursor.lockState == CursorLockMode.Locked) return;
                    Cursor.visible = false;
                    MousePosition position;
                    if (GetCursorPos(out position))
                        lastMousePosition = position;
                    Cursor.lockState = CursorLockMode.Locked;
                    break;

                default:
                    if (Cursor.lockState == CursorLockMode.Locked) {
                        Cursor.lockState = value;
                        Cursor.visible = false;
                        //lastMousePosition.x += (int)Input.GetAxisRaw("Mouse X");
                        //lastMousePosition.y += (int)-Input.GetAxisRaw("Mouse Y");
                        SetCursorPos(lastMousePosition);
                        Cursor.visible = true;
                    } else {
                        Cursor.lockState = value;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Specify a custom cursor that you wish to use as a cursor.
    /// <para>See Unity3D docs' UnityEngine.Cursor.SetCursor()</para>
    /// </summary>
    public static void SetCursor(Texture2D texture, Vector3 hotspot, CursorMode mode) {
        Cursor.SetCursor(texture, hotspot, mode);
    }

    #endregion Wrapper API for UnityEngine.Cursor
}
