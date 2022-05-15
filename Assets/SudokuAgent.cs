using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;

public class SudokuAgent : Agent
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float moveStep = 1f;
    [SerializeField] Transform movePoint;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] Vector3 initPos;
    [SerializeField] Vector2 initIdx;
    private Vector2 idx;
    bool facingRight = true;
    BoardManager boardManager;
    [SerializeField] Animator animator;
    
    [SerializeField] bool isRecord;
    Dictionary<Vector2, int> answersCopy;
    Vector2 answerIdx;
    int answerValue;
    public int minKnownNumbers;
    
    [SerializeField] private GameObject smokeEffect;
    
    void Update() {
        float movementAmout = moveSpeed * Time.deltaTime;
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, movePoint.localPosition, movementAmout);
    }

    void flip() {
        Vector3 currentScale = gameObject.transform.localScale;
        currentScale.x *= -1;
        gameObject.transform.localScale = currentScale;
        facingRight = !facingRight;
        animator.SetTrigger("doTouch");
    }
    
    private void Awake() {
        boardManager = gameObject.GetComponentInParent<BoardManager>();
    }
    // ML-agent
    public override void OnEpisodeBegin() {
        minKnownNumbers = (int) Academy.Instance.EnvironmentParameters.GetWithDefault("min_known_numbers", minKnownNumbers);
        // reset the sudoku
        boardManager.minKnownNumbers = minKnownNumbers;
        boardManager.init();
        reset();
    }    
    public void reset() {
        movePoint.parent = transform.parent;
        transform.localPosition = initPos;
        movePoint.localPosition = initPos;
        idx = initIdx;
        answersCopy = new Dictionary<Vector2, int>(boardManager.answers);
        answerIdx = new Vector2(-1, -1);
        answerValue = -1;
        
    }
    
    public override void CollectObservations(VectorSensor sensor) {
        // Agent positions
        sensor.AddObservation(idx);
        // Debug.Log($"current idx {idx.x} {idx.y}");
        // number left
        sensor.AddObservation(boardManager.numberLeft);
        // add current sudoku state
        foreach (var state in boardManager.GetCurrentState()) {
            var isClicked = state[0] == 1 ? true : false;
            // var isNumberLeft = state[1] == 1 ? true : false;
            sensor.AddObservation(isClicked);
            // sensor.AddObservation(isNumberLeft);
            // the number that is clickable in that position
            for(int i = 2; i < 11; i++) {
                sensor.AddObservation(state[i]);
            }
            // the clicked number in that position
            // one-hot encoding
            for(int i = 0; i < 10; i++) {
                sensor.AddObservation(state[11] == i ? 1 : 0);
            }
            // sensor.AddObservation(state[12]);
            // sensor.AddObservation(state[13]);
            
            // foreach (var obs in state) {
            //     sensor.AddObservation(obs);
            // }
        }
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers) {
        // action
        // 0 - move up
        // 1 - move down
        // 2 - move left
        // 3 - move right
        // 4 - 12 click number 1-9 at current position
        
        var action = actionBuffers.DiscreteActions[0];
        var stepPenalty = -0.001f;
        
        // move or click number
        if (action <= 3) {
            var movement = action;
            var hor = 0;
            var ver = 0;
            switch(movement) {
                case 0: ver = 1;  break;
                case 1: ver = -1; break;
                case 2: hor = -1; break;
                case 3: hor = 1;  break;
                default: break;
            }
            
            if (!move(hor, ver)) {
                // encourage the agent to not hit the boarder
                SetReward(-0.01f);
                // SetReward(-1.0f);
                // EndEpisode();
                // return;
            }
            // encourage the agent to move less
            // Debug.Log("choose to move");
            AddReward(stepPenalty);
        }
        else {
            if (smokeEffect.activeSelf) {
                smokeEffect.SetActive(false);
            }
            var clickAction = action-3;
            // Debug.Log($"click number {clickAction} at idx {idx.x} {idx.y}");
            // var res = boardManager.ClickNumberAtPositionSafe(idx, clickAction);
            var res = boardManager.ClickNumberAtPosition(idx, clickAction);
            
            // 0 - no effect
            // 1 - click a number and it leads to a complete solution
            // 2 - click a number and it leads to an incomplete solution
            
            
            if (res != 0) {
                // check if action is in the answer
                if (boardManager.answers.TryGetValue(idx, out int tmp) && tmp == clickAction) {
                    // same as answer!
                    // Debug.Log("in the answer");
                    AddReward(1.0f);
                }
                else {
                    // not in the answer
                    // Debug.Log("not in the answer");
                    AddReward(stepPenalty);
                }
            }
            
            animator.SetTrigger("doTouch");
            if (res == 0) {
                // encourage the agent to do action that has any effect
                // Debug.Log("action has no effect");
                AddReward(stepPenalty);
            }
            else if (res == 1) {
                // encourage the agent to find a complete solution
                // Debug.Log($"lead to a complete solution");
                
                // Get 1.0f reward after finish the puzzle
                AddReward(1/(9*9 - boardManager.knownNumbers));
                // AddReward(0.1f);
            }
            else if (res == 2) {
                // encourage the agent to find a complete solution
                // Debug.Log($"Find an incomplete solution");
                AddReward(-0.01f);
                
                // we can perform a safe action to avoid early episode end
                // SetReward(-1.0f);
                // EndEpisode();
                // return;
            }
        }
        
        // Puzzle Completed
        if (boardManager.isPuzzleComplete()) {
            // AddReward(9*9-boardManager.knownNumbers);
            SetReward(1.0f);
            Debug.Log($"Find a solution for knownNumbers={boardManager.knownNumbers}");
            EndEpisode();
        }
    }
    
    // move the agent, and check if the move hit the border
    bool move(float hor, float ver) {
        bool res = true;
        if (Vector3.Distance(transform.localPosition, movePoint.localPosition) <= 0.05f) {
            if (Mathf.Abs(hor) == 1f) {
                Vector3 newPosition = movePoint.localPosition + new Vector3(hor*moveStep, 0, 0);
                if (idx.y+hor >= 0 && idx.y+hor <= 8) {
                    movePoint.localPosition = newPosition;
                    idx.y += hor;
                    // Debug.Log($"Move to {idx.x} {idx.y}");
                }
                else {
                    // try to go over border
                    res = false;
                    // Debug.Log($"try to go over border");
                    if (!smokeEffect.activeSelf) {
                        smokeEffect.SetActive(true);
                    }
                }
            }
            else if (Mathf.Abs(ver) == 1f) {
                Vector3 newPosition = movePoint.localPosition + new Vector3(0, ver*moveStep, 0);
                if (idx.x-ver >= 0 && idx.x-ver <= 8) {
                    movePoint.localPosition = newPosition;
                    idx.x -= ver;
                    // Debug.Log($"Move to {idx.x} {idx.y}");
                }
                else {
                    // try to go over border
                    res = false;
                    // Debug.Log($"try to go over border");
                    if (!smokeEffect.activeSelf) {
                        smokeEffect.SetActive(true);
                    }
                }
            }
            animator.SetBool("isWalk", false);
        }
        else {
            animator.SetBool("isWalk", true);
            if (smokeEffect.activeSelf) {
                smokeEffect.SetActive(false);
            }
        }
        
        // flip agent
        if (hor > 0 && !facingRight) {
            flip();
        }
        else if (hor < 0 && facingRight) {
            flip();
        }
        
        return res;
    }
    
    
    // action
    // 0 - move up
    // 1 - move down
    // 2 - move left
    // 3 - move right
    // 4 - 12 click number 1-9 at current position
    public override void Heuristic(in ActionBuffers actionsOut) {
        var DiscreteActionsOut = actionsOut.DiscreteActions;
        // move to the answer position and click it
        if (isRecord) {
            if (answersCopy.Count > 0 && answerIdx.x == -1) {
                // pick a answer randomly
                var tmp = answersCopy.ElementAt(Random.Range(0, answersCopy.Count));
                answerIdx = tmp.Key;
                answerValue = tmp.Value;
                // pick a position with least choice and closest
                var leastChoice = 9;
                var choices = new List<(Vector2, int)>();
                foreach (var ans in answersCopy) {
                    var choice = boardManager.choiceLeft(ans.Key);
                    if (choice > 0 && choice <= leastChoice) {
                        if (choice != leastChoice) {
                            choices.Clear();
                        }
                        choices.Add((ans.Key, ans.Value));
                        // answerIdx = ans.Key;
                        // answerValue = ans.Value;
                        leastChoice = choice;
                    }
                }
                var closestDistance = 1000.0f;
                // pick the closest
                foreach(var ans in choices) {
                    var distance = Vector2.Distance(ans.Item1, idx);
                    if (distance < closestDistance) {
                        answerIdx = ans.Item1;
                        answerValue = ans.Item2;
                        closestDistance = distance;
                    }
                }
                
                // Debug.Log($"Try to move to {answerIdx.x} {answerIdx.y} and click number {answerValue}");
            } 
            if (idx != answerIdx) {
                // moves to that idx
                if (idx.x != answerIdx.x) {
                    // move vertical
                    DiscreteActionsOut[0] = idx.x < answerIdx.x ? 1 : 0;
                }
                else if (idx.y != answerIdx.y) {
                    // move horizontal
                    DiscreteActionsOut[0] = idx.y < answerIdx.y ? 3 : 2;
                }
            }
            else {
                // already at that position (action idx == number to click + 3)
                DiscreteActionsOut[0] = answerValue+3;
                // remove it from the answer
                answersCopy.Remove(answerIdx);
                answerIdx = new Vector2(-1, -1);
                answerValue = -1;
            }
            return;
        }
    
        
        if (Mathf.Abs(Input.GetAxis("Horizontal")) == 1f) {
            DiscreteActionsOut[0] = Input.GetAxis("Horizontal") > 0 ? 3 : 2;
        }
        else if (Mathf.Abs(Input.GetAxis("Vertical")) == 1f) {
            DiscreteActionsOut[0] = Input.GetAxis("Vertical") > 0 ? 0 : 1;
        }
        else {
            DiscreteActionsOut[0] = Random.Range(4, 13);
        }
    }
    
}
