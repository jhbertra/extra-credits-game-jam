using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    public float Strength;

    private Vector3 _lastParentPosition;

    private void Awake()
    {
        this._lastParentPosition = this.transform.parent.position;
    }

    // Update is called once per frame
    void Update()
    {
        var delta = this.transform.parent.position - this._lastParentPosition;
        this._lastParentPosition = this.transform.parent.position;

        this.transform.localPosition -= delta * this.Strength;
    }
}
