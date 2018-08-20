using JetBrains.Annotations;
using UnityEngine;

namespace UnitySampleAssets._2D
{
    public class Beam : MonoBehaviour
    {
        public Sprite StartSprite;
        public Sprite EndSprite;
        public Sprite BeamSprite;

        [CanBeNull] private GameObject _start;
        [CanBeNull] private GameObject _end;

        public void Draw(float length)
        {
            if (this._start == null)
            {
                this._start = new GameObject("Start");
                this._start.transform.parent = this.transform;
                this._start.transform.localPosition = Vector3.zero;
                var renderer = this._start.AddComponent<SpriteRenderer>();
                renderer.sprite = this.StartSprite;
                renderer.sortingLayerName = "Player";
            }
            if (this._end == null)
            {
                this._end = new GameObject("End");
                this._end.transform.parent = this.transform;
                var renderer = this._end.AddComponent<SpriteRenderer>();
                renderer.sprite = this.EndSprite;
                renderer.sortingLayerName = "Player";
            }
            this._end.transform.localPosition = new Vector2(length, 0f);
        }

        public void Erase()
        {
            if (this._start != null)
            {
                Destroy(this._start);
                this._start = null;
            }
            if (this._end != null)
            {
                Destroy(this._end);
                this._end = null;
            }
        }
    }
}
