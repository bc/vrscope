using UnityEngine;
 
public class Define2DMesh : MonoBehaviour {
    void Start ()
    {
        AnimationCurve ac = AnimationCurve.EaseInOut(0f,0f,1f,1f);
        
        Vector2[] vertices2D = new Vector2[] {
            new Vector2(0,0),
            new Vector2(0,50),
            new Vector2(50,50),
            new Vector2(50,100),
            new Vector2(0,100),
            new Vector2(0,150),
            new Vector2(150,150),
            new Vector2(150,100),
            new Vector2(100,100),
            new Vector2(100,50),
            new Vector2(150,50),
            new Vector2(150,0),
        };

        Points2MeshGameObject(vertices2D);
    }

    internal static GameObject Points2MeshGameObject(Vector2[] vertices2D)
    {
        // Use the Triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(vertices2D);
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[vertices2D.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0);
        }

        // Create the mesh
        Mesh msh = new Mesh();
        msh.vertices = vertices;
        msh.triangles = indices;
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        // Set up game object with mesh;
        var myGameObject = new GameObject();
        myGameObject.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = myGameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        if (filter != null) filter.mesh = msh;
        return myGameObject;
    }
}