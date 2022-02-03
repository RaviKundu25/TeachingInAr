using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollLayout : LayoutGroup
{
    public enum FitType
    {
        Uniform,
        Width,
        Height,
        FixedRows,
        FixedCols
    }

    public FitType fitType;
    public int rows;
    public int cols;
    public Vector2 cellSize;
    public Vector2 spacing;
    public bool fitX;
    public bool fitY;

    private void Update()
    {
        CalculateLayoutInputHorizontal();
    }

    public override void CalculateLayoutInputHorizontal()
    {
        if(transform.childCount != 3)
        {
            if (Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
            {
                if (transform.childCount == 2)
                {
                    fitType = FitType.FixedCols;
                    cols = 1;
                }
            }
            else
            {
                if (transform.childCount == 2)
                {
                    fitType = FitType.FixedCols;
                    cols = 2;
                }
            }

            if (transform.childCount == 1)
            {
                cols = 1;
            }
            if (transform.childCount == 4)
            {
                cols = 2;
            }
        }
        

        base.CalculateLayoutInputHorizontal();

        if(transform.childCount == 3)
        {
            if (Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
                threeUserP();
            else
                threeUserL();
            return;
        }

        if(fitType == FitType.Width|| fitType == FitType.Height || fitType == FitType.Uniform)
        {
            fitX = true;
            fitY = true;
            float sqrRt = Mathf.Sqrt(transform.childCount);
            rows = Mathf.CeilToInt(sqrRt);
            cols = Mathf.CeilToInt(sqrRt);
        }

        if(fitType == FitType.Width || fitType == FitType.FixedCols)
        {
            rows = Mathf.CeilToInt(transform.childCount / (float)cols);
        }
        if (fitType == FitType.Height || fitType == FitType.FixedRows)
        {
            cols = Mathf.CeilToInt(transform.childCount / (float)rows);
        }

        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;

        float cellWidth = (parentWidth / (float)cols) - ((spacing.x / (float)cols) * 2) - (padding.left / (float)cols) - (padding.right / (float)cols);
        float cellHeight = (parentHeight / (float)rows) - ((spacing.y / (float)rows) * 2) - (padding.top / (float)rows) - (padding.bottom / (float)rows);

        cellSize.x = fitX? cellWidth : cellSize.x;
        cellSize.y = fitY? cellHeight : cellSize.y;

        int rowCount = 0;
        int colCount = 0;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            rowCount = i / cols;
            colCount = i % cols;

            var item = rectChildren[i];

            var xPos = (cellSize.x * colCount) + (spacing.x * colCount) + padding.left;
            var yPos = (cellSize.y * rowCount) + (spacing.y * rowCount) + padding.top;

            SetChildAlongAxis(item, 0, xPos, cellSize.x);
            SetChildAlongAxis(item, 1, yPos, cellSize.y);
        }

    }

    void threeUserP()
    {
        fitType = FitType.FixedCols;

        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;

        var item01 = rectChildren[0];

        float cellWidth01 = parentWidth / 2;
        float cellHeight01 = parentHeight / 2;

        cellSize.x = cellWidth01;
        cellSize.y = cellHeight01;

        var xPos01 = 0;
        var yPos01 = 0;

        SetChildAlongAxis(item01, 0, xPos01, cellSize.x);
        SetChildAlongAxis(item01, 1, yPos01, cellSize.y);

        var item02 = rectChildren[1];

        float cellWidth02 = parentWidth / 2;
        float cellHeight02 = parentHeight / 2;

        cellSize.x = cellWidth02;
        cellSize.y = cellHeight02;

        var xPos02 = cellWidth02;
        var yPos02 = 0;

        SetChildAlongAxis(item02, 0, xPos02, cellSize.x);
        SetChildAlongAxis(item02, 1, yPos02, cellSize.y);

        var item03 = rectChildren[2];

        float cellWidth03 = parentWidth;
        float cellHeight03 = parentHeight / 2;

        cellSize.x = cellWidth03;
        cellSize.y = cellHeight03;

        var xPos03 = 0;
        var yPos03 = cellHeight03;

        SetChildAlongAxis(item03, 0, xPos03, cellSize.x);
        SetChildAlongAxis(item03, 1, yPos03, cellSize.y);
    }

    void threeUserL()
    {
        fitType = FitType.FixedCols;

        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;

        var item01 = rectChildren[0];

        float cellWidth01 = parentWidth / 2;
        float cellHeight01 = parentHeight / 2;

        cellSize.x = cellWidth01;
        cellSize.y = cellHeight01;

        var xPos01 = 0;
        var yPos01 = 0;

        SetChildAlongAxis(item01, 0, xPos01, cellSize.x);
        SetChildAlongAxis(item01, 1, yPos01, cellSize.y);

        var item02 = rectChildren[1];

        float cellWidth02 = parentWidth / 2;
        float cellHeight02 = parentHeight / 2;

        cellSize.x = cellWidth02;
        cellSize.y = cellHeight02;

        var xPos02 = 0;
        var yPos02 = cellHeight02;

        SetChildAlongAxis(item02, 0, xPos02, cellSize.x);
        SetChildAlongAxis(item02, 1, yPos02, cellSize.y);

        var item03 = rectChildren[2];

        float cellWidth03 = parentWidth / 2;
        float cellHeight03 = parentHeight;

        cellSize.x = cellWidth03;
        cellSize.y = cellHeight03;

        var xPos03 = cellWidth03;
        var yPos03 = 0;

        SetChildAlongAxis(item03, 0, xPos03, cellSize.x);
        SetChildAlongAxis(item03, 1, yPos03, cellSize.y);
    }

    public override void CalculateLayoutInputVertical()
    {
        
    }

    public override void SetLayoutHorizontal()
    {
        
    }

    public override void SetLayoutVertical()
    {
        
    }

}
