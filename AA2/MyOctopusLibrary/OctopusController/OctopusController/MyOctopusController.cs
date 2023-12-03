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

        }




        #endregion


        #region private and internal methods
        //todo: add here anything that you need

        void update_ccd() {
           

        }


        

        #endregion






    }
}
