using UnityEngine;

// Marks a small sphere on the tracked hand rig as a "hand point".
//
// Parent the sphere to the matching joint of the hand visual (for example
// L_Palm or R_IndexTip), set which hand and which point it is, and the
// GestureDetector on the rig will find it and use its position.
public class HandPoint : MonoBehaviour
{
    public enum Hand
    {
        Left,
        Right
    }

    public enum PointType
    {
        Wrist,
        Palm,
        ThumbTip,
        IndexTip,
        MiddleTip,
        RingTip,
        LittleTip,
        Other
    }

    public Hand hand = Hand.Left;
    public PointType pointType = PointType.Palm;

    [Tooltip("Draws a small sphere in the Scene view so the point is easy to see while placing it.")]
    public bool showGizmo = true;
    public float gizmoRadius = 0.01f;

    // A point counts as tracked while its object is active. The hand visuals
    // in the XRI hands rig usually switch off when a hand loses tracking.
    public bool IsTracked
    {
        get { return isActiveAndEnabled; }
    }

    public Vector3 Position
    {
        get { return transform.position; }
    }

    void OnDrawGizmos()
    {
        if (!showGizmo)
        {
            return;
        }

        Gizmos.color = hand == Hand.Left ? Color.cyan : Color.magenta;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
    }
}
