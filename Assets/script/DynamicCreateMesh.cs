using UnityEngine;
using System.Collections;

namespace Esakun.Sample
{
    /// <summary>
    /// 頂点カラーで三角形を描画します
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class DynamicCreateMesh : MonoBehaviour
    {
        private void Start()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] {
                new Vector3 (0, 1f),
                new Vector3 (1f, -1f),
                new Vector3 (-1f, -1f),
            };
            mesh.triangles = new int[] {
                0, 1, 2
            };

            // 変更箇所 : 各頂点に色情報を設定
            mesh.colors = new Color[] {
                Color.white,
                Color.red,
                Color.green
            };

            var filter = GetComponent<MeshFilter>();
            filter.sharedMesh = mesh;
        }
    }
}