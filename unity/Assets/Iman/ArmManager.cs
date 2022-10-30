using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityStandardAssets.Characters.FirstPerson;
 
public class ArmManager : MonoBehaviour
{
   [SerializeField] private XRBaseController _xrController;
   [SerializeField] private int _maxResetCount = 100;
   [SerializeField] private float _hapticAmplitude = 0.7f;
   [SerializeField] private float _hapticDuration = 0.5f;
 
   [SerializeField]
   [Tooltip("The Input System Action that will be used to read Snap Move data from the left hand controller. Must be a Value Vector2 Control.")]
   InputActionProperty m_LeftHandMoveArmBaseAction;
   /// <summary>
   /// The Input System Action that Unity uses to read Snap Move data sent from the left hand controller. Must be a <see cref="InputActionType.Value"/> <see cref="Vector2Control"/> Control.
   /// </summary>
   public InputActionProperty leftHandMoveArmBaseAction {
       get => m_LeftHandMoveArmBaseAction;
       set => SetInputActionProperty(ref m_LeftHandMoveArmBaseAction, value);
   }
 
//    [SerializeField]
//    [Tooltip("The Input System Action that will be used to read Snap Move data from the right hand controller. Must be a Value Vector2 Control.")]
//    InputActionProperty m_RightHandMoveArmBaseAction;
//    /// <summary>
//    /// The Input System Action that Unity uses to read Snap Move data sent from the right hand controller. Must be a <see cref="InputActionType.Value"/> <see cref="Vector2Control"/> Control.
//    /// </summary>
//    public InputActionProperty rightHandMoveArmBaseAction {
//        get => m_RightHandMoveArmBaseAction;
//        set => SetInputActionProperty(ref m_RightHandMoveArmBaseAction, value);
//    }
 
   [SerializeField]
   [Tooltip("The Input System Action that will be used to read Snap Move data from the right hand controller. Must be a Value Vector Control.")]
   InputActionProperty m_RightHandGraspAction;
   /// <summary>
   /// The Input System Action that Unity uses to read Snap Move data sent from the right hand controller. Must be a <see cref="InputActionType.Value"/> <see cref="Vector2Control"/> Control.
   /// </summary>
   public InputActionProperty rightHandGraspAction {
       get => m_RightHandGraspAction;
       set => SetInputActionProperty(ref m_RightHandGraspAction, value);
   }
 
   void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value) {
       if (Application.isPlaying)
           property.DisableDirectAction();
 
       property = value;
 
       if (Application.isPlaying && isActiveAndEnabled)
           property.EnableDirectAction();
   }
 
   [SerializeField]
   [Tooltip("The amount that the arm moves.")]
   float m_MoveArmAmount = 0.1f;
   /// <summary>
   /// The number of degrees clockwise Unity rotates the rig when snap turning clockwise.
   /// </summary>
   public float moveArmAmount {
       get => m_MoveArmAmount;
       set => m_MoveArmAmount = value;
   }
 
   private AgentManager _agentManager = null;
   private LinkedList<Vector3> _validResetPositions = new LinkedList<Vector3>();
   private LinkedList<Vector3> _validResetRotations = new LinkedList<Vector3>();
   private Vector3 _defaultPos;
   private Vector3 _defaultRot;
   private float _defaultArmHeight;
   private Vector3 _originPos;
   private Vector3 _originRot;
   private Vector3 _armPosOffset;
   private Vector3 _armRotOffset;
   private bool _isInitialized = false;
   private bool _isArmMode = false;
   private float _armHeight;
   private float _graspForce;
   private int direction = -1;
   private Vector3 _prevPos;
   private Vector3 _prevRot;
   private float _prevGrasp;
   public int steps = 0;

 
   private ArmAgentController _armAgent;
   private IK_Robot_Arm_Controller _arm;
 
   public float ArmHeight {
       get { return _armHeight; }
       set { _armHeight = Mathf.Clamp(value, 0, 1); }
   }
 
   public float GraspForce {
       get { return _graspForce; }
       set { _graspForce = Mathf.Clamp(value, 0, 30); }
   }
 
   public void Initialize() {
       _agentManager = GameObject.Find("PhysicsSceneManager").GetComponentInChildren<AgentManager>();
       var armAgent = (ArmAgentController)_agentManager.PrimaryAgent;
       var arm = armAgent.getArm();
 
       _defaultPos = arm.armTarget.localPosition;
       _defaultRot = arm.armTarget.localEulerAngles;
       _defaultArmHeight = arm.transform.localPosition.y;
       _prevPos = _defaultPos;
       _prevRot = _defaultRot;
       _isInitialized = true;
   }
 
   private void OnDestroy() {
       StopAllCoroutines();
   }
 
   public void ToggleArm(bool isArmMode) {
       if (!_isInitialized) {
           return;
       }
 
       _isArmMode = isArmMode;
 
       _armAgent = (ArmAgentController)_agentManager.PrimaryAgent;
       _arm = _armAgent.getArm();
       if (isArmMode) {
           _armAgent.agentState = AgentState.Processing;
           StartCoroutine("ArmCoroutine");
       }
       else {
           // Return to orinigal autoSimulate physics
           StopCoroutine("ArmCoroutine");
           _armAgent.ContinuousArmMoveFinish();
          
       }
   }
 
   private IEnumerator ArmCoroutine() {
       CollisionListener collisionListener = _arm.collisionListener;
 
       _originPos = _xrController.transform.localPosition;
       _originRot = _xrController.transform.localEulerAngles;
       _armPosOffset = _arm.armTarget.localPosition;
       _armRotOffset = _arm.armTarget.localEulerAngles;
       _validResetPositions.AddLast(_arm.armTarget.localPosition);
       _validResetRotations.AddLast(_arm.armTarget.eulerAngles);
 
       CapsuleCollider cc = _armAgent.GetComponent<CapsuleCollider>();
       Vector3 capsuleWorldCenter = cc.transform.TransformPoint(cc.center);
 
       while (true) {

            if (steps + _agentManager.agentSteps == 1500) {
                Debug.Log($"[STEPS COMPLETED] {steps + _agentManager.agentSteps}");
            }

            else {
                Debug.Log($"STEPS RECORDING {steps + _agentManager.agentSteps}");
            }

           _arm.armTarget.localPosition = _xrController.transform.localPosition - _originPos + _armPosOffset;
           _arm.armTarget.localEulerAngles = _xrController.transform.localEulerAngles - _originRot + _armRotOffset;

            if (Mathf.Abs(_arm.armTarget.localPosition.x - _prevPos.x) > 0.25) {
                Debug.Log("[A]");
                _prevPos.x = _arm.armTarget.localPosition.x;
                steps+=1;
            }

            if (Mathf.Abs(_arm.armTarget.localPosition.y - _prevPos.y) > 0.25) {
                Debug.Log("[B]");
                _prevPos.y = _arm.armTarget.localPosition.y;
                steps+=1;
            }

            if (Mathf.Abs(_arm.armTarget.localPosition.z - _prevPos.z) > 0.25) {
                Debug.Log("[C]");
                _prevPos.z = _arm.armTarget.localPosition.z;
                steps+=1;
            }

            if (Mathf.Abs(_arm.armTarget.localEulerAngles.x - _prevRot.x) > 30) {
                Debug.Log("[D]");
                _prevRot.x = _arm.armTarget.localEulerAngles.x;
                steps+=1;
            }

            if (Mathf.Abs(_arm.armTarget.localEulerAngles.y - _prevRot.y) > 30) {
                Debug.Log("[E]");
                _prevRot.y = _arm.armTarget.localEulerAngles.y;
                steps+=1;
            }

            if (Mathf.Abs(_arm.armTarget.localEulerAngles.z - _prevRot.z) > 30) {
                Debug.Log("[F]");
                _prevRot.z = _arm.armTarget.localEulerAngles.z;
                steps+=1;
            }
 
           float maxY = capsuleWorldCenter.y + cc.height / 2f;
           float minY = capsuleWorldCenter.y + (-cc.height / 2f) / 2f;
           float height = (maxY - minY) * _armHeight + minY;
 
           if(height == maxY || height == minY) {
               direction *= -1;
           }
 
           ArmHeight += ReadInput(direction);
           GraspForce = ReadGripForce();
 
           ToggleGrasp();
           MoveArmBase(_armAgent);
 
           PhysicsSceneManager.PhysicsSimulateTHOR(Time.deltaTime);
 
           if (collisionListener.ShouldHalt()) {
               // Set arm position to last valid position and remove from valid reset lists
               _arm.armTarget.localPosition = _validResetPositions.Last();
               _validResetPositions.RemoveLast();
               _arm.armTarget.localEulerAngles = _validResetRotations.Last();
               _validResetRotations.RemoveLast();
 
               // Set originPos and armOffset to new track hand position
 
               _originPos = _xrController.transform.localPosition;
               _originRot = _xrController.transform.localEulerAngles;
               _armPosOffset = _arm.armTarget.localPosition;
               _armRotOffset = _arm.armTarget.localEulerAngles;
 
               _xrController.SendHapticImpulse(_hapticAmplitude, _hapticDuration);
           } else {
               if (!_validResetPositions.Contains(_arm.armTarget.localPosition)) {
                   if (_validResetPositions.Count > _maxResetCount) { // Too many reset positions stored
                       _validResetPositions.RemoveFirst();
                       _validResetRotations.RemoveFirst();
                   }
 
                   _validResetPositions.AddLast(_arm.armTarget.localPosition);
                   _validResetRotations.AddLast(_arm.armTarget.localEulerAngles);
               }
           }
           _arm.AppendArmMetadataVR();
           yield return null;
       }
   }
 
   private void MoveArmBase(ArmAgentController controller) {
       CapsuleCollider cc = controller.GetComponent<CapsuleCollider>();
       Vector3 capsuleWorldCenter = cc.transform.TransformPoint(cc.center);
 
       float maxY = capsuleWorldCenter.y + cc.height / 2f;
       float minY = capsuleWorldCenter.y + (-cc.height / 2f) / 2f;
 
       // Normalized
       float height = (maxY - minY) * _armHeight + minY;
 
       var arm = controller.getArm();
       if (arm.transform.localPosition.y == height) {
           return;
       }
 
       if (height < minY || height > maxY) {
           throw new ArgumentOutOfRangeException($"height={height} value must be in [{minY}, {maxY}].");
       }
 
       arm.transform.localPosition = new Vector3(arm.transform.localPosition.x, _armHeight, arm.transform.localPosition.z);
   }
 
   protected float ReadInput(int direction) {
       var leftHandValue = m_LeftHandMoveArmBaseAction.action?.ReadValue<float>() ?? 0;
    //    var rightHandValue = m_RightHandMoveArmBaseAction.action?.ReadValue<float>() ?? 0;
    //    return rightHandValue - leftHandValue;
        if (leftHandValue > 0){
            Debug.Log("[RECORDING ACTION] Direction"+ direction.ToString());
            Debug.Log("[RECORDING ACTION] LeftHandGrip pressed"+leftHandValue.ToString());
        }
        return direction * leftHandValue;
   }
 
   protected float ReadGripForce() {
       var rightHandValue = m_RightHandGraspAction.action?.ReadValue<float>() ?? 0;
       if (rightHandValue > 0){
            Debug.Log("[RECORDING ACTION] RightHandGrip"+rightHandValue.ToString());
       }

       if (rightHandValue == 0) {
            _prevGrasp = 0;
       }

       if(Mathf.Abs(_prevGrasp - rightHandValue) > 0.05) {
            _prevGrasp = rightHandValue;
            Debug.Log("[G]");
            steps+=1;
       } 

       return rightHandValue;
   }
 
   public void ToggleGrasp() {
       var armAgent = (ArmAgentController)_agentManager.PrimaryAgent;
       var arm = armAgent.getArm();
 
       if (arm.heldObjects.Count == 0) {
           // List<SimObjPhysics> grabbableObjects = arm.WhatObjectsAreInsideMagnetSphereAsSOP(true);
           // List<string> grabbableObjectIds = new List<string>();
           string errorMessage = "";
           // foreach (SimObjPhysics sop in grabbableObjects) {
           //     grabbableObjectIds.Add(sop.objectID);
           // }
           if (!arm.PickupObject(_graspForce, ref errorMessage)) {
               print(errorMessage);
           }
       }
       else {
           arm.ChangeGraspForce(_graspForce);
       }
   }

   public void ToggleOpenOrClose() {
       Debug.Log("Check 2 Okayy");
       var armAgent = (ArmAgentController)_agentManager.PrimaryAgent;
       var arm = armAgent.getArm();

       string errorMessage = "No Openable/Closable Object within Range";
       List<SimObjPhysics> openableObjects = arm.WhatObjectsAreInsideMagnetSphereAsSOP(false);
       
       if(!OpenOrCloseObject(openableObjects, ref errorMessage)) {
            print(errorMessage);
       }
   }

    public bool OpenOrCloseObject(List<SimObjPhysics> objects, ref string errorMessage) {
        bool hasOpenedOrClosed = false;
        Debug.Log("Check 3 Okayy");
        Debug.Log("[H]");
        steps+=1;
        foreach(SimObjPhysics sop in objects) {
            if (sop.IsOpenable) {
                Debug.Log(sop.ObjectID);
                CanOpen_Object coj = sop.gameObject.GetComponent<CanOpen_Object>();
                coj.triggerEnabled = false;
                if (coj) {
                    if (!coj.isOpen) {
                        Debug.Log("Check 4 Okayy");
                        _armAgent.OpenObject(objectId: sop.objectID, forceAction: true, useGripper: true);
                        hasOpenedOrClosed = true;
                    } 
                    else if (coj.isOpen) {
                        Debug.Log("Check 5 Okayy");
                        _armAgent.CloseObject(objectId: sop.objectID, forceAction: true, useGripper: true);
                        hasOpenedOrClosed = true;
                    }
                }
            }

            if(hasOpenedOrClosed) {
                break;
            }
        }
        return hasOpenedOrClosed;
    }
 
   public void ResetArm() {
       var armAgent = (ArmAgentController)_agentManager.PrimaryAgent;
       var arm = armAgent.getArm();
 
       arm.armTarget.position = _xrController.transform.position;
       arm.armTarget.eulerAngles = _xrController.transform.eulerAngles;
 
       _originPos = _xrController.transform.localPosition;
       _originRot = _xrController.transform.localEulerAngles;
       _armPosOffset = arm.armTarget.localPosition;
       _armRotOffset = arm.armTarget.localEulerAngles;
   }
 
   public void DefaultArm() {
       var armAgent = (ArmAgentController)_agentManager.PrimaryAgent;
       var arm = armAgent.getArm();
 
       arm.armTarget.localPosition = _defaultPos;
       arm.armTarget.localEulerAngles = _defaultRot;
 
       _originPos = _xrController.transform.localPosition;
       _originRot = _xrController.transform.localEulerAngles;
       _armPosOffset = arm.armTarget.localPosition;
       _armRotOffset = arm.armTarget.localEulerAngles;
   }
}
