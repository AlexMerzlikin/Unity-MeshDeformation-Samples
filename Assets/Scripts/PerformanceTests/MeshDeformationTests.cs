using System.Collections;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PerformanceTests
{
    public class MeshDeformationTests
    {
        [UnityTest, Performance]
        public IEnumerator SingleThreaded()
        {
            yield return StartTest("SingleThreaded");
        }

        [UnityTest, Performance]
        public IEnumerator JobSystem()
        {
            yield return StartTest("JobSystem");
        }

        [UnityTest, Performance]
        public IEnumerator MeshData()
        {
            yield return StartTest("MeshData");
        }

        [UnityTest, Performance]
        public IEnumerator ComputeShader()
        {
            yield return StartTest("ComputeShader");
        }

        [UnityTest, Performance]
        public IEnumerator VertexShader()
        {
            yield return StartTest("VertexShader");
        }

        private static IEnumerator StartTest(string sceneName)
        {
            yield return LoadScene(sceneName);
            yield return RunTest();
        }

        private static IEnumerator LoadScene(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return null;
        }

        private static IEnumerator RunTest()
        {
            using (Measure.Frames().Scope())
            {
                yield return new WaitForSeconds(3);
            }
        }
    }
}