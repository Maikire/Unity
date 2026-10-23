using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SaveData.Editor
{
    /// <summary>
    /// 存档项目设置窗口，通过 Resources 资产交付给构建并支持项目接口提供器
    /// </summary>
    public sealed class SaveDataSettingsWindow : EditorWindow
    {
        /// <summary>
        /// 当前编辑的设置资产或尚未保存的工作副本
        /// </summary>
        private SaveDataSettings settings;

        /// <summary>
        /// Unity 序列化编辑对象
        /// </summary>
        private SerializedObject serializedSettings;

        /// <summary>
        /// 当前窗口滚动位置
        /// </summary>
        private Vector2 scroll;

        /// <summary>
        /// 配置验证的结果文本
        /// </summary>
        private string validationMessage;

        /// <summary>
        /// 配置验证是否成功
        /// </summary>
        private bool validationSucceeded;

        /// <summary>
        /// 从菜单打开统一设置窗口
        /// </summary>
        [MenuItem("Tools/SaveData/Settings")]
        public static void Open() => GetWindow<SaveDataSettingsWindow>("SaveData Settings");

        /// <summary>
        /// 读取设置资产，没有资产时创建未保存的工作副本
        /// </summary>
        private void OnEnable()
        {
            minSize = new Vector2(480, 580);
            settings = Resources.Load<SaveDataSettings>(SaveDataSettings.ResourceName);
            if (settings == null)
            {
                settings = CreateInstance<SaveDataSettings>();
            }

            serializedSettings = new SerializedObject(settings);
        }

        /// <summary>
        /// 释放未持久化的工作副本，保留项目资产
        /// </summary>
        private void OnDisable()
        {
            if (settings != null && !EditorUtility.IsPersistent(settings))
            {
                DestroyImmediate(settings);
            }
        }

        /// <summary>
        /// 显示所有基础可选项及项目接口提供器，在非运行状态下保存项目配置
        /// </summary>
        private void OnGUI()
        {
            if (settings == null || serializedSettings == null)
            {
                OnEnable();
                return;
            }
            EditorGUILayout.HelpBox("设置用于下次初始化存档会话，保存后自动随构建交付", MessageType.Info);
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                serializedSettings.Update();
                scroll = EditorGUILayout.BeginScrollView(scroll);
                EditorGUILayout.LabelField("文件格式", EditorStyles.boldLabel);
                Field("editorFormat", "编辑器格式");
                Field("developmentFormat", "开发构建格式");
                Field("releaseFormat", "发布构建格式");
                Field("prettyPrintJson", "JSON 缩进");
                EditorGUILayout.HelpBox("本地存档仅识别栏位名.dat 和 .backup", MessageType.Info);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("压缩", EditorStyles.boldLabel);
                Field("compressionMode", "默认压缩");
                if ((SaveDataCompressionMode)serializedSettings.FindProperty("compressionMode").enumValueIndex == SaveDataCompressionMode.Custom)
                {
                    SaveDataScriptReferenceEditor.Draw(serializedSettings.FindProperty("customCompression"),
                        "自定义压缩（ISaveDataCompression）", typeof(ISaveDataCompression));
                }
                EditorGUILayout.HelpBox("先压缩再加密，读写都使用当前压缩方式；更改压缩方式会影响旧存档和备份的读取", MessageType.Info);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("加密", EditorStyles.boldLabel);
                Field("encryptionMode", "加密方式");
                if ((SaveDataEncryptionMode)serializedSettings.FindProperty("encryptionMode").enumValueIndex == SaveDataEncryptionMode.Custom)
                {
                    SaveDataScriptReferenceEditor.Draw(serializedSettings.FindProperty("customEncryption"),
                        "自定义加密（ISaveDataEncryption）", typeof(ISaveDataEncryption));
                }
                Field("encryptionKeyId", "密钥 ID");
                DrawKeyProvider();
                var mode = (SaveDataEncryptionMode)serializedSettings.FindProperty("encryptionMode").enumValueIndex;
                if (mode == SaveDataEncryptionMode.Xor)
                {
                    EditorGUILayout.HelpBox("异或仅用于混淆，不提供防篡改保证", MessageType.Info);
                }

                if (mode == SaveDataEncryptionMode.Aes256Gcm)
                {
                    EditorGUILayout.HelpBox("AES 密钥使用 Base64 编码，解码后必须为 32 字节", MessageType.Info);
                }

                EditorGUILayout.HelpBox("加密和解密都使用当前加密方式与密钥；更改加密方式会使旧存档及备份失效，修改密钥 ID 或密钥内容也可能导致无法读取", MessageType.Warning);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("本地存储与容量", EditorStyles.boldLabel);
                Field("directoryName", "持久化相对目录");
                Field("maxBytes", "单档字节上限");
                Field("maxNodes", "逻辑节点上限");
                Field("maxSlots", "栏位数量上限");
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("生命周期", EditorStyles.boldLabel);
                Field("readinessTimeoutSeconds", "就绪超时（秒）");
                EditorGUILayout.HelpBox("Web 默认由主循环按实际时间触发就绪取消，参与模块必须响应取消并结束等待", MessageType.Info);
                Field("enforceThreadAffinity", "约束调用线程");
                Field("useBackgroundIO", "后台执行本地 IO");
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("项目数据契约", EditorStyles.boldLabel);
                Field("schemaId", "Schema ID");
                Field("schemaVersion", "结构版本");
                Field("requiredParticipants", "必需模块 ID");
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("扩展接口", EditorStyles.boldLabel);
                SaveDataScriptReferenceEditor.Draw(serializedSettings.FindProperty("optionsProvider"),
                    "项目配置提供器（ISaveDataOptionsProvider）", typeof(ISaveDataOptionsProvider));
                EditorGUILayout.HelpBox("拖入实现 ISaveDataOptionsProvider 的普通 C# 脚本，配置在最后应用；提供器中设置的选项会覆盖这里的选项，可配置计时服务、Schema、迁移、存储或服务器后端、内部编码器、当前压缩和加密实现", MessageType.Info);
                EditorGUILayout.EndScrollView();
                if (serializedSettings.ApplyModifiedProperties())
                {
                    validationMessage = null;
                }

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("验证设置"))
                {
                    ValidateSettings();
                }

                if (GUILayout.Button("保存设置"))
                {
                    SaveSettings();
                }

                EditorGUILayout.EndHorizontal();
                if (!string.IsNullOrEmpty(validationMessage))
                {
                    if (validationSucceeded)
                    {
                        EditorGUILayout.HelpBox(validationMessage, MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox(validationMessage, MessageType.Error);
                    }
                }
            }
        }

        /// <summary>
        /// 绘制含集合展开能力的序列化选项
        /// </summary>
        private void Field(string name, string label) =>
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(name), new GUIContent(label), true);

        /// <summary>
        /// 按密钥来源显示唯一的本地密钥或普通 C# 脚本
        /// </summary>
        private void DrawKeyProvider()
        {
            SerializedProperty source = serializedSettings.FindProperty("keyProviderMode");
            source.enumValueIndex = EditorGUILayout.Popup("密钥来源", source.enumValueIndex, new[]
            {
                "自定义密钥提供", "本地密钥提供"
            });
            var mode = (SaveDataKeyProviderMode)source.enumValueIndex;
            if (mode == SaveDataKeyProviderMode.Local)
            {
                Field("localKeyBase64", "本地密钥（Base64）");
                EditorGUILayout.HelpBox("填写上方密钥 ID 和一个对应密钥的 Base64 编码，AES 解码后须为 32 字节，异或须为非空字节；本地密钥会随设置资产进入构建", MessageType.Info);
                return;
            }

            SaveDataScriptReferenceEditor.Draw(serializedSettings.FindProperty("encryptionKeyProvider"),
                "自定义提供器（ISaveDataKeyProvider）", typeof(ISaveDataKeyProvider));
            EditorGUILayout.HelpBox("拖入实现接口的普通 C# 脚本，类必须公开且具有公开无参构造函数，框架在创建会话选项时生成新实例；网络密钥须在启动流程中异步预加载并等待完成，GetKey 只按密钥 ID 同步读取缓存且支持后台线程", MessageType.Info);
        }

        /// <summary>
        /// 检查脚本类型、基础选项和编码组合
        /// </summary>
        private bool ValidateSettings()
        {
            try
            {
                SaveDataScriptReferenceEditor.Synchronize(settings);
                SaveDataOptions options = settings.CreateOptions();
                if (options.Schema == null || options.MaxBytes < 1 || options.MaxNodes < 1 || options.MaxSlots < 1)
                {
                    throw new ArgumentException("数据契约和容量配置无效");
                }

                if (options.Storage == null && (string.IsNullOrWhiteSpace(options.DirectoryName) || Path.IsPathRooted(options.DirectoryName)
                                    || Array.Exists(options.DirectoryName.Split('/', '\\'), segment => segment == "..")))
                {
                    throw new ArgumentException("本地目录必须为非空且不穿越根目录的相对路径");
                }

                SaveDataSerializerFactory.Create(options);
                validationSucceeded = true;
                validationMessage = "设置有效，本地配置已检查，自定义提供器须在项目运行时预加载密钥";
                return true;
            }
            catch (Exception exception)
            {
                validationSucceeded = false;
                validationMessage = exception.Message;
                return false;
            }
        }

        /// <summary>
        /// 保存或创建默认设置资源，记录 Undo 并刷新窗口的序列化引用
        /// </summary>
        private void SaveSettings()
        {
            if (!ValidateSettings())
            {
                return;
            }

            if (!EditorUtility.IsPersistent(settings))
            {
                string directory = Path.GetDirectoryName(SaveDataSettings.AssetPath);
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
                if (AssetDatabase.LoadMainAssetAtPath(SaveDataSettings.AssetPath) != null)
                {
                    validationSucceeded = false;
                    validationMessage = "默认设置路径已被其他资产占用";
                    return;
                }
                AssetDatabase.CreateAsset(settings, SaveDataSettings.AssetPath);
            }
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            validationSucceeded = true;
            validationMessage = "已保存，配置将在下次存档会话初始化时生效";
        }

    }
}
