namespace SaveData
{
    /// <summary>
    /// 项目服务器读取接缝，项目实现检查权限、用户隔离、HTTP、重试、响应限制
    /// </summary>
    public interface IServerSaveDataBackend : ISaveDataReader
    {
    }
}
