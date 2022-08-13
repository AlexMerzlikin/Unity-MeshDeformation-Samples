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
        public IEnumerator DeformableMeshPlane_MeshData_PerformanceTest()
        {
            yield return StartTest("Sample");
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