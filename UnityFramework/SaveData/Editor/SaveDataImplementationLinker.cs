using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.UnityLinker;

namespace SaveData.Editor
{
    /// <summary>
    /// 构建前同步设置中的脚本类型，并为所选普通扩展类生成 IL2CPP 裁剪保留规则
    /// </summary>
    public sealed class SaveDataImplementationLinker : IPreprocessBuildWithReport, IUnityLinkerProcessor
    {
        /// <summary>
        /// 在其他构建处理器之前同步类名
        /// </summary>
        public int callbackOrder => -1000;

        /// <summary>
        /// 更新设置内的类名，拒绝不能交付给运行时的已选脚本
        /// </summary>
        public void OnPreprocessBuild(BuildReport report)
        {
            GetConfiguredTypes();
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// 向 UnityLinker 提供所有设置资产实际启用扩展类的保留文件
        /// </summary>
        public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            string path = Path.GetFullPath("Library/SaveData/SelectedImplementations.link.xml");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            CreateLinkXml(GetConfiguredTypes()).Save(path);
            return path;
        }

        /// <summary>
        /// 汇总配置中启用的扩展类型
        /// </summary>
        public static Type[] GetConfiguredTypes()
        {
            var types = new HashSet<Type>();
            foreach (string guid in AssetDatabase.FindAssets("t:SaveDataSettings"))
            {
                var settings = AssetDatabase.LoadAssetAtPath<SaveDataSettings>(AssetDatabase.GUIDToAssetPath(guid));
                SaveDataScriptReferenceEditor.Synchronize(settings);
                var serialized = new SerializedObject(settings);
                AddType(types, serialized.FindProperty("optionsProvider"), typeof(ISaveDataOptionsProvider));
                if ((SaveDataKeyProviderMode)serialized.FindProperty("keyProviderMode").enumValueIndex == SaveDataKeyProviderMode.Custom)
                {
                    AddType(types, serialized.FindProperty("encryptionKeyProvider"), typeof(ISaveDataKeyProvider));
                }
                if ((SaveDataCompressionMode)serialized.FindProperty("compressionMode").enumValueIndex == SaveDataCompressionMode.Custom)
                {
                    AddType(types, serialized.FindProperty("customCompression"), typeof(ISaveDataCompression));
                }
                if ((SaveDataEncryptionMode)serialized.FindProperty("encryptionMode").enumValueIndex == SaveDataEncryptionMode.Custom)
                {
                    AddType(types, serialized.FindProperty("customEncryption"), typeof(ISaveDataEncryption));
                }
            }
            return types.OrderBy(type => type.FullName, StringComparer.Ordinal).ToArray();
        }

        /// <summary>
        /// 保留接口方法和所有构造函数
        /// </summary>
        public static XDocument CreateLinkXml(IEnumerable<Type> types)
        {
            var distinctTypes = types.Distinct();
            var sortedTypes = distinctTypes.OrderBy(type => type.FullName, StringComparer.Ordinal);
            var assemblyGroups = sortedTypes.GroupBy(type => type.Assembly.GetName().Name);
            var root = new XElement("linker");

            foreach (var group in assemblyGroups)
            {
                var assemblyElement = new XElement(
                    "assembly",
                    new XAttribute("fullname", group.Key)
                );

                foreach (var type in group)
                {
                    string typeName = type.FullName.Replace('+', '/');

                    var typeElement = new XElement(
                        "type",
                        new XAttribute("fullname", typeName),
                        new XAttribute("preserve", "all")
                    );

                    assemblyElement.Add(typeElement);
                }

                root.Add(assemblyElement);
            }

            return new XDocument(root);
        }

        /// <summary>
        /// 将已选择脚本校验并加入集合，空栏位由运行时选项提供器决定
        /// </summary>
        private static void AddType(HashSet<Type> types, SerializedProperty reference, Type contract)
        {
            var script = reference.FindPropertyRelative("editorScript").objectReferenceValue as MonoScript;
            Type implementationType = SaveDataScriptReferenceEditor.GetImplementationType(script, contract);
            if (implementationType != null)
            {
                types.Add(implementationType);
            }
        }

    }
}
