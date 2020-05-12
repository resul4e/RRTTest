using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using Random = UnityEngine.Random;

public enum ExtendStatus
{
	Reached,
	Trapped,
	Advanced
}

public class Tree
{
	public Node Root;

	public Tree(Vector3 _position)
	{
		Root = new Node() {Position = _position};
		m_nodes.Add(Root);
	}

	public Node GetNew()
	{
		return m_new;
	}

	public void AddNode(Vector3 _position)
	{
		var node = new Node() {Position = _position};
		AddNode(node);
	}

	public void AddNode(Node _node)
	{
		var nearestNode = NearestNeighbour(_node.Position);
		nearestNode.Children.Add(_node);
		_node.Parent = nearestNode;
		m_nodes.Add(_node);
		m_new = _node;
	}

	public Node NearestNeighbour(Vector3 _nodePos)
	{
		var dist = float.MaxValue;
		int index = -1;
		for (int i = 0; i < m_nodes.Count; i++)
		{
			var otherNode = m_nodes[i];
			var newDist = Vector3.Distance(_nodePos, otherNode.Position);
			if (newDist < dist)
			{
				dist = newDist;
				index = i;
			}
		}

		return m_nodes[index];
	}

	private Node m_new;
	private  float m_maxDist;
	private List<Node> m_nodes = new List<Node>();
}

public class Node
{
	public Vector3 Position;
	public Node Parent;
	public List<Node> Children = new List<Node>();

	public void Draw()
	{
		foreach (var child in Children)
		{
			Gizmos.DrawLine(Position, child.Position);
			child.Draw();
		}

		Gizmos.DrawSphere(Position, 0.3f);
	}
}

public class RRTConnect : MonoBehaviour
{
	/// <summary>
	/// The rectangle that defines the area where new random points can be spawned
	/// </summary>
	public Vector2 Range;
	/// <summary>
	/// The maximum distance between the new point and the closest neighbour.
	/// This dampens the outward growth of the tree.
	/// </summary>
	public float MaxDist = 0.4f;
	/// <summary>
	/// How many steps we simulate per step. A step is executed every time the space key is pressed.
	/// </summary>
	public int IterationsPerStep = 10;
	/// <summary>
	/// The prefab used to spawn obstacles.
	/// </summary>
	public GameObject ObstaclePrefab;
	/// <summary>
	/// How many obstacles are spawned.
	/// </summary>
	public uint ObstacleAmount = 25;
	/// <summary>
	/// The <see cref="GameObject"/> that represents the goal which the start node is working towards.
	/// </summary>
	public GameObject Goal;

	void Start()
	{
		Restart();
	}

	void Restart()
	{
		m_done = false;

		foreach (var obs in m_obstacles)
		{
			Destroy(obs.gameObject);
		}
		m_obstacles.Clear();

		//position the start and end somewhere
		transform.position = new Vector3(Random.Range(-Range.x, Range.x), Random.Range(-Range.y, Range.y));
		Goal.transform.position = new Vector3(Random.Range(-Range.x, Range.x), Random.Range(-Range.y, Range.y));

		m_start = new Tree(transform.position);
		m_end = new Tree(Goal.transform.position);

		//Spawn 25 obstacles at random location within the given Range.
		for (int i = 0; i < ObstacleAmount; i++)
		{
			var obs = Instantiate(ObstaclePrefab, new Vector3(Random.Range(-Range.x, Range.x), Random.Range(-Range.y, Range.y), 0), Quaternion.identity);
			m_obstacles.Add(obs);
		}
	}

	/// <summary>
	/// Every time space is pressed we add an <see cref="IterationsPerStep"/> amount of points to the graph.
	/// </summary>
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			Restart();
		}

		//Check if the space key is pressed.
		if (Input.GetKeyDown(KeyCode.Space))
		{
			for (int k = 0; k < IterationsPerStep; k++)
			{
				var qRand = RandomState();

				if (Extend(m_start, qRand) != ExtendStatus.Trapped)
				{
					if (Connect(m_start.GetNew().Position) == ExtendStatus.Reached)
					{
						m_done = true;
						break;
					}
				}

				var temp = m_start;
				m_start = m_end;
				m_end = temp;
			}
		}
	}

	private int m_maxItter = 100;
	ExtendStatus Connect(Vector3 _q)
	{
		int itter = 0;
		ExtendStatus status = Extend(m_end, _q);
		while (m_maxItter != itter && status == ExtendStatus.Advanced)
		{
			status = Extend(m_end, _q);
			itter++;
		}

		return status;
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
		foreach (var obs in m_obstacles)
		{
			//If it does xNew will be this intersection.
			_qNew = BoxLineIntersect(obs, _qNear.Position, _qNew);
		}

		return _qNew == oldQNew;
	}

	/// <summary>
	/// Tests if a line intersects a box.
	/// </summary>
	/// <param name="_box">The box to test.</param>
	/// <param name="_start">The start position of the line</param>
	/// <param name="_end">The end position of the line</param>
	/// <returns><paramref name="_end"/> if nothing is hit. Otherwise the point just before intersection.</returns>
	Vector3 BoxLineIntersect(GameObject _box, Vector3 _start, Vector3 _end)
	{
		var maxdist = Vector3.Distance(_start, _end);
		var dir = (_end - _start).normalized;
		float dist;
		if (_box.GetComponent<Collider>().bounds.IntersectRay(new Ray(_start, dir), out dist))
		{
			if (dist < maxdist)
			{
				return _start + (dist - 0.1f) * dir;
			}
		}

		return _end;
	}

	/// <summary>
	/// Get a new random point within <see cref="Range"/>
	/// </summary>
	/// <returns>A new random point.</returns>
	Vector3 RandomState()
	{
		return new Vector3(Random.Range(-Range.x, Range.x), Random.Range(-Range.y, Range.y), 0);
	}

	/// <summary>
	/// Draw all gizmos to visualise what is happening.
	/// </summary>
	public void OnDrawGizmos()
	{
		if (m_start == null)
		{
			return;
		}

		if (m_done)
		{
			Gizmos.color = Color.grey;
			m_start.Root.Draw();
			m_end.Root.Draw();

			Gizmos.color = Color.blue;
			var parent = m_start.GetNew();
			while (parent != null)
			{
				var pos = parent.Position;
				Gizmos.DrawSphere(pos, 0.1f);
				parent = parent.Parent;
				if (parent != null)
				{
					Gizmos.DrawLine(pos, parent.Position);
				}
			}

			parent = m_end.GetNew();
			while (parent != null)
			{
				var pos = parent.Position;
				Gizmos.DrawSphere(pos, 0.1f);
				parent = parent.Parent;
				if (parent != null)
				{
					Gizmos.DrawLine(pos, parent.Position);
				}
			}
		}
		else
		{
			Gizmos.color = Color.green;
			m_start.Root.Draw();
			Gizmos.color = Color.red;
			m_end.Root.Draw();
		}
	}

	private bool m_done = false;
	private Tree m_start;
	private Tree m_end;
	private List<GameObject> m_obstacles = new List<GameObject>();

	private Vector3 m_randomPos;
}
