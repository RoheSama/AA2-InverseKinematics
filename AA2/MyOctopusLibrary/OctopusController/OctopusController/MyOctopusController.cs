using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace OctopusController
{
    public enum TentacleMode { LEG, TAIL, TENTACLE };

    public class MyOctopusController 
    {
        MyTentacleController[] _tentacles =new  MyTentacleController[4];

        Transform _currentRegion;
        Transform _target;
        private int _tentacleNear;

        Transform[] _randomTargets;// = new Transform[4];

        float _twistMin, _twistMax;
        float _swingMin, _swingMax;

        private float _start, _end;
        private bool _isShooting;
        float[] _theta, _sin, _cos;

        #region public methods
        public float TwistMin { set => _twistMin = value; }
        public float TwistMax { set => _twistMax = value; }
        public float SwingMin {  set => _swingMin = value; }
        public float SwingMax { set => _swingMax = value; }

        public void TestLogging(string objectName)
        {  
            Debug.Log("hello, I am initializing my Octopus Controller in object "+objectName);   
        }

        public void Init(Transform[] tentacleRoots, Transform[] randomTargets)
        {
            _tentacles = new MyTentacleController[tentacleRoots.Length];

            // foreach (Transform t in tentacleRoots)
            for(int i = 0;  i  < tentacleRoots.Length; i++)
            {
                _tentacles[i] = new MyTentacleController();
                _tentacles[i].LoadTentacleJoints(tentacleRoots[i],TentacleMode.TENTACLE);
            }

            _randomTargets = randomTargets; 
        }

              
        public void NotifyTarget(Transform target, Transform region)
        {
            _currentRegion = region;
            _target = target;
        }

        public void NotifyShoot() {
            Debug.Log("Shoot");

            //variables
            _start = 0;
            _end = 3;
            _isShooting = true;

            //regions
            Dictionary<string, int> regionValues = new Dictionary<string, int> //map in C++
            {
                { "region1", 0 },
                { "region2", 1 },
                { "region3", 2 },
                { "region4", 3 }
            };

            string currentRegionName = _currentRegion.gameObject.transform.name;

            if (regionValues.ContainsKey(currentRegionName))
            {
                _tentacleNear = regionValues[currentRegionName];
            }
        }

        public void UpdateTentacles()
        {
            update_ccd();

            if (_isShooting)
            {
                _start += Time.deltaTime;
                if (_start > _end)
                {
                    _start = 0;
                    _isShooting = false;
                }
            }
        }
        #endregion

        #region private and internal methods

        private Vector3 _r2;

        void update_ccd()
        {
            foreach (var tentacle in _tentacles)
            {
                _theta = new float[tentacle.Bones.Length];
                _sin = new float[tentacle.Bones.Length];
                _cos = new float[tentacle.Bones.Length];

                for (int j = tentacle.Bones.Length - 2; j >= 0; j--)
                {
                    Vector3 r1 = tentacle.Bones[tentacle.Bones.Length - 1].transform.position - tentacle.Bones[j].transform.position;

                    //shooting
                    if (_isShooting && _tentacleNear == Array.IndexOf(_tentacles, tentacle))
                    {
                        _r2 = _target.transform.position - _tentacles[_tentacleNear].Bones[j].transform.position;
                    }
                    else
                    {
                        _r2 = _randomTargets[Array.IndexOf(_tentacles, tentacle)].transform.position - tentacle.Bones[j].transform.position;
                    }

                    //avoid division
                    if (r1.magnitude * _r2.magnitude <= 0.001f)
                    {
                        _cos[j] = 1;
                        _sin[j] = 0;
                    }
                    else
                    {
                        _cos[j] = Vector3.Dot(r1, _r2) / (r1.magnitude * _r2.magnitude);
                        _sin[j] = Vector3.Cross(r1, _r2).magnitude / (r1.magnitude * _r2.magnitude);
                    }

                    Vector3 axis = Vector3.Cross(r1, _r2).normalized;
                    _theta[j] = Mathf.Acos(Mathf.Clamp(_cos[j], -1, 1));

                    if (_sin[j] < 0.0f)
                    {
                        _theta[j] *= -1.0f;
                    }

                    //asign angle and limitzrot functions
                    _theta[j] = GetAngle(_theta[j]);
                    _theta[j] = GetLimitZRot(_theta[j]);

                    tentacle.Bones[j].transform.Rotate(axis, _theta[j], Space.World);
                    Quaternion twist = new Quaternion(0, tentacle.Bones[j].transform.localRotation.y, 0, tentacle.Bones[j].transform.localRotation.w);
                    twist = twist.normalized;
                    Quaternion swing = tentacle.Bones[j].transform.localRotation * Quaternion.Inverse(twist);
                    tentacle.Bones[j].transform.localRotation = swing.normalized;
                }
            }
        }
        internal float GetLimitZRot(float theta)
        {
            theta *= Mathf.Rad2Deg;
            if (theta > 15.0f)
            {
                theta = 15;
            }
            else if (theta < -15)
            {
                theta = -15;
            }
            return theta;
        }

        internal float GetAngle(float theta)
        {
            if (theta > Mathf.PI) { theta -= Mathf.PI * 2; }
            if (theta < -Mathf.PI) { theta += Mathf.PI * 2; }

            return theta;
        }
        #endregion
    }
}
