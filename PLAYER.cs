using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]
public class PLAYER : MonoBehaviour
{
    [SerializeField]
    private float Run_speed = 0;

    [SerializeField]
    private float turn_speed = 0;

    private CharacterController controller;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()//instaize
    {
            controller = GetComponent <CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        runing();

    }

    public void runing()
    {
        //semua yg dikasik guru itu pakai sistem lama 
        //jadi ada perubahan
        float Run = 0;
        float angle = 0;

        if (Keyboard.current.aKey.isPressed)
        {
            angle = 1;
            
        }

        if (Keyboard.current.dKey.isPressed)
        {
            angle = -1;
           
        }

        if (Keyboard.current.kKey.isPressed)
        {
            Run += 0.1f;
            if (Run >= 1)
            {
                Run = 1;
            }
        }
        else if (Keyboard.current.lKey.isPressed)
        {
            Run -= 0.1f;
            if (Run <= -1)
            {
                Run = -1;
                //testing
            }
        }
        else Run = 0;

        transform.Rotate(0, angle * turn_speed * Time.deltaTime, 0);

        //Vector3 move;
        //move.x = horizontal * Run_speed;
        //move.y = 0;
        //move.z = Run_speed * Run;
        Vector3 move = transform.forward * (Run_speed * Run);
        controller.Move(move * Time.deltaTime);

    }

    
}
