using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Visuals
{
    public class Test : MonoBehaviour
    {
        // How many meshes to draw.
        public int population;
        // Range to draw meshes within.
        public float range;

        // Material to use for drawing the meshes.
        public Material material;

        private Matrix4x4[] matrices;
        private MaterialPropertyBlock block;

        private Mesh mesh;

        private void Setup()
        {
            Mesh mesh = CreateQuad(
                50,50,
                100,50,
                100,100,
                50,100);
            this.mesh = mesh;

            matrices = new Matrix4x4[population];
            Vector4[] colors = new Vector4[population];

            block = new MaterialPropertyBlock();

            for (int i = 0; i < population; i++)
            {
                // Build matrix.
                Vector3 position = new Vector3(0,0,0);
                Quaternion rotation = Quaternion.Euler(0,0, 0);
                Vector3 scale = Vector3.one;

                matrices[i] = Matrix4x4.TRS(position, rotation, scale);
                
                colors[i] = Color.Lerp(Color.red, Color.blue, Random.value);
            }

            // Custom shader needed to read these!!
            block.SetVectorArray("_Colors", colors);
            material.enableInstancing = true;
        }

        private Mesh CreateQuad(
            float x1, float y1, 
            float x2, float y2,
            float x3, float y3,
            float x4, float y4 )
        {
            // Create a quad mesh.
            var mesh = new Mesh();
            
            var vertices = new Vector3[4] {
            new Vector3(x1, y1, 0),
            new Vector3(x2, y2, 0),
            new Vector3(x3, y3, 0),
            new Vector3(x4, y4, 0)
        };
            

            var tris = new int[6] {
            // lower left tri.
            0, 2, 1,
            // lower right tri
            0, 3, 2
        };

            var normals = new Vector3[4] {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
        };

            var uv = new Vector2[4] {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
        };

            mesh.vertices = vertices;
            mesh.triangles = tris;
            mesh.normals = normals;
            mesh.uv = uv;

            return mesh;
        }

        private void Start()
        {
            Setup();
        }

        private void Update()
        {
        }

        public void DrawTest()
        {
            // Draw a bunch of meshes each frame.
            Graphics.DrawMeshInstanced(mesh, 0, material, matrices, population, block);
        }
    }
}
