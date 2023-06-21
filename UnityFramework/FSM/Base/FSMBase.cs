using ARPGDemo.Character;
using ARPGDemo.Skill;
using Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AI.FSM
{
    [RequireComponent(typeof(NavMeshAgent), typeof(CharacterStatus), typeof(CharacterSkillSystemNPC))]
    /// <summary>
    /// 状态机
    /// </summary>
    public class FSMBase : MonoBehaviour
    {
        [Tooltip("默认状态")]
        public FSMStateID DefaultStateID = FSMStateID.Idle;
        [Tooltip("有限状态机数据")]
        public FSMData Data;
        /// <summary>
        /// 状态表
        /// </summary>
        private List<FSMState> States;
        /// <summary>
        /// 当前状态
        /// </summary>
        private FSMState CurrentState;
        /// <summary>
        /// 导航
        /// </summary>
        private NavMeshAgent Navigation;

        #region 为状态和条件提供的成员
        [HideInInspector]
        [Tooltip("角色的信息")]
        public CharacterStatus Character;

        [HideInInspector]
        [Tooltip("角色的动画")]
        public Animator Anim;

        [HideInInspector]
        [Tooltip("发现的目标")]
        public Transform FoundTarget;

        [HideInInspector]
        [Tooltip("技能系统")]
        public CharacterSkillSystemNPC NPCSkillSystem;

        [HideInInspector]
        [Tooltip("当前的技能")]
        public SkillData CurrentSkill;
        #endregion

        private void Awake()
        {
            Character = this.GetComponent<CharacterStatus>();
            Anim = this.GetComponentInChildren<Animator>();
            Navigation = GetComponent<NavMeshAgent>();
            NPCSkillSystem = this.GetComponent<CharacterSkillSystemNPC>();
        }

        private void Start()
        {
            SetParameters();
            ConfigFSM();
            InitDefaultState();
        }

        private void Update()
        {
            SearchTarget(); //搜索目标
            CurrentState.JudgeState(this); //判断当前状态的条件
            CurrentState.ActionState(this); //执行当前状态的逻辑
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        private void SetParameters()
        {
            FoundTarget = null;
            CurrentSkill = null;
        }

        /// <summary>
        /// 配置状态机
        /// </summary>
        private void ConfigFSM()
        {
            States = new List<FSMState>();

            var map = FSMConfigReaderFactory.GetConfig(Data.FSMConfigPath);
            foreach (var state in map)
            {
                FSMState tempState = FSMCreateFactory.CreateState((FSMStateID)Enum.Parse(typeof(FSMStateID), state.Key));
                States.Add(tempState);
                foreach (var item in state.Value)
                {
                    FSMTriggerID tempTriggerID = (FSMTriggerID)Enum.Parse(typeof(FSMTriggerID), item.Key);
                    FSMStateID tempStateID = (FSMStateID)Enum.Parse(typeof(FSMStateID), item.Value);
                    tempState.Add(tempTriggerID, tempStateID);
                }
            }
        }

        /// <summary>
        /// 初始化默认状态
        /// </summary>
        private void InitDefaultState()
        {
            CurrentState = States.Find(s => s.StateID == DefaultStateID);
            CurrentState.EnterState(this);
        }

        /// <summary>
        /// 改变状态
        /// </summary>
        /// <param name="stateID"></param>
        public void ChangeActiveState(FSMStateID stateID)
        {
            CurrentState.ExitState(this);

            if (stateID == FSMStateID.Default)
            {
                CurrentState = States.Find(s => s.StateID == DefaultStateID);
            }
            else
            {
                CurrentState = States.Find(s => s.StateID == stateID);
            }

            CurrentState.EnterState(this);
        }

        /// <summary>
        /// 搜索目标
        /// </summary>
        public void SearchTarget()
        {
            //查找目标
            List<Transform> targets = this.transform.SelectTargets(Data.ViewDistance, Data.ViewAngle, Data.TargetTags);

            //选取活动的目标
            targets = targets.FindAll(t => t.GetComponent<CharacterStatus>().HP > 0);

            if (targets.Count == 0)
            {
                FoundTarget = null;
                return;
            }

            //是否忽略墙体
            if (CurrentState.IgnoreWalls)
            {
                FoundTarget = targets[0];
                return;
            }

            //利用射线判断是否有墙体
            RaycastHit hit;
            Physics.Raycast(this.transform.position, targets[0].position + Vector3.up * 0.5f - this.transform.position, out hit, Data.ViewDistance);
            if (hit.transform != null && hit.transform.name == targets[0].name)
            {
                FoundTarget = targets[0];
            }
            else
            {
                FoundTarget = null;
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
            Navigation.stoppingDistance = stoppingDistance;
            Navigation.speed = speed;
            Navigation.SetDestination(targetPosition);
        }


    }
}
