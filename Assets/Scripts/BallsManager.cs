using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace BubbleShooter
{
    public class BallsManager : MonoBehaviour
    {
        public static BallsManager instance;
        public List<Ball> currentBallsList;
       
        public GameObject ballPrefab;
        public GameObject projectileBallPrefab;
        public Vector2 projectileSpawnPoint;
        public ProjectileBallController currentProjectileBall;

        private GameObject ballsRoot;
        public BallsState currentBallsState = BallsState.NotReady;
        public List<GameObject> spawnedBallsObjects;


        public List<int> ballsInRows;

        public enum BallsState
        {
            Ready,
            Initializing,
            NotReady,
            Test,
            NewBall
        }

        private void Awake()
        {
            instance = this;
            InitBallsManager();
        }

        void InitBallsManager()
        {
            currentBallsList = new List<Ball>();
            spawnedBallsObjects = new List<GameObject>();
        }

        public void SpawnProjectileTest(Ball newProjectileBall)
        {
            currentProjectileBall = SpawnProjectileBall(newProjectileBall).GetComponent<ProjectileBallController>();
        }

        public void DestroyAndSpawnBallAtSquare(string squareID, int color)
        {
            spawnedBallsObjects.Find(ballObj => ballObj.name == squareID).GetComponent<BallController>().BlowupAndDestroy();
            RemoveBall(squareID);

            SpawnBallAtSquare(squareID, color);
        }

        public void SpawnBallAtSquare(string squareID, int color)
        {

            Ball newBall = new Ball();
            newBall.id = squareID;
            newBall.color = color;
            SpawnBall(newBall);
            CheckAdjacentBalls(squareID, color);
            CheckHangingBalls();

        }

        private void CheckHangingBalls()
        {

            CheckEmptyRows();
        }

        private void CheckEmptyRows()
        {
            List<Ball> ballsToDrop = new List<Ball>();

            ballsInRows = new List<int>();

            for (int i = 0; i < DataObject.instance.fieldDimensions[0]; i++)
            {
                int counter = 0;
                foreach (Ball ball in currentBallsList)
                {
                    if (ball.yPos == i)
                    {
                        counter++;
                    }
                }
                ballsInRows.Add(counter);
            }

            for (int j = 0; j < ballsInRows.Count; j++)
            {
                if (j + 1 < ballsInRows.Count)
                {
                    if (ballsInRows[j] == 0 && ballsInRows[j + 1] != 0)
                    {
                        foreach (Ball ball in currentBallsList)
                        {
                            if (ball.yPos >= j+1)
                            {
                                ballsToDrop.Add(ball);
                            }
                        }
                    }

                }
            }

            foreach (Ball ball in ballsToDrop)
            {
                spawnedBallsObjects.Find(ballObj => ballObj.name == ball.id).GetComponent<BallController>().DropAndDestroy();
                RemoveBall(ball.id);
            }

        }

        public void RemoveBall(string ballID)
        {
            GameObject ballObject = spawnedBallsObjects.Find(ballobj => ballobj.name == ballID);
            Ball ball = DataObject.instance.spawnedBalls.Find(ball => ball.id == ballID);
            if (ballObject != null)
            {
                spawnedBallsObjects.Remove(ballObject);
                DataObject.instance.spawnedBalls.Remove(ball);
                currentBallsList.Remove(ball);
            }

            DataObject.instance.currentScore += 1;
        }

        private void CheckAdjacentBalls(string squareID, int color)
        {
            Square[] adjacentSquares = FieldManager.instance.GetSurroundingSquares(squareID);

            List<Ball> ballsToDestroy = new List<Ball>();

            // Check if squares have balls

            foreach (Square square in adjacentSquares)
            {

                foreach (Ball ball in currentBallsList)
                {

                    if (ball.id == square.id)
                    {
                        if (ball.color == color)
                        {
                            ballsToDestroy.Add(ball);
                        }
                    }
                }
            }

            //  Counting matching colored balls

            List<GameObject> tempSpawnedBallsObjects = spawnedBallsObjects;

            if (ballsToDestroy.Count >= 3)
            {
                foreach(Ball ballToDestroy in ballsToDestroy)
                {
                    spawnedBallsObjects.Find(ballObj => ballObj.name == ballToDestroy.id).GetComponent<BallController>().BlowupAndDestroy();
                    RemoveBall(ballToDestroy.id);

                }
            }

        }

        private void PrepareNewProjectile()
        {
            Ball newProjectileBall = new Ball();
            newProjectileBall.color = Random.Range(0, 3);
            DataObject.instance.nextProjectileBall = newProjectileBall;
        }


        private void Update()
        {
            if (currentBallsState != BallsState.Test)
            {

                if (FieldManager.instance.currentState != FieldManager.FieldState.Ready)
                {
                    return;
                }
                else
                {
                    if (currentBallsState == BallsState.NotReady) InitBalls();
                }
            }
            if (currentBallsState == BallsState.NewBall)
            {
                if (currentProjectileBall == null)
                {
                    PrepareNewProjectile();
                    currentProjectileBall = SpawnProjectileBall(DataObject.instance.nextProjectileBall).GetComponent<ProjectileBallController>();
                }
            }
        }

        private void InitBalls()
        {
            //  Spawning balls from json

            currentBallsState = BallsState.Initializing;

            IOManager.instance.LoadJsonLocal("balls_positions.json", "balls");

            StartCoroutine(WaitForBallsLoaded());

            DataObject.instance.spawnedBalls = new List<Ball>();
            

        }

        private IEnumerator WaitForBallsLoaded()
        {
            yield return new WaitUntil(AreBallsLoaded);

            ballsRoot = new GameObject();
            ballsRoot.transform.position = Vector3.zero;
            ballsRoot.name = "BallsRoot";

            SpawnLoadedBalls();
            currentBallsState = BallsState.Ready;
        }

        bool AreBallsLoaded()
        {
            if (DataObject.instance.loadedBalls != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SpawnLoadedBalls()
        {
            int counter = 0;
            foreach(Ball ball in DataObject.instance.loadedBalls)
            {
                ball.id = null;
                SpawnBall(ball);
                counter++;
            }
        }

        private GameObject SpawnBall(Ball attachedBall)
        {
            GameObject spawnedBall = Instantiate(ballPrefab,ballsRoot.transform);
            spawnedBall.AddComponent<BallController>();
            spawnedBall.GetComponent<BallController>().InitBall(attachedBall);
            currentBallsList.Add(attachedBall);
            spawnedBallsObjects.Add(spawnedBall);
            return spawnedBall;
        }



        private GameObject SpawnProjectileBall(Ball attachedBall)
        {
            GameObject spawnedProjectileBall = Instantiate(projectileBallPrefab);
            spawnedProjectileBall.AddComponent<ProjectileBallController>();
            spawnedProjectileBall.GetComponent<ProjectileBallController>().attachedBall = attachedBall;
            spawnedProjectileBall.GetComponent<ProjectileBallController>().InitBall(projectileSpawnPoint);
            currentBallsState = BallsState.Ready;
            DataObject.instance.remainingBalls--;
            return spawnedProjectileBall;
        }

        private Ball GetBallByID(string id)
        {
            Ball result = null;
            result = currentBallsList.Find(ball => ball.id == id);
            return result;
        }

        private GameObject GetBallObjectByID(string id)
        {
            GameObject result = null;
            result = spawnedBallsObjects.Find(ballObj => ballObj.name == id);
            return result;
        }

        private Ball GetBallByRowColumn(int row, int column)
        {
            Ball result = null;
            result = currentBallsList.FirstOrDefault(ball => ball.yPos == row && ball.xPos == column);
            return result;
        }

        private void ShowCurrentBallList()
        {
            string ballIds = "";
            string ballX = "";
            string ballY = "";
            foreach(Ball ball in currentBallsList)
            {
                ballIds += ball.id + " ";
                ballX += ball.xPos + " ";
                ballY += ball.yPos + " ";
            }
            Debug.Log(ballIds);
            Debug.Log(ballX);
            Debug.Log(ballY);
        }

        private void ShowBallList(List<Ball> ballList, string message)
        {
            Debug.Log(message);
            string ballIds = "";
            string ballX = "";
            string ballY = "";
            foreach (Ball ball in ballList)
            {
                ballIds += ball.id + " ";
                ballX += ball.xPos + " ";
                ballY += ball.yPos + " ";
            }
            Debug.Log(ballIds);
//            Debug.Log(ballX);
//            Debug.Log(ballY);
        }

    }

}
