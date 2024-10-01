using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    public class DataObject : MonoBehaviour
    {
        public static DataObject instance = null;

        public List<Ball> loadedBalls;
        public List<Ball> spawnedBalls;
        public List<Square> squares;

        public Ball nextProjectileBall;

        public float currentFieldScale;
        public List<Color> ballColors;

        public int[] fieldDimensions = new int[2];

        public int currentScore;
        public int remainingBalls = 10;
        public int highScore;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else 
            {
                Destroy(gameObject);
            }

        }

        public void ResetData()
        {
            remainingBalls = 10;
            currentScore = 0;
        }
    }

}

