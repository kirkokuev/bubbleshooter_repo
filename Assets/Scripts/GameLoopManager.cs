using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    public class GameLoopManager : MonoBehaviour
    {
        public static GameLoopManager instance;
        public bool isWinning = false;

        public GameStates currentGameState = GameStates.NotReady;
        public enum GameStates
        {
            NotReady,
            Initializing,
            Ready,
            Shooting,
            Finish
        }

        private void Start()
        {
            
        }

        private void InitGame()
        {
            currentGameState = GameStates.Initializing;
            if (FieldManager.instance != null)
            {
                FieldManager.instance.CreateField(11, 6, 1f);                
            }
        }

        private bool IsGameInitialized()
        {
            bool result = false;

            if (FieldManager.instance.currentState == FieldManager.FieldState.Ready)
            {
                if (BallsManager.instance.currentBallsState == BallsManager.BallsState.Ready)
                {
                    result = true;
                }
            }
            return result;
        }

        private void CheckWinningCondition()
        {

            float ballPercentage = DataObject.instance.loadedBalls.Count/100f*30f;
            if (DataObject.instance.spawnedBalls.Count < ballPercentage)
            {
                currentGameState = GameStates.Finish;
                isWinning = true;
                ScoreManager.instance.SaveScore();
            }
        }

        private void CheckLosingCondition()
        {
            if (DataObject.instance.remainingBalls == 0)
            {
                currentGameState = GameStates.Finish;
                isWinning = false;
            }
        }

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            if (currentGameState == GameStates.NotReady)
            {
                InitGame();
            }
            if (currentGameState == GameStates.Initializing)
            {
                if (IsGameInitialized()) currentGameState = GameStates.Ready;
            }
            if (currentGameState == GameStates.Ready)
            {

                BallsManager.instance.currentBallsState = BallsManager.BallsState.NewBall;
                currentGameState = GameStates.Shooting;
            }

            if (currentGameState == GameStates.Shooting)
            {
                CheckLosingCondition();
                CheckWinningCondition();
                if (BallsManager.instance.currentBallsState == BallsManager.BallsState.Ready)
                {
                    currentGameState = GameStates.Ready;
                }
            }

        }


    }

}

