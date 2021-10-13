using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine;
using System.Globalization;

public class player : MonoBehaviour
{
    // Start is called before the first frame update

    bool jumpKeyPressed = false;
    public Transform groundCheckTransform;
    public Transform bone;
    public Transform spine;

    float horizontalInput;
    float verticalInput;
    float jumpInput;

    float[,] vals = new float[2, 4];
    Rigidbody mainPlayer;

    public const int port = 9022;
    UdpClient client;
    Thread networkThread;
    void Start()
    {
        mainPlayer = GetComponent<Rigidbody>();
        bone = GameObject.Find("Head").transform;
        spine = GameObject.Find("Spine").transform;


        client = new UdpClient();
        Array.Clear(vals, 0, 2);

        networkThread = new Thread(new ThreadStart(GetNetData));
        networkThread.IsBackground = true;
        networkThread.Start();
    }



    void GetNetData()
    {
        client = new UdpClient(port);
        client.Client.Blocking = false;
        client.Client.ReceiveTimeout = 1000;

        while (true)
        {
            try
            {
                // receive bytes
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);

                // encode UTF8-coded bytes to text format
                int NUMBER_OF_DEVICES = 2;
                int DATA_POINTS = 4;

                string text = Encoding.UTF8.GetString(data);

                // float[,] vals = new float[NUMBER_OF_DEVICES, DATA_POINTS];
                string[] devices = text.Split(':');




                for (var dev = 0; dev < NUMBER_OF_DEVICES; dev++)
                {
                    string[] single_device = devices[dev].Split(',');


                    for (var val = 0; val < DATA_POINTS; val++)
                    {
                        vals[dev, val] = float.Parse(single_device[val], System.Globalization.CultureInfo.InvariantCulture);
                    }
                }


            }
            catch (Exception err)
            {
                err.ToString();
                // Debug.Log("Error");
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpKeyPressed = true;
        }

        jumpInput = Input.GetAxis("Jump");
        horizontalInput = Input.GetAxis("Horizontal") * 1.5f;
        verticalInput = Input.GetAxis("Vertical");

        // if (Input.GetKeyDown(KeyCode.W))
        // {
        //     bone.rotation = new Quaternion(bone.rotation.x - 0.1f, bone.rotation.y, bone.rotation.z, bone.rotation.w);
        // }

        // if (Input.GetKeyDown(KeyCode.S))
        // {
        //     bone.rotation = new Quaternion(bone.rotation.x + 0.1f, bone.rotation.y, bone.rotation.z, bone.rotation.w);
        // }

        bone.rotation = new Quaternion(vals[0, 0], vals[0, 1], vals[0, 2], vals[0, 3]);
        spine.rotation = new Quaternion(vals[1, 0], vals[1, 1], vals[1, 2], vals[1, 3]);


    }

    void FixedUpdate()
    {
        mainPlayer.velocity = new Vector3(horizontalInput, mainPlayer.velocity.y, 0);
        if (Physics.OverlapSphere(groundCheckTransform.position, 0.1f).Length <= 1)
        {
            // Debug.Log("Jumps zerod: " + jumps.ToString());
            return;
        }
        if (jumpKeyPressed)
        {
            mainPlayer.AddForce(Vector3.up * 7, ForceMode.VelocityChange);
            jumpKeyPressed = false;
        }
    }

    private void onTriggerEnter(Collision other)
    {
        Debug.Log("Craash");
        if (other.gameObject.name == "coin")
        {
            Destroy(other.gameObject);

        }
    }

}

