using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace develop_timeline
{
    public class SampleDirectorPlayer : MonoBehaviour
    {
        public DirectorPlayer PlayablePrefab;
        public Animator UnitA;
        public Animator UnitB;

        private void Update()
        {
            // Debug
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                // 実行中かチェック
                GameObject director = Instantiate(PlayablePrefab.gameObject, transform.position, Quaternion.identity);
                if (director.TryGetComponent<DirectorPlayer>(out var directorPlayer))
                    directorPlayer.OnSetPlayDirector(unitA: UnitA, unitB: UnitB);

                // 破棄どする？
            }

        }
    }

}
