using develop_body;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace develop_timeline
{
    public class UnitDirectorOffset : MonoBehaviour
    {
        public List<OffsetParameter> OffsetParameters = new List<OffsetParameter>();

        private void Start()
        {
    
        }

        public Vector3 GetBodyOffset(EBodyType bodyType)
        {
            Vector3 offset = Vector3.zero;
            foreach (var offsetParameter in OffsetParameters)
            {
                if (bodyType == offsetParameter.BodyType)
                {
                    Debug.Log($"一致しました {bodyType}");
                    return offsetParameter.Offset;
                }
            }

            return offset;
        }
    }

}