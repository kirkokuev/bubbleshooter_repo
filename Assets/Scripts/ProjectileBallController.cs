using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    public class ProjectileBallController : BallController
    {
        public Ball attachedBall;


        public void InitBall(Vector2 spawnPoint)
        {
            gameObject.transform.position = spawnPoint;
            gameObject.AddComponent<ProjectilePhysics>();
            
            gameObject.GetComponent<MeshRenderer>().material.color = DataObject.instance.ballColors[attachedBall.color];
        }

        public void LaunchBall(float launchSpeed, Vector3[] linePoints, bool isSpread, bool isBottom)
        {
            if (gameObject.GetComponent<ProjectilePhysics>().currentProjectileState == ProjectilePhysics.ProjectileStates.Idle)
            {
                gameObject.GetComponent<ProjectilePhysics>().projectileSpeed = launchSpeed;
                gameObject.GetComponent<ProjectilePhysics>().linePoints = linePoints;
                gameObject.GetComponent<ProjectilePhysics>().isSpread = isSpread;
                gameObject.GetComponent<ProjectilePhysics>().isBottom = isBottom;
                gameObject.GetComponent<ProjectilePhysics>().currentProjectileState = ProjectilePhysics.ProjectileStates.Launched;
            }
        }

    }

}
