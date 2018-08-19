using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ClampMovement : MonoBehaviour
{

    public float MinX;
    public float MaxX;
    public float MinY;
    public float MaxY;

    private void Awake()
    {
        this._transform = this.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        var position = this._transform.position;
        this._transform.position = new Vector3(
            math.clamp(position.x, this.MinX, this.MaxX),
            math.clamp(position.y, this.MinY, this.MaxY),
            position.z);
    }

    private Transform _transform;

}
