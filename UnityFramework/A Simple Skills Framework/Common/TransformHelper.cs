using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// 变换组件助手类
    /// </summary>
    public static class TransformHelper
    {
        /// <summary>
        /// 未知层级，查找指定子物体的变换组件
        /// </summary>
        /// <param name="current">当前变换组件</param>
        /// <param name="childName">查找目标的名称</param>
        /// <returns></returns>
        public static Transform FindChildByName(this Transform current, string childName)
        {
            //在子物体中查找
            Transform childTransform = current.Find(childName);
            if (childTransform != null) return childTransform;

            //将任务交给子物体
            for (int i = 0; i < current.childCount; i++)
            {
                childTransform = FindChildByName(current.GetChild(i), childName);
                if (childTransform != null) return childTransform;
            }

            //查找目标不存在
            return null;
        }


    }
}
