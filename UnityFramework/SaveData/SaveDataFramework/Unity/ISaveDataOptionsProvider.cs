namespace SaveData
{
    /// <summary>
    /// 普通 C# 项目配置扩展，在基础选项建立后覆盖项目专属配置
    /// </summary>
    public interface ISaveDataOptionsProvider
    {
        /// <summary>
        /// 在初始化线程同步配置选项，同名配置覆盖窗口选项，网络请求应由启动流程异步预加载
        /// </summary>
        void Configure(SaveDataOptions options);

    }
}
