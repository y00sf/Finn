using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Locks camera Z position and smoothly flips camera Y rotation
/// when the Follow target goes behind the camera
/// </summary>
[ExecuteInEditMode]
[SaveDuringPlay]
[AddComponentMenu("")]
public class LockCameraX : CinemachineExtension
{
    [Tooltip("Lock the camera's Z position to this value")]
    public float m_ZPosition = 10f;

    [Tooltip("Y rotation when target is in front of the camera")]
    public float frontYRotation = 0f;

    [Tooltip("Y rotation when target is behind the camera")]
    public float backYRotation = 180f;

    [Tooltip("Rotation smooth speed")]
    public float rotationLerpSpeed = 5f;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Body)
            return;

        // 1️⃣ Lock Z position
        var pos = state.RawPosition;
        pos.z = m_ZPosition;
        state.RawPosition = pos;

        // 2️⃣ Safety check
        if (vcam.Follow == null)
            return;

        float targetZ = vcam.Follow.position.z;
        float cameraZ = state.RawPosition.z;

        // 3️⃣ Decide target Y rotation
        float desiredY =
            targetZ < cameraZ ? backYRotation : frontYRotation;

        // 4️⃣ Smooth Lerp rotation
        Quaternion currentRot = state.RawOrientation;
        Quaternion targetRot = Quaternion.Euler(0f, desiredY, 0f);

        float lerpT = Application.isPlaying
            ? deltaTime * rotationLerpSpeed
            : 1f;

        state.RawOrientation =
            Quaternion.Slerp(currentRot, targetRot, lerpT);
    }
}