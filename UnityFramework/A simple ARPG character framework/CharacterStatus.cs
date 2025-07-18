using UnityEngine;

namespace Character
{
    /// <summary>
    /// CharacterStatus
    /// </summary>
    public abstract class CharacterStatus : MonoBehaviour
    {
        [Tooltip("动画信息")]
        public AnimationParameter PlayerAnimationParameter;

        [Tooltip("最大生命值")]
        public float MaxHP = 500;
        [Tooltip("生命值")]
        public float HP = 500;
        [Tooltip("最大魔力")]
        public float MaxMP = 500;
        [Tooltip("魔力")]
        public float MP = 500;
        [Tooltip("防御力")]
        public float Defense = 5;
        [Tooltip("攻击力")]
        public float AttackPower = 10;

        public void Start()
        {
            HP = MaxHP;
            MP = MaxMP;
        }

        /// <summary>
        /// 受伤
        /// </summary>
        /// <param name="damage">伤害值</param>
        public virtual void Damage(float damage)
        {
            float temp = damage - Defense;

            if (temp > 0)
            {
                HP -= temp;
            }

            if (HP <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 死亡
        /// </summary>
        public abstract void Die();


    }
}
