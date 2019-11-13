using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrawQ1Plot : MonoBehaviour
{
    public GameObject pointPrefab;
    public GameObject axisPrefab;
    public List<GameObject> points = new List<GameObject>();
    public List<GameObject> xAxisPoints = new List<GameObject>();
    public AnimationCurve ac;
    public float multiplier = 1f;

    private void LateUpdate()
    {
        DestroyGameObjects(points);
        PlotIt(ac);
    }

    internal void PlotIt(AnimationCurve ac)
    {
        float[] xBounds = new float[]{ac.keys.Min(x => x.time),ac.keys.Max(x => x.time)};
        float[] yBounds = new float[]{ac.keys.Min(x => x.value),ac.keys.Max(x => x.value)};
        float xRange = xBounds[1] - xBounds[0];
        float yRange = yBounds[1] - yBounds[0];
        var localScale = transform.localScale;
        foreach (var key in ac.keys)
        {
           
            var normalizedPos = new Vector3(
                0.5f*localScale.x*(key.time - xRange/2f)/xRange,
                0.5f*localScale.y*(key.value - yRange/2f)/yRange, 
                0f) ;
            points.Add(DrawPoint(normalizedPos * multiplier));
        }
    }

    private void DestroyGameObjects(List<GameObject> vec)
    {
        foreach (var x in points)
        {
            Destroy(x);
        }
        vec.Clear();
    }

    private GameObject DrawAxis(float xValue, float value)
    {
        var xAxis = Instantiate(axisPrefab, transform.position + Vector3.right * value, Quaternion.identity);
        xAxis.GetComponent<AxisLabeler>().axisValue = xValue.ToString("F0");
        return xAxis;
    }

    private GameObject DrawPoint(Vector3 pos)
    {
        var point = Instantiate(pointPrefab, transform.position + pos, Quaternion.identity);
        point.transform.localScale = Vector3.one * 0.01f;
        point.transform.position += Vector3.back * 0.1f;
        return point;
    }
}

