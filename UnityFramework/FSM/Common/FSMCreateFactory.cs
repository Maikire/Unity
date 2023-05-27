using System;
using System.Collections.Generic;

namespace AI.FSM
{
    /// <summary>
    /// FSM创建工厂
    /// 创建FSMTrigger和FSMState对象
    /// </summary>
    public class FSMCreateFactory
    {
        /// <summary>
        /// 储存已生成的FSMTrigger对象，循环利用
        /// </summary>
        private static Dictionary<string, FSMTrigger> Cache = new Dictionary<string, FSMTrigger>();

        /// <summary>
        /// 创建FSMTrigger对象
        /// </summary>
        /// <param name="triggerID">ID</param>
        /// <returns></returns>
        public static FSMTrigger CreateTrigger(FSMTriggerID triggerID)
        {
            //命名规则：AI.FSM. + triggerID + Trigger
            string className = String.Format("AI.FSM.{0}Trigger", triggerID);

            if (Cache.ContainsKey(className))
            {
                return Cache[className];
            }
            else
            {
                Type type = Type.GetType(className);
                FSMTrigger temp = Activator.CreateInstance(type) as FSMTrigger;
                Cache.Add(className, temp);
                return temp;
            }
        }

        /// <summary>
        /// 创建FSMState对象
        /// </summary>
        /// <param name="stateID">ID</param>
        /// <returns></returns>
        public static FSMState CreateState(FSMStateID stateID)
        {
            //命名规则：AI.FSM. + stateID + State
            Type type = Type.GetType(String.Format("AI.FSM.{0}State", stateID));
            object temp = Activator.CreateInstance(type);
            return temp as FSMState;
        }


    }
}

