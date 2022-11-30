using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    float motorMultiplier = 500f;
    [SerializeField] float fitness;
    bool initialized = false;
    Transform goal;

    private NeuralNetwork net;

    Transform torso;
    Transform leftThigh;
    Transform rightThigh;
    Transform leftShin;
    Transform rightShin;
    Transform leftFoot;
    Transform rightFoot;

    HingeJoint2D leftThighHinge;
    HingeJoint2D rightThighHinge;
    HingeJoint2D leftShinHinge;
    HingeJoint2D rightShinHinge;
    HingeJoint2D leftFootHinge;
    HingeJoint2D rightFootHinge;

    JointMotor2D leftThighMotor;
    JointMotor2D rightThighMotor;
    JointMotor2D leftShinMotor;
    JointMotor2D rightShinMotor;
    JointMotor2D leftFootMotor;
    JointMotor2D rightFootMotor;

    // 14 input nodes - the rotation and speed of the torso and each part of each leg.

    
    private void Start()
    {
        // get the transform of each body part for later use
        torso = gameObject.transform.GetChild(0).GetChild(0);
        leftThigh = gameObject.transform.GetChild(0).GetChild(0).GetChild(0);
        rightThigh = gameObject.transform.GetChild(0).GetChild(0).GetChild(1);
        leftShin = gameObject.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0);
        rightShin = gameObject.transform.GetChild(0).GetChild(0).GetChild(1).GetChild(0);
        leftFoot = gameObject.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0);
        rightFoot = gameObject.transform.GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetChild(0);

        // get the hinge joints of all body parts with hinge joints on them
        leftThighHinge = leftThigh.GetComponent<HingeJoint2D>();
        rightThighHinge = rightThigh.GetComponent<HingeJoint2D>();
        leftShinHinge = leftShin.GetComponent<HingeJoint2D>();
        rightShinHinge = rightShin.GetComponent<HingeJoint2D>();
        leftFootHinge = leftFoot.GetComponent<HingeJoint2D>();
        rightFootHinge = rightFoot.GetComponent<HingeJoint2D>();

        // get the motors of all hinge joints 
        leftThighMotor = leftThighHinge.motor;
        rightThighMotor = rightThighHinge.motor;
        leftShinMotor = leftShinHinge.motor;
        rightShinMotor = rightShinHinge.motor;
        leftFootMotor = leftFootHinge.motor;
        rightFootMotor = rightFootHinge.motor;
    }

    private void FixedUpdate()
    {
        if (initialized)
        {
            // float array will store the values of the input nodes
            float[] inputs = new float[14];

            // assign inputs

            // get rotations of each body part and assign them to the appropriate input node
            inputs[0] = torso.transform.localRotation.eulerAngles.z;
            inputs[2] = leftThigh.transform.localRotation.eulerAngles.z;
            inputs[4] = rightThigh.transform.localRotation.eulerAngles.z;
            inputs[6] = leftShin.transform.localRotation.eulerAngles.z;
            inputs[8] = rightShin.transform.localRotation.eulerAngles.z;
            inputs[10] = leftFoot.transform.localRotation.eulerAngles.z;
            inputs[12] = rightFoot.transform.localRotation.eulerAngles.z;

            // assign speed of each body part to the appropriate input node
            inputs[1] = torso.GetComponent<Rigidbody2D>().velocity.magnitude;
            inputs[3] = leftThigh.GetComponent<Rigidbody2D>().velocity.magnitude;
            inputs[5] = rightThigh.GetComponent<Rigidbody2D>().velocity.magnitude;
            inputs[7] = leftShin.GetComponent<Rigidbody2D>().velocity.magnitude;
            inputs[9] = rightShin.GetComponent<Rigidbody2D>().velocity.magnitude;
            inputs[11] = leftFoot.GetComponent<Rigidbody2D>().velocity.magnitude;
            inputs[13] = rightFoot.GetComponent<Rigidbody2D>().velocity.magnitude;

            // float array will store the values of the output nodes
            float[] outputs = new float[6];

            // passing the inputs into the neural network and getting the result in the outputs array
            outputs = net.FeedForward(inputs);


            // assigning each of the output values to the appropriate motor to move the players legs
            leftThighMotor.motorSpeed = outputs[0] * motorMultiplier;
            leftThighHinge.motor = leftThighMotor;

            rightThighMotor.motorSpeed = outputs[1] * motorMultiplier;
            rightThighHinge.motor = rightThighMotor;

            leftShinMotor.motorSpeed = outputs[2] * motorMultiplier;
            leftShinHinge.motor = leftShinMotor;

            rightShinMotor.motorSpeed = outputs[3] * motorMultiplier;
            rightShinHinge.motor = rightShinMotor;

            leftFootMotor.motorSpeed = outputs[4] * motorMultiplier;
            leftFootHinge.motor = leftFootMotor;

            rightFootMotor.motorSpeed = outputs[5] * motorMultiplier;
            rightFootHinge.motor = rightFootMotor;

            // distance between the player and the goal is used to calculate fitness
            fitness = 300 - Vector2.Distance(torso.transform.position, goal.transform.position);

            //print("fitness " + fitness);
            //print("torso rot " + torso.transform.localRotation.eulerAngles.z);
            //print("torso speed " + torso.GetComponent<Rigidbody2D>().velocity.magnitude);
            net.SetFitness(fitness);
        }
    }

    //IEnumerator CalculateSpeed(Transform bodyPart)
    //{
    //    Vector3 lastPosition = bodyPart.position;
    //    yield return new WaitForFixedUpdate();
    //    string name = nameof(bodyPart);
    //    (name + "speed") = (lastPosition - bodyPart.position).magnitude / Time.deltaTime;
    //}

    public void Init(NeuralNetwork net, Transform goal)
    {
        this.net = net;
        this.goal = goal;
        initialized = true;
    }
}
