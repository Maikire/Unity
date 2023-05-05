using System;
using System.Collections;
using System.Collections.Generic;

namespace Common
{
    /// <summary>
    /// 提供一些数组常用的功能
    /// </summary>
    public static class ArrayHelper
    {
        //查找单个元素、查找所有满足条件的元素、排序（升序、降序），最大值、最小值、筛选

        /// <summary>
        /// 查找满足条件的单个元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="condition">比较方法（委托）</param>
        /// <returns>返回目标对象</returns>
        public static T Find<T>(this T[] array, Func<T, bool> condition)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (condition(array[i]))
                {
                    return array[i];
                }
            }

            return default(T);
        }

        /// <summary>
        /// 查找满足条件的所有元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="condition">比较方法（委托）</param>
        /// <returns>返回目标对象</returns>
        public static T[] FindAll<T>(this T[] array, Func<T, bool> condition)
        {
            List<T> list = new List<T>();

            for (int i = 0; i < array.Length; i++)
            {
                if (condition(array[i]))
                {
                    list.Add(array[i]);
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// 求最大值
        /// </summary>
        /// <typeparam name="T">数组类型</typeparam>
        /// <typeparam name="Q">比较依据的数据类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="condition">比较依据方法</param>
        /// <returns></returns>
        public static T GetMax<T, Q>(this T[] array, Func<T, Q> condition) where Q : IComparable<Q>
        {
            if (array == null || array.Length == 0) return default(T);

            T max = array[0];

            for (int i = 0; i < array.Length; i++)
            {
                if (condition(array[i]).CompareTo(condition(max)) > 0)
                {
                    max = array[i];
                }
            }

            return max;
        }

        /// <summary>
        /// 求最小值
        /// </summary>
        /// <typeparam name="T">数组类型</typeparam>
        /// <typeparam name="Q">比较依据的数据类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="condition">比较依据方法</param>
        /// <returns></returns>
        public static T GetMin<T, Q>(this T[] array, Func<T, Q> condition) where Q : IComparable<Q>
        {
            if (array == null || array.Length == 0) return default(T);

            T min = array[0];

            for (int i = 0; i < array.Length; i++)
            {
                if (condition(array[i]).CompareTo(condition(min)) < 0)
                {
                    min = array[i];
                }
            }

            return min;
        }

        /// <summary>
        /// 升序
        /// 选择排序
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <typeparam name="Q">比较依据</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="condition">排序依据方法</param>
        public static void AscendingOrder<T, Q>(this T[] array, Func<T, Q> condition) where Q : IComparable
        {
            int minIndex;
            T min;

            for (int i = 0; i < array.Length - 1; i++)
            {
                minIndex = i;
                min = array[i];
                for (int j = i + 1; j < array.Length; j++)
                {
                    if (condition(array[j]).CompareTo(min) < 0)
                    {
                        min = array[j];
                        minIndex = j;
                    }
                }

                if (minIndex != i)
                {
                    array[minIndex] = array[i];
                    array[i] = min;
                }
            }
        }

        /// <summary>
        /// 降序
        /// 选择排序
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <typeparam name="Q">比较依据</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="condition">排序依据方法</param>
        public static void DescendingOrder<T, Q>(this T[] array, Func<T, Q> condition) where Q : IComparable
        {
            int maxIndex;
            T max;

            for (int i = 0; i < array.Length - 1; i++)
            {
                maxIndex = i;
                max = array[i];
                for (int j = i + 1; j < array.Length; j++)
                {
                    if (condition(array[j]).CompareTo(max) > 0)
                    {
                        max = array[j];
                        maxIndex = j;
                    }
                }

                if (maxIndex != i)
                {
                    array[maxIndex] = array[i];
                    array[i] = max;
                }
            }
        }

        /// <summary>
        /// 筛选
        /// 在每个T中选出Q 返回Q[]  ---例如在所有敌人物体中获得所有敌人的动画脚本
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <typeparam name="Q">需要获取的类型</typeparam>
        /// <param name="array">原数组</param>
        /// <param name="condition">返回需要获取的类型</param>
        /// <returns>获取到类型的数组</returns>
        public static Q[] Select<T, Q>(this T[] array, Func<T, Q> condition)
        {
            Q[] res = new Q[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                res[i] = condition(array[i]);
            }

            return res;
        }

    }
}
