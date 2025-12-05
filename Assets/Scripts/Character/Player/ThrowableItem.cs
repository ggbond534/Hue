using System;
using UnityEngine;

namespace Character.Player
{
    public class ThrowableItem : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 1f;
        [SerializeField] private float torqueForce = 100f;
        public Vector3 startPos;
        public Vector3 targetPos;
        private Vector2 _targetPos2D;
        private Animator _bombAnimator;
        private Rigidbody2D _rb;
        private static readonly int IsExplode = Animator.StringToHash("IsExplode");
        private readonly bool _isMoving = true;
        private void Start()
        {
            _bombAnimator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody2D>();
            _bombAnimator.SetBool(IsExplode,false);
            _targetPos2D = new Vector2(targetPos.x, targetPos.y);
            Vector2 direction = (_targetPos2D - (Vector2)transform.position).normalized;
            _rb.velocity = direction * moveSpeed;
            _rb.AddTorque(torqueForce * Mathf.Sign(-direction.x));
        }
        void FixedUpdate()
        {
            if (!_isMoving) return;
            Vector2 currentPos = new Vector2(transform.position.x, transform.position.y);
            float distance = Vector2.Distance(currentPos, _targetPos2D);
            if (distance < 0.2f)
            {
                _rb.velocity = Vector2.zero;
                _rb.angularVelocity = 0;
                _bombAnimator.SetBool(IsExplode,true);
            }
        }
        void DeleteBomb()
        {
            Destroy(gameObject);
        }
    }
}
