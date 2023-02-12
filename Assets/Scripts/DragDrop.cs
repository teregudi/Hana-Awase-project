using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragDrop : MonoBehaviour
{
    public GameObject Canvas;
    private bool isDragging = false;
    private Vector2 startPosition;
    private bool isOverMiddleArea = false;
    public GameObject MiddleArea;
    public GameObject PlayerArea;
    private GameObject startParent;
    public GameEngine GLS;

    // Start is called before the first frame update
    void Start()
    {
        GLS = GameEngine.GetGameEngine();
    }

    private void Awake()
    {
        Canvas = GameObject.Find("TT Canvas");
        MiddleArea = GameObject.Find("MiddleArea");
    }

    // Update is called once per frame
    void Update()
    {
        if (isDragging)
        {
            transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            transform.SetParent(Canvas.transform, true);
        }
    }

    public void StartDrag()
    {
        startParent = transform.parent.gameObject;
        startPosition = transform.position;
        if (this.transform.parent.name == PlayerArea.name)
        {
            isDragging = true;
        } 
    }

    public void EndDrag()
    {
        if (isDragging && isOverMiddleArea)
        {
            isDragging = false;
            transform.SetParent(MiddleArea.transform, false);

            GridLayoutGroup glg = MiddleArea.GetComponent<GridLayoutGroup>();
            glg.spacing.Set(20, 40);

            //List<int> cardsInMiddle = new List<int>();
            //foreach (Transform child in middleArea.transform)
            //{
            //    cardsInMiddle.Add(Int32.Parse(child.name) / 10);
            //}
            //foreach (int card in cardsInMiddle)
            //{
            //    if (card == thisMonth)
            //    {
            //        match = true;
            //        break;
            //    }
            //}
        }
        else
        {
            transform.position = startPosition;
            transform.SetParent(startParent.transform, false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        isOverMiddleArea = true;
        MiddleArea = collision.gameObject;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isOverMiddleArea = false;
        MiddleArea = null;
    }
}
