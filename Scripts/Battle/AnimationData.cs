using System.Collections.Generic;
using UnityEngine;

namespace develop_common
{
    [CreateAssetMenu(fileName = "AnimationData", menuName = "FlexibleAnimator/AnimationData", order = 1)]
    public class AnimationData : ScriptableObject
    {
        public List<AnimationStep> AnimationSteps = new List<AnimationStep>();
        public Vector3 InitialPosition = Vector3.zero; // 初期位置
        public Vector3 InitialRotation = Vector3.zero; // 初期回転
        public float ReturnDuration = 0.5f; // 初期位置に戻る時間
        public float ReturnTransitionDuration = 0.2f; // 初期位置に戻る際の遷移時間
        public bool IsLooping = false; // ループ再生のフラグ
    }
}
