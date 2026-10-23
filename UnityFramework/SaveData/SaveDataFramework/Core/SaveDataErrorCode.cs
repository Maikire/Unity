namespace SaveData
{
    /// <summary>
    /// 操作错误类别，具体原因保留在异常中
    /// </summary>
    public enum SaveDataErrorCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        None,
        /// <summary>
        /// 缺档
        /// </summary>
        NotFound,
        /// <summary>
        /// 文档或版本不兼容
        /// </summary>
        InvalidData,
        /// <summary>
        /// 后端不支持此能力
        /// </summary>
        Unsupported,
        /// <summary>
        /// 调用方显式取消
        /// </summary>
        Cancelled,
        /// <summary>
        /// 恢复失败
        /// </summary>
        RestoreFailed,
        /// <summary>
        /// IO 失败
        /// </summary>
        IOFailure,
        /// <summary>
        /// 生命周期或参数无效
        /// </summary>
        InvalidOperation,
        /// <summary>
        /// 其他项目错误
        /// </summary>
        Unexpected,
        /// <summary>
        /// 加密密钥不可用或密文认证失败
        /// </summary>
        EncryptionFailed,
        /// <summary>
        /// 模块就绪等待超过配置期限
        /// </summary>
        TimedOut
    }
}
