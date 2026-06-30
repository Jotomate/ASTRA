using UnityEngine;

namespace ShootingGame.Stage
{
    /// <summary>
    /// 무한 세로 스크롤 1개 레이어. 같은 스프라이트 2장을 세로로 잇고 아래로 흘리며,
    /// 화면 아래로 완전히 빠진 장을 위로 되돌려(leapfrog) 끊김 없이 반복한다.
    /// </summary>
    public class ScrollingLayer : MonoBehaviour
    {
        SpriteRenderer tileA, tileB;
        float speed;
        float tileHeight;
        Camera cam;
        float camBottom;

        public void Init(Sprite sprite, float speed, int sortingOrder, Color tint, Material mat)
        {
            this.speed = speed;
            cam = Camera.main;

            tileA = MakeTile("TileA", sprite, sortingOrder, tint, mat);
            tileB = MakeTile("TileB", sprite, sortingOrder, tint, mat);

            float screenH = cam != null ? cam.orthographicSize * 2f : 10f;
            float screenW = cam != null ? screenH * cam.aspect : 18f;
            float spriteW = sprite.bounds.size.x;
            float spriteH = sprite.bounds.size.y;

            // 화면 폭·높이 모두 덮도록 스케일
            float scale = Mathf.Max(screenW / spriteW, screenH / spriteH);
            tileA.transform.localScale = tileB.transform.localScale = Vector3.one * scale;
            tileHeight = spriteH * scale;

            float cx = cam != null ? cam.transform.position.x : 0f;
            float cy = cam != null ? cam.transform.position.y : 0f;
            camBottom = cy - (cam != null ? cam.orthographicSize : 5f);

            tileA.transform.position = new Vector3(cx, cy, 0f);
            tileB.transform.position = new Vector3(cx, cy + tileHeight, 0f);
        }

        SpriteRenderer MakeTile(string name, Sprite sprite, int order, Color tint, Material mat)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            sr.color = tint;
            if (mat != null) sr.sharedMaterial = mat;
            return sr;
        }

        void Update()
        {
            if (tileA == null) return;
            float d = speed * Time.deltaTime;
            Scroll(tileA, d);
            Scroll(tileB, d);
        }

        void Scroll(SpriteRenderer s, float d)
        {
            Vector3 p = s.transform.position;
            p.y -= d;
            // 완전히 화면 아래로 빠지면 2장 높이만큼 위로 되돌림
            if (p.y + tileHeight * 0.5f < camBottom)
                p.y += tileHeight * 2f;
            s.transform.position = p;
        }
    }
}
