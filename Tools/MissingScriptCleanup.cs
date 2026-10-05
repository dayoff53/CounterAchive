#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MakiboMaintenance
{
    /// <summary>검증 사본의 Editor 폴더에 복사하여 실행하는 일회성 Missing Script 정리 도구.</summary>
    public static class MissingScriptCleanup
    {
        /// <summary>한 에셋의 누락 컴포넌트 개수와 위치 기록.</summary>
        [Serializable] public sealed class Entry
        {
            /// <summary>프로젝트 상대 에셋 경로.</summary>
            public string path;
            /// <summary>검사 시 실제 Unity 오브젝트에서 발견한 누락 개수.</summary>
            public int count;
            /// <summary>누락 컴포넌트를 가진 GameObject의 계층 경로와 개수.</summary>
            public List<string> objects = new List<string>();
        }
        /// <summary>정리 전·실제 수정·정리 후의 검사 결과.</summary>
        [Serializable] public sealed class Report
        {
            /// <summary>검사한 씬/프리팹 수.</summary>
            public int scanned;
            /// <summary>원본 상태의 누락 목록. 상속된 프리팹 컴포넌트는 인스턴스별로 중복 집계된다.</summary>
            public List<Entry> before = new List<Entry>();
            /// <summary>실제로 컴포넌트를 제거하고 저장한 에셋 목록.</summary>
            public List<Entry> changed = new List<Entry>();
            /// <summary>정리 후 남은 누락 목록. 비어 있어야 성공이다.</summary>
            public List<Entry> after = new List<Entry>();
            /// <summary>처리 중 발생한 오류. 실패해도 중간 기록을 남긴다.</summary>
            public string error;
        }
        /// <summary>검증 사본에서만 모든 씬/프리팹을 검사하고 누락 컴포넌트만 제거한다.</summary>
        public static void Run()
        {
            // report: 예외가 발생해도 저장할 진행 기록.
            var report = new Report();
            try
            {
                if (!Application.dataPath.Replace('\\', '/').EndsWith("/Logs/StageValidation/Assets"))
                    throw new InvalidOperationException("Run only in Logs/StageValidation, never the working editor.");
                // paths: ScriptableObject, 모델, 패키지를 제외한 실제 씬과 프리팹 파일.
                var paths = Directory.GetFiles("Assets", "*", SearchOption.AllDirectories)
                    .Select(p => p.Replace('\\', '/')).Where(p => p.EndsWith(".prefab") || p.EndsWith(".unity"))
                    .OrderBy(p => p, StringComparer.Ordinal).ToArray();
                report.scanned = paths.Length;
                // structures: 정리 전후 GameObject·정상 컴포넌트·Transform·시각 에셋의 동일성 기록.
                var structures = new Dictionary<string, string>();
                foreach (var path in paths) structures[path] = Inspect(path, false, report.before);
                SaveReport(report);
                // visited/ordered: 원본 프리팹부터 정리하여 상속된 누락을 불필요한 오버라이드로 저장하지 않는다.
                var visited = new HashSet<string>(); var ordered = new List<string>();
                foreach (var path in paths.Where(p => p.EndsWith(".prefab"))) OrderPrefab(path, visited, ordered);
                foreach (var path in ordered.Concat(paths.Where(p => p.EndsWith(".unity"))))
                    Inspect(path, true, report.changed);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                foreach (var path in paths)
                    if (structures[path] != Inspect(path, false, report.after))
                        throw new InvalidOperationException("Retained object/resource structure changed: " + path);
                if (report.after.Count != 0) throw new InvalidOperationException("Missing components remain after cleanup.");
            }
            catch (Exception exception) { report.error = exception.ToString(); }
            SaveReport(report);
            Debug.Log("Missing Script cleanup: scanned=" + report.scanned + ", changed=" + report.changed.Count + ", remaining=" + report.after.Count);
            EditorApplication.Exit(string.IsNullOrEmpty(report.error) ? 0 : 1);
        }
        /// <summary>의존 프리팹을 먼저 방문한 뒤 대상 프리팹을 순서에 넣는다.</summary>
        /// <param name="path">현재 프리팹 경로.</param>
        /// <param name="visited">순환과 중복 처리를 막는 경로 집합.</param>
        /// <param name="ordered">의존 순서로 정렬된 결과.</param>
        private static void OrderPrefab(string path, HashSet<string> visited, List<string> ordered)
        {
            if (!visited.Add(path)) return;
            foreach (var dependency in AssetDatabase.GetDependencies(path, false))
                if (dependency.StartsWith("Assets/") && dependency.EndsWith(".prefab")) OrderPrefab(dependency, visited, ordered);
            ordered.Add(path);
        }
        /// <summary>프리팹 또는 씬의 비활성 자식까지 조사하고, 요청된 경우 실제 누락이 있는 에셋만 저장한다.</summary>
        /// <param name="path">검사할 에셋 경로.</param>
        /// <param name="remove">true면 Missing Script만 제거한다.</param>
        /// <param name="entries">누락이 발견된 에셋의 결과 목록.</param>
        private static string Inspect(string path, bool remove, List<Entry> entries)
        {
            // entry/root/scene: 이 파일의 결과와 반드시 해제할 임시 로드 객체.
            var entry = new Entry { path = path }; GameObject root = null; Scene scene = default;
            try
            {
                if (path.EndsWith(".prefab")) root = PrefabUtility.LoadPrefabContents(path);
                else scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                // roots: 씬 또는 프리팹의 전체 루트. 비활성 자식도 포함한다.
                var roots = root != null ? new[] { root } : scene.GetRootGameObjects();
                // retained: 누락된 컴포넌트를 제외한 오브젝트 구조의 안정적인 지문.
                string retained = Fingerprint(roots);
                foreach (var top in roots)
                    foreach (var transform in top.GetComponentsInChildren<Transform>(true))
                    {
                        // count: 정상 스크립트의 빈 Inspector 필드는 세지 않고 누락 MonoBehaviour만 센다.
                        int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
                        if (count == 0) continue;
                        entry.count += count;
                        entry.objects.Add(AnimationUtility.CalculateTransformPath(transform, null) + " (" + count + ")");
                        if (remove && GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject) != count)
                            throw new InvalidOperationException("Removal count mismatch: " + path);
                    }
                if (entry.count == 0) return retained;
                entries.Add(entry);
                if (!remove) return retained;
                if (root != null)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path, out bool success);
                    if (!success) throw new IOException("Could not save prefab: " + path);
                }
                else if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save scene: " + path);
                return retained;
            }
            finally
            {
                if (root != null) PrefabUtility.UnloadPrefabContents(root);
                if (scene.IsValid()) EditorSceneManager.CloseScene(scene, true);
            }
        }
        /// <summary>누락 제거와 무관한 계층·정상 컴포넌트·주요 시각 참조가 유지되는지 검사할 지문을 만든다.</summary>
        /// <param name="roots">검사할 씬/프리팹 루트 목록.</param>
        private static string Fingerprint(GameObject[] roots)
        {
            // rows: 저장 중 인스턴스 ID와 직렬화 순서가 바뀌어도 같은 리소스임을 비교할 내용.
            var rows = new List<string>();
            foreach (var root in roots)
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                {
                    // go/key: 비활성 상태를 포함한 실제 게임 오브젝트의 구조와 위치.
                    var go = transform.gameObject;
                    string key = AnimationUtility.CalculateTransformPath(transform, null);
                    rows.Add(key + "|" + go.activeSelf + "|" + go.layer + "|" + go.tag + "|" + transform.localPosition.ToString("R") + "|" + transform.localRotation.ToString("R") + "|" + transform.localScale.ToString("R"));
                    foreach (var component in go.GetComponents<Component>())
                    {
                        if (component == null) continue;
                        rows.Add(key + "|" + component.GetType().FullName);
                        if (component is SpriteRenderer sprite) rows.Add(key + "|sprite|" + AssetKey(sprite.sprite));
                        if (component is Animator animator) rows.Add(key + "|animation|" + AssetKey(animator.runtimeAnimatorController));
                        if (component is UnityEngine.UI.Image image) rows.Add(key + "|image|" + AssetKey(image.sprite));
                    }
                }
            rows.Sort(StringComparer.Ordinal);
            using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(string.Join("\n", rows))));
        }
        /// <summary>시각 리소스의 GUID와 서브 에셋 ID를 얻는다.</summary>
        /// <param name="asset">참조된 이미지 또는 애니메이션 에셋.</param>
        private static string AssetKey(UnityEngine.Object asset)
        {
            if (asset == null) return "null";
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long id);
            return guid + ":" + id;
        }
        /// <summary>검증 프로젝트의 로그에 현재 기록을 저장한다.</summary>
        /// <param name="report">검사와 변경의 집계.</param>
        private static void SaveReport(Report report)
        {
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/missing-script-cleanup.json", JsonUtility.ToJson(report, true));
        }
    }
}
#endif
