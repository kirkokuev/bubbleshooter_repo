using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BubbleShooter
{
    public class FieldManager : MonoBehaviour
    {
        public static FieldManager instance;

        public Texture2D fieldTexture;

        public FieldState currentState = FieldState.Preparing;

        public GameObject[] fieldSquares;
        private Field currentField;
        private Sprite[] fieldSprites;

        private GameObject fieldRoot;

        public enum FieldState
        {
            Ready,
            Preparing
        }

        private void Awake()
        {
            instance = this;
        }

        public Square GetSquareByID(string id)
        {
            Square result = null;
            foreach (Square square in DataObject.instance.squares)
            {
                if (square.id == id)
                {
                    result = square;
                }
            }
            return result;
        }

        public Square[] GetSurroundingSquares(string id)
        {
            Square[] result = new Square[9];
            Square targetSquare = GetSquareByID(id);
            if (targetSquare != null)
            {
                //  Left
                //  Right
                //  Top


                int counter = 0;
                for (int i = -1; i <= 1; i++)        // Row
                {
                    for (int j = -1; j <= 1; j++)    // Column
                    {
                        Square tempSquare = null;
                        if (targetSquare.column != 0 && targetSquare.column < DataObject.instance.fieldDimensions[1]-1 
                            && targetSquare.row != 0)
                        {
                            //  Middle
                            tempSquare = GetSquareByRowColumn(targetSquare.row + i, targetSquare.column + j);

                        } else if (targetSquare.column == 0 && targetSquare.row != 0)
                        {
                            //  Left
                            tempSquare = GetSquareByRowColumn(targetSquare.row + i, targetSquare.column + (j+1));

                        } else if (targetSquare.column == DataObject.instance.fieldDimensions[1]-1 && targetSquare.row != 0)
                        {
                            //  Right
                            tempSquare = GetSquareByRowColumn(targetSquare.row + i, targetSquare.column + (j-1));

                        } else if (targetSquare.column != 0 && targetSquare.column < DataObject.instance.fieldDimensions[1]-1
                            && targetSquare.row == 0)
                        {
                            //  Top
                            tempSquare = GetSquareByRowColumn(targetSquare.row + (i+1), targetSquare.column + j);

                        } else if (targetSquare.column == 0 && targetSquare.row == 0)
                        {
                            //  Top left
                            tempSquare = GetSquareByRowColumn(targetSquare.row + (i+1), targetSquare.column + (j + 1));

                        } else if (targetSquare.column == DataObject.instance.fieldDimensions[1]-1 && targetSquare.row == 0)
                        {
                            //  Top right
                            tempSquare = GetSquareByRowColumn(targetSquare.row + (i+1), targetSquare.column + (j - 1));

                        }
                        if (tempSquare != null)
                        {
                            result[counter] = tempSquare;
                            counter++;
                        } 
                    }
                    
                }
            }



            return result;
        }

        public Square GetSquareByRowColumn(int row, int column)
        {

            Square result = null;
            result = DataObject.instance.squares.FirstOrDefault(square => square.row == row && square.column == column);
            return result;
        }


        public void CreateField(int rowsCount, int columnsCount, float scale)
        {
            //  Dispose existing field sprites
            if (fieldSquares != null && fieldSquares.Length > 0)
            {
                foreach (GameObject fieldSquare in fieldSquares)
                {
                    Destroy(fieldSquare);
                }
            }
            fieldSquares = new GameObject[rowsCount * columnsCount];
            //  Dispose existing field
            currentField = null;
            Destroy(fieldRoot);

            //  Init new field
            fieldRoot = new GameObject("FieldRoot");
            fieldRoot.transform.position = Vector2.zero;

            DataObject.instance.squares = new List<Square>();

            currentField = new Field();
            currentField.columnsCount = columnsCount;
            currentField.rowsCount = rowsCount;

            int counter = 0;

            //  Generate sprites and squares for new field
            for (int i = 0; i < currentField.rowsCount; i++)
            {
                for (int j = 0; j < currentField.columnsCount; j++)
                {
                    Sprite newFieldSprite = Sprite.Create(fieldTexture, new Rect(0, 0, 256, 256), new Vector2(0f, 1f), 256f);
                    newFieldSprite.name = "fieldSprite_" + counter;
                    GameObject newFieldSquare = new GameObject();
                    newFieldSquare.transform.parent = fieldRoot.transform;
                    newFieldSquare.name = counter.ToString();
                    newFieldSquare.AddComponent<SpriteRenderer>();
                    newFieldSquare.GetComponent<SpriteRenderer>().sprite = newFieldSprite;
                    newFieldSquare.transform.position = new Vector2(j-columnsCount/2, -i+rowsCount/2);
                    newFieldSquare.AddComponent<SquareController>();
                    newFieldSquare.GetComponent<SquareController>().InitSquare(counter.ToString(),i,j);

                    //  Adding square collider

                    newFieldSquare.AddComponent<BoxCollider2D>();
                    newFieldSquare.tag = "SquareCollider";
                    newFieldSquare.layer = 8;
                    fieldSquares[counter] = newFieldSquare;

                    //  Adding square attaching point



                    counter++;
                }
            }

            //  Setting scale for the field

            fieldRoot.transform.localScale = new Vector3(scale, scale, scale);
            DataObject.instance.currentFieldScale = scale;
            currentState = FieldState.Ready;

            DataObject.instance.fieldDimensions = new int[2];
            DataObject.instance.fieldDimensions[0] = rowsCount;
            DataObject.instance.fieldDimensions[1] = columnsCount;
        }


    }

}
