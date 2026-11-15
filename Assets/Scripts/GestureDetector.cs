using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Detects body gestures by comparing distances between HandPoints and the
// player's head and chest. Put this on the player rig (MR Interaction Setup).
//
// Gestures:
//   Prayer         - palms together and middle fingertips together, in front of the chest.
//   Hands Raised   - both wrists above the head.
//   Hands To Chest - both palms on or near the chest.
//
// Each gesture has Started / Ended events that can be wired up in the
// Inspector (for example to FogRaiser.StartRaisingFog and FogRaiser.StopFog).
public class GestureDetector : MonoBehaviour
{
    [Serializable]
    public class Gesture
    {
        [Tooltip("True while the gesture is being held.")]
        public bool isActive;

        public UnityEvent onStarted = new UnityEvent();
        public UnityEvent onEnded = new UnityEvent();

        [HideInInspector] public float heldTime;
        [HideInInspector] public float releasedTime;
    }

    [Header("Body")]
    [Tooltip("The headset camera. Left empty, the Main Camera is used.")]
    public Transform head;

    [Tooltip("How far below the head the chest point sits, in meters.")]
    public float chestDrop = 0.3f;

    [Tooltip("How far in front of the head the chest point sits, in meters.")]
    public float chestForward = 0.1f;

    [Tooltip("Where to look for HandPoints. Left empty, this object and its children are searched.")]
    public Transform handPointRoot;

    [Header("Timing")]
    [Tooltip("Seconds a pose must be held before the gesture starts.")]
    public float holdTime = 0.3f;

    [Tooltip("Seconds a pose can drop out before the gesture ends. Stops flicker.")]
    public float releaseTime = 0.2f;

    [Header("Prayer")]
    public float prayerPalmDistance = 0.08f;
    public float prayerFingerDistance = 0.08f;
    [Tooltip("How far the joined hands may be from the chest point.")]
    public float prayerChestRange = 0.45f;
    public Gesture prayer = new Gesture();

    [Header("Hands Raised")]
    [Tooltip("How far above the head both wrists must be.")]
    public float raiseHeightAboveHead = 0.1f;
    public Gesture handsRaised = new Gesture();

    [Header("Hands To Chest")]
    [Tooltip("How close each palm must be to the chest point.")]
    public float chestRadius = 0.2f;
    public Gesture handsToChest = new Gesture();

    // Lookup of hand points by hand and type, filled in Start.
    readonly Dictionary<(HandPoint.Hand, HandPoint.PointType), HandPoint> points =
        new Dictionary<(HandPoint.Hand, HandPoint.PointType), HandPoint>();

    Vector3 chestPosition;

    void Start()
    {
        if (head == null && Camera.main != null)
        {
            head = Camera.main.transform;
        }

        if (head == null)
        {
            Debug.LogWarning("GestureDetector: no head camera found.", this);
            enabled = false;
            return;
        }

        FindHandPoints();
    }

    // Collects every HandPoint under handPointRoot, including inactive ones
    // (hands are often untracked when the scene starts). Call again if hand
    // points are added at runtime.
    public void FindHandPoints()
    {
        points.Clear();
        Transform root = handPointRoot != null ? handPointRoot : transform;
        foreach (HandPoint point in root.GetComponentsInChildren<HandPoint>(true))
        {
            var key = (point.hand, point.pointType);
            if (points.ContainsKey(key) && point.pointType != HandPoint.PointType.Other)
            {
                Debug.LogWarning("GestureDetector: more than one " + point.hand + " " + point.pointType + " hand point. Using the first one.", point);
                continue;
            }
            points[key] = point;
        }
    }

    void Update()
    {
        chestPosition = GetChestPosition(head);

        bool isPraying = CheckPrayer();
        bool isRaised = CheckHandsRaised();
        // Joined hands in front of the chest can also be close to the chest,
        // so prayer takes priority over hands-to-chest.
        bool isOnChest = !isPraying && CheckHandsToChest();

        UpdateGesture(prayer, isPraying);
        UpdateGesture(handsRaised, isRaised);
        UpdateGesture(handsToChest, isOnChest);
    }

    Vector3 GetChestPosition(Transform h)
    {
        // Use the flat forward direction so looking up or down does not move the chest.
        Vector3 flatForward = Vector3.ProjectOnPlane(h.forward, Vector3.up);
        if (flatForward.sqrMagnitude < 0.0001f)
        {
            flatForward = Vector3.ProjectOnPlane(h.up, Vector3.up);
        }
        flatForward.Normalize();

        return h.position + Vector3.down * chestDrop + flatForward * chestForward;
    }

    bool CheckPrayer()
    {
        if (!TryGetBoth(HandPoint.PointType.Palm, out Vector3 leftPalm, out Vector3 rightPalm) ||
            !TryGetBoth(HandPoint.PointType.MiddleTip, out Vector3 leftTip, out Vector3 rightTip))
        {
            return false;
        }

        Vector3 handsCenter = (leftPalm + rightPalm) * 0.5f;

        return Vector3.Distance(leftPalm, rightPalm) < prayerPalmDistance
            && Vector3.Distance(leftTip, rightTip) < prayerFingerDistance
            && Vector3.Distance(handsCenter, chestPosition) < prayerChestRange;
    }

    bool CheckHandsRaised()
    {
        if (!TryGetBoth(HandPoint.PointType.Wrist, out Vector3 leftWrist, out Vector3 rightWrist))
        {
            return false;
        }

        float minHeight = head.position.y + raiseHeightAboveHead;
        return leftWrist.y > minHeight && rightWrist.y > minHeight;
    }

    bool CheckHandsToChest()
    {
        if (!TryGetBoth(HandPoint.PointType.Palm, out Vector3 leftPalm, out Vector3 rightPalm))
        {
            return false;
        }

        return Vector3.Distance(leftPalm, chestPosition) < chestRadius
            && Vector3.Distance(rightPalm, chestPosition) < chestRadius;
    }

    // Gets the left and right positions of one point type, if both are tracked.
    bool TryGetBoth(HandPoint.PointType type, out Vector3 left, out Vector3 right)
    {
        left = Vector3.zero;
        right = Vector3.zero;

        if (!points.TryGetValue((HandPoint.Hand.Left, type), out HandPoint leftPoint) ||
            !points.TryGetValue((HandPoint.Hand.Right, type), out HandPoint rightPoint) ||
            !leftPoint.IsTracked || !rightPoint.IsTracked)
        {
            return false;
        }

        left = leftPoint.Position;
        right = rightPoint.Position;
        return true;
    }

    void UpdateGesture(Gesture gesture, bool poseIsHeld)
    {
        if (poseIsHeld)
        {
            gesture.heldTime += Time.deltaTime;
            gesture.releasedTime = 0f;

            if (!gesture.isActive && gesture.heldTime >= holdTime)
            {
                gesture.isActive = true;
                gesture.onStarted.Invoke();
            }
        }
        else
        {
            gesture.heldTime = 0f;

            if (gesture.isActive)
            {
                gesture.releasedTime += Time.deltaTime;
                if (gesture.releasedTime >= releaseTime)
                {
                    gesture.isActive = false;
                    gesture.releasedTime = 0f;
                    gesture.onEnded.Invoke();
                }
            }
        }
    }

    // Shows the chest point and the gesture ranges in the Scene view.
    void OnDrawGizmosSelected()
    {
        Transform h = head;
        if (h == null && Camera.main != null)
        {
            h = Camera.main.transform;
        }
        if (h == null)
        {
            return;
        }

        Vector3 chest = GetChestPosition(h);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(chest, chestRadius);
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(chest, prayerChestRange);
        Gizmos.color = Color.green;
        Vector3 raiseLine = h.position + Vector3.up * raiseHeightAboveHead;
        Gizmos.DrawLine(raiseLine - h.right * 0.3f, raiseLine + h.right * 0.3f);
    }
}
