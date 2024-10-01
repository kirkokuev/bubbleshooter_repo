using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BubbleShooter
{
    public class TrajectoryManager : MonoBehaviour
    {
        public float speedMultiplier;
        public float speedMin;
        public float speedMax;
        public float directionMultiplier;
        public float directionHeight;


        public GameObject trajectoryLinePrefab;
        public Material preciseTrajectoryMaterial;
        public Material spreadTrajectoryMaterial;

        public TrajectoryState currentTrajectoryState = TrajectoryState.Precise;

        public BallsManager currentBallsManager;

        private GameObject preciseTrajectory;
        private GameObject preciseReflectedTrajectory;
        private GameObject spreadTrajectoryMin;
        private GameObject spreadTrajectoryMax;
        private GameObject reflectedSpreadTrajectoryMin;
        private GameObject reflectedSpreadTrajectoryMax;
        private GameObject visibleTrajectory;

        List<Vector3> launchTrajectory;
        List<Vector3> reflectionTrajectory;

        //  Simulation parameters

        private float collisionCheckRadius = .15f;
        private float simulateForDuration = 5f;
        private float simulationStep = 0.02f;
        private float launchSpeed = 2f;

        private string currentTargetSquareID;
        private bool isTouchingBottom = false;

        public enum TrajectoryState
        {
            Precise,
            Spread
        }

        private void Update()
        {

            if (GameLoopManager.instance.currentGameState == GameLoopManager.GameStates.Shooting)
            {
                TrackSpread();
                TrackMouseInput();

            }

        }



        private void Start()
        {
            CreateTrajectoryLines();
            //            currentBallsManager.SpawnProjectileTest();

        }

        private void UpdateLineRenderer(LineRenderer lr, List<Vector3> positions)
        {
            lr.positionCount = positions.Count;
            lr.SetPositions(positions.ToArray());
            if (currentTrajectoryState == TrajectoryState.Precise)
            {
                lr.material = preciseTrajectoryMaterial;
                lr.startWidth = 0.03f;
                lr.endWidth = 0.03f;
            } else
            {
                lr.material = spreadTrajectoryMaterial;
                lr.startWidth = 0.03f;
                lr.endWidth = CalculateLaunchSpeed(speedMultiplier, speedMin, speedMax, currentBallsManager.projectileSpawnPoint) / 2;

            }
        }

        private RaycastHit2D GetCollisionHit(Vector2 calculatedPosition)
        {
            RaycastHit2D result = new RaycastHit2D();
            var wallLayerMask = LayerMask.NameToLayer("WallCollider");
            RaycastHit2D[] hits = Physics2D.CircleCastAll(calculatedPosition, 0.5f, Vector2.zero);
            if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.CompareTag("Wall"))
                    {
                        result = hit;
                    }
                }
            }
            return result;
        }

        private void CheckBottomCollision(Vector2 calculatedPosition)
        {
            RaycastHit2D[] hits = Physics2D.CircleCastAll(calculatedPosition, collisionCheckRadius, Vector2.down, 0.1f);
            if (hits.Length > 0)
            {
                bool result = false;
                foreach(RaycastHit2D hit in hits)
                {

                    if (hit.collider.CompareTag("BottomCollider"))
                    {
                        result = true;
                    }
                }
                isTouchingBottom = result;

            }
        }

        private bool CheckReflectedTrajectoryCollision(Vector2 position, RaycastHit2D hit)
        {

            RaycastHit2D[] hits = Physics2D.CircleCastAll(position, collisionCheckRadius, Vector2.zero);
            if (hits.Length > 0)
            {
                if (hits[0].collider != hit.collider)
                {
                    if (hits[0].collider.CompareTag("ProjectileCollider") || hits[0].collider.CompareTag("SquareCollider"))
                    {
                        return false;
                    } else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CheckSquareCollision(Vector2 calculatedPosition)
        {
            bool result = false;
            RaycastHit2D[] hits = Physics2D.CircleCastAll(calculatedPosition, collisionCheckRadius, Vector2.down,0.1f);
            if (hits.Length > 0)
            {

                for (int i = hits.Length - 1; i >= 0; i--)
                {
                    if (hits[i].collider.CompareTag("SquareCollider"))
                    {
                        if (IsSquareVacant(hits[i].collider.gameObject.GetComponent<SquareController>().square.id))
                        {
                            // Colliding with vacant square
                            currentTargetSquareID = hits[i].collider.gameObject.GetComponent<SquareController>().square.id;
                            result = true;
                            return result;
                            break;
                        }

                    }

                }
            }
            return result;
        }

        private bool CheckBallCollision(Vector2 calculatedPosition)
        {
            RaycastHit2D[] hits = Physics2D.CircleCastAll(calculatedPosition, collisionCheckRadius*10, Vector2.zero);
            if (hits.Length > 0)
            {
                List<RaycastHit2D> ballHits = new List<RaycastHit2D>();
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.CompareTag("BallCollider"))
                    {
                        ballHits.Add(hit);
                    }
                }
                if (ballHits.Count > 0)
                {
                    int randomBallIndex = Random.Range(0, ballHits.Count);
                    currentTargetSquareID = ballHits[randomBallIndex].collider.name;
                    return true;
                } else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private bool CheckTrajectoryBallCollision(Vector2 calculatedPosition)
        {
            RaycastHit2D[] hits = Physics2D.CircleCastAll(calculatedPosition, collisionCheckRadius, Vector2.zero);
            if (hits.Length > 0)
            {
               if (hits[0].collider.CompareTag("BallCollider"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private bool CheckTrajectoryCollision(Vector2 calculatedPosition)
        {
            RaycastHit2D[] hits = Physics2D.CircleCastAll(calculatedPosition, collisionCheckRadius, Vector2.zero);
            if (hits.Length > 0)
            {
                if (hits[0].collider.CompareTag("ProjectileCollider") || hits[0].collider.CompareTag("SquareCollider"))
                {
                    return false;
                } else
                {
                    if (!hits[0].collider.CompareTag("BallCollider"))
                    {
                        if (CheckForCollision(calculatedPosition))//if you hit something
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }

                }

            } else
            {
                return false;
            }
        }

        private Vector3[] MergeTrajectoryPoints(List<Vector3> launchTrajectory, List<Vector3> reflectedTrajectory)
        {
            Vector3[] pointsArray = new Vector3[launchTrajectory.Count + reflectedTrajectory.Count];
            launchTrajectory.ToArray().CopyTo(pointsArray, 0);
            reflectedTrajectory.ToArray().CopyTo(pointsArray, launchTrajectory.Count);
            return pointsArray;
        }

        private List<Vector3> MergeTrajectoryToList(List<Vector3> launchTrajectory, List<Vector3> reflectedTrajectory)
        {
            List<Vector3> pointsList = new List<Vector3>();
            if (reflectedTrajectory != null)
            {
                pointsList = launchTrajectory.Concat(reflectedTrajectory).ToList();
            } else
            {
                pointsList = launchTrajectory;
            }

            return pointsList;
        }

        private void SimulateReflectedTrajectory(RaycastHit2D hit, Vector2 launchPosition, Vector2 reflectionDirection, Vector2 originPosition)
        {
            int steps = (int)(simulateForDuration / simulationStep);

            Vector3 calculatedPosition;
            List<Vector3> trajectoryPoints = new List<Vector3>();
            float launchSpeed = CalculateLaunchSpeed(speedMultiplier, speedMin, speedMax, originPosition);
            Vector2 reflectedDirection = Vector3.Reflect(reflectionDirection, hit.normal);
            for (int i = 0; i < steps; ++i)
            {
                calculatedPosition = launchPosition + (reflectedDirection * (launchSpeed * i * simulationStep));
                calculatedPosition.y += Physics2D.gravity.y * (i * simulationStep) * (i * simulationStep);
                if (Screen.safeArea.Contains(Camera.main.WorldToScreenPoint(calculatedPosition)))
                {
                    trajectoryPoints.Add(calculatedPosition);
                }
                else {  break;  }

                if (CheckReflectedTrajectoryCollision(calculatedPosition, hit)) {   break;  }

                if (CheckTrajectoryBallCollision(calculatedPosition)) { break;  }
            }

//            UpdateLineRenderer(preciseReflectedTrajectory.GetComponent<LineRenderer>(), trajectoryPoints);
            reflectionTrajectory = trajectoryPoints;
        }

        private void SimulateTrajectory(Vector2 launchPosition)
        {
            int steps = (int)(simulateForDuration / simulationStep);
            Vector3 calculatedPosition;
            List<Vector3> trajectoryPoints = new List<Vector3>();
            float launchSpeed = CalculateLaunchSpeed(speedMultiplier, speedMin, speedMax, launchPosition);
            Vector2 directionVector = CalculateDirection(directionMultiplier, directionHeight);
            bool isReflecting = false;
            Vector2 reflectionPosition = Vector2.zero;
            for (int i = 0; i < steps; ++i)
            {
                calculatedPosition = launchPosition + (directionVector * (launchSpeed * i * simulationStep));
                calculatedPosition.y += Physics2D.gravity.y * (i * simulationStep) * (i * simulationStep);

                // Check if position is visible
                if (Screen.safeArea.Contains(Camera.main.WorldToScreenPoint(calculatedPosition)))
                {
                    trajectoryPoints.Add(calculatedPosition);
                } else
                {
                    break;
                }

                if (CheckTrajectoryCollision(calculatedPosition))
                {


                    if (CheckTrajectoryBallCollision(calculatedPosition))
                    {
                        
                        isReflecting = false;
                    }
                    else
                    {
                        isReflecting = true;
                        reflectionPosition = calculatedPosition;
                    }

                    break;
                }
                else
                {
                    isReflecting = false;
                }
            }
            if (isReflecting)
            {
                SimulateReflectedTrajectory(GetCollisionHit(reflectionPosition), reflectionPosition, directionVector, launchPosition);
            } else
            {
                reflectionTrajectory = new List<Vector3>();
            }

//            UpdateLineRenderer(preciseTrajectory.GetComponent<LineRenderer>(), trajectoryPoints);
            launchTrajectory = trajectoryPoints;

        }

        private void TrackSpread()
        {
            if (BallsManager.instance.currentProjectileBall != null)
            {
                if (CalculateLaunchSpeed(speedMultiplier, speedMin, speedMax, currentBallsManager.currentProjectileBall.transform.position) > 5)
                {
                    currentTrajectoryState = TrajectoryState.Spread;
                }
                else
                {
                    currentTrajectoryState = TrajectoryState.Precise;
                }

            }
        }

        private void ToggleTrajectories(bool areTrajectoriesVisible)
        {
            if (areTrajectoriesVisible)
            {
                visibleTrajectory.SetActive(true);
                if (currentTrajectoryState == TrajectoryState.Precise)
                {
                    preciseTrajectory.SetActive(false);
                    preciseReflectedTrajectory.SetActive(false);
                }
                else if (currentTrajectoryState == TrajectoryState.Spread)
                {
                    preciseTrajectory.SetActive(false);
                    preciseReflectedTrajectory.SetActive(false);
                }
            } else
            {
                visibleTrajectory.SetActive(false);
                preciseTrajectory.SetActive(false);
                preciseReflectedTrajectory.SetActive(false);
            }
        }

        private void TrackMouseInput()
        {
            if (Input.GetMouseButton(0))
            {
                ToggleTrajectories(true);
                SimulateTrajectory(currentBallsManager.projectileSpawnPoint);
                UpdateLineRenderer(visibleTrajectory.GetComponent<LineRenderer>(),
                    MergeTrajectoryToList(launchTrajectory, reflectionTrajectory));
            }
            else if (Input.GetMouseButtonUp(0))
            {
                ToggleTrajectories(false);

                //  If we have spread state - check if colliding with ball
                //  else check if colliding with square

                Vector3[] testTrajectory = MergeTrajectoryPoints(launchTrajectory, reflectionTrajectory);
                CheckBottomCollision(testTrajectory[testTrajectory.Length - 1]);
                if (currentTrajectoryState == TrajectoryState.Precise)
                {
                    //  Square
                 

                    if (CheckSquareCollision(testTrajectory[testTrajectory.Length - 1]))
                    {
                        currentBallsManager.currentProjectileBall.GetComponent<ProjectilePhysics>().targetSquareID = currentTargetSquareID;
                        currentBallsManager.currentProjectileBall.LaunchBall(
                            CalculateLaunchSpeed(speedMultiplier, speedMin, speedMax, currentBallsManager.projectileSpawnPoint) * 3,
                            MergeTrajectoryPoints(launchTrajectory, reflectionTrajectory), false, isTouchingBottom);
                    }
                } else if (currentTrajectoryState == TrajectoryState.Spread)
                {
                    //  Ball

                    if (CheckBallCollision(testTrajectory[testTrajectory.Length - 1]))
                    {
                        currentBallsManager.currentProjectileBall.GetComponent<ProjectilePhysics>().targetSquareID = currentTargetSquareID;
                        currentBallsManager.currentProjectileBall.LaunchBall(
                            CalculateLaunchSpeed(speedMultiplier, speedMin, speedMax, currentBallsManager.projectileSpawnPoint) * 3,
                            MergeTrajectoryPoints(launchTrajectory, reflectionTrajectory), true, isTouchingBottom);
                    }
                }
            }
        }

        private void CreateTrajectoryLines()
        {
            GameObject trajectoryRoot = new GameObject("TrajectoryRoot");

            preciseTrajectory = Instantiate(trajectoryLinePrefab, trajectoryRoot.transform);
            preciseReflectedTrajectory = Instantiate(trajectoryLinePrefab, trajectoryRoot.transform);

            visibleTrajectory = Instantiate(trajectoryLinePrefab, trajectoryRoot.transform);

            //            spreadTrajectoryMax = Instantiate(trajectoryLinePrefab, trajectoryRoot.transform);
            //            spreadTrajectoryMin = Instantiate(trajectoryLinePrefab, trajectoryRoot.transform);
            //            reflectedSpreadTrajectoryMax = Instantiate(trajectoryLinePrefab, trajectoryRoot.transform);
            //            reflectedSpreadTrajectoryMin = Instantiate(trajectoryLinePrefab, trajectoryRoot.transform);

            preciseTrajectory.name = "PreciseTrajectory";
            preciseReflectedTrajectory.name = "PreciseReflectedTrajectory";
            visibleTrajectory.name = "VisibleTrajectory";
            //            spreadTrajectoryMax.name = "SpreadTrajectoryMax";
            //            spreadTrajectoryMin.name = "SpreadTrajectoryMin";
            //            reflectedSpreadTrajectoryMax.name = "ReflectedSpreadTrajectoryMax";
            //            reflectedSpreadTrajectoryMin.name = "ReflectedSpreadTrajectoryMin";

            preciseTrajectory.GetComponent<LineRenderer>().material = preciseTrajectoryMaterial;
            preciseReflectedTrajectory.GetComponent<LineRenderer>().material = preciseTrajectoryMaterial;
            visibleTrajectory.GetComponent<LineRenderer>().material = preciseTrajectoryMaterial;
            //            spreadTrajectoryMax.GetComponent<LineRenderer>().material = spreadTrajectoryMaterial;
            //            spreadTrajectoryMin.GetComponent<LineRenderer>().material = spreadTrajectoryMaterial;
            //            reflectedSpreadTrajectoryMax.GetComponent<LineRenderer>().material = spreadTrajectoryMaterial;
            //            reflectedSpreadTrajectoryMin.GetComponent<LineRenderer>().material = spreadTrajectoryMaterial;

            preciseTrajectory.SetActive(false);
            preciseReflectedTrajectory.SetActive(false);
            visibleTrajectory.SetActive(false);
            //            spreadTrajectoryMax.SetActive(false);
            //            spreadTrajectoryMin.SetActive(false);
            //            reflectedSpreadTrajectoryMax.SetActive(false);
            //            reflectedSpreadTrajectoryMin.SetActive(false);



        }

        private float CalculateLaunchSpeed(float speedMultiplier, float speedMin, float speedMax, Vector3 originPoint)
        {
            Vector3 worldMousePos = Input.mousePosition;
            worldMousePos = Camera.main.ScreenToWorldPoint(worldMousePos);
            return Mathf.Clamp((-worldMousePos.y + originPoint.y + 1) * speedMultiplier, speedMin, speedMax);
        }

        private Vector2 CalculateDirection(float directionMultiplier, float directionHeight)
        {
            Vector3 worldMousePos = Input.mousePosition;
            worldMousePos = Camera.main.ScreenToWorldPoint(worldMousePos);
            Vector2 calculatedDirection = new Vector2(-worldMousePos.x * directionMultiplier, directionHeight);
            return calculatedDirection;
        }

        private bool CheckForCollision(Vector2 position)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(position, collisionCheckRadius);
            if (hits.Length > 0)
            {
                return true;
            }
            return false;
        }

        private bool IsSquareVacant(string id)
        {
            bool result = true;

            foreach (Ball spawnedBall in DataObject.instance.spawnedBalls)
            {
                
                if (id == spawnedBall.id)
                {
                    result = false;
                }
            }
            return result;
        }


    }






}


