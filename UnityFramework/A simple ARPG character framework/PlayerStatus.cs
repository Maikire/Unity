using UnityEngine;

namespace Character
{
    /// <summary>
    /// PlayerStatus
    /// </summary>
    public class PlayerStatus : CharacterStatus
    {
        private Animator PlayerAnimator;

        private void Awake()
        {
            PlayerAnimator = this.transform.GetComponentInChildren<Animator>();
        }

        ////隐藏方法
        //private new void Start()
        //{
        //    base.Start();
        //}

        /// <summary>
        /// 死亡
        /// </summary>
        public override void Die()
        {
            PlayerAnimator.SetBool(PlayerAnimationParameter.Death, true);
        }


    }
}
