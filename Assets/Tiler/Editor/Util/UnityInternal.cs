using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tiler
{
    public enum DockPosition
    {
        Left,
        Right,
        Bottom
    }

    [Serializable]
    public static class UnityInternal
    {
        private static bool _error;
        private static bool _errorMajor;
        private static readonly Assembly Assembly;

        static UnityInternal()
        {
            Assembly = typeof (EditorWindow).Assembly;
        }

        public static void SetFreezeDisplay(bool freeze)
        {
            var containerWindow = Assembly.GetType("UnityEditor.ContainerWindow");

            var setFreezeDisplay = containerWindow.GetMethod("SetFreezeDisplay",
                                                             BindingFlags.Public | BindingFlags.Static);

            setFreezeDisplay.Invoke(null, new object[] {freeze});
        }

        public static void ShowWithModePopup(EditorWindow window)
        {
            var showWithMode = window.GetType()
                                     .GetMethod("ShowWithMode",
                                                BindingFlags.NonPublic | BindingFlags.Instance |
                                                BindingFlags.FlattenHierarchy);

            showWithMode.Invoke(window, new object[] {1});
        }

        public static void AddGlobalEventHandler(EditorApplication.CallbackFunction func)
        {
            var fieldInfo = typeof (EditorApplication).GetField("globalEventHandler",
                                                                BindingFlags.NonPublic | BindingFlags.Static);
            var field = fieldInfo.GetValue(null) as EditorApplication.CallbackFunction;
            Delegate handler = Delegate.CreateDelegate(fieldInfo.FieldType, func.Target, func.Method);

            if (field != null)
            {
                Delegate old = Delegate.CreateDelegate(fieldInfo.FieldType, field.Target, field.Method);
                handler = Delegate.Combine(old, handler);
            }

            fieldInfo.SetValue(null, handler);
        }

        public static void RemoveGlobalEventHandler(EditorApplication.CallbackFunction func)
        {
            var fieldInfo = typeof (EditorApplication).GetField("globalEventHandler",
                                                                BindingFlags.NonPublic | BindingFlags.Static);
            var field = fieldInfo.GetValue(null) as EditorApplication.CallbackFunction;

            if (field != null)
            {
                Delegate handler = Delegate.CreateDelegate(fieldInfo.FieldType, func.Target, func.Method);
                handler = Delegate.Remove(field, handler);

                fieldInfo.SetValue(null, handler);
            }
        }

        public static object ObjectListArea(string searchFilter, string requiredType, HierarchyType hierarchy)
        {
            var assembly = typeof (EditorWindow).Assembly;
            var objectListAreaType = assembly.GetType("UnityEditor.ObjectListArea");

            var objectListArea = Activator.CreateInstance(objectListAreaType, new object[] {null, null, true});

            var init = objectListAreaType.GetMethod("Init", BindingFlags.Public | BindingFlags.Instance);
            init.Invoke(objectListArea, new object[] {searchFilter, requiredType, null, hierarchy});

            return objectListArea;
        }

        public static IList GetAssetGroups(object objectListArea)
        {
            var assembly = typeof (EditorWindow).Assembly;
            var objectListAreaType = assembly.GetType("UnityEditor.ObjectListArea");

            var assetGroup = objectListAreaType.GetField("m_AssetGroups", BindingFlags.NonPublic | BindingFlags.Instance);

            return (IList) assetGroup.GetValue(objectListArea);
        }

        public static List<AssetResult> GetAssetsInProjectOfType(Type type, HierarchyType assets = HierarchyType.Assets)
        {
            try
            {
                var assembly = typeof (EditorWindow).Assembly;
                var filteredHierarchyType = assembly.GetType("UnityEditor.FilteredHierarchy");

                var filteredHierarchy = Activator.CreateInstance(filteredHierarchyType, new object[] {assets});

#if UNITY_3_4 || UNITY_3_5
                var objectListAreaType = assembly.GetType("UnityEditor.ObjectListArea");

                var filterProperty = filteredHierarchyType.GetProperty("filter",
                                                                       BindingFlags.Public | BindingFlags.Instance);
                var searchModeProperty = filteredHierarchyType.GetProperty("searchMode",
                                                                           BindingFlags.Public | BindingFlags.Instance);

                var createFilterString = objectListAreaType.GetMethod("CreateFilterString",
                                                                      BindingFlags.NonPublic | BindingFlags.Static);

                var filterString = createFilterString.Invoke(null, new object[] {"", type.Name, null});

                filterProperty.SetValue(filteredHierarchy, filterString, null);
                searchModeProperty.SetValue(filteredHierarchy, 0, null);
#else
        var searchFilterType = assembly.GetType("UnityEditor.SearchFilter");
        var classNames = searchFilterType.GetProperty("classNames", BindingFlags.Instance | BindingFlags.Public);

        var searchFilterProperty = filteredHierarchyType.GetProperty("searchFilter", BindingFlags.Public | BindingFlags.Instance);

        var sfilter = Activator.CreateInstance(searchFilterType);

        classNames.SetValue(sfilter, new[] { type.ToString() }, null);

        searchFilterProperty.SetValue(filteredHierarchy, sfilter, null);
#endif

                var resultField = filteredHierarchyType.GetField("m_Results",
                                                                 BindingFlags.NonPublic | BindingFlags.Instance);

                var results = (Array) resultField.GetValue(filteredHierarchy);

                var list = new List<AssetResult>();

                foreach (var r in results)
                {
                    var assetResult = ObjToFilterResult(r);
                    list.Add(assetResult);
                }

                return list;
            }
            catch (Exception)
            {
                if (!_errorMajor)
                {
                    Debug.LogError(
                        "For some reason it appears something went wrong with Tilers Asset Search. Tiler is unable to operate properly." +
                        "\nYour version is: " + Application.unityVersion +
                        " Please report this on the forums and I'll try fix it asap.");
                    _errorMajor = true;
                }

                return new List<AssetResult>();
            }
        }

        public static List<AssetResult> GetAssetsInProjectOfType<T>(HierarchyType assets = HierarchyType.Assets)
            where T : Object
        {
            return GetAssetsInProjectOfType(typeof (T), assets);
        }

        private static AssetResult ObjToFilterResult(object obj)
        {
            var assembly = typeof (EditorWindow).Assembly;
            var filteredHierarchyType = assembly.GetType("UnityEditor.FilteredHierarchy");
            var filterResultType = filteredHierarchyType.GetNestedType("FilterResult");

            var instanceID = filterResultType.GetField("instanceID");
            var name = filterResultType.GetField("name");
            var hasChildren = filterResultType.GetField("hasChildren");
            var colorCode = filterResultType.GetField("colorCode");
            var isMainRepresentation = filterResultType.GetField("isMainRepresentation");
            var type = filterResultType.GetField("type");

            var asset = new AssetResult();

            asset.InstanceID = (int) instanceID.GetValue(obj);
            asset.Name = (string) name.GetValue(obj);
            asset.HasChildren = (bool) hasChildren.GetValue(obj);
            asset.ColorCode = (int) colorCode.GetValue(obj);
            asset.IsMainRepresentation = (bool) isMainRepresentation.GetValue(obj);
            asset.Type = (HierarchyType) type.GetValue(obj);

            return asset;
        }

        public static Type GetEditorClassType(string name)
        {
            var assembly = typeof (EditorWindow).Assembly;
            var gameView = assembly.GetType("UnityEditor." + name);

            return gameView;
        }

    }

    [Serializable]
    public class AssetResult
    {
        public int InstanceID;
        public string Name;
        public bool HasChildren;
        public int ColorCode;
        //public string Guid; // 3.6 only
        public bool IsMainRepresentation;
        //public Texture2D Icon; // 3.6 only
        public HierarchyType Type;
    }
}