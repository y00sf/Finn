using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Rope : MonoBehaviour
{
    static Material fallbackLineMaterial;

    [Header("Endpoints")]
    public Transform startPoint;
    public Transform endPoint;      
    public bool pinEndPoint = true;

    [Header("Rope Shape")]
    [Min(2)]    public int   segmentCount          = 20;
    [Min(0.01f)] public float ropeLength           = 5f;
    public bool clampLengthToEndpoints = true;   // auto-extend rope if endpoints are farther apart

    [Header("Simulation")]
    [Range(2, 16)] public int   subSteps             = 8;   // ← the main anti-stretch / anti-twitch knob
    [Range(1, 80)] public int   constraintIterations = 15;  // per sub-step
    [Range(0f, 1f)] public float damping             = 0.98f;
    public float gravityScale     = 1f;
    public Vector3 gravityDirection = Vector3.down;

    [Header("Line Renderer")]
    public float lineWidth = 0.05f;
    public Color lineColor = Color.white;

    // ── internals ──────────────────────────────────────────────────────────────
    LineRenderer lr;
    Vector3[] pos;
    Vector3[] prevPos;

    // Effective rest length per segment (recalculated each step so it hot-reloads)
    float SegmentRestLength
    {
        get
        {
            float len = ropeLength;
            if (clampLengthToEndpoints && startPoint && endPoint)
                len = Mathf.Max(len, Vector3.Distance(startPoint.position, endPoint.position));
            return len / (segmentCount - 1);
        }
    }

    // ── Unity callbacks ────────────────────────────────────────────────────────
    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        ConfigureLineRenderer();
        InitRope();
    }

    void LateUpdate()
    {
        if (!startPoint) return;

        float subDt = Time.deltaTime / subSteps;

        for (int s = 0; s < subSteps; s++)
        {
            Integrate(subDt);
            SolveConstraints();
        }

        Draw();
        UpdateFreeEnd();
    }

    void OnValidate()
    {
        if (lr == null)
            lr = GetComponent<LineRenderer>();

        if (lr == null) return;

        if (pos == null || pos.Length != segmentCount)
            InitRope();

        ConfigureLineRenderer();
    }

    // ── init ──────────────────────────────────────────────────────────────────
    void InitRope()
    {
        pos     = new Vector3[segmentCount];
        prevPos = new Vector3[segmentCount];

        Vector3 start = startPoint ? startPoint.position : transform.position;
        Vector3 dir   = gravityDirection.sqrMagnitude > 0.0001f
                        ? gravityDirection.normalized
                        : Vector3.down;

        float segLen = ropeLength / (segmentCount - 1);
        for (int i = 0; i < segmentCount; i++)
        {
            pos[i]     = start + dir * (segLen * i);
            prevPos[i] = pos[i];
        }

        lr.positionCount = segmentCount;
        ConfigureLineRenderer();
        lr.SetPositions(pos);
    }

    void ConfigureLineRenderer()
    {
        lr.startWidth = lineWidth;
        lr.endWidth   = lineWidth;
        lr.startColor = lineColor;
        lr.endColor   = lineColor;

        if (lr.sharedMaterial == null)
            lr.sharedMaterial = GetFallbackLineMaterial();

        if (lr.sharedMaterial != null)
        {
            if (lr.sharedMaterial.HasProperty("_Color"))
                lr.sharedMaterial.color = lineColor;

            if (lr.sharedMaterial.HasProperty("_BaseColor"))
                lr.sharedMaterial.SetColor("_BaseColor", lineColor);
        }
    }

    static Material GetFallbackLineMaterial()
    {
        if (fallbackLineMaterial != null)
            return fallbackLineMaterial;

        string[] shaderNames =
        {
            "Sprites/Default",
            "Universal Render Pipeline/Unlit",
            "Unlit/Color"
        };

        foreach (string shaderName in shaderNames)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null) continue;

            fallbackLineMaterial = new Material(shader)
            {
                color = Color.white,
                hideFlags = HideFlags.HideAndDontSave
            };
            return fallbackLineMaterial;
        }

        return null;
    }

    // ── simulation ────────────────────────────────────────────────────────────

    /// Verlet integration — advances free nodes, pins anchors.
    void Integrate(float dt)
    {
        // gravity contribution = a * dt²  (Verlet form)
        Vector3 gravity = gravityDirection.normalized * (9.81f * gravityScale * dt * dt);

        int last   = segmentCount - 1;
        bool pinEnd = endPoint != null && pinEndPoint;

        // Pin start — store previous so anchor motion carries into the rope naturally
        prevPos[0] = pos[0];
        pos[0]     = startPoint.position;

        // Pin end
        if (pinEnd)
        {
            prevPos[last] = pos[last];
            pos[last]     = endPoint.position;
        }

        int loopEnd = pinEnd ? last - 1 : last;
        for (int i = 1; i <= loopEnd; i++)
        {
            Vector3 velocity = (pos[i] - prevPos[i]) * damping;
            prevPos[i]  = pos[i];
            pos[i]     += velocity + gravity;
        }
    }

    /// XPBD-style distance constraint solver — removes stretch and jitter.
    void SolveConstraints()
    {
        float segLen = SegmentRestLength;
        int   last   = segmentCount - 1;
        bool  pinEnd = endPoint != null && pinEndPoint;

        for (int iter = 0; iter < constraintIterations; iter++)
        {
            // Re-pin anchors every iteration (prevents drift under heavy load)
            pos[0] = startPoint.position;
            if (pinEnd) pos[last] = endPoint.position;

            for (int i = 0; i < last; i++)
            {
                Vector3 delta = pos[i + 1] - pos[i];
                float   dist  = delta.magnitude;
                if (dist < 0.000001f) continue;

                // How much the segment needs to shrink/grow, split evenly
                float   error      = (dist - segLen) / dist;
                Vector3 correction = delta * (error * 0.5f);   // half per node

                bool p1Pinned = i == 0;
                bool p2Pinned = pinEnd && (i + 1 == last);

                if (!p1Pinned && !p2Pinned)
                {
                    pos[i]     += correction;       // move toward p2
                    pos[i + 1] -= correction;       // move toward p1
                }
                else if (p1Pinned)
                {
                    pos[i + 1] -= correction * 2f;  // p1 can't move, dump all onto p2
                }
                else // p2 pinned
                {
                    pos[i]     += correction * 2f;  // p2 can't move, dump all onto p1
                }
            }
        }
    }

    // ── output ────────────────────────────────────────────────────────────────

    void Draw()
    {
        if (lr.positionCount != segmentCount)
            lr.positionCount = segmentCount;

        ConfigureLineRenderer();
        lr.SetPositions(pos);
    }

    /// If the end is free (not pinned), push the end Transform to follow the rope tip.
    void UpdateFreeEnd()
    {
        if (endPoint == null || pinEndPoint) return;
        endPoint.position = pos[segmentCount - 1];
    }
}
