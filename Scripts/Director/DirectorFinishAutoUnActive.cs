using System.Collections;
using UnityEngine;

namespace develop_timeline
{
    public class DirectorFinishAutoUnActive : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            DirectorManager.Instance.FinishEvent += () => { gameObject.SetActive(false); };
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDestroy()
        {
            if(DirectorManager.Instance != null)
                DirectorManager.Instance.FinishEvent -= () => { gameObject.SetActive(false); };
        }
    }
}