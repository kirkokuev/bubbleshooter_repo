using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    public class ProjectilePhysics : MonoBehaviour
    {
        public float projectileSpeed = 2f;
        public float speed = 30.0f;

        public Vector3 target;
        public Vector2[] targets;
        public Vector3 vectorToTarget;
        public bool isSpread;
        public bool isBottom;

        public string targetSquareID;

        public ProjectileStates currentProjectileState = ProjectileStates.Idle;
        public enum ProjectileStates
        {
            Idle,
            Launched,
            Attaching,
            Collided
        }

        private Vector3 initialPos;
        public Vector3 directionVector;

        public Vector3[] linePoints;

        int index = 0;

        private Transform squareTransform;



        // Start is called before the first frame update
        void Start()
        {
            initialPos = transform.position;

//            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
//            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            //            transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * speed);

        }

        public void SetVectorToTarget()
        {
            vectorToTarget = target - transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            if (currentProjectileState == ProjectileStates.Launched)
            {
                if (!isSpread)
                {
                    Move();
                } else
                {
                    RandomizeTrajectory();
                    Move();
                }
            }

            if (currentProjectileState == ProjectileStates.Collided)
            {
                if (!isSpread)
                {
                    if (!isBottom)
                    {
                        BallsManager.instance.SpawnBallAtSquare(targetSquareID, BallsManager.instance.currentProjectileBall.attachedBall.color);
                        Destroy(gameObject);
                    } else
                    {
                        Destroy(gameObject);
                    }
                } else
                {
                        BallsManager.instance.DestroyAndSpawnBallAtSquare(targetSquareID, BallsManager.instance.currentProjectileBall.attachedBall.color);
                        Destroy(gameObject);

                }
            }
        }
        


        void RandomizeTrajectory() {
            for(int i=0;i<linePoints.Length;i++)
            {
                linePoints[i] += Random.onUnitSphere * .05f;
                linePoints[i].z = 0;
            }
        }

        void Move()
        {
            transform.position = Vector3.MoveTowards(transform.position, linePoints[index], projectileSpeed * Time.deltaTime);
            if (transform.position == linePoints[index])
            {
                index++;
            }
            if (index == linePoints.Length)
            {
                currentProjectileState = ProjectileStates.Collided;
            }
        }

    }

}
