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

        /// <summary>
        /// 查找扇形（或圆形）范围内的目标
        /// </summary>
        /// <param name="center">中心物体</param>
        /// <param name="distance">距离</param>
        /// <param name="angle">角度</param>
        /// <param name="targetTags">标签</param>
        /// <returns></returns>
        public static List<Transform> SelectTargets(this Transform center, float distance, float angle, string[] targetTags = null)
        {
            List<Transform> targets = new List<Transform>();

            //使用球形射线
            RaycastHit[] raycastHits = Physics.SphereCastAll(center.position, distance, center.forward, distance);

            //获取范围内有特定标签的目标
            if (targetTags != null)
            {
                foreach (string tag in targetTags)
                {
                    foreach (RaycastHit raycast in raycastHits)
                    {
                        if (raycast.transform.tag == tag)
                        {
                            targets.Add(raycast.transform);
                        }
                    }
                }
            }
            else
            {
                foreach (RaycastHit raycast in raycastHits)
                {
                    //不能选择自己
                    if (raycast.transform.name != center.name)
                    {
                        targets.Add(raycast.transform);
                    }
                }
            }

            //扇形选取
            targets = targets.FindAll(t => Vector3.Angle(center.forward, t.position - center.position) <= (angle / 2));

            return targets;
        }


    }
}
