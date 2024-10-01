using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    public class SquareController : MonoBehaviour
    {
        public Square square;

        public void InitSquare(string id, int xPos, int yPos)
        {
            square = new Square();
            square.column = yPos;
            square.row = xPos;
            square.id = id;
            DataObject.instance.squares.Add(square);

        }
    }

}
