using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRRT
{
	void Init(Vector3 _start, Vector3 _goal);
	bool Step(List<Tree> _trees);
}
