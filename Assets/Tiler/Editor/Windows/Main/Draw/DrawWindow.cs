using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TileDraw;
using Tiler;
using Tiler.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

public sealed class DrawWindow : Section
{
    private static DateTime _lastSave;

    private const float BrushPreviewSize = 128;
    private const int RightWidth = 66 * 3 + 2 * WindowPadding + ScrollBarWidth;
    private const int BrushHeight = RightWidth + WindowTitle;
    
    private Camera _camera;
    private Light _light;
    private GUIText _text;
    private float _height = 2;

    private int _mapIndex = -1;
    private List<TilerMap> _tilerMaps;

    private Point _currentPoint = new Point(-1, -1);
    private Point _lastPlaced = new Point(-1, -1);
    private bool _isPainting;

    // Right click copy
    private Point _startCopyPoint;
    private bool _isDraggingCopy;

    // windows
    private DrawTileSetWindow _drawTileSetWindow;
    private BrushWindow _brushWindow;

    public IDrawTool DrawTool { get; private set; }

    public TilerMap TilerMap { get; private set; }
    public TilerMapEdit TilerMapEditFunctions { get; private set; }

    private TilerMap _newMap;

    private int _mapMask = int.MaxValue;

    public DrawWindow(TilerWindow window) 
        : base(window)
    {
        OnEnable();
    }
    
    public override void OnEnable()
    {
        base.OnEnable();

        _lastSave = DateTime.Now;

        DrawTool = new PaintTool(this, null);

        UnityInternal.AddGlobalEventHandler(GlobalInputHack);
        EditorApplication.hierarchyWindowChanged += OnHierachyChange;

        _drawTileSetWindow = new DrawTileSetWindow(this);
        _brushWindow = new BrushWindow(this);

        RefreshMaps();

        var cameraGo = GameObject.Find("TilerCamera");
        if (cameraGo == null)
        {
            cameraGo = EditorUtility.CreateGameObjectWithHideFlags("TilerCamera", (HideFlags)13, new[] { typeof(Camera) });
            cameraGo.AddComponent<GUILayer>();
            cameraGo.AddComponent<FlareLayer>();
            UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(cameraGo, "Assets/Tiler/Editor/Windows/Main/Draw/DrawWindow.cs (78,13)", "HaloLayer");
        }
        _camera = cameraGo.GetComponent<Camera>();
        _camera.enabled = false;
        _height = Mathf.Log(16);
        _camera.transform.eulerAngles = new Vector3(90, 0, 0);
        SetCameraHeight();

        var lightGo = GameObject.Find("TilerLight");
// ReSharper disable ConvertIfStatementToNullCoalescingExpression
        if (lightGo == null)
// ReSharper restore ConvertIfStatementToNullCoalescingExpression
            lightGo = EditorUtility.CreateGameObjectWithHideFlags("TilerLight", (HideFlags) 13, new[] {typeof (Light)});

        _light = lightGo.GetComponent<Light>();
        _light.type = LightType.Directional;
        _light.intensity = 0.5f;
        _light.enabled = false;

        var textGo = GameObject.Find("TilerText");
        if (textGo == null)
        {
            textGo = EditorUtility.CreateGameObjectWithHideFlags("TilerText", (HideFlags) 13, new[] {typeof (GUIText)});
            textGo.transform.position = new Vector3(0.5f, 1, 0);
        }

        
        _text = textGo.GetComponent<GUIText>();
        _text.anchor = TextAnchor.UpperCenter;
        _text.alignment = TextAlignment.Center;
        //_text.font.material.color = Color.white;
        //_text.text = "Remember to save. Last save more than 10 minutes ago.";
        _text.enabled = false;
    }
    public override void OnDisable(bool compiling)
    {
        base.OnDisable(compiling);

        UnityInternal.RemoveGlobalEventHandler(GlobalInputHack);
        EditorApplication.hierarchyWindowChanged -= OnHierachyChange;

        if (_camera)
            Object.DestroyImmediate(_camera.gameObject, true);
        if (_text)
            Object.DestroyImmediate(_text.gameObject, true);
        /* causes internal unity error and a crash for some reason, so we just keep it in scene hidden instead
        if (_light)
            Object.DestroyImmediate(_light.gameObject, true);*/

        _drawTileSetWindow.OnDisable();
    }
    public override void OnGUI()
    {
        var rect = new Rect
                       {
                           width = Parent.position.width - RightWidth, 
                           height = Parent.position.height
                       };

        GUILayout.BeginArea(rect, WindowNoTitleStyle);
        base.OnGUI();

        DrawMap();

        if (TilerMap != null)
        {
            DrawSelectionBox();
            Events();

            var time = (DateTime.Now - _lastSave);
            _text.enabled = (DateTime.Now - _lastSave).TotalMinutes >= 5;
            _text.text = string.Format("Last save {0} minutes ago.", time.Minutes);
        }

        GUILayout.EndArea();

        Windows();
    }
    public override void Destroy()
    {
        base.Destroy();

        if (DrawTool != null)
            DrawTool.GetBrush().Destroy();
    }
    public override void Close()
    {
        base.Close();

        /*if (_drawTileSetWindow)
            _drawTileSetWindow.Close();
        if(_brushWindow)
            _brushWindow.Close();*/

        Destroy();
    }

    public override void New()
    {
        var fullPath = Application.dataPath + TilerWindow.DataPath + "Maps/";

        // Check if directory exists, if not create it
        if (!Directory.Exists(fullPath))
            Directory.CreateDirectory(fullPath);

        var path = EditorUtility.SaveFilePanel("New Tileset", fullPath, "map", "prefab");
        if (String.IsNullOrEmpty(path))
            return;

        var index = path.IndexOf("Assets", StringComparison.Ordinal);
        if (index == -1)
        {
            Debug.Log("Path must be inside project asset directory");
            return;
        }

        var unityPath = path.Substring(index);

        var mmw = EditorWindow.GetWindow<TilerMapWizard>();
        mmw.Set(unityPath, OnNew);      
    }

    private void OnNew(Object obj)
    {
        _newMap = obj as TilerMap;

        if (_newMap != null)
        {
            _newMap.Layer = 0;

            // sort by their order
            _tilerMaps.Sort((x, y) => x.Layer.CompareTo(y.Layer));

            foreach (var map in _tilerMaps)
            {
                var l = map.Layer;

                //map.transform.position += new Vector3(0, l * 0.1f, 0);

                foreach (var cell in map.Cells)
                {
                    if (cell.GetComponent<Renderer>() && cell.GetComponent<Renderer>().sharedMaterial)
                        cell.GetComponent<Renderer>().sharedMaterial.renderQueue += l;
                }
            }

            // then reset the order
            for (int index = 0; index < _tilerMaps.Count; index++)
            {
                var sort = _tilerMaps[index];
                sort.Layer = (index + 1);
            }

            foreach (var map in _tilerMaps)
            {
                var l = map.Layer;
                //map.transform.position -= new Vector3(0,l * 0.1f, 0);

                foreach (var cell in map.Cells)
                {
                    if (cell.GetComponent<Renderer>() && cell.GetComponent<Renderer>().sharedMaterial)
                        cell.GetComponent<Renderer>().sharedMaterial.renderQueue -= l;
                }
            }
        }
    }

    public override void ToolBar()
    {
        base.ToolBar();

        GUILayout.Box("Misc", EditorStyles.toolbarDropDown);
        Rect miscRect = GUILayoutUtility.GetLastRect();
        if (MyGUI.ButtonMouseDown(miscRect))
        {
            var misc = PopupWindow.ShowAtPosition<MiscPopup>(new Rect(miscRect.x, miscRect.y + miscRect.height, 120f, 60));
            misc.Setup(TilerMap);
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUI.enabled = TilerMapEditFunctions.Undo.IsUndo();
        if (GUILayout.Button("<", EditorStyles.toolbarButton))
        {
            TilerMapEditFunctions.Undo.PerformUndo();
            Repaint();
        }
        GUI.enabled = TilerMapEditFunctions.Undo.IsRedo();
        if (GUILayout.Button(">", EditorStyles.toolbarButton))
        {
            TilerMapEditFunctions.Undo.PerformRedo();
            Repaint();
        }
        GUI.enabled = true;

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (MyGUI.HasToggledPositive(DrawTool is PaintTool, "P", EditorStyles.toolbarButton))
        {
            DrawTool = new PaintTool(this, DrawTool.GetBrush() as NormalBrush);
            Repaint();
        }
        if (MyGUI.HasToggledPositive(DrawTool is FillTool, "F", EditorStyles.toolbarButton))
        {
            DrawTool = new FillTool(this, DrawTool.GetBrush() as NormalBrush);
            Repaint();
        }
        if (MyGUI.HasToggledPositive(DrawTool is ReplaceTool, "R", EditorStyles.toolbarButton))
        {
            DrawTool = new ReplaceTool(this, DrawTool.GetBrush() as NormalBrush);
            Repaint();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("O", EditorStyles.toolbarButton))
        {
            var p = _camera.transform.position;
            p.x = 0;
            p.z = 0;
            _camera.transform.position = p;
        }

        EditorGUILayout.Space();

        //var currentIndex = _mapIndex;
        //_mapIndex = EditorGUILayout.Popup(_mapIndex, _mapNames, EditorStyles.toolbarPopup);

        //options = new[] { "CanJump", "CanShoot", "CanSwim" };
        //flags = EditorGUILayout.MaskField("Layers", 1, options, EditorStyles.toolbarPopup);

        GUILayout.Box("Layers", EditorStyles.toolbarDropDown);
        Rect last = GUILayoutUtility.GetLastRect();
        if (MyGUI.ButtonMouseDown(last))
        {
            var height = 20*_tilerMaps.Count + 20;// +25;

            var lp = PopupWindow.ShowAtPosition<LayerPopup>(new Rect(last.x, last.y + last.height, 150f, height));
            lp.Setup(_mapIndex, _mapMask, _tilerMaps, SetMapChange, SetMapMask, Save);
            GUIUtility.ExitGUI();
        }

        /*if (currentIndex != _mapIndex)
        {
            Save();
            SetMap(_mapIndex);
        }*/
    }

    private void Windows()
    {
        var windowRect = Parent.position;
        windowRect.x = windowRect.width - RightWidth;
        windowRect.width = RightWidth;
        windowRect.height -= BrushHeight;
        windowRect.y = 0;

        GUILayout.BeginArea(windowRect, WindowNoTitleStyle);
        _drawTileSetWindow.OnGUI();
        GUILayout.EndArea();

        windowRect.y += windowRect.height;
        windowRect.height = BrushHeight;

        GUILayout.BeginArea(windowRect, WindowNoTitleStyle);
        _brushWindow.OnGUI();
        GUILayout.EndArea();
    }

    private void DrawMap()
    {
        var rect = Parent.position;
        rect.x = 2;
        rect.y = ToolbarHeight + 2;
        rect.width -= RightWidth + 4;
        rect.height -= ToolbarHeight + 4;

        Handles.SetCamera(rect, _camera);

        if (Event.current.type == EventType.Repaint)
        {
            _light.transform.rotation = _camera.transform.rotation;
            InternalEditorUtility.SetCustomLighting(new[] {_light}, new Color(57.0f/500.0f, 0.125f, 0.133f, 1f));

            if (TilerMap != null)
            {
                var renders = Object.FindObjectsOfType(typeof (Renderer)).Cast<Renderer>().ToArray();
                var renderState = new Dictionary<Renderer, bool>();

                // Disable all renderers
                foreach (var r in renders)
                {
                    renderState[r] = r.enabled;
                    r.enabled = false;
                }

                for (var index = 0; index < _tilerMaps.Count; index++)
                {
                    if ((_mapMask & 1 << index) == 1 << index)
                    {
                        var tileMap = _tilerMaps[index];

                        foreach (Transform t in tileMap.transform)
                        {
                            if (t.GetComponent<Renderer>())
                            {
                                t.GetComponent<Renderer>().enabled = true;
                            }
                        }
                    }
                }

                var color = _text.font.material.color;
                _text.font.material.color = Color.red;
                Handles.DrawCamera(rect, _camera);
                _text.font.material.color = color;

                foreach (var r in renderState)
                {
                    r.Key.enabled = r.Value;
                }

            }

            InternalEditorUtility.RemoveCustomLighting();
        }
    }
    private void Events()
    {
        MouseEvents();

        var e = Event.current;

        if (e.type == EventType.MouseUp)
        {
            _isPainting = false;
        }
    }

    private void MouseEvents()
    {
        var e = Event.current;

        var rect = Parent.position;
        rect.x += 2;
        rect.y += ToolbarHeight + 2;
        rect.width -= RightWidth + 4;
        rect.height -= ToolbarHeight + 4;
        
        var mousePos = GUIUtility.GUIToScreenPoint(e.mousePosition);
        if (!rect.Contains(mousePos))
        {
            return;
        }

        // Left mouse button events
        if (Event.current.button == 0 && !NoRightMouseAltInput())
        {
            if (e.type == EventType.MouseDown)
            {
                TilerMapEditFunctions.Undo.NewUndo();

                Paint();
                _lastPlaced = _currentPoint;
                _isPainting = true;
            }
            if (e.type == EventType.MouseDrag)
            {
                if (_isPainting && _lastPlaced != _currentPoint)
                {
                    Paint();
                    _lastPlaced = _currentPoint;
                }
            }
        }

        // Right mouse button events
        if (Event.current.button == 1 || NoRightMouseAltInput())
        {
            if (e.type == EventType.MouseDown)
            {
                _isDraggingCopy = true;
                _startCopyPoint = _currentPoint;
            }
            if (e.type == EventType.MouseUp)
            {
                if (_isDraggingCopy)
                {
                    var properties = new List<Brush>();

                    var start = new Point
                                    {
                                        X = Math.Min(_currentPoint.X, _startCopyPoint.X),
                                        Y = Math.Min(_currentPoint.Y, _startCopyPoint.Y)
                                    };

                    var count = new Point
                                    {
                                        X = Math.Abs(_currentPoint.X - _startCopyPoint.X) + 1,
                                        Y = Math.Abs(_currentPoint.Y - _startCopyPoint.Y) + 1
                                    };

                    for (var y = start.Y; y < start.Y + count.Y; y++)
                    {
                        for (var x = start.X; x < start.X + count.X; x++)
                        {
                            var p = new Point(x, y);
                            properties.Add(TilerMap.GetTileBrush(p));
                        }
                    }

                    var copy = new CopyBrush(properties.ToArray(), new Point(count.X, count.Y));

                    if (DrawTool is PaintTool)
                    {
                        DrawTool.SetBrush(copy);
                    }
                    else
                    {
                        DrawTool = new PaintTool(this, copy);
                    }

                    Repaint();

                    _isDraggingCopy = false;
                }
            }
        }

        if (e.type == EventType.MouseDrag)
        {
            if (Event.current.button == 2)
            {
                PanWindow();
                Repaint();
            }
        }
        else if (e.type == EventType.scrollWheel)
        {
            _height += Event.current.delta.y / 10f;
            _height = Mathf.Clamp(_height, -1, 5);
            SetCameraHeight();
            Repaint();
        }

        if (e.type == EventType.MouseDrag || e.type == EventType.MouseMove || e.type == EventType.scrollWheel)
            GetGridPoint();
    }

    public bool SetBrush(TileTexture texture)
    {
        if (TilerMap == null) return false;

        DrawTool.SetBrush(new NormalBrush(TilerMap.TileResolution, texture));

        Repaint();

        return true;
    }
    private void OnHierachyChange()
    {
        RefreshMaps();

        foreach (var map in _tilerMaps)
        {
            // check cells that are null
            var cells = map.Cells;
            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];

                if (cell == null)
                {
                    cells.Remove(cell);
                    i--;
                }
            }
        }

        if (DuplicateCheck()) return;

        // If new map has been set, match it
        if (_newMap)
        {
            for (var index = 0; index < _tilerMaps.Count; index++)
            {
                var m = _tilerMaps[index];

                if (_newMap == m)
                {
                    SetMap(index);
                    break;
                }
            }
            _newMap = null;
        }
    }

    private bool DuplicateCheck()
    {
        foreach (TilerMap map1 in _tilerMaps)
        {
            var prefab1 = PrefabUtility.GetPrefabParent(map1.gameObject);
            var mapPath1 = AssetDatabase.GetAssetPath(prefab1);
            if (!string.IsNullOrEmpty(mapPath1))
            {
                bool match = false;
                foreach (TilerMap map2 in _tilerMaps)
                {
                    var prefab2 = PrefabUtility.GetPrefabParent(map2.gameObject);
                    var mapPath2 = AssetDatabase.GetAssetPath(prefab2);

                    if (mapPath1 == mapPath2)
                    {
                        if (match)
                        {
                            Debug.LogError("There is more than one layer in the scene sharing the prefab \"" +
                                           prefab2.name + "\". You can not save until resolved.");
                            return true;
                        }
                        match = true;
                    }
                }
            }
        }

        return false;
    }

    private void RefreshMaps()
    {
        _tilerMaps = Object.FindObjectsOfType(typeof(TilerMap)).Cast<TilerMap>().OrderBy(tile => tile.Layer).ToList();
        //_mapNames = _tilerMaps.Select(t => t.name).ToArray();

        var mIndex = -1;
        
        if (TilerMap != null)
        {
            var instanceID = TilerMap.GetInstanceID();

            for (int index = 0; index < _tilerMaps.Count; index++)
            {
                if (instanceID == _tilerMaps[index].GetInstanceID())
                {
                    mIndex = index;
                    break;
                }
            }
        }

        if (mIndex == -1)
        {
            if (_tilerMaps.Count > 0)
            {
                mIndex = 0;
            }
            SetMap(mIndex);
        }

        Repaint();
    }
    private void SetMap(int index)
    {
        if (index < _tilerMaps.Count && index > -1)
        {
            TilerMap = _tilerMaps[index];

            DrawTool.SetBrush(new NormalBrush(TilerMap.TileResolution, TileTexture.None));

            _mapIndex = index;
        }
        else
        {
            _mapIndex = -1;
            TilerMap = null;
        }

        TilerMapEditFunctions = new TilerMapEdit(TilerMap);
    }
    
    private void GetGridPoint()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        var plane = new Plane(Vector3.up, Vector3.zero);
        float dist;
        if (!plane.Raycast(ray, out dist))
        {
            throw new UnityException("Ray missed. Shouldn't happen.");
        }

        var hitPoint = ray.GetPoint(dist);

        _currentPoint = TilerMap.GetWorldTileIDFromWorldCoords(hitPoint);

        Repaint();
    }
    private void PanWindow()
    {
        var mod = _camera.transform.position.y / 500f;
        var delta = Event.current.delta * mod;

        _camera.transform.position += new Vector3(-delta.x, 0, delta.y);
    }
    private void SetCameraHeight()
    {
        var p = _camera.transform.position;
        p.y = Mathf.Exp(_height);

        _camera.transform.position = p;
    }

    private void DrawSelectionBox()
    {
        var tileSize = TilerMap.CellSize / TilerMap.TilesPerCell;
        var offset = new Vector2(TilerMap.transform.position.x, TilerMap.transform.position.z);

        if (_isDraggingCopy)
        {
            var start = new Point
                            {
                                X = Math.Min(_currentPoint.X, _startCopyPoint.X),
                                Y = Math.Min(_currentPoint.Y, _startCopyPoint.Y)
                            };

            var count = new Point
                            {
                                X = Math.Abs(_currentPoint.X - _startCopyPoint.X) + 1,
                                Y = Math.Abs(_currentPoint.Y - _startCopyPoint.Y) + 1
                            };

            var p = new Vector2(start.X * TilerMap.TileSize, start.Y * TilerMap.TileSize) + offset - new Vector2(tileSize / 2f, tileSize / 2f);

            var y = TilerMap.transform.position.y;

            // /2 for int round
            var right = count.X* tileSize;
            var top = count.Y*tileSize;

            var verts = new[]
                            {
                                new Vector3(p.x, y, p.y),
                                new Vector3(p.x, y, p.y + top),
                                new Vector3(p.x + right, y, p.y + top),
                                new Vector3(p.x + right, y, p.y)
                            };

            Handles.DrawSolidRectangleWithOutline(verts, new Color(1, 1, 1, 0.2f), Color.white);
        }
        else
        {
            
            var p = new Vector2(_currentPoint.X*TilerMap.TileSize, _currentPoint.Y*TilerMap.TileSize) + offset;
            var y = TilerMap.transform.position.y;
            var r = tileSize/2f;

            var brush = DrawTool.GetBrush();

            // /2 for int round
            var right = r + (brush.BrushSize.X / 2) * 2 * r;
            var left = r + ((brush.BrushSize.X - 1) / 2) * 2 * r;
            var top = r + (brush.BrushSize.Y / 2) * 2 * r;
            var bottom = r + ((brush.BrushSize.Y - 1) / 2) * 2 * r;



            var verts = new[]
                            {
                                new Vector3(p.x - left, y, p.y - bottom),
                                new Vector3(p.x - left, y, p.y + top),
                                new Vector3(p.x + right, y, p.y + top),
                                new Vector3(p.x + right, y, p.y - bottom)
                            };

            Handles.DrawSolidRectangleWithOutline(verts, new Color(1, 1, 1, 0.2f), Color.white);
        }
    }

    private void GlobalInputHack()
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.D)
            {
                DrawTool.GetBrush().Rotate();
                Parent.Repaint();
                Repaint();
            }
            if (e.keyCode == KeyCode.Alpha1)
            {
                DrawTool = new PaintTool(this, DrawTool.GetBrush());
                Repaint();
            }
            if (e.keyCode == KeyCode.Alpha2)
            {
                DrawTool = new FillTool(this, DrawTool.GetBrush() as NormalBrush);
                Repaint();
            }
            if (e.keyCode == KeyCode.Alpha3)
            {
                DrawTool = new ReplaceTool(this, DrawTool.GetBrush() as NormalBrush);
                Repaint();
            }

            var brush = DrawTool.GetBrush();

            if (DrawTool is PaintTool && brush is NormalBrush)
            {
                if (e.keyCode == KeyCode.A)
                {
                    //_brushWindow.BrushSizeAdd(-1);
                }

                if (e.keyCode == KeyCode.S)
                {
                    //_brushWindow.BrushSizeAdd(1);
                }
            }

            if (e.keyCode == KeyCode.Alpha3)
            {
                _drawTileSetWindow.SetBrush(-1);
            }
        }
    }
    private void Paint()
    {
        DrawTool.DoAction(_currentPoint);
        Repaint();
    }

    private bool NoRightMouseAltInput()
    {
        var e = Event.current;
        return e.button == 0 && e.control;
    }

    private void SetMapMask(int mask)
    {
        _mapMask = mask;
        Repaint();
    }
    private void SetMapChange(int index)
    {
        _mapIndex = index;
        Save();
        SetMap(_mapIndex);
    }

    public override void Save()
    {
        if (_tilerMaps == null) return;
        if (DuplicateCheck()) return;

        _lastSave = DateTime.Now;

        foreach (var map in _tilerMaps)
        {
            SaveMap(map);
        }

        TilerMapEditFunctions.Undo.Clear();
    }

    /*private void Backup(TilerMap map)
    {
        var prefab = PrefabUtility.GetPrefabParent(map.gameObject);
        var mapPath = AssetDatabase.GetAssetPath(prefab);

        if (string.IsNullOrEmpty(mapPath))
        {
            return;
        }

        // Potential Backup
        var backupMapPath = mapPath + ".backup.prefab";
        var backup = AssetDatabase.LoadAssetAtPath(backupMapPath, typeof (GameObject)) as GameObject;

        if (backup == null)
        {
            PrefabUtility.CreatePrefab(backupMapPath, TilerMap.gameObject);
        }
        else
        {
            PrefabUtility.ReplacePrefab((GameObject)prefab, backup);
        }
    }*/

    private void SaveMap(TilerMap map)
    {
        var prefab = PrefabUtility.GetPrefabParent(map.gameObject);
        var mapPath = AssetDatabase.GetAssetPath(prefab);

        if (string.IsNullOrEmpty(mapPath))
        {
            Debug.LogError("Unable to save \"" + map.name + "\": Prefab has been deleted. Create a prefab, attach the gameobject and try save again.");

            // Set all cells to dirty so when attached, all will save out.
            foreach (var cell in map.Cells)
                cell.IsDirty = true;
            return;
        }

        // Check if we actually save anything. If not don't override prefab. This greatly improves performance.
        var isDoUpdate = false;

        // Setup paths
        var fullPath = Application.dataPath;
        var strippedPath = Path.GetDirectoryName(mapPath) + "/" + Path.GetFileNameWithoutExtension(mapPath) + "/";

        var index = fullPath.IndexOf("Assets", StringComparison.Ordinal);
        if (index == -1)
            return;

        fullPath = fullPath.Substring(0, index);
        fullPath += strippedPath;

        // Check if directory exists, if not create it
        if (!Directory.Exists(fullPath))
            Directory.CreateDirectory(fullPath);

        // We share the mesh between all cells
        if (!map.SharedMesh) return;

        if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(map.SharedMesh)))
        {
            AssetDatabase.DeleteAsset(strippedPath + map.SharedMesh.name + ".asset");
            AssetDatabase.CreateAsset(map.SharedMesh, strippedPath + map.SharedMesh.name + ".asset");
        }

        var meshAsset = (Mesh)AssetDatabase.LoadAssetAtPath(strippedPath + map.SharedMesh.name + ".asset", typeof(Mesh));

        foreach (var cell in map.Cells)
        {
            if (cell == null) continue;

            var go = cell.gameObject;
            if (!go.GetComponent<Renderer>()) continue;
            var mat = go.GetComponent<Renderer>().sharedMaterial;
            if (!mat) continue;

            // Render queue doesn't serialize out so we save it seperately.
            int renderQueue = mat.renderQueue;

            // Only update texture on changes
            if (cell.IsDirty)
            {
                cell.IsDirty = false;
                isDoUpdate = true;

                // Save the mesh as it may not have been done
                go.GetComponent<MeshFilter>().sharedMesh = meshAsset;

                var tex = (Texture2D) mat.mainTexture;

                // Store data before destroying
                var bytes = tex.EncodeToPNG();
                var texName = tex.name;

                // Clean up old texture - stop memory leaks
                Object.DestroyImmediate(tex, true);

                // First we save the texture as a png, this lets us modify import properties compared to saving it as an asset
                Util.SaveTextureToFile(bytes, strippedPath + texName + ".png");
                AssetDatabase.Refresh();
                // Load the now saved png 
                var texAsset = (Texture2D)AssetDatabase.LoadAssetAtPath(strippedPath + texName + ".png", typeof(Texture2D));

                // Assign texture importer settings
                var path = AssetDatabase.GetAssetPath(texAsset);
                EditorUtil.SetTextureImportSettings(path, TilerMap.TextureResolution);

                // Assign this new texture to the material
                mat.mainTexture = texAsset;

                // If material doesn't exist
                if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(mat)))
                {
                    AssetDatabase.DeleteAsset(strippedPath + mat.name + ".mat");
                    AssetDatabase.CreateAsset(mat, strippedPath + mat.name + ".mat");

                    // Load the new material
                    var matAsset = (Material) AssetDatabase.LoadAssetAtPath(strippedPath + mat.name + ".mat", typeof (Material));

                    // Assign this new material/mesh to the go
                    go.GetComponent<Renderer>().sharedMaterial = matAsset;
                }
            }

            // Always apply renderqueue update
            go.GetComponent<Renderer>().sharedMaterial.renderQueue = renderQueue;
        }

        var prefabMap = ((GameObject) prefab).GetComponent<TilerMap>();
        // do a check of tilermap properties to see if we need to update
        if (prefabMap.Layer != map.Layer || 
            prefabMap.DefaultShader != map.DefaultShader)
        {
            isDoUpdate = true;
        }

        // Only replace if something changed. Adds a lot of annoying checks but much better performance
        if (isDoUpdate)
            PrefabUtility.ReplacePrefab(map.gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
    }
}