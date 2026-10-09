using CharacterSystem;
using Common;
using SkillSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviorTree
{
    /// <summary>
    /// NPC 行为树驱动，负责组件绑定、感知及每帧更新
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCBehaviorTree : MonoBehaviour
    {
        [Tooltip("NPC 行为树数据")]
        public Blackboard blackboard = new();

        /// <summary>
        /// 由此组件驱动的框架执行器
        /// </summary>
        public BehaviorTree Tree { get; } = new();

        private void Awake()
        {
            blackboard.driver = this;
            blackboard.owner = transform;
            blackboard.character = GetComponent<CharacterStatus>();
            blackboard.npcSkill = GetComponent<CharacterSkillSystemNPC>();
            blackboard.anim = GetComponentInChildren<Animator>();
            blackboard.navigation = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            blackboard.foundTargets = null;
            blackboard.currentSkill = null;
            Tree.Initialize(blackboard);
        }

        private void Update()
        {
            SearchTarget();
            Tree.Tick();
        }

        /// <summary>
        /// 中断此 NPC 当前执行的行为
        /// </summary>
        public void AbortAll()
        {
            Tree.AbortAll();
        }

        /// <summary>
        /// 搜索存活目标，并根据当前业务节点决定是否检查遮挡
        /// </summary>
        public void SearchTarget()
        {
            bool ignoreWalls = blackboard.Current is NPCBehaviorNode node && node.IgnoreWalls;
            List<Transform> targets = transform.SelectTargets(
                blackboard.viewDistance, transform.forward, blackboard.viewAngle, blackboard.targetTags);
            targets = targets.FindAll(t => t.TryGetComponent<CharacterStatus>(out var status) && status.HP > 0);

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

            List<Transform> validTargets = new();
            foreach (var target in targets)
            {
                Physics.Raycast(transform.position, target.position + Vector3.up * 0.5f - transform.position,
                    out RaycastHit hit, blackboard.viewDistance);
                if (hit.transform != null && hit.transform.gameObject == target.gameObject)
                {
                    validTargets.Add(target);
                }
            }

            blackboard.foundTargets = validTargets.Count == 0 ? null : validTargets.ToArray();
        }

        /// <summary>
        /// 移动此 NPC 到目标位置
        /// </summary>
        public void MoveToTarget(Vector3 targetPosition, float stoppingDistance, float speed)
        {
            blackboard.navigation.stoppingDistance = stoppingDistance;
            blackboard.navigation.speed = speed;
            blackboard.navigation.SetDestination(targetPosition);
        }


    }
}
