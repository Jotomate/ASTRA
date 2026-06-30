using UnityEngine;

namespace ShootingGame.Stage
{
    /// <summary>
    /// StageData를 읽어 다중 스크롤 배경을 구성한다. 레이어마다 ScrollingLayer 1개 생성.
    /// </summary>
    public class StageBackground : MonoBehaviour
    {
        [SerializeField] StageData stage;
        [Tooltip("배경 스프라이트에 쓸 Unlit 머티리얼")]
        [SerializeField] Material spriteMaterial;

        void Start()
        {
            if (stage == null || stage.layers == null) return;

            for (int i = 0; i < stage.layers.Length; i++)
            {
                BackgroundLayer layer = stage.layers[i];
                if (layer == null || layer.sprite == null) continue;

                var go = new GameObject("Layer_" + layer.name);
                go.transform.SetParent(transform, false);
                var scroller = go.AddComponent<ScrollingLayer>();
                scroller.Init(layer.sprite,
                              stage.baseScrollSpeed * layer.scrollFactor,
                              layer.sortingOrder,
                              layer.tint,
                              spriteMaterial);
            }
        }
    }
}
