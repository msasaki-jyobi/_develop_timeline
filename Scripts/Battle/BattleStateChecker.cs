using develop_common;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace develop_battle
{
   
    class BattleStateChecker : MonoBehaviour
    {
        public EKeyType SelectKey { get; private set; }
        public int CommandNum { get; private set; } // 条件用：ランダムに選定、数値に応じた技
        public EGameState OldGameState { get; private set; }
        public ReactiveProperty<EGameState> GameState { get; private set; } = new ReactiveProperty<EGameState>();

    }
}
