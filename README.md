# Curved NavMeshAgent Paths

In this tutorial repository you will learn how to make NavMeshAgents move on a NavMesh with a curved path. 

Generally, when a NavMeshAgent moves along a path they move linearly between the points calculated by NavMesh.CalculatePath via NavMeshAgent.SetDestination, or by your code. We'll use the same concept of calculating a path, then smooth that path using Bezier Curves and postprocess that smoothed path to ensure all the points are still going in the direction the Agent should go, and are still on the path.

To do click-to-move, disable the "Locations" GameObject and enable Click to Move on the SmoothAgentMovement script.

To just compare smoothed paths versus non-smoothed paths, set up the path smoothing configuration you'd like to use on the SmoothAgentMovement script and enable the "Locations" GameObject. 

[![Youtube Tutorial](./Video%20Screenshot.png)](https://youtu.be/o10IwnX1W0A)

## Patreon Supporters
Have you been getting value out of these tutorials? Do you believe in LlamAcademy's mission of helping everyone make their game dev dream become a reality? Consider becoming a Patreon supporter and get your name added to this list, as well as other cool perks.
Head over to https://patreon.com/llamacademy to show your support.

### Gold Tier Supporters
* YOUR NAME HERE!

### Silver Tier Supporters
* Raphael
* Andrew Bowen
* Gerald Anderson
* YOUR NAME HERE!

### Bronze Tier Supporters
* Bastian
* Trey Briggs
* AudemKay
* YOUR NAME HERE!

## Other Projects
Interested in other AI Topics in Unity, or other tutorials on Unity in general? 

* [Check out the LlamAcademy YouTube Channel](https://youtube.com/c/LlamAcademy)!
* [Check out the LlamAcademy GitHub for more projects](https://github.com/llamacademy)

## Requirements
* Requires Unity 2020.3 LTS or higher. 