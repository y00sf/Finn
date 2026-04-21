using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class FishingMiniGame : MiniGameBase, IDifficultyScaler
{
    [Header("UI")]
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private Slider reelProgressSlider;
    [SerializeField] private Image reelProgressFill;
    [SerializeField] private Gradient reelProgressGradient;
    
    [Header("Input")]
    [SerializeField] private InputAction mouseDeltaAction;
    [SerializeField] private InputAction reelStickAction;
    [SerializeField] private bool useInputActionOverrides = false;
    [SerializeField] private float stickDeadZone = 0.25f;
    [SerializeField] private float mouseDeadZone = 1.5f;
    [SerializeField] private float minAngleDeltaToCount = 0.15f;
    [SerializeField] private float inputDropResetDelay = 0.2f;
    [SerializeField] private bool requireSingleReelDirection = false;
    [SerializeField] private bool reelClockwise = true;

    [Header("Reel Visual")]
    [SerializeField] private Transform reelWheelVisual;
    [SerializeField] private float reelWheelDegreesPerInputDegree = 1.2f;
    [SerializeField] private bool invertReelWheelRotation = true;

    [Header("Fight Rules")]
    [FormerlySerializedAs("startDistance")]
    [SerializeField] private float startProgress = 55f;
    [FormerlySerializedAs("loseDistance")]
    [SerializeField] private float catchProgress = 100f;
    [SerializeField] private float reelGainPerTurn = 9f;
    [SerializeField] private float baseFishPullPerSecond = 3.2f;
    [SerializeField] [Range(0.85f, 1f)] private float catchThresholdPercent = 0.98f;

    [Header("Audio")]
    [FormerlySerializedAs("successClip")]
    [SerializeField] private AudioClip reelTurnClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip loseClip;
    [SerializeField] private AudioSource audioSource;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs;

    private float currentProgress;
    private float accumulatedTurnAngle;
    private bool hasPrevReelDirection;
    private float prevReelAngle;
    private float noInputDuration;
    private bool disableStickActionRead;
    private bool disableMouseActionRead;

    private float difficultyMultiplier = 1f;

    public void SetDifficultyMultiplier(float multiplier)
    {
        difficultyMultiplier = Mathf.Max(0.2f, multiplier);
    }

    private void Update()
    {
        if (!_isRunning) return;

        float fishPull = GetCurrentFishPullPerSecond();
        currentProgress -= fishPull * Time.deltaTime;
        currentProgress = Mathf.Clamp(currentProgress, 0f, catchProgress);

        Vector2 reelDirection;
        try
        {
            reelDirection = ReadReelDirection();
        }
        catch (Exception ex)
        {
          
            useInputActionOverrides = false;
            disableStickActionRead = true;
            disableMouseActionRead = true;
            reelDirection = Vector2.zero;
            Debug.LogWarning($"[FishingMiniGame] Input fallback activated after exception: {ex.GetType().Name} - {ex.Message}");
        }
        TrackReelTurns(reelDirection, Time.deltaTime);

        PushFightProgressToBait();

        UpdateUI();
        TryFinishGame();
    }

    protected override void OnStart()
    {
        catchProgress = Mathf.Max(1f, catchProgress);
        currentProgress = Mathf.Clamp(startProgress, 0f, catchProgress);
        accumulatedTurnAngle = 0f;
        hasPrevReelDirection = false;
        prevReelAngle = 0f;
        noInputDuration = 0f;
        disableStickActionRead = false;
        disableMouseActionRead = false;

        if (gamePanel != null) gamePanel.SetActive(true);

        if (useInputActionOverrides)
        {
            TryEnableAction(mouseDeltaAction, "mouseDeltaAction");
            TryEnableAction(reelStickAction, "reelStickAction");
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        }

        PushFightProgressToBait();
        UpdateUI();
        Log($"Started with progress: {currentProgress:0.0}");
    }

    protected override void OnCleanup()
    {
        if (useInputActionOverrides)
        {
            TryDisableAction(mouseDeltaAction);
            TryDisableAction(reelStickAction);
        }

        if (gamePanel != null) gamePanel.SetActive(false);
    }

    private void TrackReelTurns(Vector2 reelDirection, float deltaTime)
    {
        if (reelDirection.sqrMagnitude <= 0.0001f)
        {
            noInputDuration += deltaTime;
            if (noInputDuration >= inputDropResetDelay)
            {
                hasPrevReelDirection = false;
            }
            return;
        }

        noInputDuration = 0f;
        float currentAngle = Mathf.Atan2(reelDirection.y, reelDirection.x) * Mathf.Rad2Deg;

        if (!hasPrevReelDirection)
        {
            prevReelAngle = currentAngle;
            hasPrevReelDirection = true;
            return;
        }

        float signedAngleDelta = Mathf.DeltaAngle(prevReelAngle, currentAngle);
        prevReelAngle = currentAngle;

        if (Mathf.Abs(signedAngleDelta) < minAngleDeltaToCount) return;

        RotateReelWheel(signedAngleDelta);

        float reelingAngleDelta = GetReelingAngleDelta(signedAngleDelta);
        if (reelingAngleDelta <= 0f) return;

        accumulatedTurnAngle += reelingAngleDelta;

        if (accumulatedTurnAngle < 360f) return;

        int fullTurns = Mathf.FloorToInt(accumulatedTurnAngle / 360f);
        accumulatedTurnAngle -= fullTurns * 360f;

        ApplyPlayerReel(fullTurns);
    }

    private float GetReelingAngleDelta(float signedAngleDelta)
    {
        if (!requireSingleReelDirection)
        {
            return Mathf.Abs(signedAngleDelta);
        }

        if (reelClockwise)
        {
            return Mathf.Max(0f, -signedAngleDelta);
        }

        return Mathf.Max(0f, signedAngleDelta);
    }

    private void RotateReelWheel(float signedAngleDelta)
    {
        if (reelWheelVisual == null) return;

        float directionMultiplier = invertReelWheelRotation ? -1f : 1f;
        float zRotation = signedAngleDelta * reelWheelDegreesPerInputDegree * directionMultiplier;
        reelWheelVisual.Rotate(0f, 0f, zRotation);
    }

    private void ApplyPlayerReel(int fullTurns)
    {
        float progressGain = reelGainPerTurn * fullTurns;
        currentProgress += progressGain;
        currentProgress = Mathf.Clamp(currentProgress, 0f, catchProgress);

        PushFightProgressToBait();

        PlaySound(reelTurnClip);
        Log($"Reel turn x{fullTurns}, progress={currentProgress:0.0}");
    }

    private float GetCurrentFishPullPerSecond()
    {
        return baseFishPullPerSecond * difficultyMultiplier;
    }

    private Vector2 ReadReelDirection()
    {
        Vector2 stickInput = ReadStickInput();
        if (stickInput.sqrMagnitude >= stickDeadZone * stickDeadZone)
        {
            return stickInput.normalized;
        }

        Vector2 mouseDelta = ReadMouseDeltaInput();
        return mouseDelta.sqrMagnitude >= mouseDeadZone * mouseDeadZone
            ? mouseDelta.normalized
            : Vector2.zero;
    }

    private Vector2 ReadStickInput()
    {
        if (useInputActionOverrides &&
            TryReadActionVector2(reelStickAction, ref disableStickActionRead, "reelStickAction", out Vector2 actionValue))
        {
            if (actionValue.sqrMagnitude > 0.0001f)
            {
                return actionValue;
            }
        }

        Vector2 best = Vector2.zero;

        if (Gamepad.current != null)
        {
            Vector2 left = Gamepad.current.leftStick.ReadValue();
            Vector2 right = Gamepad.current.rightStick.ReadValue();
            best = left.sqrMagnitude >= right.sqrMagnitude ? left : right;
        }

        if (Joystick.current != null)
        {
            Vector2 joy = Joystick.current.stick.ReadValue();
            if (joy.sqrMagnitude > best.sqrMagnitude)
            {
                best = joy;
            }
        }

        return best;
    }

    private Vector2 ReadMouseDeltaInput()
    {
        if (useInputActionOverrides &&
            TryReadActionVector2(mouseDeltaAction, ref disableMouseActionRead, "mouseDeltaAction", out Vector2 actionValue))
        {
            if (actionValue.sqrMagnitude > 0.0001f)
            {
                return actionValue;
            }
        }

        if (Pointer.current != null)
        {
            Vector2 pointerDelta = Pointer.current.delta.ReadValue();
            if (pointerDelta.sqrMagnitude > 0.0001f)
            {
                return pointerDelta;
            }
        }

        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            if (mouseDelta.sqrMagnitude > 0.0001f)
            {
                return mouseDelta;
            }
        }

        return Vector2.zero;
    }

    private bool TryReadActionVector2(InputAction action, ref bool disabledFlag, string actionName, out Vector2 value)
    {
        value = Vector2.zero;
        if (action == null || disabledFlag) return false;

        try
        {
            if (!action.enabled)
            {
                action.Enable();
            }

            if (action.controls.Count == 0)
            {
                return false;
            }

            value = action.ReadValue<Vector2>();
            return true;
        }
        catch (Exception ex)
        {
            disabledFlag = true;
            TryDisableAction(action);
            Debug.LogWarning($"[FishingMiniGame] Disabled {actionName} after InputAction read error: {ex.GetType().Name} - {ex.Message}");
            return false;
        }
    }

    private void TryEnableAction(InputAction action, string actionName)
    {
        if (action == null) return;

        try
        {
            if (!action.enabled)
            {
                action.Enable();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[FishingMiniGame] Could not enable {actionName}: {ex.GetType().Name} - {ex.Message}");
        }
    }

    private void TryDisableAction(InputAction action)
    {
        if (action == null) return;

        try
        {
            if (action.enabled)
            {
                action.Disable();
            }
        }
        catch
        {
            // Ignore disable failures; fallback direct device reads still work.
        }
    }

    private void UpdateUI()
    {
        float normalizedCatch = catchProgress <= 0.001f ? 0f : (currentProgress / catchProgress);

        if (reelProgressSlider != null)
        {
            reelProgressSlider.minValue = 0f;
            reelProgressSlider.maxValue = 1f;
            reelProgressSlider.SetValueWithoutNotify(normalizedCatch);
        }

        if (reelProgressFill != null && reelProgressGradient != null)
        {
            reelProgressFill.color = reelProgressGradient.Evaluate(normalizedCatch);
        }

        
    }

    private void TryFinishGame()
    {
        float catchThreshold = catchProgress * Mathf.Clamp01(catchThresholdPercent);
        if (currentProgress >= catchThreshold)
        {
            currentProgress = catchProgress;
            PushFightProgressToBait();
            Log($"Catch reached at {currentProgress:0.0}/{catchProgress:0.0}");
            PlaySound(winClip);
            EndGame(true);
            return;
        }

        if (currentProgress <= 0f)
        {
            currentProgress = 0f;
            PushFightProgressToBait();
            Log("Fish escaped (progress reached 0)");
            PlaySound(loseClip);
            EndGame(false);
        }
    }

    private void PushFightProgressToBait()
    {
        if (!_isRunning) return;
        if (catchProgress <= 0.001f) return;

        FishingManager manager = FishingManager.Instance;
        if (manager == null) return;

        manager.SetActiveBaitFightProgress01(currentProgress / catchProgress);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }

    private void Log(string msg)
    {
        if (!showDebugLogs) return;
        Debug.Log($"[FishingMiniGame] {msg}");
    }
}
