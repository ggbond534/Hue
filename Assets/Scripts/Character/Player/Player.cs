using System.Diagnostics.Contracts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Character.Player
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private float bombCooldown = 1.0f;
        [SerializeField] private GameObject hiddenItem;
        private float _nextThrowTime = 0f;
        public Vector2 moveVector;
        private Rigidbody2D _rb;
        private Animator _playerAnimator;
        private GameObject _bomb;
        private SpriteRenderer _playerRenderer;
        [SerializeField]private UnityEngine.Camera mainCamera;
        [SerializeField]private float speed;
        [SerializeField] private GameObject bomb;
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
        public void ShowHiddenItem()
        {
            if (hiddenItem)
                hiddenItem.SetActive(true);
        }

        public void HideHiddenItem()
        {
            if (hiddenItem)
                hiddenItem.SetActive(false);
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

        public void ThrowBomb(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            if (Time.time < _nextThrowTime) return;
            _nextThrowTime = Time.time + bombCooldown;

            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, 10)
            );

            Vector3 bombPos = transform.GetChild(0).position;
            _bomb = Instantiate(bomb, bombPos, Quaternion.identity);

            ThrowableItem bombScript = _bomb.GetComponent<ThrowableItem>();
            if (bombScript != null)
            {
                bombScript.startPos = bombPos;
                bombScript.targetPos = worldPos;
            }
        }
    }
}
