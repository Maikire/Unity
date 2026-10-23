namespace SaveData
{
    /// <summary>
    /// 项目相邻版本迁移，可处理模块结构和字符串 ID 重定向
    /// </summary>
    public interface ISaveDataMigration
    {
        /// <summary>
        /// 此步骤的源版本，目标版本由管线递增
        /// </summary>
        int FromVersion { get; }

        /// <summary>
        /// 转换文档字段
        /// </summary>
        void Migrate(SaveDataDocument document);

    }
}
