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
        Transform[] legTargets = new Transform[6];
        Transform[] legFutureBases = new Transform[6];
        MyTentacleController[] _legs = new MyTentacleController[6];

        //variables
        private bool _startWalk = false;
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

                legFutureBases[i] = LegFutureBases[i];
                legTargets[i] = LegTargets[i];
            }
            _distances = new float[_legs[0].Bones.Length - 1];
            _copy = new Vector3[_legs[0].Bones.Length];
        }

        public void InitTail(Transform TailBase)
        {
            _tail = new MyTentacleController();
            _tail.LoadTentacleJoints(TailBase, TentacleMode.TAIL);

            tailEndEffector = _tail.Bones[_tail.Bones.Length - 1];
        }

        public void NotifyTailTarget(Transform target)
        {
            tailTarget = target;
        }

        public void NotifyStartWalk()
        {
            _startWalk = true;
            animationRange = 5;
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
                if (_animTime < animationRange)
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
          
            float targetRootDist = Vector3.Distance(_copy[0], legTargets[ID].position);
           
            if (targetRootDist < _distances.Sum())
            {
                while (Vector3.Distance(_copy[_copy.Length - 1], legTargets[ID].position) != 0 || Vector3.Distance(_copy[0], _legs[ID].Bones[0].position) != 0)
                {
                    //Forward
                    _copy[_copy.Length - 1] = legTargets[ID].position;
                    for (int i = _legs[ID].Bones.Length - 2; i >= 0; i--)
                    {
                        Vector3 vecDir = GetDirNormalized(_copy[i + 1], _copy[i]);
                        Vector3 moveVec = vecDir * _distances[i];
                        _copy[i] = _copy[i + 1] - moveVec;
                    }

                    _copy[0] = _legs[ID].Bones[0].position;
                    
                    //Backward
                    for (int i = 1; i < _legs[ID].Bones.Length - 1; i++)
                    {
                        Vector3 vecDir = GetDirNormalized(_copy[i - 1], _copy[i]);
                        Vector3 moveVec = vecDir * _distances[i - 1];
                        _copy[i] = _copy[i - 1] - moveVec;
                    }
                }

                //Update rotations
                for (int i = 0; i <= _legs[ID].Bones.Length - 2; i++)
                {
                    Vector3 direction = GetDirNormalized(_copy[i + 1], _copy[i]);
                    Vector3 lastDir = GetDirNormalized(_legs[ID].Bones[i + 1].position, _legs[ID].Bones[i].position);
                    Quaternion rot = Quaternion.FromToRotation(lastDir, direction);
                    _legs[ID].Bones[i].rotation = rot * _legs[ID].Bones[i].rotation;
                }
            }
        }
        internal Vector3 GetDirNormalized(Vector3 vec1, Vector3 vec2)
        {
            return (vec1 - vec2).normalized;
        }
        #endregion
    }
}
