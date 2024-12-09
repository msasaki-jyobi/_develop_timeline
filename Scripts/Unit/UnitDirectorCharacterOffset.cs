using develop_body;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace develop_timeline
{
    public class UnitDirectorCharacterOffset : MonoBehaviour
    {
        public bool IsStartDefaultSet;
        public Vector3 DefaultPos = new Vector3(0, -0.65f, 0);

        public List<PlayableOffset> OffsetParameters = new List<PlayableOffset>();
        
        private void Start()
        {
            if (IsStartDefaultSet)
                transform.localPosition = DefaultPos;
        }


        public void OnSetLocalPos(string playableObjectName, bool unitA)
        {
            foreach (var offset in OffsetParameters) 
            { 
                if(offset.PlayableObjectName == playableObjectName) 
                {
                    if(unitA == offset.IsUnitA)
                    {
                        transform.localPosition = offset.LocalPos;
                        transform.localRotation = Quaternion.Euler(offset.LocalRot);
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class PlayableOffset
    {
        public string PlayableObjectName;
        [Header("True：UnitAの時　False:UnitBの時(UnitBは基本 敵サイド）")]
        public bool IsUnitA; // trueで一致している？
        public Vector3 LocalPos;
        public Vector3 LocalRot;
    }
}