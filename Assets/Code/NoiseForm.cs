﻿using UnityEngine;
using System.Collections;

public class NoiseForm : MonoBehaviour {

    public Vector3 size; 
    public Vector2 cellCount; // The number of slices on each axis
    public Vector2 noiseStart;
    public Vector2 noiseDelta;
    public Color color;

    Vector2 sliceSize; // The size of each slice

    Vector3[] initialVertices;
    Vector3[] initialNormals;
    Vector2[] meshUv;
    Color[] colours;
    int[] meshTriangles;
    Vector2[] uvSeq = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1f, 1)
                                      , new Vector2(1f, 1), new Vector2(1f, 0), new Vector2(0, 0)
                                };
    [HideInInspector]
    public float maxY;

    private bool generated = false;
    TextureGenerator textureGenerator;

    public static Color HexToColor(string hex)
    {
        hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
        hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
        byte a = 255;//assume fully visible unless specified in hex
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        //Only use alpha if the string has enough characters
        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color32(r, g, b, a);
    }

    public NoiseForm()
    {
        size = new Vector3(100, 100, 100);
        cellCount = new Vector3(10, 10);
        noiseStart = new Vector2(0, 0);
        noiseDelta = new Vector2(0.1f, 0.1f);

        color = HexToColor("0x2A59AD");
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, size);
    }

    void MaxY(float y)
    {
        if (y > maxY)
        {
            maxY = y;
        }
    }

    public void Generate()
    {
        if (generated)
        {
            return;
        }
        sliceSize = new Vector2(size.x / cellCount.x, size.z / cellCount.y);

        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = true;
        if (renderer == null)
        {
            Debug.Log("Renderer is null 1");
        }

        Mesh mesh = gameObject.AddComponent<MeshFilter>().mesh;
        mesh.Clear();

        int verticesPerSegment = 6;

        int vertexCount = verticesPerSegment * ((int)cellCount.x) * ((int)cellCount.y);
        
        initialVertices = new Vector3[vertexCount];
        initialNormals = new Vector3[vertexCount];
        meshUv = new Vector2[vertexCount];
        meshTriangles = new int[vertexCount];
        colours = new Color[vertexCount];

        Vector3 bottomLeft = -(size / 2);

        Vector2 noiseXY = noiseStart;
        int vertex = 0;
        float noiseHeight = size.y;

        for (int z = 0; z < cellCount.y; z++)
        {
            noiseXY.x = noiseStart.x;
            for (int x = 0; x < cellCount.x; x++)
            {

                int startVertex = vertex;
                // Calculate some stuff
                Vector3 sliceBottomLeft = bottomLeft + new Vector3(x * sliceSize.x, Mathf.PerlinNoise(noiseXY.x, noiseXY.y) * noiseHeight, z * sliceSize.y);
                Vector3 sliceTopLeft = bottomLeft + new Vector3(x * sliceSize.x, Mathf.PerlinNoise(noiseXY.x, noiseXY.y + noiseDelta.y) * noiseHeight, (z + 1) * sliceSize.y);
                Vector3 sliceTopRight = bottomLeft + new Vector3((x + 1) * sliceSize.x, Mathf.PerlinNoise(noiseXY.x + noiseDelta.x, noiseXY.y + noiseDelta.y) * noiseHeight, (z + 1) * sliceSize.y);
                Vector3 sliceBottomRight = bottomLeft + new Vector3((x + 1) * sliceSize.x, Mathf.PerlinNoise(noiseXY.x + noiseDelta.x, noiseXY.y) * noiseHeight, z * sliceSize.y);
                MaxY(sliceTopLeft.y); MaxY(sliceTopRight.y);
                    
                // Make the vertices
                initialVertices[vertex++] = sliceBottomLeft;
                initialVertices[vertex++] = sliceTopLeft;
                initialVertices[vertex++] = sliceTopRight;
                initialVertices[vertex++] = sliceTopRight;
                initialVertices[vertex++] = sliceBottomRight;
                initialVertices[vertex++] = sliceBottomLeft;

                    // Make the normals, UV's and triangles                
                for (int i = 0; i < 6; i++)
                {
                    //initialNormals[startVertex + i] = (i < 6) ? Vector3.forward : -Vector3.forward;
                    meshUv[startVertex + i] = uvSeq[i % 6];
                    meshTriangles[startVertex + i] = startVertex + i;
                }                
                noiseXY.x += noiseDelta.x;
            }
            noiseXY.y += noiseDelta.y;
        }

        for (int i = 0; i < meshUv.Length; i++)
        {
            Vector3 v = initialVertices[i] + (size / 2.0f);
            meshUv[i] = new Vector2(v.x / size.x, v.z / size.z);
        }

        mesh.vertices = initialVertices;
        mesh.uv = meshUv;
        mesh.triangles = meshTriangles;
        mesh.RecalculateNormals();

        //renderer.material.color = color;

        Shader shader = Shader.Find("Diffuse");
        Material material = new Material(shader);
        
        textureGenerator = GetComponent<TextureGenerator>();
        if (textureGenerator != null)
        {
            material.mainTexture = textureGenerator.GenerateTexture();
        }
        else
        {
            material.color = color;
        }
        renderer.material = material;
        generated = true;
    }
	
	void Start () {
        Generate();	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
