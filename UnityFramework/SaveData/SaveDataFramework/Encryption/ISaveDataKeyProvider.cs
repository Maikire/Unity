namespace SaveData
{
    /// <summary>
    /// 项目密钥来源，只按当前配置的字符串密钥 ID 提供密钥
    /// </summary>
    public interface ISaveDataKeyProvider
    {
        /// <summary>
        /// 同步读取预加载密钥并支持后台查询，框架复制后使用
        /// 网络请求须在启动流程异步等待完成，不能在此阻塞等待主线程网络 API
        /// </summary>
        /// <param name="keyId">
        /// 当前设置选择的字符串密钥 ID
        /// </param>
        /// <returns>
        /// 对应的密钥字节，缺失时应抛出密钥异常
        /// </returns>
        byte[] GetKey(string keyId);

    }
}
