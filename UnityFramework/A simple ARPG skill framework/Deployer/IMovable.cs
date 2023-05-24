using UnityEngine;

namespace ARPGDemo.Skill
{
    /// <summary>
    /// 可移动的
    /// </summary>
    public interface IMovable
    {
        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="position">目标位置 </param>
        public void OnMove(Vector3 position);
    }
}
