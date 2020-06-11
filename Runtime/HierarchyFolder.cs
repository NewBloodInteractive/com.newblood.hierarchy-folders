using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
#if UNITY_EDITOR
using System;
using System.Reflection;
using NewBlood.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Experimental;
using Object = UnityEngine.Object;
#endif

namespace NewBlood
{
    [ExecuteAlways]
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public sealed class HierarchyFolder : MonoBehaviour
#if UNITY_EDITOR
        , ISceneHierarchyCallbackReceiver
#endif
    {
    #if UNITY_EDITOR
        static readonly Action<Object, Texture2D> SetIconForObject = (Action<Object, Texture2D>)typeof(EditorGUIUtility)
            .GetMethod("SetIconForObject", BindingFlags.NonPublic | BindingFlags.Static)
            .CreateDelegate(typeof(Action<Object, Texture2D>));

        static Texture2D openIcon;
        static Texture2D closedIcon;

        [InitializeOnLoadMethod]
        static void EditorInitialize()
        {
            openIcon   = EditorGUIUtility.FindTexture(EditorResources.emptyFolderIconName);
            closedIcon = EditorGUIUtility.FindTexture(EditorResources.folderIconName);
        }

        [MenuItem("GameObject/Create Folder", false, 0)]
        static void CreateFolder()
        {
            var folder = ObjectFactory.CreateGameObject("New Folder", typeof(HierarchyFolder));
            GameObjectUtility.SetParentAndAlign(folder.gameObject, Selection.activeGameObject);
            Undo.RegisterCreatedObjectUndo(folder, "Create Folder");
            Selection.activeGameObject = folder;
            SetIconForObject(folder, closedIcon);
        }

        public void OnSceneHierarchyGUI(TreeViewItem item, Rect selectionRect, bool expanded)
        {
            item.icon = expanded ? openIcon : closedIcon;
        }
    #endif

        void Awake()
        {
            hideFlags           |= HideFlags.HideInInspector;
            transform.hideFlags |= HideFlags.HideInInspector;
        }

        void Update()
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale    = Vector3.one;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                var children = new List<Transform>();
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var folder in root.GetComponentsInChildren<HierarchyFolder>(includeInactive: true))
                    {
                        for (int i = 0; i < folder.transform.childCount; i++)
                            children.Add(folder.transform.GetChild(i));

                        foreach (var child in children)
                            child.SetParent(folder.transform.parent);

                        Destroy(folder.gameObject);
                        children.Clear();
                    }
                }
            };
        }
    }
}
