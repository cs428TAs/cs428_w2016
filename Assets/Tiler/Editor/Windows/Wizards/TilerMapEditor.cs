using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using TileDraw;
using Tiler;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(TilerMap))]
public class TilerMapEditor : Editor
{
    private string _shader;
    private string _shaderError = string.Empty;

    public void OnEnable()
    {
        var map = (TilerMap)target;

        _shader = map.DefaultShader;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        var map = (TilerMap) target;

        _shader = EditorGUILayout.TextField("Shader", _shader);
        if (GUILayout.Button("Set"))
        {
            _shaderError = string.Empty;

            var shader = Shader.Find(_shader);
            if (shader == null)
                _shaderError = "Shader doesn't exist. Check spelling";
            else
                map.SetShader(shader);
        }

        EditorGUILayout.LabelField(_shaderError);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Colliders"))
        {
            GenerateColliders();
        }
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1
        GUI.enabled = InternalEditorUtility.HasPro();
#endif
        if (GUILayout.Button("Generate Navmesh"))
        {
            GenerateNavMesh();
        }
        GUI.enabled = true;

    }

    public void GenerateColliders()
    {
        var map = (TilerMap)target;

        var collectionSize = 1;
        foreach (var cell in map.Cells)
        {
            foreach (var t in cell.Tiles)
            {
                var size = t.Collision.Length;
                if (size > collectionSize)
                    collectionSize = size;
            }
        }

        collectionSize = (int)Mathf.Sqrt(collectionSize);

        foreach (var cell in map.Cells)
        {
            var tiles = cell.Tiles;

            var goTrans = cell.transform.Find("colliders");

            if (goTrans != null)
                DestroyImmediate(goTrans.gameObject);

            goTrans = new GameObject("colliders").transform;

            goTrans.parent = cell.transform;
            goTrans.localPosition = new Vector3();

            var collection = new bool[map.TilesPerCell, map.TilesPerCell][];

            for (var y = 0; y < map.TilesPerCell; y++)
            {
                for (var x = 0; x < map.TilesPerCell; x++)
                {
                    collection[y, x] = tiles[y * map.TilesPerCell + x].Collision;
                    
                }
            }

            var merged = Util.MergeArrays(collection, collectionSize);

            var c = new Combine();
            var r = c.FindRect(merged);

            var size = r.GetLength(0);
            var sizePerCollider = map.CellSize / size;
            var halfCellSize = map.CellSize / 2f;

            var offset = sizePerCollider / 2f;

            var p = new Point();

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var start = r[y, x];
                    if (start != p)
                    {
                        var xx = (x + x + start.X - 1) / 2f;
                        var yy = (y + y - start.Y + 1) / 2f;

                        var posX = sizePerCollider * xx - (halfCellSize - offset);
                        var posY = sizePerCollider * yy - (halfCellSize - offset);

                        var go = new GameObject("c");
                        go.isStatic = true;
                        go.transform.parent = goTrans;
                        var bc = go.AddComponent<BoxCollider>();

                        bc.size = new Vector3(start.X * sizePerCollider, 1, start.Y * sizePerCollider);

                        var goPos = new Vector3();
                        goPos.x = posX;
                        goPos.y = map.transform.position.y + 0.5f;
                        goPos.z = posY;

                        go.transform.localPosition = goPos;
                    }
                }
            }
        }
    }

    private void GenerateNavMesh()
    {
        var map = (TilerMap)target;

        var collectionSize = 1;
        foreach (var cell in map.Cells)
        {
            foreach (var t in cell.Tiles)
            {
                var size = t.Collision.Length;
                if (size > collectionSize)
                    collectionSize = size;
            }
        }

        collectionSize = (int)Mathf.Sqrt(collectionSize);

        foreach (var cell in map.Cells)
        {
            var tiles = cell.Tiles;

            var goTrans = cell.transform.Find("navmesh");

            if (goTrans != null)
                DestroyImmediate(goTrans.gameObject);

            goTrans = new GameObject("navmesh").transform;

            goTrans.parent = cell.transform;
            goTrans.localPosition = new Vector3();

            var collection = new bool[map.TilesPerCell, map.TilesPerCell][];

            for (var y = 0; y < map.TilesPerCell; y++)
            {
                for (var x = 0; x < map.TilesPerCell; x++)
                {
                    collection[y, x] = tiles[y * map.TilesPerCell + x].Collision;
                }
            }

            var merged = Util.MergeArrays(collection, collectionSize);

            var c = new Combine();
            var r = c.FindRect(merged);

            var size = r.GetLength(0);
            var sizePerCollider = map.CellSize / size;
            var halfCellSize = map.CellSize / 2f;

            var offset = sizePerCollider / 2f;

            var p = new Point();

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var start = r[y, x];
                    if (start != p)
                    {
                        var xx = (x + x + start.X - 1) / 2f;
                        var yy = (y + y - start.Y + 1) / 2f;

                        var posX = sizePerCollider * xx - (halfCellSize - offset);
                        var posY = sizePerCollider * yy - (halfCellSize - offset);

                        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        go.isStatic = true;
                        go.transform.parent = goTrans;

                        var goPos = new Vector3();
                        goPos.x = posX;
                        goPos.y = 0;
                        goPos.z = posY;

                        go.transform.localPosition = goPos;
                        go.transform.localScale = new Vector3(start.X * sizePerCollider, 1, start.Y * sizePerCollider);

                        GameObjectUtility.SetNavMeshLayer(go, 1);
                    }
                }
            }
        }

        NavMeshBuilder.ClearAllNavMeshes();
        NavMeshBuilder.BuildNavMesh();



        foreach (var cell in map.Cells)
        {
            var goTrans = cell.transform.Find("navmesh");
            DestroyImmediate(goTrans.gameObject);
        }
    }
}