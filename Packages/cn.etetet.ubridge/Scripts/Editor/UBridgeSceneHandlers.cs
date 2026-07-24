using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ET
{
    /// <summary> 场景层级树 </summary>
    public static class UBridgeSceneGetHierarchyHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<SceneGetHierarchyRequest>(p);
            var resp = SceneGetHierarchyResponse.Create();
            var scene = SceneManager.GetActiveScene();
            resp.SceneName = scene.name; resp.ScenePath = scene.path;
            foreach (var go in scene.GetRootGameObjects())
                resp.RootNodes.Add(BuildNode(go, r?.MaxDepth ?? -1, 0));
            resp.NodeCount = CountNodes(resp.RootNodes);
            return UBridgeJsonHelper.ToJson(resp);
        }
        static BridgeSceneNode BuildNode(GameObject go, int max, int d)
        {
            var n = BridgeSceneNode.Create(); var i = BridgeObjectInfo.Create();
            FillObject(go, i); n.Object = i;
            if (max < 0 || d < max) for (int c = 0; c < go.transform.childCount; c++)
                    n.Children.Add(BuildNode(go.transform.GetChild(c).gameObject, max, d + 1));
            return n;
        }
        static void FillObject(GameObject go, BridgeObjectInfo info)
        {
            info.InstanceId = go.GetInstanceID(); info.Name = go.name; info.Tag = go.tag;
            info.Layer = go.layer; info.ActiveSelf = go.activeSelf; info.ActiveInHierarchy = go.activeInHierarchy;
            info.Transform = BuildTransform(go.transform);
        }
        static int CountNodes(System.Collections.Generic.List<BridgeSceneNode> nodes) { int c = nodes.Count; foreach (var n in nodes) c += CountNodes(n.Children); return c; }
        public static BridgeTransformInfo BuildTransform(Transform t)
        {
            var r = BridgeTransformInfo.Create();
            r.Position = V3(t.position); r.RotationEuler = V3(t.eulerAngles);
            r.Rotation = Q(t.rotation); r.LocalScale = V3(t.localScale);
            return r;
        }
        public static BridgeVector3 V3(Vector3 v) { var r = BridgeVector3.Create(); r.X = v.x; r.Y = v.y; r.Z = v.z; return r; }
        public static BridgeQuaternion Q(Quaternion q) { var r = BridgeQuaternion.Create(); r.X = q.x; r.Y = q.y; r.Z = q.z; r.W = q.w; return r; }
    }

    /// <summary> 当前激活场景 </summary>
    public static class UBridgeSceneGetActiveHandler
    {
        public static string Handle(string p)
        {
            var resp = SceneGetActiveResponse.Create();
            var scene = SceneManager.GetActiveScene();
            resp.SceneName = scene.name; resp.ScenePath = scene.path; resp.BuildIndex = scene.buildIndex;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    /// <summary> 加载场景 </summary>
    public static class UBridgeSceneLoadHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<SceneLoadRequest>(p);
            var resp = SceneLoadResponse.Create();
            if (!string.IsNullOrEmpty(r?.ScenePath))
                EditorSceneManager.OpenScene(r.ScenePath);
            else if (r?.BuildIndex >= 0)
                EditorSceneManager.OpenScene(EditorBuildSettings.scenes[r.BuildIndex].path);
            resp.ScenePath = SceneManager.GetActiveScene().path;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    /// <summary> 保存场景 </summary>
    public static class UBridgeSceneSaveHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<SceneSaveRequest>(p);
            var resp = SceneSaveResponse.Create();
            var scene = SceneManager.GetActiveScene();
            string path = !string.IsNullOrEmpty(r?.ScenePath) ? r.ScenePath : scene.path;
            EditorSceneManager.SaveScene(scene, path);
            resp.ScenePath = path;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    /// <summary> 新建场景 </summary>
    public static class UBridgeSceneNewHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<SceneNewRequest>(p);
            var resp = SceneNewResponse.Create();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            resp.SceneName = scene.name; resp.ScenePath = scene.path;
            if (!string.IsNullOrEmpty(r?.SceneName)) { resp.SceneName = r.SceneName; resp.ScenePath = "Assets/" + r.SceneName + ".unity"; }
            return UBridgeJsonHelper.ToJson(resp);
        }
    }
}