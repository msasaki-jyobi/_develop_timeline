using System.Collections.Generic;
using UnityEngine;

namespace develop_common
{
    [CreateAssetMenu(fileName = "AnimationData", menuName = "FlexibleAnimator/AnimationData", order = 1)]
    public class AnimationData : ScriptableObject
    {
        public List<AnimationStep> AnimationSteps = new List<AnimationStep>();
        public Vector3 InitialPosition = Vector3.zero; // �����ʒu
        public Vector3 InitialRotation = Vector3.zero; // ������]
        public float ReturnDuration = 0.5f; // �����ʒu�ɖ߂鎞��
        public float ReturnTransitionDuration = 0.2f; // �����ʒu�ɖ߂�ۂ̑J�ڎ���
        public bool IsLooping = false; // ���[�v�Đ��̃t���O
    }
}
