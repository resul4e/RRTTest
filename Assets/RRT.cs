using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RRT : MonoBehaviour
{
	public Vector2 Range;
	public float MaxDist = 0.4f;
	public int IterationsPerStep = 10;
	public GameObject Prefab;

	// Start is called before the first frame update
	void Start()
	{
		m_positions.Add(transform.position);

		for (int i = 0; i < 25; i++)
		{
			var obs = Instantiate(Prefab, new Vector3(Random.Range(-Range.x, Range.x), Random.Range(-Range.y, Range.y), 0), Quaternion.identity);
			m_obstacles.Add(obs);
		}

	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			for (int i = 0; i < IterationsPerStep; i++)
			{
				var xRand = RandomState();
				var xNearIndex = NearestNeighbour(xRand);
				var xNew = NewState(xRand, xNearIndex);
				foreach (var obs in m_obstacles)
				{
					xNew = BoxLineIntersect(obs, m_positions[xNearIndex], xNew);
				}

				AddNode(xNew);
				ConnectEdge(xNearIndex);
			}
		}
	}

	Vector3 RandomState()
	{
		return new Vector3(Random.Range(-Range.x, Range.x), Random.Range(-Range.y, Range.y), 0);
	}

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

	void AddNode(Vector3 _pos)
	{
		m_positions.Add(_pos);
	}

	void ConnectEdge(int _nnIndex)
	{
		m_edges.Add(new Tuple<Vector3, Vector3>(m_positions[m_positions.Count - 1], m_positions[_nnIndex]));
	}

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

	public void OnDrawGizmos()
	{
		if (m_positions.Count < 2)
		{
			return;
		}

		foreach (var pos in m_positions)
		{
			Gizmos.DrawSphere(pos, 0.05f);
		}

		for (int i = 0; i < m_edges.Count; i++)
		{
			Gizmos.DrawLine(m_edges[i].Item1, m_edges[i].Item2);
		}
	}

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

	private List<Vector3> m_positions = new List<Vector3>();
	private List<Tuple<Vector3, Vector3>> m_edges = new List<Tuple<Vector3, Vector3>>();
	private List<GameObject> m_obstacles = new List<GameObject>();
}
