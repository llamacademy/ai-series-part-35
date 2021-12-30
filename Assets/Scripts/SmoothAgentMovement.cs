using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent))]
public class SmoothAgentMovement : MonoBehaviour
{
    [SerializeField]
    private Camera Camera;
    [SerializeField]
    private LayerMask FloorLayer;
    public bool EnableClickToMove = true;
    [SerializeField]
    private bool UsePathSmoothing;
    [Header("Path Smoothing")]
    [SerializeField]
    private float SmoothingLength = 0.25f;
    [SerializeField]
    private int SmoothingSections = 10;
    [SerializeField]
    [Range(-1, 1)]
    private float SmoothingFactor = 0;
    private NavMeshAgent Agent;
    private NavMeshPath CurrentPath;
    public Vector3[] PathLocations = new Vector3[0];
    [SerializeField]
    private int PathIndex = 0;

    [Header("Movement Configuration")]
    [SerializeField]
    [Range(0, 0.99f)]
    private float Smoothing = 0.25f;
    [SerializeField]
    private float TargetLerpSpeed = 1;

    [SerializeField]
    private Vector3 TargetDirection;
    private float LerpTime = 0;
    [SerializeField]
    private Vector3 MovementVector;

    private Vector3 InfinityVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        CurrentPath = new NavMeshPath();
    }

    public void SetDestination(Vector3 Position)
    {
        Agent.ResetPath();
        NavMesh.CalculatePath(transform.position, Position, Agent.areaMask, CurrentPath);
        Vector3[] corners = CurrentPath.corners;

        if (corners.Length > 2)
        {
            BezierCurve[] curves = new BezierCurve[corners.Length - 1];

            SmoothCurves(curves, corners);

            PathLocations = GetPathLocations(curves);

            PathIndex = 0;
        }
        else
        {
            PathLocations = corners;
            PathIndex = 0;
        }
    }

    public void SetAgentDestination(Vector3 Position)
    {
        PathIndex = int.MaxValue;
        Agent.SetDestination(Position);
        PathLocations = Agent.path.corners;
    }

    private void Update()
    {
        if (EnableClickToMove)
        {
            HandleInput();
        }

        MoveAgent();
    }

    private void MoveAgent()
    {
        if (PathIndex >= PathLocations.Length)
        {
            return;
        }

        if (Vector3.Distance(transform.position, PathLocations[PathIndex] + (Agent.baseOffset * Vector3.up)) <= Agent.radius)
        {
            PathIndex++;
            LerpTime = 0;

            if (PathIndex >= PathLocations.Length)
            {
                return;
            }
        }

        MovementVector = (PathLocations[PathIndex] + (Agent.baseOffset * Vector3.up) - transform.position).normalized;

        TargetDirection = Vector3.Lerp(
            TargetDirection,
            MovementVector,
            Mathf.Clamp01(LerpTime * TargetLerpSpeed * (1 - Smoothing))
        );

        Vector3 lookDirection = MovementVector;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(lookDirection),
                Mathf.Clamp01(LerpTime * TargetLerpSpeed * (1 - Smoothing))
            );
        }

        Agent.Move(TargetDirection * Agent.speed * Time.deltaTime);

        LerpTime += Time.deltaTime;
    }

    private void HandleInput()
    {
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Ray ray = Camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, FloorLayer))
            {
                if (UsePathSmoothing)
                {
                    SetDestination(hit.point);
                }
                else
                {
                    SetAgentDestination(hit.point);
                }
            }
        }
    }

    private Vector3[] GetPathLocations(BezierCurve[] Curves)
    {
        Vector3[] pathLocations = new Vector3[Curves.Length * SmoothingSections];

        int index = 0;
        for (int i = 0; i < Curves.Length; i++)
        {
            Vector3[] segments = Curves[i].GetSegments(SmoothingSections);
            for (int j = 0; j < segments.Length; j++)
            {
                pathLocations[index] = segments[j];
                index++;
            }
        }

        pathLocations = PostProcessPath(Curves, pathLocations);

        return pathLocations;
    }

    private Vector3[] PostProcessPath(BezierCurve[] Curves, Vector3[] Path)
    {
        Vector3[] path = RemoveOversmoothing(Curves, Path);

        path = RemoveTooClosePoints(path);

        path = SamplePathPositions(path);

        return path;
    }

    private Vector3[] SamplePathPositions(Vector3[] Path)
    {
        for (int i = 0; i < Path.Length; i++)
        {
            if (NavMesh.SamplePosition(Path[i], out NavMeshHit hit, Agent.radius * 1.5f, Agent.areaMask))
            {
                Path[i] = hit.position;
            }
            else
            {
                Debug.LogWarning($"No NavMesh point close to {Path[i]}. Check your smoothing settings!");
                Path[i] = InfinityVector;
            }
        }

        return Path.Except(new Vector3[] { InfinityVector }).ToArray();
    }

    private Vector3[] RemoveTooClosePoints(Vector3[] Path)
    {
        if (Path.Length <= 2)
        {
            return Path;
        }

        int lastIndex = 0;
        int index = 1;
        for (int i = 0; i < Path.Length - 1; i++)
        {
            if (Vector3.Distance(Path[index], Path[lastIndex]) <= Agent.radius)
            {
                Path[index] = InfinityVector;
            }
            else
            {
                lastIndex = index;
            }
            index++;
        }

        return Path.Except(new Vector3[] { InfinityVector }).ToArray();
    }

    private Vector3[] RemoveOversmoothing(BezierCurve[] Curves, Vector3[] Path)
    {
        if (Path.Length <= 2)
        {
            return Path;
        }

        int index = 1;
        int lastIndex = 0;
        for (int i = 0; i < Curves.Length; i++)
        {
            Vector3 targetDirection = (Curves[i].EndPosition - Curves[i].StartPosition).normalized;

            for (int j = 0; j < SmoothingSections - 1; j++)
            {
                Vector3 segmentDirection = (Path[index] - Path[lastIndex]).normalized;
                float dot = Vector3.Dot(targetDirection, segmentDirection);
                Debug.Log($"Target Direction: {targetDirection}. segment direction: {segmentDirection} = dot {dot} with index {index} & lastIndex {lastIndex}");
                if (dot <= SmoothingFactor)
                {
                    Path[index] = InfinityVector;
                }
                else
                {
                    lastIndex = index;
                }

                index++;
            }

            index++;
        }

        Path[Path.Length - 1] = Curves[Curves.Length - 1].EndPosition;

        Vector3[] TrimmedPath = Path.Except(new Vector3[] { InfinityVector }).ToArray();

        Debug.Log($"Original Smoothed Path: {Path.Length}. Trimmed Path: {TrimmedPath.Length}");

        return TrimmedPath;
    }

    private void SmoothCurves(BezierCurve[] Curves, Vector3[] Corners)
    {
        for (int i = 0; i < Curves.Length; i++)
        {
            if (Curves[i] == null)
            {
                Curves[i] = new BezierCurve();
            }

            Vector3 position = Corners[i];
            Vector3 lastPosition = i == 0 ? Corners[i] : Corners[i - 1];
            Vector3 nextPosition = Corners[i + 1];

            Vector3 lastDirection = (position - lastPosition).normalized;
            Vector3 nextDirection = (nextPosition - position).normalized;

            Vector3 startTangent = (lastDirection + nextDirection) * SmoothingLength;
            Vector3 endTangent = (nextDirection + lastDirection) * -1 * SmoothingLength;

            Curves[i].Points[0] = position; // Start Position (P0)
            Curves[i].Points[1] = position + startTangent; // Start Tangent (P1)
            Curves[i].Points[2] = nextPosition + endTangent; // End Tangent (P2)
            Curves[i].Points[3] = nextPosition; // End Position (P3)
        }


        // Apply look-ahead for first curve and retroactively apply the end tangent
        {
            Vector3 nextDirection = (Curves[1].EndPosition - Curves[1].StartPosition).normalized;
            Vector3 lastDirection = (Curves[0].EndPosition - Curves[0].StartPosition).normalized;

            Curves[0].Points[2] = Curves[0].Points[3] +
                (nextDirection + lastDirection) * -1 * SmoothingLength;
        }
    }
}
