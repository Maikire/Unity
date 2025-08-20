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
        //储存已生成的对象，循环利用
        private static Dictionary<string, object> Cache = new Dictionary<string, object>();

        /// <summary>
        /// 创建对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="className"></param>
        /// <returns></returns>
        private static T CreateObject<T>(string className) where T : class
        {
            if (Cache.ContainsKey(className))
            {
                return Cache[className] as T;
            }
            else
            {
                Type skillType = Type.GetType(className);
                object temp = Activator.CreateInstance(skillType);
                Cache.Add(className, temp);
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
            //选区对象命名规则：SkillSystem. + SelectorType + AttackSelector
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
            //影响效果对象命名规则：SkillSystem. + ImpactType[i] + Impact
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

