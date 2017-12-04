using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class Example : MonoBehaviour {


    private Vector3[] vertices;
    private int[] triangles;        //三角形索引 


    /*  Mesh属性 
     *      长宽 
     *      段数 
     *      高度 
     */
    private Vector2 size;
    private Vector2 segment;

    private GameObject mMesh;
    public Material mMaterial;


    private int[] GetTriangles()
    {
        int sum = Mathf.FloorToInt(segment.x * segment.y * 6);//三角形顶点总数：假设是1*1的网格，会有2个顶点复用，因此是6个顶点。假设是2*2的网格，则是4个1*1的网格，即4*6即2*2*6！  
        triangles = new int[sum];
        uint index = 0;
        for (int i = 0; i < segment.y; i++)
        {
            for (int j = 0; j < segment.x; j++)
            {
                int role = Mathf.FloorToInt(segment.x) + 1;
                int self = j + (i * role);
                int next = j + ((i + 1) * role);
                //顺时针  

                //第一个三角形  
                triangles[index] = self;
                triangles[index + 1] = next + 1;
                triangles[index + 2] = self + 1;

                //第二个三角形  
                triangles[index + 3] = self;
                triangles[index + 4] = next;
                triangles[index + 5] = next + 1;
                index += 6;
            }
        }
        return triangles;
    }

    private void ComputeVertexs()
    {
        int sum = Mathf.FloorToInt((segment.x + 1) * (segment.y + 1));   // num of points
        float w = size.x / segment.x;
        float h = size.y / segment.y;

        GetTriangles();

        int index = 0;
        vertices = new Vector3[sum];
        for (int i = 0; i < segment.y + 1; ++i)
            for (int j = 0; j < segment.x + 1; ++j)
            {
                vertices[index] = new Vector3(j * w, 0, i * h );
                index++;
            }
    }

    private void DrawMesh()
    {
        mMesh.AddComponent<MeshFilter>();
        mMesh.AddComponent<MeshRenderer>();
        if (mMaterial)
            GetComponent<Renderer>().sharedMaterial = mMaterial;
        else
        {
            GetComponent<Renderer>().sharedMaterial = GetComponent<Renderer>().material;
            GetComponent<Renderer>().sharedMaterial.color = Color.white;
        }

        /*设置mesh*/
        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mMesh.GetComponent<MeshFilter>().sharedMesh = mesh;
    }


    private void CreateMesh(float width, float height, uint segmentX, uint segmentY, int min, int max)
    {
        size = new Vector2(width, height);
        segment = new Vector2(segmentX, segmentY);

        mMesh = gameObject;

        ComputeVertexs();
        DrawMesh();
    }


    // Use this for initialization
    void OnEnable()
    {
        CreateMesh(1000,2000 ,100 , 200, -10, 10);
    }

    


}
