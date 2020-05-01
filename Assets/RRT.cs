using System;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using Random = UnityEngine.Random;

public class RRT : MonoBehaviour
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

	public GameObject Goal;

	void Start()
	{
		Restart();
	}

	/// <summary>
	/// Clean up all old data if present and set everything up.
	/// </summary>
	void Restart()
	{
		//Remove all old obstacles.
		foreach (var obs in m_obstacles)
		{
			Destroy(obs.gameObject);
		}

		//Clear all lists.
		m_positions = new List<Vector3>();
		m_edges = new List<Tuple<Vector3, Vector3>>();
		m_obstacles = new List<GameObject>(25);

		//position the start and end somewhere
		transform.position = new Vector3(Random.Range(-Range.x, Range.x), Random.Range(-Range.y, Range.y));
		Goal.transform.position = new Vector3(Random.Range(-Range.x, Range.x), Random.Range(-Range.y, Range.y));

		//Add the initial position to the list of tree positions
		m_positions.Add(transform.position);

		//Spawn 25 obstacles at random location within the given Range.
		for (int i = 0; i < 25; i++)
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
		//If 'R' is pressed Restart the RRT.
		if (Input.GetKeyDown(KeyCode.R))
		{
			Restart();
		}

		//Check if the space key is pressed.
		if (Input.GetKeyDown(KeyCode.Space))
		{
			//Add IterationsPerStep number of points.
			for (int i = 0; i < IterationsPerStep; i++)
			{
				//Get a new random point within the given range.
				var xRand = RandomState();
				//Check what existing point is closest to this new random point.
				var xNearIndex = NearestNeighbour(xRand);
				//Create a new point that is a maximum of MaxDistance away from its nearest Neighbour.
				var xNew = NewState(xRand, xNearIndex);

				var oldNew = xNew;

				//Check if the edge create by xNew and xNear collides with any obstacles.
				foreach (var obs in m_obstacles)
				{
					//If it does xNew will be this intersection.
					xNew = BoxLineIntersect(obs, m_positions[xNearIndex], xNew);
				}

				//If we collided with something, don't add it to the list.
				if (oldNew != xNew)
				{
					continue;
				}

				//Add the new node and the edge from nearest neighbour to xNew
				AddNode(xNew);
				ConnectEdge(xNearIndex);

				if (CheckGoal())
				{
					int ind = NearestNeighbour(Goal.transform.position);
					ConnectEdge(m_positions[ind], Goal.transform.position);
				}
			}
		}
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
	/// Find the nearest neighbour in the <see cref="m_positions"/> list.
	/// </summary>
	/// <param name="_xRand">The new point we want the nearest neighbour for.</param>
	/// <remarks>We should probably just return the position directly as the index is never used other than looking up the position.</remarks>
	/// <returns>The index of the nearest neighbour.</returns>
	int NearestNeighbour(Vector3 _xRand)
	{
		var dist = float.MaxValue;
		int index = -1;
		for(int i = 0; i < m_positions.Count; i++)
		{
			var p = m_positions[i];
			var newDist = Vector3.Distance(_xRand, p);
			if (newDist < dist)
			{
				dist = newDist;
				index = i;
			}
		}

		return index;
	}

	/// <summary>
	/// Return the new point that lies on the line created by the new random point and its nearest neighbour that is a maximum of <see cref="MaxDist"/> away from <paramref name="_nnIndex"/>.
	/// </summary>
	/// <param name="_xRand">The newly created random point.</param>
	/// <param name="_nnIndex">The index of the nearest neighbour.</param>
	/// <returns>A point in between <paramref name="_xRand"/> and <paramref name="_nnIndex"/> or <paramref name="_xRand"/> if <see cref="MaxDist"/> is not reached.</returns>
	Vector3 NewState(Vector3 _xRand, int _nnIndex)
	{
		var dist = Vector3.Distance(_xRand, m_positions[_nnIndex]);
		if (dist > MaxDist)
		{
			var diff = _xRand - m_positions[_nnIndex];
			var norm = diff.normalized;
			return m_positions[_nnIndex] + norm * MaxDist;
		}

		return _xRand;
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
	/// Add the node to the <see cref="m_positions"/> list.
	/// </summary>
	/// <param name="_pos">The position to add.</param>
	void AddNode(Vector3 _pos)
	{
		m_positions.Add(_pos);
	}

	/// <summary>
	/// Add a connection from the nearest neighbour to the point added last.
	/// </summary>
	/// <param name="_nnIndex">The nearest neighbour to the newly added point.</param>
	void ConnectEdge(int _nnIndex)
	{
		ConnectEdge(m_positions[m_positions.Count - 1], m_positions[_nnIndex]);
	}

	/// <summary>
	/// Add a connection between two points.
	/// </summary>
	/// <param name="_start">The start of a connection.</param>
	/// <param name="_end">The end of a connection.</param>
	void ConnectEdge(Vector3 _start, Vector3 _end)
	{
		m_edges.Add(new Tuple<Vector3, Vector3>(_start, _end));
	}

	/// <summary>
	/// Check if we are close enough to the goal
	/// </summary>
	bool CheckGoal()
	{
		int ind = NearestNeighbour(Goal.transform.position);
		return Vector3.Distance(m_positions[ind], Goal.transform.position) < MaxDist;
	}

	/// <summary>
	/// Draw all gizmos to visualise what is happening.
	/// </summary>
	public void OnDrawGizmos()
	{
		if (m_positions == null)
		{
			return;
		}

		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(transform.position, 0.1f);
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(Goal.transform.position, 0.1f);
		Gizmos.color = Color.white;

		foreach (var pos in m_positions)
		{
			Gizmos.DrawSphere(pos, 0.05f);
		}

		if (m_positions.Count < 2)
		{
			return;
		}

		for (int i = 0; i < m_edges.Count; i++)
		{
			Gizmos.DrawLine(m_edges[i].Item1, m_edges[i].Item2);
		}
	}

	private List<Vector3> m_positions;
	private List<Tuple<Vector3, Vector3>> m_edges;
	private List<GameObject> m_obstacles = new List<GameObject>();
}
