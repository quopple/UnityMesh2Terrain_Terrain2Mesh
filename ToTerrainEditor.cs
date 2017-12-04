using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(ToTerrain))]
[CanEditMultipleObjects]
public class ToTerrainEditor : Editor
{

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Parse"))
        {
            GenTerrain();
        }
    }

    float GetWeight( float dist )
    {
        if (dist < 0) dist = -dist;

        if (dist < 1.0)
        {
            float distSQ = Mathf.Pow( dist,2);
            return 0.5f * distSQ * dist - distSQ + 2.0f / 3.0f;
        }
        else if (dist < 2.0f)
        {
            dist = 2.0f - dist;
            dist = Mathf.Pow(dist, 3);
            return dist / 6.0f;
        }
        else
        {
            return 0.0f;
        }
    }

    void GetNeighborsWeight( float u, float v, out float[] weightsX, out float[] weightsY )
    {
        weightsX = new float[4];
        weightsY = new float[4];

        weightsX[0] = GetWeight(1f + u);
        weightsX[1] = GetWeight(u);
        weightsX[2] = GetWeight(1f - u);
        weightsX[3] = GetWeight(2f - u);

        weightsY[0] = GetWeight(1f + v);
        weightsY[1] = GetWeight(v);
        weightsY[2] = GetWeight(1f - v);
        weightsY[3] = GetWeight(2f - v);
    }

    void Get4x4Neighbors( int X, int Y, Vector3[] src, int cols ,int rows, out float[,] neighbors)
    {
        int[] xx = new int[4] { -1, 0, 1, 2 };
        int[] yy = new int[4] { -1, 0, 1, 2 }; //邻域坐标单位距离

        //边界处理
        if ((X - 1) < 0) xx[0] = 0;
        if ((X + 1) >= cols) xx[2] = 0;
        if ((X + 2) >= cols) xx[3] = ((xx[2] == 0) ? 0 : 1);

        if ((Y - 1) < 0) yy[0] = 0;
        if ((Y + 1) >= rows) yy[2] = 0;
        if ((Y + 2) >= rows) yy[3] = ((yy[2] == 0) ? 0 : 1);

        //邻域高度值
        neighbors = new float[4, 4];
        for (int r = 0; r < 4; ++r)
        {
            for (int c = 0; c < 4; ++c)
            {
                neighbors[c, r] = src[(Y + yy[r]) * cols + (X + xx[c])].y;
            }
        }
    }

    float CubicInterpolation(float fx, float fy, Vector3[] src, int cols, int rows)
    {
        int X, Y;
        X = Mathf.FloorToInt(fx);
        Y = Mathf.FloorToInt(fy);

        float[,] neighbors;
        Get4x4Neighbors(X, Y, src, cols, rows, out neighbors);

        float u, v;
        u = fx - X;
        v = fy - Y;
        //邻域值的权重
        float[] weightsX;
        float[] weightsY;
        GetNeighborsWeight(u, v, out weightsX, out weightsY);

        float[] tRes = new float[4] { 0f, 0f, 0f, 0f };

        for (int r = 0; r < 4; ++r)
        {
            for (int c = 0; c < 4; ++c)
            {
                tRes[r] += neighbors[c, r] * weightsX[c];
            }
        }

        return tRes[0] * weightsY[0] + tRes[1] * weightsY[1] + tRes[2] * weightsY[2] + tRes[3] * weightsY[3];
    }

    void Scale( int orgCols, int orgRows, Vector3[] org, int dstCols, int dstRows, out float[,] dst )
    {
        dst = new float[dstRows, dstCols];

        float scaleX = orgCols * 1f / dstCols;
        float scaleY = orgRows * 1f / dstRows;

        float xf, yf;

        for( int r = 0; r < dstRows; ++r )
        {
            yf = r * scaleY;
            for (int c = 0; c < dstCols; ++c)
            {
                xf = c * scaleX;

                if ((int)xf == xf && (int)yf == yf)
                    dst[r, c] = org[ (int)(yf * orgCols + xf)].y;
                else
                    dst[r, c] = CubicInterpolation(xf, yf, org, orgCols, orgRows);
            }
        }
    }

    private void GenTerrain()
    {

        Mesh mesh = Selection.activeGameObject.GetComponent<MeshFilter>().sharedMesh;
        mesh.RecalculateBounds();
        Bounds bound = mesh.bounds;
        Vector3[] vec = mesh.vertices;

        TerrainData terrain = new TerrainData();
        terrain.heightmapResolution = 1024;

        GameObject terrainObject = Terrain.CreateTerrainGameObject(terrain);

        terrain.size = bound.size;

        Debug.Log(vec.Length);
        Debug.Log(terrain.heightmapWidth + "   "  +terrain.heightmapHeight + "   reso:" + terrain.heightmapResolution);
        Debug.Log(terrain.size.ToString() );
        Debug.Log(terrain.heightmapScale.ToString());
        Debug.Log(terrainObject.transform.position);


        float minH, maxH;
        minH = float.MaxValue;
        maxH = float.MinValue;

        for (int y = 0; y < 201; ++y)
        {
            for (int x = 0; x < 101; ++x)
            {
                if (vec[y * 101 + x].y > maxH) maxH = vec[y * 101 + x].y;
                if (vec[y * 101 + x].y < minH) minH = vec[y * 101 + x].y;
            }
        }

        Debug.Log("min:" + minH + "  max:" + maxH);
        if (minH != maxH)
        {
            float[,] heights;
            Scale(101, 201, vec, terrain.heightmapWidth, terrain.heightmapHeight,out heights);

            for (int r = 0; r < terrain.heightmapHeight; ++r)
            {
                for (int c = 0; c < terrain.heightmapWidth; ++c)
                {
                    //       Vector2 index = new Vector2(x * bound.size.x / 100 / terrain.size.x * (terrain.heightmapWidth - 1),
                    //          y * bound.size.z / 200 / terrain.size.z * (terrain.heightmapHeight - 1));
                    //     heights[(int)index.y, (int)index.x] = vec[y * 101 + x].y / terrain.heightmapScale.y;

                    heights[r,c] = ( heights[r, c] - minH )/ terrain.heightmapScale.y;
                }
            }
            terrain.SetHeights(0, 0, heights);
        }
    }
}
