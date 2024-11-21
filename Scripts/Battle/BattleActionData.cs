using develop_common;
using System.Collections;
using UnityEngine;

namespace develop_battle
{
    public class BattleActionData : MonoBehaviour
    {
        //[Header("どの秒数で何を生成？初回なら？どこにダメージ発生させる？")]

        [Header("発生時間")]
        public float PlayTime;

        [Space(10)]
        [Header("生成プレハブ")]
        public GameObject SummonPrefab;
        [Header("どこに向かわせ、固定するか？")]
        public string PrefabTargetBodyName = "";

        [Space(10)]
        [Header("ダメージ")]
        public int DamageAmount;

        [Space(10)]
        [Header("どこにダメージを発生させるか？")]
        public string DamageBodyName = "";
        [Header("エフェクト")]
        public GameObject DamageEffectPrefab;
        [Header("SE")]
        public ClipData DamageSE;
        [Header("Voice")]
        public string DamageVoice;
        [Header("セリフ")]
        public string DamageMessage;

        [Space(10)]
        [Header("状態管理")]
        public bool IsInitPlay; // 初回実行済みか
        public bool IsPlay; // 実行済みか

        public BattleActionData(BattleActionData battleEnemyData)
        {
            PlayTime = battleEnemyData.PlayTime;
            SummonPrefab = battleEnemyData.SummonPrefab;
            PrefabTargetBodyName = battleEnemyData.PrefabTargetBodyName;
            DamageBodyName = battleEnemyData.DamageBodyName;
            DamageEffectPrefab = battleEnemyData.DamageEffectPrefab;
            DamageSE = battleEnemyData.DamageSE;
            DamageVoice = battleEnemyData.DamageVoice;
            DamageMessage = battleEnemyData.DamageMessage;
            IsInitPlay = battleEnemyData.IsInitPlay;
            IsPlay = battleEnemyData.IsPlay;
        }
    }
}