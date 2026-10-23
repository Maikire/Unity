using System;
using System.Reflection;
using UnityEngine;

namespace SaveData
{
    /// <summary>
    /// 普通 C# 扩展类的持久化类型引用，运行时使用类名与程序集名创建独立实例
    /// </summary>
    [Serializable]
    public sealed class SaveDataImplementationReference
    {
#if UNITY_EDITOR

        [Tooltip("拖入实现目标接口的普通 C# 脚本")]
        [SerializeField]
        private UnityEngine.Object editorScript;

        /// <summary>
        /// 供编辑器同步脚本选择的引用
        /// </summary>
        public UnityEngine.Object EditorScript
        {
            get => editorScript;
            set => editorScript = value;
        }

#endif

        [Tooltip("由脚本选择同步的完整类名")]
        [SerializeField]
        private string typeName = "";

        [Tooltip("由脚本选择同步的程序集名称")]
        [SerializeField]
        private string assemblyName = "";

        /// <summary>
        /// 设置并校验实现类，空类型清除选择，不创建实例或读取密钥
        /// </summary>
        public void SetType(Type implementationType, Type contract)
        {
            if (implementationType == null)
            {
                typeName = "";
                assemblyName = "";
                return;
            }
            ValidateType(implementationType, contract);
            typeName = implementationType.FullName;
            assemblyName = implementationType.Assembly.GetName().Name;
        }

        /// <summary>
        /// 按保存的名称解析并校验类型，未选择时返回空，缺失类型时失败
        /// </summary>
        public Type ResolveType(Type contract)
        {
            if (string.IsNullOrWhiteSpace(typeName) && string.IsNullOrWhiteSpace(assemblyName))
                return null;

            if (string.IsNullOrWhiteSpace(typeName) || string.IsNullOrWhiteSpace(assemblyName))
                throw new ArgumentException("扩展类名与程序集名必须同时存在");

            Type implementationType = Type.GetType(typeName + ", " + assemblyName, false);
            if (implementationType == null)
                throw new ArgumentException("无法加载存档扩展类 " + typeName + "，请重新拖入脚本并确认构建包含其程序集");

            ValidateType(implementationType, contract);
            return implementationType;
        }

        /// <summary>
        /// 创建接口实现，加密类优先使用接收 ISaveDataKeyProvider 的公开构造函数
        /// </summary>
        public T Create<T>(ISaveDataKeyProvider keys = null) where T : class
        {
            Type implementationType = ResolveType(typeof(T));
            if (implementationType == null)
                return null;

            ConstructorInfo constructor = GetKeyConstructor(implementationType, typeof(T));
            object[] arguments = Array.Empty<object>();
            if (constructor != null)
            {
                if (keys == null)
                    throw new ArgumentException("自定义加密类的构造函数需要 ISaveDataKeyProvider，请先配置密钥来源");

                arguments = new object[]
                {
                    keys
                };
            }
            else
            {
                constructor = implementationType.GetConstructor(Type.EmptyTypes);
            }
            try
            {
                return (T)constructor.Invoke(arguments);
            }
            catch (TargetInvocationException exception)
            {
                throw new InvalidOperationException("存档扩展类构造失败 " + implementationType.FullName, exception.InnerException ?? exception);
            }
        }

        /// <summary>
        /// 拒绝接口不匹配、抽象或泛型类型、Unity 对象、缺少支持构造函数的类
        /// </summary>
        public static void ValidateType(Type implementationType, Type contract)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (implementationType == null || !implementationType.IsClass || !implementationType.IsVisible
                || implementationType.IsAbstract || implementationType.ContainsGenericParameters
                || typeof(UnityEngine.Object).IsAssignableFrom(implementationType) || !contract.IsAssignableFrom(implementationType))
                throw new ArgumentException("必须选择公开、非抽象、非开放泛型且实现 " + contract.Name + " 的普通 C# 类");

            if (implementationType.GetConstructor(Type.EmptyTypes) == null && GetKeyConstructor(implementationType, contract) == null)
                throw new ArgumentException(implementationType.FullName + " 需要公开无参构造函数，加密类也可使用公开的 ISaveDataKeyProvider 构造函数");
        }

        /// <summary>
        /// 为加密接口查询符合要求的构造函数
        /// </summary>
        private static ConstructorInfo GetKeyConstructor(Type implementationType, Type contract)
        {
            if (contract == typeof(ISaveDataEncryption))
            {
                return implementationType.GetConstructor(new[]
                {
                    typeof(ISaveDataKeyProvider)
                });
            }
            else
            {
                return null;
            }
        }

    }
}
