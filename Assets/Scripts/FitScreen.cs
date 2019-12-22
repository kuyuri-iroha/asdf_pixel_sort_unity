using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitScreen : MonoBehaviour
{
    [SerializeField]
    new Camera camera = null;

    [SerializeField]
    float distance = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        if(camera == null)
        {
            camera = Camera.main;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = camera.transform.forward.normalized * distance + camera.transform.position;

        transform.LookAt(camera.transform.position);

        const float PLANE_SCALE = 0.1f;
        var frustumHeight = 2.0f * distance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * PLANE_SCALE;
        var frustumWidth = frustumHeight * camera.aspect;

        var scale = new Vector3( frustumWidth, frustumHeight, 1.0f );
        transform.localScale = scale;
    }
}
