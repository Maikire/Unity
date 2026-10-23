using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SaveData.Editor
{
    /// <summary>
    /// 将普通 C# 脚本选择转换为运行时类型引用
    /// </summary>
    [InitializeOnLoad]
    public static class SaveDataScriptReferenceEditor
    {
        /// <summary>
        /// 按接口缓存筛选结果，避免每次展开都遍历项目脚本
        /// </summary>
        private static readonly Dictionary<Type, MonoScript[]> selectableScripts = new Dictionary<Type, MonoScript[]>();

        /// <summary>
        /// 缓存可实例化的接口实现类型，普通类解析使用 Unity 类型索引
        /// </summary>
        private static readonly Dictionary<Type, Type[]> implementationTypes = new Dictionary<Type, Type[]>();

        /// <summary>
        /// 缓存已选脚本的解析结果，路径参与索引以识别脚本移动
        /// </summary>
        private static readonly Dictionary<(int ScriptId, Type Contract, string Path), Type> resolvedTypes =
            new Dictionary<(int ScriptId, Type Contract, string Path), Type>();

        /// <summary>
        /// 缓存无效脚本的诊断，避免重复查询编译管线
        /// </summary>
        private static readonly Dictionary<(int ScriptId, Type Contract, string Path), string> resolutionErrors =
            new Dictionary<(int ScriptId, Type Contract, string Path), string>();

        /// <summary>
        /// 首次实际选择脚本时读取运行时程序集列表，空栏位不查询编译管线
        /// </summary>
        private static HashSet<string> playerAssemblies;

        /// <summary>
        /// 所有接口共享一次运行时脚本枚举
        /// </summary>
        private static MonoScript[] runtimeScripts;

        /// <summary>
        /// 项目资源改变或程序集重载时清除缓存，后续选择重新校验
        /// </summary>
        static SaveDataScriptReferenceEditor()
        {
            EditorApplication.projectChanged += InvalidateCaches;
            AssemblyReloadEvents.beforeAssemblyReload += InvalidateCaches;
        }

        /// <summary>
        /// 清除依赖资源路径与编译结果的缓存
        /// </summary>
        private static void InvalidateCaches()
        {
            selectableScripts.Clear();
            implementationTypes.Clear();
            resolvedTypes.Clear();
            resolutionErrors.Clear();
            playerAssemblies = null;
            runtimeScripts = null;
        }

        /// <summary>
        /// 绘制可拖入脚本的接口选择栏位，选择列表仅显示有效的运行时实现类
        /// </summary>
        public static void Draw(SerializedProperty reference, string label, Type contract)
        {
            SerializedProperty script = reference.FindPropertyRelative("editorScript");
            Rect position = EditorGUILayout.GetControlRect();
            Rect field = EditorGUI.PrefixLabel(position, new GUIContent(label, "拖入实现 " + contract.Name + " 的普通 C# 脚本"));
            GUIContent content;
            if (script.objectReferenceValue != null)
            {
                content = EditorGUIUtility.ObjectContent(script.objectReferenceValue, typeof(MonoScript));
            }
            else
            {
                content = new GUIContent("未选择（" + contract.Name + "）");
            }
            if (EditorGUI.DropdownButton(field, content, FocusType.Keyboard, EditorStyles.objectField))
            {
                UnityEngine.Object[] targets = reference.serializedObject.targetObjects;
                string propertyPath = reference.propertyPath;
                var dropdown = new ImplementationDropdown(contract, selected =>
                {
                    if (targets.Any(target => target == null))
                        return;

                    var serialized = new SerializedObject(targets);
                    SerializedProperty selectedReference = serialized.FindProperty(propertyPath);
                    selectedReference.FindPropertyRelative("editorScript").objectReferenceValue = selected;
                    SynchronizeReference(selectedReference, contract);
                    serialized.ApplyModifiedProperties();
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                });
                dropdown.Show(field);
            }
            try
            {
                Event current = Event.current;
                if (GUI.enabled && field.Contains(current.mousePosition)
                    && (current.type == EventType.DragUpdated || current.type == EventType.DragPerform))
                {
                    MonoScript dragged;
                    if (DragAndDrop.objectReferences.Length == 1)
                    {
                        dragged = DragAndDrop.objectReferences[0] as MonoScript;
                    }
                    else
                    {
                        dragged = null;
                    }
                    if (dragged == null)
                        throw new ArgumentException("请拖入一个实现 " + contract.Name + " 的 C# 脚本");

                    GetImplementationType(dragged, contract);
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    if (current.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        script.objectReferenceValue = dragged;
                    }
                    current.Use();
                }
                SynchronizeReference(reference, contract);
            }
            catch (Exception exception)
            {
                if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                }
                EditorGUILayout.HelpBox(exception.Message, MessageType.Error);
            }
        }

        /// <summary>
        /// 从接口类型索引筛选运行时脚本并缓存，返回副本以免调用方污染缓存
        /// </summary>
        public static MonoScript[] GetSelectableScripts(Type contract)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (selectableScripts.TryGetValue(contract, out MonoScript[] cached))
                return (MonoScript[])cached.Clone();

            var names = new HashSet<string>(GetImplementationTypes(contract).Select(type => type.Name), StringComparer.Ordinal);
            var selectable = new List<MonoScript>();
            runtimeScripts = runtimeScripts ?? MonoImporter.GetAllRuntimeMonoScripts();
            foreach (MonoScript script in runtimeScripts)
            {
                if (script == null || !names.Contains(script.name))
                    continue;

                try
                {
                    GetImplementationType(script, contract);
                    selectable.Add(script);
                }
                catch (ArgumentException)
                {
                    continue;
                }
            }
            cached = selectable.Distinct().OrderBy(AssetDatabase.GetAssetPath, StringComparer.Ordinal).ToArray();
            selectableScripts.Add(contract, cached);
            return (MonoScript[])cached.Clone();
        }

        /// <summary>
        /// 同步当前启用的脚本引用，构建前刷新重命名后的类名与程序集名
        /// </summary>
        public static void Synchronize(SaveDataSettings settings)
        {
            var serialized = new SerializedObject(settings);
            SynchronizeReference(serialized.FindProperty("optionsProvider"), typeof(ISaveDataOptionsProvider));
            if ((SaveDataKeyProviderMode)serialized.FindProperty("keyProviderMode").enumValueIndex == SaveDataKeyProviderMode.Custom)
            {
                SynchronizeReference(serialized.FindProperty("encryptionKeyProvider"), typeof(ISaveDataKeyProvider));
            }
            if ((SaveDataCompressionMode)serialized.FindProperty("compressionMode").enumValueIndex == SaveDataCompressionMode.Custom)
            {
                SynchronizeReference(serialized.FindProperty("customCompression"), typeof(ISaveDataCompression));
            }
            if ((SaveDataEncryptionMode)serialized.FindProperty("encryptionMode").enumValueIndex == SaveDataEncryptionMode.Custom)
            {
                SynchronizeReference(serialized.FindProperty("customEncryption"), typeof(ISaveDataEncryption));
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// 解析并缓存普通脚本类型，空引用直接返回，重绘不重复查询编译管线
        /// </summary>
        public static Type GetImplementationType(MonoScript script, Type contract)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (script == null)
                return null;

            string path = AssetDatabase.GetAssetPath(script);
            var key = (script.GetInstanceID(), contract, path);
            if (resolvedTypes.TryGetValue(key, out Type cached))
                return cached;

            if (resolutionErrors.TryGetValue(key, out string error))
                throw new ArgumentException(error);

            try
            {
                if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("请拖入 .cs 脚本文件");

                playerAssemblies = playerAssemblies ?? new HashSet<string>(
                    CompilationPipeline.GetAssemblies(AssembliesType.Player).Select(assembly => assembly.name));
                string assemblyName = Path.GetFileNameWithoutExtension(CompilationPipeline.GetAssemblyNameFromScriptPath(path));
                if (!playerAssemblies.Contains(assemblyName))
                    throw new ArgumentException("扩展类必须位于运行时程序集，不能放在 Editor 文件夹或仅编辑器程序集");

                Type implementationType = script.GetClass();
                if (implementationType == null)
                {
                    string className = Path.GetFileNameWithoutExtension(path);
                    Type[] candidates = GetImplementationTypes(contract).Where(type => type.Name == className
                        && type.Assembly.GetName().Name == assemblyName).ToArray();
                    if (candidates.Length != 1)
                        throw new ArgumentException("脚本应包含一个与文件同名且实现 " + contract.Name + " 的公开普通类");

                    implementationType = candidates[0];
                }
                SaveDataImplementationReference.ValidateType(implementationType, contract);
                resolvedTypes.Add(key, implementationType);
                return implementationType;
            }
            catch (ArgumentException exception)
            {
                resolutionErrors[key] = exception.Message;
                throw;
            }
        }

        /// <summary>
        /// 查询 Unity 类型索引并仅校验目标接口实现，避免对无关脚本逐个反射和抛异常
        /// </summary>
        private static Type[] GetImplementationTypes(Type contract)
        {
            if (implementationTypes.TryGetValue(contract, out Type[] cached))
                return cached;

            var types = new List<Type>();
            foreach (Type type in TypeCache.GetTypesDerivedFrom(contract))
            {
                try
                {
                    SaveDataImplementationReference.ValidateType(type, contract);
                    types.Add(type);
                }
                catch (ArgumentException)
                {
                    continue;
                }
            }
            cached = types.ToArray();
            implementationTypes.Add(contract, cached);
            return cached;
        }

        /// <summary>
        /// 同步一个脚本引用的可持久化类型信息，无效选择清除旧类型以免沿用旧实现
        /// </summary>
        public static void SynchronizeReference(SerializedProperty reference, Type contract)
        {
            UnityEngine.Object selected = reference.FindPropertyRelative("editorScript").objectReferenceValue;
            if (selected != null && !(selected is MonoScript))
                throw new ArgumentException("请拖入实现 " + contract.Name + " 的 C# 脚本，不接受对象资产");

            try
            {
                Type implementationType = GetImplementationType(selected as MonoScript, contract);
                SetTypeNames(reference, implementationType);
            }
            catch
            {
                reference.FindPropertyRelative("typeName").stringValue = "";
                reference.FindPropertyRelative("assemblyName").stringValue = "";
                throw;
            }
        }

        /// <summary>
        /// 仅在名称改变时修改序列化数据
        /// </summary>
        private static void SetTypeNames(SerializedProperty reference, Type implementationType)
        {
            SerializedProperty typeName = reference.FindPropertyRelative("typeName");
            SerializedProperty assemblyName = reference.FindPropertyRelative("assemblyName");
            string nextType = implementationType?.FullName ?? "";
            string nextAssembly = implementationType?.Assembly.GetName().Name ?? "";
            if (typeName.stringValue != nextType)
            {
                typeName.stringValue = nextType;
            }
            if (assemblyName.stringValue != nextAssembly)
            {
                assemblyName.stringValue = nextAssembly;
            }
        }

        /// <summary>
        /// 支持搜索的接口实现选择列表，保留清除选择的入口
        /// </summary>
        private sealed class ImplementationDropdown : AdvancedDropdown
        {
            /// <summary>
            /// 当前筛选的接口
            /// </summary>
            private readonly Type contract;

            /// <summary>
            /// 选择完成后更新序列化引用
            /// </summary>
            private readonly Action<MonoScript> selected;

            /// <summary>
            /// 初始化独立的可搜索下拉窗口
            /// </summary>
            public ImplementationDropdown(Type contract, Action<MonoScript> selected) : base(new AdvancedDropdownState())
            {
                this.contract = contract;
                this.selected = selected;
                minimumSize = new Vector2(480, 320);
            }

            /// <summary>
            /// 显示实现类的完整名称与来源脚本，支持同名类区分
            /// </summary>
            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem(contract.Name);
                root.AddChild(new ScriptItem("未选择", null));
                foreach (MonoScript script in GetSelectableScripts(contract))
                {
                    Type type = GetImplementationType(script, contract);
                    root.AddChild(new ScriptItem(type.FullName + " (" + script.name + ".cs)", script));
                }
                return root;
            }

            /// <summary>
            /// 提交有效脚本或清除当前选择
            /// </summary>
            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                if (item is ScriptItem script)
                {
                    selected(script.Script);
                }
            }

        }

        /// <summary>
        /// 将筛选后的脚本绑定到选择列表项
        /// </summary>
        private sealed class ScriptItem : AdvancedDropdownItem
        {
            /// <summary>
            /// 该列表项对应的脚本，空值表示清除选择
            /// </summary>
            public MonoScript Script { get; }

            /// <summary>
            /// 创建脚本列表项
            /// </summary>
            public ScriptItem(string name, MonoScript script) : base(name)
            {
                Script = script;
            }

        }

    }
}
