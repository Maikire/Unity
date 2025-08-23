using Character;
using Common;
using SkillSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviorTree
{
    /// <summary>
    /// 行为树
    /// </summary>	
    [RequireComponent(typeof(NavMeshAgent), typeof(CharacterStatus), typeof(CharacterSkillSystemNPC))]
    public class BehaviorTree : MonoBehaviour
    {
        // 本文使用的是手动填写配置
        // 想要通过配置表自动填写数据，只需要额外写一个类实现读取和配置数据，然后在 ConfigBT() 中调用即可
        /// <summary>
        /// 行为树数据（黑板）
        /// </summary>
        public BTBlackboard blackboard;
        /// <summary>
        /// 导航
        /// </summary>
        private NavMeshAgent navigation;

        private void Awake()
        {
            blackboard.character = this.GetComponent<CharacterStatus>();
            blackboard.anim = this.GetComponentInChildren<Animator>();
            blackboard.npcSkill = this.GetComponent<CharacterSkillSystemNPC>();
            navigation = GetComponent<NavMeshAgent>();
            blackboard.interruptions = new List<Func<bool>>();
        }

        private void Start()
        {
            blackboard.nodeStack = new Stack<BTNode>();
            SetParameters();
            ConfigBT();
        }

        private void Update()
        {
            SearchTarget();
            Tick();
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        private void SetParameters()
        {
            blackboard.foundTargets = null;
            blackboard.currentSkill = null;
        }

        /// <summary>
        /// 配置行为树
        /// </summary>
        private void ConfigBT()
        {
            BTCreateFactory.BuildBehaviorTree(
                blackboard.nodeAsset.config,
                out blackboard.root,
                out blackboard.priorityNode
            );
        }

        /// <summary>
        /// 执行节点
        /// </summary>
        private void Tick()
        {
            // 外部中断
            foreach (var item in blackboard.interruptions)
            {
                if (item.Invoke())
                {
                    AbortAll();
                    return;
                }
            }

            BTState state;

            // 优先级节点
            state = blackboard.priorityNode.TickNode(this);
            if (state == BTState.Success || state == BTState.Running)
            {
                return;
            }

            // 根节点
            if (blackboard.root.State != BTState.Running)
            {
                blackboard.root.EnterNode(this);
            }

            state = blackboard.root.TickNode(this);

            if (state != BTState.Running)
            {
                blackboard.root.ExitNode(this);
            }
        }

        /// <summary>
        /// 中断所有节点
        /// </summary>
        public void AbortAll()
        {
            BTNode node = blackboard.Current;
            while (node != null)
            {
                node.Abort(this);
                node = blackboard.Current;
            }
        }

        /// <summary>
        /// 搜索目标
        /// </summary>
        public void SearchTarget()
        {
            bool ignoreWalls = false;
            BTNode node = blackboard.Current;

            if (node != null && node is BehaviorNode)
            {
                ignoreWalls = (node as BehaviorNode).IgnoreWalls;
            }

            List<Transform> targets = this.transform.SelectTargets(blackboard.viewDistance, this.transform.forward, blackboard.viewAngle, blackboard.targetTags);
            targets = targets.FindAll(t => t.GetComponent<CharacterStatus>().HP > 0);
            if (targets.Count == 0)
            {
                blackboard.foundTargets = null;
                return;
            }

            if (ignoreWalls)
            {
                blackboard.foundTargets = targets.ToArray();
                return;
            }

            RaycastHit hit;
            List<Transform> validTargets = new List<Transform>();
            foreach (var item in targets)
            {
                Physics.Raycast(this.transform.position, item.position + Vector3.up * 0.5f - this.transform.position, out hit, blackboard.viewDistance);
                if (hit.transform != null && hit.transform.gameObject == item.gameObject)
                {
                    validTargets.Add(item);
                }
            }

            if (validTargets.Count != 0)
            {
                blackboard.foundTargets = validTargets.ToArray();
            }
            else
            {
                blackboard.foundTargets = null;
            }
        }

        /// <summary>
        /// 自动寻路
        /// </summary>
        /// <param name="targetPosition">目标位置</param>
        /// <param name="stoppingDistance">停止移动的距离</param>
        /// <param name="speed">移动速度</param>
        public void MoveToTarget(Vector3 targetPosition, float stoppingDistance, float speed)
        {
            navigation.stoppingDistance = stoppingDistance;
            navigation.speed = speed;
            navigation.SetDestination(targetPosition);
        }

    }
}
