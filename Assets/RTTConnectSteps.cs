using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RTTConnectSteps : MonoBehaviour
{
	enum Steps
	{
		Start,
		Goal1,
		Node1,
		Extend1,
		Connect1Loop1,
		Connect1Loop2,
		Connect1Loop3,
		Connect1LoopEnd,
		Swap1,
		Goal2,
		Node2,
		Extend2,
		Connect2Loop1,
		Connect2Loop2,
		Connect2LoopEnd,
		Swap2,
		Goal3,
		Node3,
		Extend3,
		Connect3Loop1,
		Finished
	}

	/// <summary>
	/// The rectangle that defines the area where new random points can be spawned
	/// </summary>
	public Vector2 Range;
	/// <summary>
	/// The maximum distance between the new point and the closest neighbour.
	/// This dampens the outward growth of the tree.
	/// </summary>
	public float MaxDist = 5f;
	/// <summary>
	/// How many steps we simulate per step. A step is executed every time the space key is pressed.
	/// </summary>
	public int IterationsPerStep = 10;
	/// <summary>
	/// The prefab used to spawn obstacles.
	/// </summary>
	public List<GameObject> Obstacles;
	/// <summary>
	/// The <see cref="GameObject"/> that represents the goal which the start node is working towards.
	/// </summary>
	public GameObject GoalObject;

	public GameObject StartObject;

	public GameObject TargetObject;

	// Start is called before the first frame update
	void Start()
    {
		m_start = new Tree(StartObject.transform.position);
		m_end = new Tree(GoalObject.transform.position);
	}

    // Update is called once per frame
    void Update()
    {
	    if (Input.GetKeyDown(KeyCode.Space))
	    {
		    m_step++;

			switch (m_step)
			{
				case Steps.Start:
					break;
				case Steps.Goal1:
					TargetObject.transform.position = new Vector3(-3.0f, 9.1f, 0);
					break;
				case Steps.Node1:
					Extend(m_start, TargetObject.transform.position);
					TargetObject.transform.position = new Vector3(-15, 0, 0);
					break;
				case Steps.Extend1:
					break;
				case Steps.Connect1Loop1:
					Vector3 ot;
					NewConfig(m_start.GetNew().Position, m_end.NearestNeighbour(m_start.GetNew().Position), out ot);
					m_end.AddNode(ot);
					break;
				case Steps.Connect1Loop2:
					NewConfig(m_start.GetNew().Position, m_end.NearestNeighbour(m_start.GetNew().Position), out ot);
					m_end.AddNode(ot);
					break;
				case Steps.Connect1Loop3:
					NewConfig(m_start.GetNew().Position, m_end.NearestNeighbour(m_start.GetNew().Position), out ot);
					m_end.AddNode(ot);
					break;
				case Steps.Connect1LoopEnd:
					break;
				case Steps.Swap1:
					var sprite = StartObject.GetComponent<SpriteRenderer>().sprite;
					StartObject.GetComponent<SpriteRenderer>().sprite =
						GoalObject.GetComponent<SpriteRenderer>().sprite;
					GoalObject.GetComponent<SpriteRenderer>().sprite = sprite;
					var start = m_start;
					m_start = m_end;
					m_end = start;
					break;
				case Steps.Goal2:
					TargetObject.transform.position = new Vector3(5f, 5f, 0);
					break;
				case Steps.Node2:
					Extend(m_start, TargetObject.transform.position);
					TargetObject.transform.position = new Vector3(-15, 0, 0);
					break;
				case Steps.Extend2:
					break;
				case Steps.Connect2Loop1:
					NewConfig(m_start.GetNew().Position, m_end.NearestNeighbour(m_start.GetNew().Position), out ot);
					m_end.AddNode(ot);
					break;
				case Steps.Connect2Loop2:
					NewConfig(m_start.GetNew().Position, m_end.NearestNeighbour(m_start.GetNew().Position), out ot);
					m_end.AddNode(ot);
					break;
				case Steps.Connect2LoopEnd:
					break;
				case Steps.Swap2:
					sprite = StartObject.GetComponent<SpriteRenderer>().sprite;
					StartObject.GetComponent<SpriteRenderer>().sprite =
						GoalObject.GetComponent<SpriteRenderer>().sprite;
					GoalObject.GetComponent<SpriteRenderer>().sprite = sprite;
					start = m_start;
					m_start = m_end;
					m_end = start;
					break;
				case Steps.Goal3:
					TargetObject.transform.position = new Vector3(-0.71f, 4.3f, 0);
					break;
				case Steps.Node3:
					Extend(m_start, TargetObject.transform.position);
					TargetObject.transform.position = new Vector3(-15, 0, 0);
					break;
				case Steps.Extend3:
					break;
				case Steps.Connect3Loop1:
					NewConfig(m_start.GetNew().Position, m_end.NearestNeighbour(m_start.GetNew().Position), out ot);
					m_end.AddNode(ot);
					break;
				case Steps.Finished:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}

	void OnDrawGizmos()
    {
	    if (!Application.isPlaying)
	    {
		    return;
	    }

		switch (m_step)
		{
			case Steps.Start:
				break;
			case Steps.Goal1:
				Handles.color = Color.green;
				Handles.DrawDottedLine(m_start.Root.Position, TargetObject.transform.position, 10);
				break;
			case Steps.Node1:
				break;
			case Steps.Extend1:
				Handles.color = Color.blue;
				Handles.DrawDottedLine(m_end.NearestNeighbour(m_start.GetNew().Position).Position, m_start.GetNew().Position, 10);
				break;
			case Steps.Connect1Loop1:
				Handles.color = Color.blue;
				Handles.DrawDottedLine(m_end.NearestNeighbour(m_start.GetNew().Position).Position, m_start.GetNew().Position, 10);
				break;
			case Steps.Connect1Loop2:
				Handles.color = Color.blue;
				Handles.DrawDottedLine(m_end.NearestNeighbour(m_start.GetNew().Position).Position, m_start.GetNew().Position, 10);
				break;
			case Steps.Connect1Loop3:
				Handles.color = Color.blue;
				Handles.DrawDottedLine(m_end.NearestNeighbour(m_start.GetNew().Position).Position, m_start.GetNew().Position, 10);
				break;
			case Steps.Connect1LoopEnd:
				Handles.color = Color.blue;
				Handles.DrawDottedLine(m_end.NearestNeighbour(m_start.GetNew().Position).Position, m_start.GetNew().Position, 10);
				Gizmos.color = Color.red;
				var dir = (m_end.GetNew().Position - m_end.Root.Position).normalized;
				Gizmos.DrawSphere(m_end.GetNew().Position + dir * MaxDist, 0.3f);
				break;
			case Steps.Swap1:
				break;
			case Steps.Goal2:
				Handles.color = Color.green;
				Handles.DrawDottedLine(m_start.NearestNeighbour(TargetObject.transform.position).Position, TargetObject.transform.position, 10);
				break;
			case Steps.Node2:
				break;
			case Steps.Extend2:
				Handles.color = Color.blue;
				Handles.DrawDottedLine(m_end.NearestNeighbour(m_start.GetNew().Position).Position, m_start.GetNew().Position, 10);
				break;
			case Steps.Connect2Loop1:
				Handles.color = Color.blue;
				Handles.DrawDottedLine(m_end.NearestNeighbour(m_start.GetNew().Position).Position, m_start.GetNew().Position, 10);
				break;
			case Steps.Connect2Loop2:
				Handles.color = Color.blue;
				Handles.DrawDottedLine(m_end.NearestNeighbour(m_start.GetNew().Position).Position, m_start.GetNew().Position, 10);
				break;
			case Steps.Connect2LoopEnd:
				Handles.color = Color.blue;
				Handles.DrawDottedLine(m_end.NearestNeighbour(m_start.GetNew().Position).Position, m_start.GetNew().Position, 10);
				Gizmos.color = Color.red;
				dir = (m_end.GetNew().Position - m_end.GetNew().Parent.Position).normalized;
				Gizmos.DrawSphere(m_end.GetNew().Position + dir * MaxDist, 0.3f);
				break;
			case Steps.Swap2:
				break;
			case Steps.Goal3:
				Handles.color = Color.green;
				Handles.DrawDottedLine(m_start.NearestNeighbour(TargetObject.transform.position).Position, TargetObject.transform.position, 10);
				break;
			case Steps.Node3:
				break;
			case Steps.Extend3:
				Handles.color = Color.blue;
				Handles.DrawDottedLine(m_end.NearestNeighbour(m_start.GetNew().Position).Position, m_start.GetNew().Position, 10);
				break;
			case Steps.Connect3Loop1:
				Handles.color = Color.blue;
				Handles.DrawDottedLine(m_end.NearestNeighbour(m_start.GetNew().Position).Position, m_start.GetNew().Position, 10);
				break;
			case Steps.Finished:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		if (m_step != Steps.Finished)
		{
			foreach (var child in m_start.Root.Children)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawLine(m_start.Root.Position, child.Position);
				child.Draw();
			}

			foreach (var child in m_end.Root.Children)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(m_end.Root.Position, child.Position);
				child.Draw();
			}
		}
		else
		{
			foreach (var child in m_start.Root.Children)
			{
				Gizmos.color = Color.magenta;
				Gizmos.DrawLine(m_start.Root.Position, child.Position);
				child.Draw();
			}

			foreach (var child in m_end.Root.Children)
			{
				Gizmos.color = Color.magenta;
				Gizmos.DrawLine(m_end.Root.Position, child.Position);
				child.Draw();
			}

			Gizmos.DrawLine(m_start.GetNew().Position, m_end.GetNew().Position);
		}
    }

	public ExtendStatus Extend(Tree _t, Vector3 _q)
    {
	    var nearNode = _t.NearestNeighbour(_q);
	    Vector3 qNew;
	    if (NewConfig(_q, nearNode, out qNew))
	    {
		    _t.AddNode(qNew);

		    if (qNew == _q)
		    {
			    return ExtendStatus.Reached;
		    }
		    return ExtendStatus.Advanced;
	    }
	    return ExtendStatus.Trapped;
    }

    bool NewConfig(Vector3 _q, Node _qNear, out Vector3 _qNew)
    {
	    var dist = Vector3.Distance(_q, _qNear.Position);
	    if (dist > MaxDist)
	    {
		    var diff = _q - _qNear.Position;
		    var norm = diff.normalized;
		    _qNew = _qNear.Position + norm * MaxDist;
	    }
	    else
	    {
		    _qNew = _q;
	    }

	    var oldQNew = _qNew;
	    foreach (var obs in Obstacles)
	    {
		    //If it does xNew will be this intersection.
		    _qNew = BoxLineIntersect(obs, _qNear.Position, _qNew);
	    }

	    return _qNew == oldQNew;
    }

    Vector3 BoxLineIntersect(GameObject _box, Vector3 _start, Vector3 _end)
    {
	    var maxdist = Vector3.Distance(_start, _end);
	    var dir = (_end - _start).normalized;
	    float dist;
	    if (_box.GetComponent<Collider2D>().bounds.IntersectRay(new Ray(_start, dir), out dist))
	    {
		    if (dist < maxdist)
		    {
			    return _start + (dist - 0.1f) * dir;
		    }
	    }

	    return _end;
    }

	private bool m_done = false;
    private Tree m_start;
    private Tree m_end;

    private Vector3 m_randomPos;
    private Steps m_step = Steps.Start;
}
