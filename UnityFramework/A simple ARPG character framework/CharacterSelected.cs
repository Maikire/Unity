using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.Character
{
    /// <summary>
    /// 角色选择器
    /// 添加给有选择功能的角色
    /// </summary>
    public class CharacterSelected : MonoBehaviour
    {
        //1.选中目标（启用目标的一个子物体用来表示选中），间隔指定时间后取消选中（禁用这个子物体）
        //2.选中A目标，在自动取消选中前，如果选中B目标，则立即取消选中A目标并选中B目标
        //3.选中A目标，在自动取消选中前，如果继续选中A目标，

        [Tooltip("选中特效的名字")]
        public string SelectedName = "SelectTarget";
        [Tooltip("选中时间")]
        public float SelectedTime = 3;
        private List<Transform> SelectedTarget; //上一群被选中的目标的选中特效

        private void Start()
        {
            SelectedTarget = new List<Transform>();
        }

        /// <summary>
        /// 选中目标，开启选中特效
        /// </summary>
        /// <param name="targets">目标</param>
        /// <param name="time">选中时间</param>
        public void SelectTargets(Transform[] targets)
        {
            if (targets == null)
            {
                return;
            }

            //禁用上一群被选中的目标的选中特效
            foreach (var item in SelectedTarget)
            {
                item.gameObject.SetActive(false);
            }

            //刷新目标
            Transform[] Selected = new Transform[targets.Length];
            for (int i = 0; i < targets.Length; i++)
            {
                Selected[i] = TransformHelper.FindChildByName(targets[i], SelectedName);
                Selected[i].gameObject.SetActive(true);
            }

            //刷新协程
            this.StopAllCoroutines();
            this.StartCoroutine(CancelSelection(Selected, SelectedTime));

            //刷新选中特效列表
            SelectedTarget.Clear();
            SelectedTarget.AddRange(Selected);
        }

        /// <summary>
        /// 指定时间后自动取消选中
        /// </summary>
        /// <param name="target"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private IEnumerator CancelSelection(Transform[] Selected, float time)
        {
            yield return new WaitForSeconds(time);
            foreach (Transform selected in Selected)
            {
                selected.gameObject.SetActive(false);
            }
        }

        #region 使用Update实现（当前脚本自动禁用）
        //private float HidTime;

        //public void SetSelectedActive(bool status, float time)
        //{
        //    //....SetActive(status); //设置物体激活
        //    this.enabled = status; //设置当前脚本的激活状态
        //    if (status)
        //    {
        //        HidTime = Time.time + time;
        //    }
        //}

        //private void Update()
        //{
        //    if (HidTime <= Time.time)
        //    {
        //        SetSelectedActive(false, 0);
        //    }
        //}
        #endregion


    }
}
