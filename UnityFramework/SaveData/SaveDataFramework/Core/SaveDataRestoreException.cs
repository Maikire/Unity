using System;

namespace SaveData
{
    /// <summary>
    /// 恢复提交失败，明确告知调用方回退是否完整成功
    /// </summary>
    public sealed class SaveDataRestoreException : Exception
    {
        /// <summary>
        /// 全部被修改模块是否成功回退
        /// </summary>
        public bool RuntimeStateConsistent { get; }

        /// <summary>
        /// 保存提交和回退失败的诊断信息
        /// </summary>
        public SaveDataRestoreException(bool consistent, Exception exception)
            : base(CreateMessage(consistent), exception) => RuntimeStateConsistent = consistent;

        /// <summary>
        /// 根据运行时回退结果选择诊断消息
        /// </summary>
        private static string CreateMessage(bool consistent)
        {
            if (consistent)
            {
                return "恢复失败，运行时已回退";
            }
            else
            {
                return "恢复及回退失败，需要重建运行时";
            }
        }

    }
}
