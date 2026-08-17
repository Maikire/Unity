namespace Common
{
    /// <summary>
    /// 表达可重置的
    /// 配合对象池一起使用
    /// </summary>
    public interface IResettable
    {
        /// <summary>
        /// 重置数据
        /// </summary>
        void OnReset();
    }
}
