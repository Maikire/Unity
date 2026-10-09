using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// 行为树配置
    /// </summary>
    [CreateAssetMenu(menuName = "Behavior Tree Asset")]
    public class BTAsset : ScriptableObject
    {
        [Tooltip("行为树的节点配置")]
        [TextArea(10, 80)]
        public string config;

    }
}
