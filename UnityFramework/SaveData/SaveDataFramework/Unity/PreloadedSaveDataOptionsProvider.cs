using System;

namespace SaveData.Examples
{
    /// <summary>
    /// 将本脚本拖入设置窗口的项目配置提供器，演示在最后覆盖窗口中的同名选项
    /// </summary>
    public sealed class PreloadedSaveDataOptionsProvider : ISaveDataOptionsProvider
    {
        /// <summary>
        /// 配置项目目录、容量和超时，复杂接口也可在此赋给 options
        /// </summary>
        public void Configure(SaveDataOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            options.DirectoryName = "SaveData/ExampleProject";
            options.MaxSlots = 32;
            options.ReadinessTimeout = TimeSpan.FromSeconds(15);
            options.CompressionMode = SaveDataCompressionMode.None;
            options.Compression = new NoSaveDataCompression();
        }

    }
}
