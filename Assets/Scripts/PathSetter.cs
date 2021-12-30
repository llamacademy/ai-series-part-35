using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PathSetter : MonoBehaviour
{
    [SerializeField]
    private SmoothAgentMovement SmoothedAgent;
    private NavMeshAgent Agent;

    [SerializeField]
    private Transform[] Locations = new Transform[0];

    private Coroutine Scenario;

    private void Awake()
    {
        Agent = SmoothedAgent.GetComponent<NavMeshAgent>();
        SmoothedAgent.EnableClickToMove = false;
    }

    private IEnumerator Start()
    {
        TrailRenderer trail = SmoothedAgent.GetComponent<TrailRenderer>();
        float pauseTime = trail.time;
        for (int i = 0; i < Locations.Length; i++)
        {
            yield return StartCoroutine(RunScenario(i));
            yield return new WaitUntil(() =>
            {
                return Vector3.Distance(Locations[i].position + Vector3.up * Agent.baseOffset, Agent.transform.position) < Agent.radius;
            });
            yield return new WaitForSeconds(pauseTime);
        }
    }

    //private void OnGUI()
    //{
    //    for (int i = 0; i < Locations.Length; i++)
    //    {
    //        if (GUI.Button(new Rect(25, 35 * (i + 2), 200, 35), $"Run Scenario {i + 1}"))
    //        {
    //            if (Scenario != null)
    //            {
    //                StopCoroutine(Scenario);
    //            }
    //            Scenario = StartCoroutine(RunScenario(i));
    //        }
    //    }
    //}

    private IEnumerator RunScenario(int Index)
    {
        TrailRenderer trail = SmoothedAgent.GetComponent<TrailRenderer>();
        float pauseTime = trail.time;

        Vector3 startPosition = SmoothedAgent.transform.position;

        SmoothedAgent.SetDestination(Locations[Index].position);

        yield return new WaitUntil(() =>
        {
            return Vector3.Distance(Locations[Index].position + Vector3.up * Agent.baseOffset, Agent.transform.position) < Agent.radius;
        });
        yield return new WaitForSeconds(pauseTime);

        trail.enabled = false;
        Agent.Warp(startPosition);
        SmoothedAgent.SetAgentDestination(Locations[Index].position);
        trail.enabled = true;
    }
}
