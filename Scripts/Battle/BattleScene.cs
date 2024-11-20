using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace develop_battle
{
    public class BattleScene : MonoBehaviour
    {

        [Header("条件")]
        public bool PlayerWin; // WinPlayerCommand
        public bool IsCheckOldgameState;
        public EGameState OldGameState; // 変更される前のGameState
        public bool IsCheckSelectGameState;
        public EGameState SelectGameState; // 変更される予定のGameState
        public bool IsCheckCommandNum;
        public int CommandNum = 0; // CommandNum

        [Header("モーション")]
        public bool IsPlayerChangeState;
        public string ChangePlayerStateName;
        public bool IsPlayerApplyChange;
        [Space(10)]
        public bool IsEnemyChangeState;
        public string ChangeEnemyStateName;
        public bool IsEnemyApplyChange;

        [Header("座標")]
        public bool IsPlayerPos;
        public bool IsPlayerRot;
        public GameObject TargetPlayerTransform;
        [Space(10)]
        public bool IsEnemyPos;
        public bool IsEnemyRot;
        public GameObject TargetEnemyTransform;

        [Header("切り替えるカメラ")]
        public CinemachineVirtualCamera ChangeVcam;
        public List<CinemachineVirtualCamera> ChangeRandomVcams = new List<CinemachineVirtualCamera>();
    }
}