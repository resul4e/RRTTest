using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class RRTController : MonoBehaviour
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

	void Restart()
	{
		foreach (var obs in m_obstacles)
		{
			Destroy(obs.gameObject);
		}

		m_obstacles = new List<GameObject>(25);

		//position the start and end somewhere
		transform.position = new Vector3(Random.Range(-Range.x, Range.x), Random.Range(-Range.y, Range.y));
		Goal.transform.position = new Vector3(Random.Range(-Range.x, Range.x), Random.Range(-Range.y, Range.y));

		m_startTree = new Tree();
		m_startTree.AddNode(transform.position);

		m_goalTree = new Tree();
		m_goalTree.AddNode(Goal.transform.position);

		//Spawn 25 obstacles at random location within the given Range.
		for (int i = 0; i < 25; i++)
		{
			var obs = Instantiate(ObstaclePrefab, new Vector3(Random.Range(-Range.x, Range.x), Random.Range(-Range.y, Range.y), 0), Quaternion.identity);
			m_obstacles.Add(obs);
		}

		if (true)
		{
			//m_rrtAlgo = new RTTConnect();
			m_trees.Add(m_startTree);
			m_trees.Add(m_goalTree);
		}
	}

	// Update is called once per frame
	void Update()
    {
		if (Input.GetKeyDown(KeyCode.R))
		{
			Restart();
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (m_rrtAlgo.Step(m_trees))
			{
				Debug.Log("REACHED");
			}
		}
    }

	public void OnDrawGizmos()
	{
		if (m_startTree == null)
		{
			return;
		}

		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(transform.position, 0.1f);
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(Goal.transform.position, 0.1f);
		Gizmos.color = Color.white;

		var node = m_startTree.Root;
		while(node.Child != null)
		{ 
			Gizmos.DrawSphere(node.Position, 0.05f);
			node = node.Child;
		}

		node = m_goalTree.Root;
		while (node.Child != null)
		{
			Gizmos.DrawSphere(node.Position, 0.05f);
			node = node.Child;
		}
	}

	private Tree m_startTree;
	private Tree m_goalTree;
	private List<GameObject> m_obstacles = new List<GameObject>();

	private IRRT m_rrtAlgo;
	private List<Tree> m_trees = new List<Tree>();
}
