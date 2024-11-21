using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace develop_battle
{
    public class BattleScene : MonoBehaviour
    {

        [Header("条件")]
        public bool IsCheckPlayerWin;
        public bool PlayerWin; // WinPlayerCommand
        public bool IsCheckDamagerWin;
        public bool DamagerWin;
        public bool IsCheckOldgameState;
        public EGameState OldGameState; // 変更される前のGameState
        public bool IsCheckSelectGameState;
        public EGameState SelectGameState; // 変更される予定のGameState
        public bool IsCheckCommandNum;
        public int CommandNum = 0; // CommandNum

        [Header("切り替えるステート")]
        public bool IsSetGameState;
        public EGameState SetGameState = EGameState.Battle;

        [Header("ディレイ")]
        public bool IsDelay;
        public float DelayTime;
        public float SetTimeScale = 0;

        [Header("ループを行うタイム")]
        public float LoopTime = 3f;
        [Header("BattleSceneData；どの秒数で何を生成？初回なら？どこにダメージ発生させる？")]
        public List<BattleActionData> EnemyDatas = new List<BattleActionData>();

        [Header("モーション")]
        public bool IsDamagerChangeState;
        public string ChangeDamagerStateName;
        public bool IsDamagerApplyChange;
        [Space(10)]
        public bool IsAttakerChangeState;
        public GameObject AttakerAction;

        [Header("座標")]
        public bool IsDamagerPos;
        public bool IsDamagerRot;
        public GameObject TargetDamagerTransform;
        [Space(10)]
        public bool IsAttackerPos;
        public bool IsAttackerRot;
        public GameObject TargetAttakerTransform;

        [Header("切り替えるカメラ")]
        public bool LookDamager;
        public float BrandTime = -1;
        public CinemachineVirtualCamera ChangeVcam;
        public List<CinemachineVirtualCamera> ChangeRandomVcams = new List<CinemachineVirtualCamera>();
    }
}