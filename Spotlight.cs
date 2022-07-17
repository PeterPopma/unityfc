using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spotlight : MonoBehaviour
{
    new Light light;

    // Start is called before the first frame update
    void Start()
    {
        light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = (Time.time*20) %80 - 40;
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
        light.intensity = 250 - Mathf.Abs(x)*12.5f;
    }
}
