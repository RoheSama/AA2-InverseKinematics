using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace OctopusController
{
  
    public class MyScorpionController
    {
        //TAIL
        Transform tailTarget;
        Transform tailEndEffector;
        MyTentacleController _tail;
        float animationRange;

        //LEGS
        Transform[] legTargets;
        Transform[] legFutureBases;
        MyTentacleController[] _legs = new MyTentacleController[6];

        //variables
        private bool _startWalk = false;
        private float _animRange;
        private float _animTime = 0;

        private Vector3[] _copy;
        private float[] _distances;

        #region public
        public void InitLegs(Transform[] LegRoots,Transform[] LegFutureBases, Transform[] LegTargets)
        {
            _legs = new MyTentacleController[LegRoots.Length];
            
            //Init
            for(int i = 0; i < LegRoots.Length; i++)
            {
                _legs[i] = new MyTentacleController();
                _legs[i].LoadTentacleJoints(LegRoots[i], TentacleMode.LEG);
            }
        }

        public void InitTail(Transform TailBase)
        {
            _tail = new MyTentacleController();
            _tail.LoadTentacleJoints(TailBase, TentacleMode.TAIL);
        }

        public void NotifyTailTarget(Transform target)
        {
            tailTarget = target;
        }

        public void NotifyStartWalk()
        {
            _startWalk = true;
            _animRange = 5;
            _animTime = 0;
        }

        public void UpdateIK()
        {
            if (Vector3.Distance(tailEndEffector.transform.position, tailTarget.transform.position) > 0.05f)
            {
                updateTail();
            }

            if (_startWalk)
            {
                _animTime += Time.deltaTime;
                if (_animTime < _animRange)
                {
                    updateLegPos();
                }
                else
                {
                    _startWalk = false;
                }
            }
        }
        #endregion


        #region private
        internal float delta = 0.01f;

        private void updateLegPos()
        {
            //check for the distance to move leg
            foreach (var leg in _legs)
            {
                if (Vector3.Distance(leg.Bones[0].position, legFutureBases[Array.IndexOf(_legs, leg)].position) > 1f)
                {
                    leg.Bones[0].position = Vector3.Lerp(leg.Bones[0].position, legFutureBases[Array.IndexOf(_legs, leg)].position, 1.4f);
                }
                updateLegs(Array.IndexOf(_legs, leg));
            }
        }
       
        private void updateTail()
        {
            Vector3.Lerp(tailEndEffector.transform.position, tailTarget.transform.position, 1f);
            for (int i = 0; i < _tail.Bones.Length - 2; i++)
            {
                float distEndEffectorToTarget = Vector3.Distance(tailEndEffector.transform.position, tailTarget.transform.position);
                _tail.Bones[i].transform.Rotate(Vector3.forward * delta);
                float tempDistEndToTarget = Vector3.Distance(tailEndEffector.transform.position, tailTarget.transform.position);
                _tail.Bones[i].transform.Rotate(Vector3.forward * -delta);
                float _slope = (tempDistEndToTarget - distEndEffectorToTarget) / delta;

                _tail.Bones[i].transform.Rotate((Vector3.forward * -_slope) * 120f);
            }
        }
       
        private void updateLegs(int ID)
        {
            //Make copy
            for (int i = 0; i <= _legs[0].Bones.Length - 1; i++)
            {
                _copy[i] = _legs[ID].Bones[i].position;
            }

            //Bones distances
            for (int i = 0; i <= _legs[ID].Bones.Length - 2; i++)
            {
                _distances[i] = Vector3.Distance(_legs[ID].Bones[i].position, _legs[ID].Bones[i + 1].position);
            }
        }
    
        #endregion
    }
}
