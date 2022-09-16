using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DataProcessing.VisualRestrictor
{
    [System.Serializable]
    class SerializableMeshInfo
    {
        [SerializeField] public float[] vertices;
        [SerializeField] public int[] triangles;
        [SerializeField] public float[] uv;
        [SerializeField] public float[] uv2;
        [SerializeField] public float[] normals;
        [SerializeField] public Color[] colors;

// Constructor: takes a mesh and fills out SerializableMeshInfo data structure which basically mirrors Mesh object's parts.
        public SerializableMeshInfo(Mesh m)
        {
            vertices = new float[m.vertexCount * 3]; // initialize vertices array.
            for (int i = 0; i < m.vertexCount; i++) // Serialization: Vector3's values are stored sequentially.
            {
                vertices[i * 3] = m.vertices[i].x;
                vertices[i * 3 + 1] = m.vertices[i].y;
                vertices[i * 3 + 2] = m.vertices[i].z;
            }

            triangles = new int[m.triangles.Length]; // initialize triangles array
            for (int i = 0;
                i < m.triangles.Length;
                i++) // Mesh's triangles is an array that stores the indices, sequentially, of the vertices that form one face
            {
                triangles[i] = m.triangles[i];
            }

            uv = new float[m.uv.Length * 2]; // initialize uvs array
            for (int i = 0; i < m.uv.Length; i++) // uv's Vector2 values are serialized similarly to vertices' Vector3
            {
                uv[i * 2] = m.uv[i].x;
                uv[i * 2 + 1] = m.uv[i].y;
            }

            uv2 = new float[m.uv2.Length]; // uv2
            for (int i = 0; i < m.uv2.Length; i++)
            {
                uv[i * 2] = m.uv2[i].x;
                uv[i * 2 + 1] = m.uv2[i].y;
            }

            normals = new float[m.normals.Length]; // normals are very important
            for (int i = 0; i < m.normals.Length; i++) // Serialization
            {
                normals[i * 3] = m.normals[i].x;
                normals[i * 3 + 1] = m.normals[i].y;
                normals[i * 3 + 2] = m.normals[i].z;
            }

            colors = new Color[m.colors.Length];
            for (int i = 0; i < m.colors.Length; i++)
            {
                colors[i] = m.colors[i];
            }
        }

        // GetMesh gets a Mesh object from currently set data in this SerializableMeshInfo object.
        // Sequential values are deserialized to Mesh original data types like Vector3 for vertices.
        public Mesh GetMesh()
        {
            Mesh m = new Mesh();
            List<Vector3> verticesList = new List<Vector3>();
            for (int i = 0; i < vertices.Length / 3; i++)
            {
                verticesList.Add(new Vector3(
                    vertices[i * 3], vertices[i * 3 + 1], vertices[i * 3 + 2]
                ));
            }

            m.SetVertices(verticesList);
            m.triangles = triangles;
            List<Vector2> uvList = new List<Vector2>();
            for (int i = 0; i < uv.Length / 2; i++)
            {
                uvList.Add(new Vector2(
                    uv[i * 2], uv[i * 2 + 1]
                ));
            }

            m.SetUVs(0, uvList);
            List<Vector2> uv2List = new List<Vector2>();
            for (int i = 0; i < uv2.Length / 2; i++)
            {
                uv2List.Add(new Vector2(
                    uv2[i * 2], uv2[i * 2 + 1]
                ));
            }

            m.SetUVs(1, uv2List);
            List<Vector3> normalsList = new List<Vector3>();
            for (int i = 0; i < normals.Length / 3; i++)
            {
                normalsList.Add(new Vector3(
                    normals[i * 3], normals[i * 3 + 1], normals[i * 3 + 2]
                ));
            }

            m.SetNormals(normalsList);
            m.colors = colors;

            return m;
        }
    }

    public static class MeshDumper
    {
        /// <summary>
        /// Creates a binary dump of a mesh
        /// </summary>
        public static void MeshDump(Mesh mesh, string path)
        {
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf =
                new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create);
            SerializableMeshInfo smi = new SerializableMeshInfo(mesh);
            bf.Serialize(fs, smi);
            fs.Close();
        }

        /// <summary>
        /// Loads a mesh from a binary dump
        /// </summary>
        public static Mesh MeshLoad(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                throw new Exception("meshFile.dat file does not exist.");
            }

            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf =
                new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open);
            SerializableMeshInfo smi = (SerializableMeshInfo) bf.Deserialize(fs);
            Mesh res = smi.GetMesh();
            fs.Close();

            return res;
        }
    }
}