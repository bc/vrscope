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
    public List<Vector2> xyCurve;
    public float multiplier = 1f;
    public Material auc;
    private GameObject aucMesh;

    private void LateUpdate()
    {
        DestroyGameObjects(points);
        Destroy(aucMesh);
        PlotIt(xyCurve);
    }

    internal void PlotIt(List<Vector2> curve)
    {
        
        float[] xBounds = new float[]{curve.Min(v => v.x),curve.Max(v => v.x)};
        float[] yBounds = new float[]{curve.Min(v => v.y),curve.Max(v => v.y)};
        float xRange = xBounds[1] - xBounds[0];
        float yRange = yBounds[1] - yBounds[0];
        var localScale = transform.localScale;
        
        foreach (var key in curve)
        {
           
            var normalizedPos = new Vector3(
                0.5f*localScale.x*(key.x - xRange/2f)/xRange,
                0.5f*localScale.y*(key.y - yRange/2f)/yRange, 
                0f) ;
            points.Add(DrawPoint(normalizedPos * multiplier));
        }

        aucMesh = DrawFilledMesh();
        Instantiate(aucMesh,transform.TransformVector(transform.position),Quaternion.identity);
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

    private GameObject DrawPoint(Vector3 pos, float forwardDisplacement = 0.01f)
    {
        var point = Instantiate(pointPrefab, transform.position + pos, Quaternion.identity);
        point.transform.localScale = Vector3.one * 0.01f;
        point.transform.position += Vector3.back * forwardDisplacement;
        
        return point;
    }

    private GameObject DrawFilledMesh()
    {
        var newMesh = Define2DMesh.Points2MeshGameObject(xyCurve.ToArray());
        newMesh.GetComponent<Renderer>().material = auc;
        return newMesh;
    }
}

