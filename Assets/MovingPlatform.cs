using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    private Vector3 OriginalPosition;
    public float MovementRange;

	// Use this for initialization
	void Start ()
	{
	    this.OriginalPosition = this.transform.position;
	}

    private void Update()
    {
        var positionModifier = new Vector3(Mathf.PingPong(Time.time, this.MovementRange), 0, 0);
        this.transform.position = this.OriginalPosition + positionModifier;
    }


}
