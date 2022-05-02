using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
 
public class solverAgent : Agent
{
    [SerializeField] private Vector3 initPos;
    [SerializeField] private Vector2 pos;
    Rigidbody rBody;
    // Start is called before the first frame update
    GridManager gridManager;
    // private Block currentBlock;
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        gridManager = GridManager.Instance;
        // currentBlock = null;
        pos = new Vector2(4, 4);
    }

    public override void OnEpisodeBegin() {
        // if the agent fell, zero its momentum
        // if (this.transform.localPosition.y < 0)
        // {
        //     this.rBody.angularVelocity = Vector3.zero;
        //     this.rBody.velocity = Vector3.zero;
        // }
        
        // reset agent's position
        // this.transform.localPosition = initPos;
        this.transform.localPosition = new Vector3(pos.x, 0.5f, pos.y);
        pos = new Vector2(4, 4);
        // reset the sudoku
        gridManager.resetPuzzle();
    }
    public override void CollectObservations(VectorSensor sensor) {
        // Agent positions and velocity
        // sensor.AddObservation(this.transform.localPosition);
        // sensor.AddObservation(rBody.velocity.x);
        // sensor.AddObservation(rBody.velocity.z);
        sensor.AddObservation(pos);
        // add current sudoku state
        foreach (int state in gridManager.GetCurrentState()) {
            for(int i = 0; i <= 10; i++) {
                if (state == i) {
                    sensor.AddObservation(1);
                }
                else {
                    sensor.AddObservation(0);
                }
            }
        }
        // add each int individually (?)
    }
    
    // private void OnTriggerEnter(Collider other) {
    //     if (other.gameObject.tag == "Block") {
    //         currentBlock = other.GetComponent<Block>();
    //     }
    // }
    public float forceMultiplier = 10;
    public override void OnActionReceived(ActionBuffers actionBuffers) {
        // Actions, size = 2
        
        // Vector3 controlSignal = Vector3.zero;
        // controlSignal.x = actionBuffers.ContinuousActions[0];
        // controlSignal.z = actionBuffers.ContinuousActions[1];
        // rBody.AddForce(controlSignal * forceMultiplier);
    
        var isComplete = true;
        var action = actionBuffers.DiscreteActions[0];
        // move or click number
        if (action <= 3) {
            var movement = action;
            if (movement == 0) { pos.x++; } // up
            if (movement == 1) { pos.x--; } // down
            if (movement == 2) { pos.y++; } // right
            if (movement == 3) { pos.y--; } //left
        }
        else {
            var clickAction = action-3;
            // click a number at current position
            // if (currentBlock != null) {
            //     var Pos = currentBlock.GetBlockPos();
            // }
            Debug.Log($"click number {clickAction} at pos {pos.x} {pos.y}");
            isComplete = gridManager.ClickNumberAtPosition(pos, clickAction);
        }
        
        // Rewards
        if (!isComplete) {
            SetReward(-1.0f);
            // incomplete solution
            Debug.Log("incomplete!");
            
            EndEpisode();
            return;
        }
        // Puzzle Completed
        if (gridManager.isPuzzleComplete()) {
            SetReward(1.0f);
            Debug.Log("complete!");
            EndEpisode();
            return;
        }
        // Fell off platform
        if (pos.x > 8 || pos.y > 8 || pos.x < 0 || pos.y < 0) {
            SetReward(-0.01f);
            pos = new Vector2(Mathf.Clamp(pos.x, 0, 8), Mathf.Clamp(pos.y, 0, 8));
            // EndEpisode();
            // return; 
        }
        this.transform.position = new Vector3(pos.x, 0.5f, pos.y);
        
        // -0.0005 for every step.
        SetReward(-0.005f);
        
        // click a number
        // 1. not take an action to click a number 
        // 2. take an action to click a number, but 
        // 2.1 it has no effect (the number already click in that position)
        // 2.2 after the number click it leads to an incomplete solution

        
    }
    
    public override void Heuristic(in ActionBuffers actionsOut) {
        // var continuousActionsOut = actionsOut.ContinuousActions;
        // continuousActionsOut[0] = Input.GetAxis("Horizontal");
        // continuousActionsOut[1] = Input.GetAxis("Vertical");
        
        var DiscreteActionsOut = actionsOut.DiscreteActions;
        DiscreteActionsOut[0] = 0;
        if(Input.inputString != ""){
            int number;
            bool is_a_number = int.TryParse(Input.inputString, out number);
            if (is_a_number && number >= 0 && number < 10){
                DiscreteActionsOut[0] = number;
            }
        }
        DiscreteActionsOut[1] = 0;
        // if (Input.GetKeyDown(KeyCode.UpArrow)) {
        //     DiscreteActionsOut[1] = 2;
        // }
        // else if (Input.GetKeyDown(KeyCode.DownArrow)) {
        //     DiscreteActionsOut[1] = 1;
        // }
        // else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
        //     DiscreteActionsOut[1] = 4;
        // }
        // else if (Input.GetKeyDown(KeyCode.RightArrow)) {
        //     DiscreteActionsOut[1] = 3;
        // }
        
        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0) {
            DiscreteActionsOut[1] = 0;
        }
        else {
            DiscreteActionsOut[1] = Input.GetAxis("Horizontal") > 0 ? 2 : 1;
            DiscreteActionsOut[1] = Input.GetAxis("Vertical") > 0 ? 4 : 3;
        }
        Debug.Log($"action : {DiscreteActionsOut[1]}");
    }
}
