using UnityEngine;

namespace Character
{
    /// <summary>
    /// EnemyStatus
    /// </summary>
    public class EnemyStatus : CharacterStatus
    {
        private Animator EnemyAnimator;

        private void Awake()
        {
            EnemyAnimator = this.transform.GetComponentInChildren<Animator>();
        }

        /// <summary>
        /// 死亡
        /// </summary>
        public override void Die()
        {
            EnemyAnimator.SetBool(PlayerAnimationParameter.Death, true);
        }


    }
}
