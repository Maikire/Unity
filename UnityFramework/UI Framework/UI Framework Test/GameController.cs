using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// GameController
    /// </summary>
    public abstract class GameController : MonoSingleton<GameController>
    {
        public virtual void Start()
        {
            BeforeGameStart();
        }

        /// <summary>
        /// 游戏开始前
        /// </summary>
        protected abstract void BeforeGameStart();

        /// <summary>
        /// 游戏开始
        /// </summary>
        public abstract void GameStart();


    }
}
