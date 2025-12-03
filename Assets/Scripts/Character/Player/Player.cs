using UnityEngine;
using UnityEngine.InputSystem;

namespace Character.Player
{
    public class Player : MonoBehaviour
    {
        public Vector2 moveVector;      //存储玩家的移动方向和速度
        private Rigidbody2D _rb;        //存储玩家的 Rigidbody2D 组件，用于处理物理交互
        private Animator _playerAnimator;       //存储玩家的 Animator 组件，用于控制动画
        private SpriteRenderer _playerRenderer;     //存储玩家的 SpriteRenderer 组件，用于渲染玩家的精灵
        [SerializeField]private float speed;        //玩家的移动速度
        public static Player Instance;      //单例模式的实例，确保只有一个 Player 实例存在
        private static readonly int IsRun = Animator.StringToHash("IsRun");     //动画器中 "IsRun" 参数的哈希值，用于标识玩家是否在跑动

        void Start()
        {
            Instance = this;        //将当前实例赋值给 Instance，确保单例模式
            //获取 Rigidbody2D、Animator 和 SpriteRenderer 组件，并将其赋值给相应的私有变量
            _rb = GetComponent<Rigidbody2D>();
            _playerAnimator = GetComponent<Animator>();
            _playerRenderer = GetComponent<SpriteRenderer>();
        }
        void Update()
        {//根据玩家是否在移动（即 Rigidbody2D 的速度是否为零）来更新动画器中的 "IsRun" 参数
            _rb.velocity = moveVector * speed;
            _playerAnimator.SetBool(IsRun, _rb.velocity != Vector2.zero);
        }
        public void Move(InputAction.CallbackContext ctx)
        {//Move 方法处理玩家的移动输入
            //从输入上下文 ctx 中读取移动向量并赋值给 moveVector
            moveVector = ctx.ReadValue<Vector2>();
<<<<<<< Updated upstream
=======
            //根据 moveVector 的 x 方向翻转玩家的精灵。如果 moveVector.x 小于 0，则翻转精灵；否则，不翻转
>>>>>>> Stashed changes
            if(moveVector.x < 0)
            {
                _playerRenderer.flipX = true;
            }
            else if(moveVector.x > 0)
            {
                _playerRenderer.flipX = false;
            }
        }
    }
}
