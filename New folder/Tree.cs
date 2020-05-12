using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree
{
	public Node Root;

	public void AddNode(Vector3 _position)
	{
		var node = new Node();
		node.Position = _position;
		if (m_nodes.Count >= 1)
		{
			node.Parent = NearestNeighbour(_position);
		}

		if (node.Parent != null)
		{
			node.Parent.Child = node;
		}
		else
		{
			Root = node;
		}

		m_nodes.Add(node);
	}

	public Node NearestNeighbour(Vector3 _position)
	{
		var dist = float.MaxValue;
		int index = -1;
		for (int i = 0; i < m_nodes.Count; i++)
		{
			var p = m_nodes[i];
			var newDist = Vector3.Distance(_position, p.Position);
			if (newDist < dist)
			{
				dist = newDist;
				index = i;
			}
		}

		return m_nodes[index];
	}


	private List<Node> m_nodes = new List<Node>();
}
