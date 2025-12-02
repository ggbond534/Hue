using UnityEngine;
using UnityEngine.InputSystem;

namespace Character.Player
{
    public class Player : MonoBehaviour
    {
        public Vector2 moveVector;
        private Rigidbody2D _rb;
        private Animator _playerAnimator;
        private SpriteRenderer _playerRenderer;
        [SerializeField]private float speed;
        public static Player Instance;
        private static readonly int IsRun = Animator.StringToHash("IsRun");

        void Start()
        {
            Instance = this;
            _rb = GetComponent<Rigidbody2D>();
            _playerAnimator = GetComponent<Animator>();
            _playerRenderer = GetComponent<SpriteRenderer>();
        }
        void Update()
        {
            _rb.velocity = moveVector * speed;
            _playerAnimator.SetBool(IsRun, _rb.velocity != Vector2.zero);
        }

        public void Move(InputAction.CallbackContext ctx)
        {
            moveVector = ctx.ReadValue<Vector2>();
            if (moveVector.x < 0 && transform.localScale.x > 0)
            {
                Vector3 scale = transform.localScale;
                scale.x *= -1;
                transform.localScale = scale;
            }
            else if (moveVector.x > 0 && transform.localScale.x < 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }
    }
}
