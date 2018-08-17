using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    private enum MoveTo
    {
        Up = 1,
        Down,
        Left,
        Right,
        None = 0
    }

    public float fSpeed = 10.0f;
    public Rigidbody rigidbody;
    private MoveTo moveIndex = MoveTo.None;
	// Use this for initialization
	void Start () {
        Debug.Log(Screen.width + "-" + Screen.height );
	}
	
	// Update is called once per frame
	void Update ()
    {
        PlayerMove();
    }

    private void PlayerMove()
    {
        switch (moveIndex)
        {
            case MoveTo.Up:
                rigidbody.MovePosition(this.transform.position + Vector3.up * fSpeed * Time.deltaTime);
                break;
            case MoveTo.Down:
                rigidbody.MovePosition(this.transform.position + Vector3.down * fSpeed * Time.deltaTime);
                break;
            case MoveTo.Left:
                rigidbody.MovePosition(this.transform.position + Vector3.left * fSpeed * Time.deltaTime);
                break;
            case MoveTo.Right:
                rigidbody.MovePosition(this.transform.position + Vector3.right * fSpeed * Time.deltaTime);
                break;
            case MoveTo.None:
                break;
            default:
                break;
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height - 300, 200, 100), "UP"))
        {
            moveIndex = MoveTo.Up;
        }
        if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height - 100, 200, 100), "Down"))
        {
            moveIndex = MoveTo.Down;
        }
        if (GUI.Button(new Rect(Screen.width / 2 - 300, Screen.height - 200, 200, 100), "Left"))
        {
            moveIndex = MoveTo.Left;
        }
        if (GUI.Button(new Rect(Screen.width / 2 + 100, Screen.height - 200, 200, 100), "Right"))
        {
            moveIndex = MoveTo.Right;
        }
    }

}
