using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System.IO;

[CustomEditor(typeof(ToMesh))]
[CanEditMultipleObjects]
public class ToMeshEditor : Editor{

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Parse"))
        {
            GenMesh();
        }
    }

    void GenMesh()
    {
        Terrain terrainObject = Selection.activeGameObject.GetComponent<Terrain>() ;

        TerrainData terrain = terrainObject.terrainData;

        int dstCols, dstRows;
        dstCols = 101;
        dstRows = 201;

        Vector3[] vertices = new Vector3[dstRows * dstCols];
        Vector3 size = terrain.size;
        Vector3 point = new Vector3();

        Debug.Log("size  " + size.ToString());
        Debug.Log("scale " + terrain.heightmapScale.ToString());
        Debug.Log("reso " + terrain.heightmapResolution);

        string txt = "";
        string sample = "";
        for( int y = 0; y < dstRows; ++y )
        {
            for( int x = 0; x < dstCols; ++x )
            {
                Vector2 index = new Vector2(x * size.x / 100 / terrain.size.x * (terrain.heightmapWidth - 1),
                          y * size.z / 200 / terrain.size.z * (terrain.heightmapHeight - 1));

                point.x = x * size.x / 100;
                point.z = y * size.z / 200;
                point.y = terrain.GetInterpolatedHeight(index.x/terrain.heightmapWidth ,index.y/terrain.heightmapHeight);// * terrain.heightmapScale.y ;

                vertices[y * dstCols + x] = point;

                txt += vertices[y * dstCols + x].ToString() + "  ;";
                sample += terrain.GetInterpolatedHeight(index.x / terrain.heightmapWidth, index.y / terrain.heightmapHeight) + "  ;";
            }
            txt += "\n";
            sample += "\n";
        }

        File.WriteAllText("./Log.txt", txt);
        File.WriteAllText("./Log1.txt", sample);

        GameObject obj = new GameObject("MeshHolder");

        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshRenderer>();

        obj.GetComponent<MeshRenderer>().sharedMaterial = obj.GetComponent<MeshRenderer>().material;
        obj.GetComponent<MeshRenderer>().sharedMaterial.color = Color.white;

        int sum = Mathf.FloorToInt(( dstCols -1 ) * ( dstRows -1 ) * 6);//三角形顶点总数：假设是1*1的网格，会有2个顶点复用，因此是6个顶点。假设是2*2的网格，则是4个1*1的网格，即4*6即2*2*6！  
        int[] triangles = new int[sum];
        uint currIndex = 0;
        for (int i = 0; i < dstRows-1; i++)
        {
            for (int j = 0; j < dstCols-1; j++)
            {
                int role = dstCols;
                int self = j + (i * role);
                int next = j + ((i + 1) * role);
                //顺时针  

                //第一个三角形  
                triangles[currIndex] = self;
                triangles[currIndex + 1] = next + 1;
                triangles[currIndex + 2] = self + 1;

                //第二个三角形  
                triangles[currIndex + 3] = self;
                triangles[currIndex + 4] = next;
                triangles[currIndex + 5] = next + 1;
                currIndex += 6;
            }
        }

        Mesh mesh = new Mesh(); 
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        obj.GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}
