using Cinemachine;
using develop_common;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using developClipData = develop_common.ClipData;

namespace develop_battle
{
    public class BattleActionData : MonoBehaviour
    {
        //[Header("どの秒数で何を生成？初回なら？どこにダメージ発生させる？")]

        [Header("発生時間")]
        public float PlayTime;

        [Space(10)]
        public bool IsFlexibleAnimator;
        public FlexibleAnimator FlexibleAnimator;
        public bool IsFlexibleSetTarget;
        public string FlexibleTargetBodyName;
        //[Space(10)]
        //[Header("生成プレハブ")]
        //public GameObject SummonPrefab;
        //[Header("どこに向かわせ、固定するか？")]
        //public string PrefabTargetBodyName = "";

        [Space(10)]
        [Header("ダメージ")]
        public int DamageAmount;

        [Space(10)]
        [Header("どこにダメージを発生させるか？")]
        public string DamageBodyName = "";
        [Header("エフェクト")]
        public GameObject DamageEffectPrefab;
        [Header("SE")]
        public developClipData DamageSE;
        [Header("Voice")]
        public string DamageVoice;
        [Header("セリフ")]
        public string DamageMessage;
        [Header("切り替えるカメラ")]
        public bool LookDamager;
        public float BrandTime = -1;
        public CinemachineVirtualCamera ChangeVcam;
        public List<CinemachineVirtualCamera> ChangeRandomVcams = new List<CinemachineVirtualCamera>();

        [Header("Animation Additive")]
        public bool IsAdditive;
        public string AdditiveStateName;


        [Space(10)]
        [Header("状態管理")]
        public bool IsInitPlay; // 初回実行済みか
        public bool IsPlay; // 実行済みか

        public BattleActionData(BattleActionData battleEnemyData)
        {
            PlayTime = battleEnemyData.PlayTime;
            IsFlexibleAnimator = battleEnemyData.IsFlexibleAnimator;
            FlexibleAnimator = battleEnemyData.FlexibleAnimator;
            IsFlexibleSetTarget = battleEnemyData.IsFlexibleSetTarget;
            FlexibleTargetBodyName = battleEnemyData.FlexibleTargetBodyName;
            //SummonPrefab = battleEnemyData.SummonPrefab;
            //PrefabTargetBodyName = battleEnemyData.PrefabTargetBodyName;
            DamageBodyName = battleEnemyData.DamageBodyName;
            DamageEffectPrefab = battleEnemyData.DamageEffectPrefab;
            DamageSE = battleEnemyData.DamageSE;
            DamageVoice = battleEnemyData.DamageVoice;
            DamageMessage = battleEnemyData.DamageMessage;
            IsInitPlay = battleEnemyData.IsInitPlay;
            IsPlay = battleEnemyData.IsPlay;
            LookDamager = battleEnemyData.LookDamager;
            BrandTime = battleEnemyData.BrandTime;
            ChangeVcam = battleEnemyData.ChangeVcam;
            ChangeRandomVcams = battleEnemyData.ChangeRandomVcams;
            IsAdditive = battleEnemyData.IsAdditive;
            AdditiveStateName = battleEnemyData.AdditiveStateName;
        }
    }
}