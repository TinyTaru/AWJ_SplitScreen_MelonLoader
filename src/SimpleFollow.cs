using UnityEngine;

namespace AWJSplitScreen
{
    public sealed class SimpleFollow : MonoBehaviour
    {
        private Transform _target;
        private Vector3 _offset;
        private bool _inited;

        public void Init(Transform target)
        {
            _target = target;
            _offset = transform.position - target.position;
            _inited = true;
        }

        private void LateUpdate()
        {
            if (!_inited || _target == null) return;
            transform.position = _target.position + _offset;
        }
    }
}
