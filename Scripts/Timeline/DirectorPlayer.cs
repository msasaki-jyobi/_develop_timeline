using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace develop_timeline
{

    [AddComponentMenu("DebugPlayable�F�Đ�/�t�Đ����\�ɂ���")]
    public class DirectorPlayer : SingletonMonoBehaviour<DirectorPlayer>
    {
        //[Header("�t�Đ��@�\�@ON/OFF")]
        //private bool IsReversing;
        [Header("���x")]
        public float Speed = 1.0f;
        [Header("�ω�����")]
        public float ChangeTime = 0.05f;

        [Header("�����A�^�b�`")]
        public PlayableDirector Director;

        void Update()
        {
            if (Director == null) return;
            if (Input.GetKeyDown(KeyCode.F))
            {
                Director.time = 0;
                Director.Play();
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                ChangeSpeed(-Speed, ChangeTime);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                ChangeSpeed(Speed, ChangeTime);
            }
            //if (Input.GetKeyDown(KeyCode.E))
            //{
            //    IsReversing = !IsReversing;
            //}

            if (Director != null)
                if (Director.time < 0.01f)
                {
                    if (Director.state == PlayState.Playing)
                    {
                        Director.time = 0.01f;
                        Director.playableGraph.GetRootPlayable(0).SetSpeed(0);
                    }
                }
        }

        public void ChangeSpeed(float newSpeed, float duration)
        {
            float currentSpeed = (float)Director.playableGraph.GetRootPlayable(0).GetSpeed();
            DOTween.To(() => currentSpeed, x =>
            {
                Director.playableGraph.GetRootPlayable(0).SetSpeed(x);
                currentSpeed = x;
            }, newSpeed, duration);
        }
    }
}
