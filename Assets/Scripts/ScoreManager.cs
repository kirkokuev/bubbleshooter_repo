using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager instance;

        private void Awake()
        {
            instance = this;
        }

        public void LoadScore()
        {
            DataObject.instance.highScore = PlayerPrefs.GetInt("Highscore");
        }

        public void SaveScore()
        {
            if (DataObject.instance.currentScore > DataObject.instance.highScore)
            {
                PlayerPrefs.SetInt("Highscore", DataObject.instance.currentScore);
            }
        }
    }

}
