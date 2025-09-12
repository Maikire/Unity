using System;
using System.Collections.Generic;

namespace SkillSystem
{
    /// <summary>
    /// 释放器工厂
    /// 创建释放器算法
    /// </summary>
    public class DeployerFactory
    {
        /// <summary>
        /// 储存已生成的对象，循环利用
        /// </summary>
        private static Dictionary<string, object> ObjectCache;
        /// <summary>
        /// Type 缓存
        /// </summary>
        private static Dictionary<string, Type> TypeCache;

        static DeployerFactory()
        {
            ObjectCache = new Dictionary<string, object>();
            TypeCache = new Dictionary<string, Type>();
        }

        /// <summary>
        /// 获取类型
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static Type GetType(string typeName)
        {
            if (TypeCache.ContainsKey(typeName))
            {
                return TypeCache[typeName];
            }

            Type type = Type.GetType(typeName);

            if (type == null)
            {
                throw new Exception($"找不到类型: {typeName}");
            }

            TypeCache.Add(typeName, type);

            return type;
        }

        /// <summary>
        /// 创建对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="className"></param>
        /// <returns></returns>
        private static T CreateObject<T>(string className) where T : class
        {
            if (ObjectCache.ContainsKey(className))
            {
                return ObjectCache[className] as T;
            }
            else
            {
                Type skillType = GetType(className);
                object temp = Activator.CreateInstance(skillType);
                ObjectCache.Add(className, temp);
                return temp as T;
            }
        }

        /// <summary>
        /// 创建攻击选区
        /// </summary>
        /// <param name="skillData"></param>
        /// <returns></returns>
        public static IAttackSelector CreateAttackSelector(SkillData skillData)
        {
            // 选区对象命名规则：SkillSystem. + SelectorType + AttackSelector
            string selectorClassName = String.Format("SkillSystem.{0}AttackSelector", skillData.SelectorType);
            return CreateObject<IAttackSelector>(selectorClassName);
        }

        /// <summary>
        /// 创建影响效果
        /// </summary>
        /// <param name="skillData"></param>
        /// <returns></returns>
        public static IImpactEffect[] CreateImpactEffects(SkillData skillData)
        {
            // 影响效果对象命名规则：SkillSystem. + ImpactType [i] + Impact
            IImpactEffect[] Impacts = new IImpactEffect[skillData.ImpactType.Length];
            for (int i = 0; i < skillData.ImpactType.Length; i++)
            {
                string impactClassName = String.Format("SkillSystem.{0}Impact", skillData.ImpactType[i]);
                Impacts[i] = CreateObject<IImpactEffect>(impactClassName);
            }
            return Impacts;
        }
    }
}
