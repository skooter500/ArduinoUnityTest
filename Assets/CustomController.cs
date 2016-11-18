using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.Threading;

public class CustomController : MonoBehaviour
{
    private SerialPort controller;
    private string messageFromController;
    private bool runThread = true;

    public bool Connected = false;
    public string portName = "COM10";

    public volatile float pot;
    public volatile bool button;

    float scale = 1.0f;

    // Use this for initialization
    void Start()
    {
        LookForController();

        // create the thread
        runThread = true;
        Thread ThreadForController = new Thread(new ThreadStart(ThreadWorker));
        ThreadForController.Start();

    }

    public float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    // Update is called once per frame
    void Update()
    {
        float angle = map(pot, 0, 1024, 360.0f, 0);
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
        if (button)
        {
            GetComponent<Rigidbody>().AddForce(transform.forward * 2);
        }
    }

    void ProcessMessage(string message)
    {
        string[] decoded = message.Split(':');
        float value = float.Parse(decoded[1]);
        switch (decoded[0])
        {
            case "P":
                pot = value;
                break;
            case "B":
                button = (value == 1);
                break;
        }

    }

    void ThreadWorker()
    {
        while (runThread)
        {
            if (controller != null && controller.IsOpen)
            {
                try
                {
                    messageFromController = controller.ReadLine();
                    ProcessMessage(messageFromController);
                }
                catch (System.Exception) { }
            }
            else
            {
                Thread.Sleep(50);
            }
        }
    }


    void OnApplicationQuit()
    {
        controller.Close();
        runThread = false;
    }

    public void LookForController()
    {
        string[] ports = SerialPort.GetPortNames();
        Debug.Log(ports.Length);

        if (ports.Length == 0)
        {
            Debug.Log("No controller detected");
        }
        else
        {
            Debug.Log("Ports: " + string.Join(", ", ports));
            portName = "\\\\.\\" + ports[ports.Length - 1];
            Debug.Log("Port Name: " + portName);
            Connected = true;

            // check the default port
            controller = new SerialPort(portName, 9600);
            controller.ReadTimeout = 100;
            controller.Open();
        }
    }
}
