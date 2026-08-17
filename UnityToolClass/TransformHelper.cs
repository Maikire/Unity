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
        /// <param name="direction">方向</param>
        /// <param name="angle">角度</param>
        /// <param name="targetTags">标签</param>
        /// <returns></returns>
        public static List<Transform> SelectTargets(this Transform center, float distance, Vector3 direction, float angle, string[] targetTags = null)
        {
            List<Transform> targets = new List<Transform>();

            //使用球形射线
            RaycastHit[] raycastHits = Physics.SphereCastAll(center.position, distance, direction, distance);

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
            targets = targets.FindAll(t => Vector3.Angle(direction, t.position - center.position) <= (angle / 2));

            return targets;
        }

        /// <summary>
        /// 查找扇形（或圆形）范围内的目标
        /// </summary>
        /// <param name="center">中心物体</param>
        /// <param name="distance">距离</param>
        /// <param name="direction">方向</param>
        /// <param name="angle">角度</param>
        /// <param name="layerMask">层级</param>
        /// <param name="targetTags">标签</param>
        /// <returns></returns>
        public static List<Transform> SelectTargets(this Transform center, float distance, Vector3 direction, float angle, LayerMask layerMask, string[] targetTags = null)
        {
            List<Transform> targets = new List<Transform>();

            //使用球形射线
            RaycastHit[] raycastHits = Physics.SphereCastAll(center.position, distance, direction, distance, layerMask);

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
            targets = targets.FindAll(t => Vector3.Angle(direction, t.position - center.position) <= (angle / 2));

            return targets;
        }

        /// <summary>
        /// 查找扇形（或圆形）范围内的目标
        /// </summary>
        /// <param name="center">中心物体</param>
        /// <param name="distance">距离</param>
        /// <param name="direction">方向</param>
        /// <param name="angle">角度</param>
        /// <param name="targetTags">标签</param>
        /// <returns></returns>
        public static List<Transform> SelectTargets2D(this Transform center, float distance, Vector2 direction, float angle, string[] targetTags = null)
        {
            List<Transform> targets = new List<Transform>();

            //使用圆形射线
            RaycastHit2D[] raycastHits = Physics2D.CircleCastAll(center.position, distance, direction, distance);

            //获取范围内有特定标签的目标
            if (targetTags != null)
            {
                foreach (string tag in targetTags)
                {
                    foreach (var raycast in raycastHits)
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
                foreach (var raycast in raycastHits)
                {
                    //不能选择自己
                    if (raycast.transform.name != center.name)
                    {
                        targets.Add(raycast.transform);
                    }
                }
            }

            //扇形选取
            targets = targets.FindAll(t => Vector3.Angle(direction, t.position - center.position) <= (angle / 2));

            return targets;
        }

        /// <summary>
        /// 查找扇形（或圆形）范围内的目标
        /// </summary>
        /// <param name="center">中心物体</param>
        /// <param name="distance">距离</param>
        /// <param name="direction">方向</param>
        /// <param name="angle">角度</param>
        /// <param name="layerMask">层级</param>
        /// <param name="targetTags">标签</param>
        /// <returns></returns>
        public static List<Transform> SelectTargets2D(this Transform center, float distance, Vector2 direction, float angle, LayerMask layerMask, string[] targetTags = null)
        {
            List<Transform> targets = new List<Transform>();

            //使用圆形射线
            RaycastHit2D[] raycastHits = Physics2D.CircleCastAll(center.position, distance, direction, distance, layerMask);

            //获取范围内有特定标签的目标
            if (targetTags != null)
            {
                foreach (string tag in targetTags)
                {
                    foreach (var raycast in raycastHits)
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
                foreach (var raycast in raycastHits)
                {
                    //不能选择自己
                    if (raycast.transform.name != center.name)
                    {
                        targets.Add(raycast.transform);
                    }
                }
            }

            //扇形选取
            targets = targets.FindAll(t => Vector3.Angle(direction, t.position - center.position) <= (angle / 2));

            return targets;
        }

        /// <summary>
        /// 朝向目标，仅沿Y轴旋转
        /// </summary>
        /// <param name="center">原物体</param>
        /// <param name="target">目标物体</param>
        public static void LookAtTarget(this Transform center, Transform target)
        {
            if (target != null)
            {
                Vector3 temp = new Vector3(target.position.x, center.position.y, target.position.z);
                center.LookAt(temp);
            }
        }

        /// <summary>
        /// 朝向目标，仅沿Y轴旋转
        /// </summary>
        /// <param name="center">原物体</param>
        /// <param name="target">目标物体</param>
        public static void LookAtTarget(this Transform center, Vector3 target)
        {
            if (target != null)
            {
                Vector3 temp = new Vector3(target.x, center.position.y, target.z);
                center.LookAt(temp);
            }
        }

        /// <summary>
        /// 朝向目标，仅沿Y轴旋转
        /// </summary>
        /// <param name="center">原物体</param>
        /// <param name="target">目标物体</param>
        public static void LookAtTarget2D(this Transform center, Transform target)
        {
            if (target != null)
            {
                float y = center.position.x - target.position.x >= 0 ? 0 : 180;
                center.rotation = Quaternion.Euler(0, y, 0);
            }
        }

        /// <summary>
        /// 朝向目标，仅沿Y轴旋转
        /// </summary>
        /// <param name="center">原物体</param>
        /// <param name="target">目标物体</param>
        public static void LookAtTarget2D(this Transform center, Vector2 target)
        {
            if (target != null)
            {
                float y = center.position.x - target.x >= 0 ? 0 : 180;
                center.rotation = Quaternion.Euler(0, y, 0);
            }
        }

    }
}
