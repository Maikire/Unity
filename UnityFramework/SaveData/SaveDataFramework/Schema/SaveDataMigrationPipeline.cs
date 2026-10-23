using System;
using System.Collections.Generic;
using System.IO;

namespace SaveData
{
    /// <summary>
    /// 按项目 schema 迁移逻辑字段，与文件编码及存储后端分离
    /// </summary>
    public sealed class SaveDataMigrationPipeline
    {
        /// <summary>
        /// 以源版本索引相邻迁移步骤
        /// </summary>
        private readonly Dictionary<int, ISaveDataMigration> migrations = new Dictionary<int, ISaveDataMigration>();

        /// <summary>
        /// 管线使用的项目契约
        /// </summary>
        private readonly ISaveDataSchema schema;

        /// <summary>
        /// 创建指定 schema 的迁移管线
        /// </summary>
        public SaveDataMigrationPipeline(ISaveDataSchema schema) => this.schema = schema ?? throw new ArgumentNullException(nameof(schema));

        /// <summary>
        /// 注册相邻版本迁移，拒绝重复或越界步骤
        /// </summary>
        public void Register(ISaveDataMigration migration)
        {
            if (migration == null)
            {
                throw new ArgumentNullException(nameof(migration));
            }

            if (migration.FromVersion < 0 || migration.FromVersion >= schema.CurrentVersion)
            {
                throw new ArgumentOutOfRangeException(nameof(migration));
            }

            if (migrations.ContainsKey(migration.FromVersion))
            {
                throw new ArgumentException("迁移源版本重复", nameof(migration));
            }

            migrations.Add(migration.FromVersion, migration);
        }

        /// <summary>
        /// 逐级迁移，拒绝不匹配 schema、未来版本、缺失步骤
        /// </summary>
        public SaveDataDocument Upgrade(SaveDataDocument document)
        {
            if (document == null || document.SchemaId != schema.SchemaId)
            {
                throw new InvalidDataException("存档 schema 不匹配");
            }

            if (document.Version < 0 || document.Version > schema.CurrentVersion)
            {
                throw new InvalidDataException("不支持的版本");
            }

            while (document.Version < schema.CurrentVersion)
            {
                int previous = document.Version;
                if (!migrations.TryGetValue(previous, out ISaveDataMigration migration))
                {
                    throw new InvalidDataException($"缺少版本 {previous} 的迁移");
                }

                migration.Migrate(document);
                if (document.Version != previous || document.SchemaId != schema.SchemaId)
                {
                    throw new InvalidDataException("迁移不能自行修改版本号或 schema");
                }

                document.Version++;
            }
            schema.Validate(document);
            return document;
        }

    }
}
