using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    private CharacterController controller;

    public void Initialize()
    {
        GameObject cube = GameObject.Find("Cube");
        controller = GetComponent<CharacterController>();
        speed = 5.0f;

        if (cube != null)
        {
            cube.AddComponent<Player>();
        }

        cube.transform.Translate(0, 0, 0);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        //controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    public void Update()
    {
        float x = 0f;
        float y = 0f;

        // 방향키 입력
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            x = -1f;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            x = 1f;
        }

        if ( Input.GetKey(KeyCode.UpArrow)) 
        {
            y = 1f;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            y = -1f;
        }

        Vector3 move = new Vector3(x, y, 0);
        //controller.Move(move * speed * Time.deltaTime);
    }
}
