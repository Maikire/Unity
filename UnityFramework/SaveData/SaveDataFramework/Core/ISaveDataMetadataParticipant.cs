using Newtonsoft.Json.Linq;

namespace SaveData
{
    /// <summary>
    /// 可选的模块元信息
    /// </summary>
    public interface ISaveDataMetadataParticipant
    {
        /// <summary>
        /// 采集归属于本模块 ID 的独立元信息
        /// </summary>
        JToken CaptureMetadata();

        /// <summary>
        /// 全部模块恢复成功后通知模块，避免抛出异常
        /// </summary>
        void OnRestored(bool usedDefaults);

    }
}
