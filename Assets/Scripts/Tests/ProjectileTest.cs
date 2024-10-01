using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    public class ProjectileTest : MonoBehaviour
    {

        private float collisionCheckRadius = .15f;
        float simulateForDuration = 5f;//simulate for 5 secs in the furture
        float simulationStep = 0.02f;//Will add a point every 0.1 secs.
        float launchSpeed = 2f;//Example speed per secs.

        public LineRenderer line1;
        public LineRenderer line2;
        public LineRenderer line1Spread;
        public LineRenderer line2Spread;

        public Material spreadMaterial;
        public Material trajectoryMaterial;

        private bool isDrawingReflection = false;

        private Vector2 reflectionPos;
        private Vector2 collisionPos;
        private Vector2 reflectionLaunchPos;
        private Vector2 reflectionSpreadLaunchPos;
        private Vector2 reflectionDirection;
        private Collider2D firstHitCollider;

        private Vector2 directionVector;
        List<Vector3> line1RendererPoints;
        List<Vector3> line2RendererPoints;
        List<Vector3> line1SpreadRendererPoints;
        List<Vector3> line2SpreadRendererPoints;
        // Start is called before the first frame update
        void Start()
        {
            Ball newProjectileBall = new Ball();
            GetComponent<BallsManager>().SpawnProjectileTest(newProjectileBall);

        }


        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                line1.gameObject.SetActive(true);


                SimulateLine1();
                if (isDrawingReflection)
                {
                    line2.gameObject.SetActive(true);
    //                line2Spread.gameObject.SetActive(true);
                    SimulateLine2();
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                line1.gameObject.SetActive(false);
                line2.gameObject.SetActive(false);
                line1Spread.gameObject.SetActive(false);
    //            line2Spread.gameObject.SetActive(false);
                Vector2[] targets = new Vector2[2];
                targets[0] = reflectionPos;
                targets[1] = collisionPos;
                Vector3[] pointsArray = new Vector3[line1RendererPoints.Count + line2RendererPoints.Count];
                line1RendererPoints.ToArray().CopyTo(pointsArray,0);
                line2RendererPoints.ToArray().CopyTo(pointsArray, line1RendererPoints.Count);

                GetComponent<BallsManager>().currentProjectileBall.LaunchBall(launchSpeed*2,pointsArray,false,false);
            }
        }



        private void SimulateLine1()
        {
            int steps = (int)(simulateForDuration / simulationStep);//50 in this example
            line1RendererPoints = new List<Vector3>();
            line1SpreadRendererPoints = new List<Vector3>();
            Vector3 calculatedPosition;
            Vector3 calculatedSpreadPosition;

            Vector3 worldMousePos = Input.mousePosition;
            worldMousePos = Camera.main.ScreenToWorldPoint(worldMousePos);

            Vector2 calculatedDirection = new Vector2 (-worldMousePos.x*2,4f);
            //            Vector2 calculatedDirection = new Vector2(
            //                (-Input.mousePosition.x + Screen.width/2)/100,
            //                (Screen.height/500));

            launchSpeed = Mathf.Clamp((-worldMousePos.y+GetComponent<BallsManager>().projectileSpawnPoint.y+1)*3,3,6);
            directionVector = calculatedDirection;//new Vector2(0.5f, 0.5f);//You plug you own direction here this is just an example
            Vector2 launchPosition = GetComponent<BallsManager>().projectileSpawnPoint;//Position where you launch from


            for (int i = 0; i < steps; ++i)
            {
                float spreadMultiplier = ((launchSpeed - 3) / 6);
                if (launchSpeed > 5)
                {
                    Vector2 spreadVectorMin = new Vector2(directionVector.x - spreadMultiplier, directionVector.y);
                    calculatedPosition = launchPosition + (spreadVectorMin * (launchSpeed * i * simulationStep));
                } else
                {
                    calculatedPosition = launchPosition + (directionVector * (launchSpeed * i * simulationStep));
                }

                calculatedPosition.y += Physics2D.gravity.y * (i * simulationStep) * (i * simulationStep);

                Vector2 spreadVectorMax = new Vector2(directionVector.x + spreadMultiplier, directionVector.y);
                calculatedSpreadPosition = launchPosition + (spreadVectorMax * (launchSpeed * i * simulationStep));
                calculatedSpreadPosition.y += Physics2D.gravity.y * (i * simulationStep) * (i * simulationStep);
                line1RendererPoints.Add(calculatedPosition);
                line1SpreadRendererPoints.Add(calculatedSpreadPosition);
                RaycastHit2D[] hits = Physics2D.CircleCastAll(calculatedPosition, collisionCheckRadius, Vector2.zero);
                if (hits.Length > 0)
                {
                    if (!hits[0].collider.CompareTag("BallCollider"))
                    {
                        if (CheckForCollision(calculatedPosition))//if you hit something
                        {
                            isDrawingReflection = true;
                            reflectionLaunchPos = calculatedPosition;
                            reflectionSpreadLaunchPos = calculatedSpreadPosition;
                            reflectionDirection = directionVector;
                            reflectionPos = calculatedPosition;
                            break;
                        }
                    }
                    else
                    {
                        isDrawingReflection = false;
                        break;
                    }

                }

                RaycastHit2D[] spreadHits = Physics2D.CircleCastAll(calculatedSpreadPosition, collisionCheckRadius, Vector2.zero);
                if (spreadHits.Length > 0)
                {
                    if (!spreadHits[0].collider.CompareTag("BallCollider"))
                    {
                        if (CheckForCollision(calculatedSpreadPosition))//if you hit something
                        {
                            reflectionSpreadLaunchPos = calculatedSpreadPosition;
                        }
                    }
                }


            }




            line1.positionCount = line1RendererPoints.Count;
            line1.SetPositions(line1RendererPoints.ToArray());

            if (launchSpeed > 5)
            {
                line1Spread.gameObject.SetActive(true);
                line1Spread.positionCount = line1SpreadRendererPoints.Count;
                line1Spread.SetPositions(line1SpreadRendererPoints.ToArray());
                line1.material = spreadMaterial;

            } else
            {
                line1Spread.gameObject.SetActive(false);
                line1.material = trajectoryMaterial;
            }

        }

        private void SimulateLine2()
        {
            line2RendererPoints = new List<Vector3>();
            line2SpreadRendererPoints = new List<Vector3>();
            int steps = (int)(simulateForDuration / simulationStep);//50 in this example
            Vector3 reflectedPosition;
            Vector3 calculatedSpreadReflectedPosition;
            RaycastHit2D[] hits = Physics2D.CircleCastAll(reflectionLaunchPos, collisionCheckRadius, Vector2.zero);
            if (hits.Length > 0)
            {
                firstHitCollider = hits[0].collider;
                for (int j = 0; j < steps; ++j)
                {
                    Vector2 reflectedDirection = Vector3.Reflect(reflectionDirection, hits[0].normal);

                    float spreadMultiplier = ((launchSpeed - 3) / 6);
                    if (launchSpeed > 5)
                    {
                        Vector2 spreadVectorMin = new Vector2(reflectedDirection.x - spreadMultiplier, reflectedDirection.y);
                        
                        reflectedPosition = reflectionLaunchPos + (spreadVectorMin * (launchSpeed * j * simulationStep));
                    }
                    else
                    {
                        reflectedPosition = reflectionLaunchPos + (reflectedDirection * (launchSpeed * j * simulationStep));
                    }
                   
                    reflectedPosition.y += Physics2D.gravity.y * (j * simulationStep) * (j * simulationStep);

                    Vector2 spreadVectorMax = new Vector2(reflectedDirection.x + spreadMultiplier, directionVector.y);
                    calculatedSpreadReflectedPosition = reflectionSpreadLaunchPos + (spreadVectorMax * (launchSpeed * j * simulationStep));
                    calculatedSpreadReflectedPosition.y += Physics2D.gravity.y * (j * simulationStep) * (j * simulationStep);

                    line2SpreadRendererPoints.Add(calculatedSpreadReflectedPosition);

                    line2RendererPoints.Add(reflectedPosition);
                    if (CheckForReflectedCollision(reflectedPosition))
                    {
                        collisionPos = reflectedPosition;
                        break;
                    }
                }

                line2.positionCount = line2RendererPoints.Count;
                line2.SetPositions(line2RendererPoints.ToArray());

                if (launchSpeed > 5)
                {
                    line2Spread.gameObject.SetActive(true);
                    line2Spread.positionCount = line2SpreadRendererPoints.Count;
                    line2Spread.SetPositions(line2SpreadRendererPoints.ToArray());
                    line2.material = spreadMaterial;

                }
                else
                {
                    line2Spread.gameObject.SetActive(false);
                    line2.material = trajectoryMaterial;
                }

            }

        }

        private bool CheckForReflectedCollision(Vector2 position)
        {

            RaycastHit2D[] hits = Physics2D.CircleCastAll(position, collisionCheckRadius, Vector2.zero);
            if (hits.Length > 0)
            {
                if (hits[0].collider != firstHitCollider)
                {
                    return true;

                }
            }
            return false;
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


    }

}
