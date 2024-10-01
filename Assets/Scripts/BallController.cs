using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    public class BallController : MonoBehaviour
    {
        public Ball attachedBall;

        public GameObject jointBase;

        public void InitBall(Ball ballToAttach)
        {
            attachedBall = ballToAttach;
            jointBase = this.transform.Find("JointBase").gameObject;

            //  Spawning loaded balls without id

            if (attachedBall.id == null)
            {
                foreach (Square square in DataObject.instance.squares)
                {
                    if (square.column == attachedBall.xPos && square.row == attachedBall.yPos)
                    {
                        foreach (GameObject fieldSquare in FieldManager.instance.fieldSquares)
                        {
                            if (fieldSquare.name == square.id)
                            {

                                attachedBall.id = square.id;
                                gameObject.name = attachedBall.id;
                                float shiftValue = 0.5f * DataObject.instance.currentFieldScale;
                                Vector3 ballPos = new Vector3(fieldSquare.transform.position.x + shiftValue, fieldSquare.transform.position.y - shiftValue, 0f);
                                gameObject.transform.position = ballPos;
                                gameObject.transform.localScale = new Vector3(DataObject.instance.currentFieldScale, DataObject.instance.currentFieldScale, DataObject.instance.currentFieldScale);
                                gameObject.GetComponent<MeshRenderer>().material.color = DataObject.instance.ballColors[attachedBall.color];
                                DataObject.instance.spawnedBalls.Add(attachedBall);
                            }
                        }
                    }
                }
            //  If spawning ball from projectile
            } else
            {
                foreach (GameObject fieldSquare in FieldManager.instance.fieldSquares)
                {
                    if (fieldSquare.name == attachedBall.id)
                    {
                        //  Adding row and column

                        foreach (Square square in DataObject.instance.squares)
                        {
                            if (square.id == attachedBall.id)
                            {
                                attachedBall.xPos = square.column;
                                attachedBall.yPos = square.row;
                            }
                        }


                        gameObject.name = attachedBall.id;
                        float shiftValue = 0.5f * DataObject.instance.currentFieldScale;
                        Vector3 ballPos = new Vector3(fieldSquare.transform.position.x + shiftValue, fieldSquare.transform.position.y - shiftValue, 0f);
                        gameObject.transform.position = ballPos;
                        gameObject.transform.localScale = new Vector3(DataObject.instance.currentFieldScale, DataObject.instance.currentFieldScale, DataObject.instance.currentFieldScale);
                        gameObject.GetComponent<MeshRenderer>().material.color = DataObject.instance.ballColors[attachedBall.color];
                        DataObject.instance.spawnedBalls.Add(attachedBall);
                    }
                }
            }
        }

        public void BlowupAndDestroy()
        {
            this.GetComponent<Animation>().Play("Blowup");
            StartCoroutine(BlowupAndDestroyBall());
        }

        IEnumerator BlowupAndDestroyBall()
        {
            yield return new WaitForSeconds(.5f);
            Destroy(gameObject);
        }

        public void DropAndDestroy()
        {
            this.GetComponent<Rigidbody2D>().mass = 1f;
            jointBase.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
            StartCoroutine(DropAndDestroyBall());
        }

        IEnumerator DropAndDestroyBall()
        {
            yield return new WaitForSeconds(2f);
            Destroy(gameObject);
        }
    }
}
