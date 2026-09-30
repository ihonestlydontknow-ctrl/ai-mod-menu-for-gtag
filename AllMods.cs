using UnityEngine;
using UnityEngine.XR;

public class AllMods : MonoBehaviour
{
    // ------------------------------------------------------------------
    // Rig Variables
    // ------------------------------------------------------------------
    public Rigidbody body;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Transform leftHandModel;
    public Transform rightHandModel;
    public Transform rigRoot;
 
    // ------------------------------------------------------------------
    // Mod Settings
    // ------------------------------------------------------------------
    public float flySpeed = 8f;
    public float flyBrake = 6f;
 
    public float armMultiplier = 1.6f;
    public Vector3 shoulderOffset = new Vector3(0.18f, -0.2f, 0f);
 
    public float speedMultiplier = 1.5f;
    public float maxHorizontalSpeed = 12f;
    public bool useBuiltInSpeedBoost = true;
 
    public static float SpeedMultiplier = 1f;
    
    // States
    bool flyOn, armsOn, speedOn;
    bool hadGravity;
    Vector3 lPos, rPos;
    Quaternion lRot, rRot;
 
    // ------------------------------------------------------------------
    // Unity Lifecycle Methods
    // ------------------------------------------------------------------
    void Awake()
    {
        if (head == null && Camera.main != null) head = Camera.main.transform;
    }
 
    void Start()
    {
        // Registers your buttons to the WristMenu template
        WristMenu.Register("Fly", SetFly);
        WristMenu.Register("Long Arms", SetLongArms);
        WristMenu.Register("Speed Boost", SetSpeedBoost);
    }

    void OnDestroy()
    {
        SpeedMultiplier = 1f;
    }
 
    // ------------------------------------------------------------------
    // Button Activation Callbacks
    // ------------------------------------------------------------------
    void SetFly(bool on)
    {
        flyOn = on;
        if (body == null) return;
        if (on) { hadGravity = body.useGravity; body.useGravity = false; }
        else body.useGravity = hadGravity;
    }
 
    void SetLongArms(bool on)
    {
        armsOn = on;
        if (on)
        {
            if (leftHandModel == null || rightHandModel == null) return;
            lPos = leftHandModel.localPosition;  lRot = leftHandModel.localRotation;
            rPos = rightHandModel.localPosition; rRot = rightHandModel.localRotation;
        }
        else
        {
            if (leftHandModel != null) leftHandModel.SetLocalPositionAndRotation(lPos, lRot);
            if (rightHandModel != null) rightHandModel.SetLocalPositionAndRotation(rPos, rRot);
        }
    }
 
    void SetSpeedBoost(bool on)
    {
        speedOn = on;
        SpeedMultiplier = on ? speedMultiplier : 1f;
    }
 
    // ------------------------------------------------------------------
    // Physics Loops & Operations
    // ------------------------------------------------------------------
    void FixedUpdate()
    {
        if (body == null) return;
 
        // Handles Flight Mod
        if (flyOn && rightHand != null)
        {
            Vector3 v = body.linearVelocity;
            // Calls our custom input checker passing 'true' for Right Hand
            if (Pressed(true, "primaryButton")) 
                v = rightHand.forward * flySpeed;
            else
                v = Vector3.MoveTowards(v, Vector3.zero, flyBrake * Time.fixedDeltaTime);
            body.linearVelocity = v;
        }
 
        // Handles Speed Boost Mod
        if (speedOn && useBuiltInSpeedBoost)
        {
            Vector3 v = body.linearVelocity;
            Vector3 h = new Vector3(v.x, 0f, v.z);
            if (h.magnitude > 0.5f)
            {
                h = Vector3.ClampMagnitude(h * (1f + (speedMultiplier - 1f) * 2f * Time.fixedDeltaTime), maxHorizontalSpeed);
                body.linearVelocity = new Vector3(h.x, v.y, h.z);
            }
        }
    }
 
    void LateUpdate()
    {
        // Handles Long Arms Mod
        if (armsOn && head != null)
        {
            ExtendArm(leftHand, leftHandModel, -1f);
            ExtendArm(rightHand, rightHandModel, 1f);
        }
    }
 
    void ExtendArm(Transform anchor, Transform model, float side)
    {
        if (anchor == null || model == null) return;
        Quaternion yaw = Quaternion.Euler(0f, head.eulerAngles.y, 0f);
        Vector3 shoulder = head.position + yaw * new Vector3(shoulderOffset.x * side, shoulderOffset.y, shoulderOffset.z);
        model.SetPositionAndRotation(shoulder + (anchor.position - shoulder) * armMultiplier, anchor.rotation);
    }

    // ------------------------------------------------------------------
    // Real Input Processing Function (Bypasses Generic Type Errors)
    // ------------------------------------------------------------------
    bool Pressed(bool isRightHand, string buttonName)
    {
        var devices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
        var characteristics = isRightHand ? UnityEngine.XR.InputDeviceCharacteristics.Right : UnityEngine.XR.InputDeviceCharacteristics.Left;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(characteristics, devices);

        if (devices.Count > 0)
        {
            var device = devices[0];
            if (buttonName == "primaryButton" && device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool primaryPressed))
            {
                return primaryPressed;
            }
            if (buttonName == "gripButton" && device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out bool gripPressed))
            {
                return gripPressed;
            }
        }
        return false;
    }
}