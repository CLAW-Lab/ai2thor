
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections;
using TMPro;
using System;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;
using Newtonsoft.Json.Linq;
using Thor.Procedural.Data;
using Thor.Procedural;

[RequireComponent(typeof(XROrigin))]
public class XRManager : MonoBehaviour
{
    /// <summary>
    /// The Input Manager assigns callback functions to certain actions that can be perfromed by the XR controllers.
    /// </summary>
    /// 

    [SerializeField] private TMP_Text _notificationText;
    [SerializeField] private CanvasGroup _notificationCanvasGroup;
    [SerializeField] private Canvas _armMenu;
    [SerializeField] private TMP_Text _locomotionText;
    [SerializeField] private TMP_Text _armText;
    [SerializeField] private TMP_Text _povText;
    [SerializeField] private float _fadeTime = 1.0f;

    [Header("Right Input Action References")]
    //[SerializeField] private InputActionReference _rightThumbstickPressReference = null;
    [SerializeField] private InputActionReference _rightPrimaryTapReference = null;
    //[SerializeField] private InputActionReference _rightSecondaryTapReference = null;
    [SerializeField] private InputActionReference _rightSecondaryHoldReference = null;
    //[SerializeField] private InputActionReference _rightGripPressReference = null;

    [Header("Left Input Action References")]
    [SerializeField] private InputActionReference _leftMenuPressReference = null;
    //[SerializeField] private InputActionReference _leftThumbstickClickReference = null;
    [SerializeField] private InputActionReference _leftPrimaryTapReference = null;
    [SerializeField] private InputActionReference _leftSecondaryTapReference = null;
    //[SerializeField] private InputActionReference _leftSecondaryHoldReference = null;
    //[SerializeField] private InputActionReference _leftGripPressReference = null;

    [Header("Events")]
    [SerializeField] private UnityEvent _onUserLocomotionEvent = new UnityEvent();
    [SerializeField] private UnityEvent _onAgentLocomotionEvent = new UnityEvent();

    [SerializeField] private UnityEvent<bool> _onTogglePOVEvent = new UnityEvent<bool>();

    [SerializeField] private UnityEvent<bool> _onArmOnEvent = new UnityEvent<bool>();
    [SerializeField] private UnityEvent<bool> _onArmOffEvent = new UnityEvent<bool>();

    [SerializeField] private UnityEvent _onResetArmEvent = new UnityEvent();
    [SerializeField] private UnityEvent _onDefaultArmEvent = new UnityEvent();

    [SerializeField] private UnityEvent _onOpenObjectEvent = new UnityEvent();

    [SerializeField] private UnityEvent<bool> _onToggleGuideEvent = new UnityEvent<bool>();

    [SerializeField] private UnityEvent _onInitializedEvent = new UnityEvent();


    private enum ControllerMode {
        user = 0,
        agent = -1
    }

    private AgentManager _agentManager = null;
    private ControllerMode _locomotionMode = ControllerMode.user;
    private bool _isInitialized = false;
    private bool _isFPSMode = false;
    private bool _isArmMode = false;
    private bool _isCrouching = false;
    private bool _showingGuide = true;

    public bool IsFPSMode{
        get { return _isFPSMode; }
    }

    public TMP_Text ModeText {
        get { return _notificationText; }
    }

    public static XRManager Instance { get; private set; }

    BaseFPSAgentController CurrentActiveController() {
        return _agentManager.PrimaryAgent;
    }

    private void Awake() {

        Debug.Log("I'm Up!!!");

        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this) {
            Destroy(Instance.gameObject);
            Instance = this;
        } else {
            Instance = this;
        }

        Debug.Log("I'm Up 2!!!");

        _agentManager = GameObject.Find("PhysicsSceneManager").GetComponentInChildren<AgentManager>();


        _leftMenuPressReference.action.performed += (InputAction.CallbackContext context) => { ToggleMenu(); };

        _leftPrimaryTapReference.action.performed += (InputAction.CallbackContext context) => { ToggleCrouch(); };

        //_rightThumbstickPressReference.action.performed += (InputAction.CallbackContext context) => { ToggleMoveArmBase(); };
        //_leftThumbstickPressReference.action.performed += (InputAction.CallbackContext context) => { ToggleMoveArmBase(); };

        _rightPrimaryTapReference.action.performed += (InputAction.CallbackContext context) => { ToggleOpenClose(); };
        //_leftPrimaryTapReference.action.performed += (InputAction.CallbackContext context) => { TogglePOV(); };

        _leftSecondaryTapReference.action.performed += (InputAction.CallbackContext context) => { ToggleGuide(); };

        //_rightSecondaryTapReference.action.performed += (InputAction.CallbackContext context) => { ToggleArm(); };
        //_leftSecondaryTapReference.action.performed += (InputAction.CallbackContext context) => { ToggleArm(); };

        _rightSecondaryHoldReference.action.performed += (InputAction.CallbackContext context) => { ResetArm(); };
        //_leftSecondaryHoldReference.action.performed += (InputAction.CallbackContext context) => { ResetArm(); };

        //_rightGripPressReference.action.performed += (InputAction.CallbackContext context) => { ToggleGrasp(); };
        //_leftGripPressReference.action.performed += (InputAction.CallbackContext context) => { ToggleGrasp(); };
    }

    private void OnDestroy() {
        StopAllCoroutines();

        _leftMenuPressReference.action.performed -= (InputAction.CallbackContext context) => { ToggleMenu(); };

        _leftPrimaryTapReference.action.performed -= (InputAction.CallbackContext context) => { ToggleCrouch(); };

        //_rightThumbstickPressReference.action.performed -= (InputAction.CallbackContext context) => { ToggleMoveArmBase(); };
        //_leftThumbstickPressReference.action.performed -= (InputAction.CallbackContext context) => { ToggleMoveArmBase(); };

        _rightPrimaryTapReference.action.performed -= (InputAction.CallbackContext context) => { ToggleOpenClose(); };
        //_leftPrimaryTapReference.action.performed -= (InputAction.CallbackContext context) => { TogglePOV(); };

        _leftSecondaryTapReference.action.performed -= (InputAction.CallbackContext context) => { ToggleGuide(); };

        //_rightSecondaryTapReference.action.performed -= (InputAction.CallbackContext context) => { ToggleArm(); };
        //_leftSecondaryTapReference.action.performed -= (InputAction.CallbackContext context) => { ToggleArm(); };

        _rightSecondaryHoldReference.action.performed -= (InputAction.CallbackContext context) => { ResetArm(); };
        //_leftSecondaryHoldReference.action.performed -= (InputAction.CallbackContext context) => { ResetArm(); };

        //_rightGripPressReference.action.performed -= (InputAction.CallbackContext context) => { ToggleGrasp(); };
        //_leftGripPressReference.action.performed -= (InputAction.CallbackContext context) => { ToggleGrasp(); };
    }
        

    public void Initialize() {
        Dictionary<string, object> action = new Dictionary<string, object>();
        // if you want to use smaller grid size step increments, initialize with a smaller/larger gridsize here
        // by default the gridsize is 0.25, so only moving in increments of .25 will work
        // so the MoveAhead action will only take, by default, 0.25, .5, .75 etc magnitude with the default
        // grid size!
        // action.renderNormalsImage = true;
        // action.renderDepthImage = true;
        // action.renderSemanticSegmentation = true;
        // action.renderInstanceSegmentation = true;
        // action.renderFlowImage = true;
        // action.rotateStepDegrees = 30;
        // action.ssao = "default";
        // action.snapToGrid = true;
        // action.makeAgentsVisible = false;
        action["agentMode"] = "vr";
        action["fieldOfView"] = 90f;
        // action.cameraY = 2.0f;
        action["snapToGrid"] = true;
        // action.rotateStepDegrees = 45;
        action["autoSimulation"] = true;
        action["action"] = "Initialize";
        CurrentActiveController().ProcessControlCommand(new DynamicServerAction(action), _agentManager);

        _onUserLocomotionEvent?.Invoke();
        _onInitializedEvent?.Invoke();

        _isInitialized = true;
        Debug.Log("[RECORDING ACTION] In Game Start Button Pressed");
    }

    private void Start() {
        // Set as user mode
        //_onUserLocomotionEvent?.Invoke();
    }

    public void CreateProceduralHouse() {
       // initialize new house for procthor
       Debug.Log("Reached Here.");
       //    Dictionary<string, object> action = new Dictionary<string, object>();
       if (_agentManager.PrimaryAgent.IsProcessing) {
           Debug.Log("Cannot execute command while last action has not completed.");
       }
       //    action["action"] = "CreateHouse";
       var jsonStr = @"{'doors': [{'assetId': 'Doorway_9', 'id': 'door|2|6', 'openable': true, 'openness': 1, 'room0': 'room|6', 'room1': 'room|2', 'wall0': 'wall|6|3.63|3.63|7.25|3.63', 'wall1': 'wall|2|3.63|3.63|7.25|3.63', 'holePolygon': [{'x': 0.8750242861399969, 'y': 0, 'z': 0}, {'x': 1.8730481537471135, 'y': 2.0743770599365234, 'z': 0}], 'assetPosition': {'x': 1.374677255790921, 'y': 1.0371885299682617, 'z': 0}}, {'assetId': 'Doorway_Double_4', 'id': 'door|1|6', 'openable': false, 'openness': 0, 'room0': 'room|6', 'room1': 'room|6', 'wall0': 'wall|6|0.00|7.25|7.25|7.25', 'wall1': 'wall|exterior|0.00|7.25|7.25|7.25', 'holePolygon': [{'x': 0.18210122650476937, 'y': 0, 'z': 0}, {'x': 2.175666070636239, 'y': 2.1049129962921143, 'z': 0}], 'assetPosition': {'x': 1.178351359534886, 'y': 1.0524564981460571, 'z': 0}}], 'metadata': {'agent': {'horizon': 30, 'position': {'x': 2.75, 'y': 0.95, 'z': 6.25}, 'rotation': {'x': 0, 'y': 270, 'z': 0}, 'standing': true}, 'roomSpecId': 'kitchen-living-bedroom-room2', 'schema': '1.0.0', 'warnings': {}, 'agentPoses': {'arm': {'horizon': 30, 'position': {'x': 2.75, 'y': 0.95, 'z': 6.25}, 'rotation': {'x': 0, 'y': 270, 'z': 0}, 'standing': true}, 'default': {'horizon': 30, 'position': {'x': 2.75, 'y': 0.95, 'z': 6.25}, 'rotation': {'x': 0, 'y': 270, 'z': 0}, 'standing': true}, 'locobot': {'horizon': 30, 'position': {'x': 2.75, 'y': 0.95, 'z': 6.25}, 'rotation': {'x': 0, 'y': 270, 'z': 0}}, 'stretch': {'horizon': 30, 'position': {'x': 2.75, 'y': 0.95, 'z': 6.25}, 'rotation': {'x': 0, 'y': 270, 'z': 0}, 'standing': true}}}, 'objects': [{'assetId': 'Countertop_L_10x8', 'children': [{'assetId': 'RoboTHOR_cellphone_blackberry_v', 'id': 'CellPhone|surface|6|1', 'kinematic': false, 'position': {'x': 5.321451187133789, 'y': 0.9505196213722229, 'z': 7.000960826873779}, 'rotation': {'x': 0.0, 'y': 180.0, 'z': 0.0}, 'layer': 'Procedural1'}, {'assetId': 'Kettle_1', 'id': 'Kettle|surface|6|2', 'kinematic': false, 'position': {'x': 4.557376861572266, 'y': 1.0367436408996582, 'z': 7.0750627517700195}, 'rotation': {'x': 0.0, 'y': 180.0, 'z': 0.0}, 'layer': 'Procedural1'}, {'assetId': 'Plate_4', 'id': 'Plate|surface|6|3', 'kinematic': false, 'position': {'x': 4.407819747924805, 'y': 1.0016630172729493, 'z': 5.475604057312012}, 'rotation': {'x': 0.0, 'y': 180.0, 'z': 0.0}, 'layer': 'Procedural1'}, {'assetId': 'Cup_8', 'id': 'Cup|surface|6|5', 'kinematic': false, 'position': {'x': 4.25351095199585, 'y': 1.0695941925048829, 'z': 6.392690181732178}, 'rotation': {'x': 0.0, 'y': 180.0, 'z': 0.0}, 'layer': 'Procedural1'}, {'assetId': 'Apple_22', 'id': 'Apple|surface|6|9', 'kinematic': false, 'position': {'x': 6.467563629150391, 'y': 0.9922797679901123, 'z': 6.8467326164245605}, 'rotation': {'x': 0.0, 'y': 180.0, 'z': 0.0}, 'layer': 'Procedural1'}, {'assetId': 'Bowl_20', 'id': 'Bowl|surface|6|10', 'kinematic': false, 'position': {'x': 4.6412177085876465, 'y': 1.0360098958015442, 'z': 5.016264915466309}, 'rotation': {'x': 0.0, 'y': 180.0, 'z': 0.0}, 'layer': 'Procedural1'}, {'assetId': 'Houseplant_20', 'id': 'HousePlant|surface|6|11', 'kinematic': false, 'position': {'x': 4.340981960296631, 'y': 1.265755534172058, 'z': 5.697815895080566}, 'rotation': {'x': 0.0, 'y': 180.0, 'z': 0.0}, 'layer': 'Procedural1'}, {'assetId': 'Book_2', 'id': 'Book|surface|6|12', 'kinematic': false, 'position': {'x': 4.640815734863281, 'y': 0.957991898059845, 'z': 5.704165458679199}, 'rotation': {'x': 0.0, 'y': 180.0, 'z': 0.0}, 'layer': 'Procedural1'}, {'assetId': 'Soap_Bottle_22', 'id': 'SoapBottle|surface|6|13', 'kinematic': false, 'position': {'x': 5.703788757324219, 'y': 1.0544524192810059, 'z': 7.0780744552612305}, 'rotation': {'x': 0.0, 'y': 180.0, 'z': 0.0}, 'layer': 'Procedural1'}, {'assetId': 'Spoon_1', 'id': 'Spoon|surface|6|14', 'kinematic': false, 'position': {'x': 4.719936370849609, 'y': 0.9518508911132812, 'z': 6.176292896270752}, 'rotation': {'x': 0.0, 'y': 180.0, 'z': 0.0}, 'layer': 'Procedural1'}, {'assetId': 'Fork_1', 'id': 'Fork|surface|6|16', 'kinematic': false, 'position': {'x': 6.085526466369629, 'y': 0.9541671276092529, 'z': 6.868941307067871}, 'rotation': {'x': 0.0, 'y': 180.0, 'z': 0.0}, 'layer': 'Procedural1'}, {'assetId': 'Ladle_3', 'id': 'Ladle|surface|6|17', 'kinematic': false, 'position': {'x': 4.4873247146606445, 'y': 0.9718015193939209, 'z': 5.011041164398193}, 'rotation': {'x': 0.0, 'y': 180.0, 'z': 0.0}, 'layer': 'Procedural1'}, {'assetId': 'Potato_27', 'id': 'Potato|surface|6|18', 'kinematic': false, 'position': {'x': 5.31583309173584, 'y': 0.9851284027099609, 'z': 6.8400774002075195}, 'rotation': {'x': 0.0, 'y': 180.0, 'z': 0.0}, 'layer': 'Procedural1'}, {'assetId': 'Salt_Shaker_1', 'id': 'SaltShaker|surface|6|19', 'kinematic': false, 'position': {'x': 4.408278465270996, 'y': 0.9977200031280518, 'z': 5.933465957641602}, 'rotation': {'x': 0.0, 'y': 180.0, 'z': 0.0}, 'layer': 'Procedural1'}, {'assetId': 'Toaster_20', 'id': 'Toaster|surface|6|20', 'kinematic': false, 'position': {'x': 4.563570976257324, 'y': 1.0599498748779297, 'z': 6.37637996673584}, 'rotation': {'x': 0.0, 'y': 180.0, 'z': 0.0}, 'layer': 'Procedural1'}, {'assetId': 'Pepper_Shaker_2', 'id': 'PepperShaker|surface|6|22', 'kinematic': false, 'position': {'x': 4.6412177085876465, 'y': 1.0418901443481445, 'z': 5.930578231811523}, 'rotation': {'x': 0.0, 'y': 180.0, 'z': 0.0}, 'layer': 'Procedural1'}], 'id': 'CounterTop|6|0', 'kinematic': true, 'position': {'x': 5.702900484085083, 'y': 0.46919137239456177, 'z': 6.00879891204834}, 'rotation': {'x': 0, 'y': 180, 'z': 0}, 'layer': 'Procedural1', 'material': null}, {'assetId': 'Fridge_14', 'children': [{'assetId': 'Egg_20', 'id': 'Egg|surface|6|6', 'kinematic': false, 'position': {'x': 0.5278591513633728, 'y': 0.91966472864151, 'z': 6.123994827270508}, 'rotation': {'x': -0.0, 'y': 90.0, 'z': -0.0}, 'layer': 'Procedural1'}, {'assetId': 'Egg_22', 'id': 'Egg|surface|6|7', 'kinematic': false, 'position': {'x': 0.4239487051963806, 'y': 1.3727908611297608, 'z': 6.508115768432617}, 'rotation': {'x': -0.0, 'y': 90.0, 'z': -0.0}, 'layer': 'Procedural1'}, {'assetId': 'Egg_7', 'id': 'Egg|surface|6|8', 'kinematic': false, 'position': {'x': 0.2675052881240845, 'y': 0.9075589656829834, 'z': 6.409137725830078}, 'rotation': {'x': -0.0, 'y': 90.0, 'z': -0.0}, 'layer': 'Procedural1'}, {'assetId': 'Lettuce_11', 'id': 'Lettuce|surface|6|21', 'kinematic': false, 'position': {'x': 0.5213821530342102, 'y': 0.4388054311275482, 'z': 6.413703918457031}, 'rotation': {'x': -0.0, 'y': 90.0, 'z': -0.0}, 'layer': 'Procedural1'}], 'id': 'Fridge|6|1', 'kinematic': true, 'position': {'x': 0.4081499814987183, 'y': 0.763818621635437, 'z': 6.247683081166544}, 'rotation': {'x': 0, 'y': 90, 'z': 0}, 'layer': 'Procedural1', 'material': null}, {'assetId': 'Shelving_Unit_307_1', 'children': [{'assetId': 'Towel_Statue_1', 'id': 'Statue|surface|6|0', 'kinematic': false, 'position': {'x': 3.937598466873169, 'y': 1.415320086479187, 'z': 7.079460144042969}, 'rotation': {'x': 0.0, 'y': 180.0, 'z': 0.0}, 'layer': 'Procedural1'}, {'assetId': 'Spray_Bottle_6', 'id': 'SprayBottle|surface|6|4', 'kinematic': false, 'position': {'x': 3.8192250728607178, 'y': 0.4363059401512146, 'z': 7.164740562438965}, 'rotation': {'x': 0.0, 'y': 270.0, 'z': 0.0}, 'layer': 'Procedural1'}, {'assetId': 'Vase_Open_1', 'color': {'b': 0.9100006752636188, 'g': 0.6669740210487086, 'r': 0.4153091963637816}, 'id': 'Vase|surface|6|15', 'kinematic': false, 'position': {'x': 3.381804943084717, 'y': 1.3329482555389405, 'z': 7.108337879180908}, 'rotation': {'x': 0.0, 'y': 180.0, 'z': 0.0}, 'layer': 'Procedural1'}], 'id': 'ShelvingUnit|6|2', 'kinematic': true, 'position': {'x': 3.60434134552425, 'y': 0.5777348875999451, 'z': 7.108961418151855}, 'rotation': {'x': 0, 'y': 180, 'z': 0}, 'layer': 'Procedural1', 'material': null}, {'assetId': 'TV_Stand_206_1', 'children': [{'assetId': 'Television_8', 'id': 'Television|7|0|1', 'kinematic': true, 'position': {'x': 0.6274313569068906, 'y': 1.1540268957614899, 'z': 2.139758289337158}, 'rotation': {'x': 0, 'y': 0.0, 'z': 0}, 'layer': 'Procedural1'}, {'assetId': 'Fertility_Statue_2', 'color': {'b': 0.017408230394763358, 'g': 0.44201380496872733, 'r': 0.942616050671908}, 'id': 'Statue|surface|7|24', 'kinematic': false, 'position': {'x': 0.17688018083572388, 'y': 0.9750107407569886, 'z': 1.9125752449035645}, 'rotation': {'x': -0.0, 'y': 0.0, 'z': 0.0}, 'layer': 'Procedural1'}, {'assetId': 'Remote_2', 'id': 'RemoteControl|surface|7|25', 'kinematic': false, 'position': {'x': 0.6975793242454529, 'y': 0.18983325362205505, 'z': 2.0840961933135986}, 'rotation': {'x': -0.0, 'y': 0.0, 'z': 0.0}, 'layer': 'Procedural1'}, {'assetId': 'Keychain_1', 'id': 'KeyChain|surface|7|27', 'kinematic': false, 'position': {'x': 1.0447421073913574, 'y': 0.761028528213501, 'z': 1.9952168464660645}, 'rotation': {'x': -0.0, 'y': 0.0, 'z': 0.0}, 'layer': 'Procedural1'}, {'assetId': 'Keychain_1', 'id': 'KeyChain|surface|7|28', 'kinematic': false, 'position': {'x': 0.4441554546356201, 'y': 0.7579448223114014, 'z': 2.145353078842163}, 'rotation': {'x': -0.0, 'y': 0.0, 'z': 0.0}, 'layer': 'Procedural1'}], 'id': 'TVStand|7|0|0', 'kinematic': true, 'position': {'x': 0.6274313569068906, 'y': 0.37626194953918457, 'z': 2.139758289337158}, 'rotation': {'x': 0, 'y': 0.0, 'z': 0}, 'layer': 'Procedural1', 'material': null}, {'assetId': 'RoboTHOR_dining_table_extendable', 'children': [{'assetId': 'Remote_1', 'id': 'RemoteControl|surface|7|29', 'kinematic': false, 'position': {'x': 2.720364570617676, 'y': 0.7567200064659119, 'z': 2.188589334487915}, 'rotation': {'x': -0.0, 'y': 0.0, 'z': 0.0}, 'layer': 'Procedural1'}, {'assetId': 'Plate_26', 'id': 'Plate|surface|7|30', 'kinematic': false, 'position': {'x': 2.72048282623291, 'y': 0.80412917137146, 'z': 2.5334811210632324}, 'rotation': {'x': -0.0, 'y': 0.0, 'z': 0.0}, 'layer': 'Procedural1'}], 'id': 'DiningTable|7|1', 'kinematic': true, 'position': {'x': 2.2896949780445772, 'y': 0.3721551299095154, 'z': 2.3010918502807614}, 'rotation': {'x': 0, 'y': 0, 'z': 0}, 'layer': 'Procedural1', 'material': null}, {'assetId': 'RoboTHOR_sofa_alrid', 'children': [{'assetId': 'Laptop_4', 'id': 'Laptop|surface|7|23', 'kinematic': false, 'openness': 1, 'position': {'x': 0.6017045378684998, 'y': 0.6204867839813233, 'z': 4.313706398010254}, 'rotation': {'x': -0.0, 'y': 90.0, 'z': -0.0}, 'layer': 'Procedural1'}, {'assetId': 'Remote_1', 'id': 'RemoteControl|surface|7|26', 'kinematic': false, 'position': {'x': 0.7393728494644165, 'y': 0.4373481273651123, 'z': 3.7066421508789062}, 'rotation': {'x': -0.0, 'y': 90.0, 'z': -0.0}, 'layer': 'Procedural1'}], 'id': 'Sofa|7|2', 'kinematic': true, 'position': {'x': 0.4985131561756134, 'y': 0.4085739254951477, 'z': 4.16194122795421}, 'rotation': {'x': 0, 'y': 90, 'z': 0}, 'layer': 'Procedural1', 'material': null}, {'assetId': 'Dog_Bed_1_1', 'id': 'DogBed|7|3', 'kinematic': false, 'position': {'x': 2.069180043888071, 'y': 0.054420676082372665, 'z': 4.675698927416474}, 'rotation': {'x': 0, 'y': 270, 'z': 0}, 'layer': 'Procedural1', 'material': null}, {'assetId': 'RoboTHOR_bed_kritter', 'children': [{'assetId': 'Alarm_Clock_26', 'id': 'AlarmClock|surface|2|31', 'kinematic': false, 'position': {'x': 6.836429595947266, 'y': 0.4797007441520691, 'z': 1.4202826023101807}, 'rotation': {'x': -0.0, 'y': 0.0, 'z': 0.0}, 'layer': 'Procedural2'}, {'assetId': 'Teddy_Bear_1', 'id': 'TeddyBear|surface|2|33', 'kinematic': false, 'position': {'x': 6.840456962585449, 'y': 0.6240270137786865, 'z': 0.2627936601638794}, 'rotation': {'x': -0.0, 'y': 0.0, 'z': 0.0}, 'layer': 'Procedural2'}, {'assetId': 'pillow_3', 'id': 'Pillow|surface|2|36', 'kinematic': false, 'position': {'x': 6.749236583709717, 'y': 0.458018958568573, 'z': 0.8368757367134094}, 'rotation': {'x': -0.0, 'y': 0.0, 'z': 0.0}, 'layer': 'Procedural2'}], 'id': 'Bed|2|0', 'isDirty': false, 'kinematic': true, 'position': {'x': 6.841713026046753, 'y': 0.33533501625061035, 'z': 0.8505797624588013}, 'rotation': {'x': 0, 'y': 0, 'z': 0}, 'layer': 'Procedural2', 'material': null}, {'assetId': 'Dresser_318_2', 'children': [{'assetId': 'CreditCard_3', 'id': 'CreditCard|surface|2|32', 'kinematic': false, 'position': {'x': 4.104500770568848, 'y': 0.09809717535972595, 'z': 2.313260078430176}, 'rotation': {'x': -0.0, 'y': 0.0, 'z': 0.0}, 'layer': 'Procedural2'}, {'assetId': 'RoboTHOR_remote_coolux_v', 'id': 'RemoteControl|surface|2|34', 'kinematic': false, 'position': {'x': 4.08823299407959, 'y': 0.9880679845809937, 'z': 2.324049949645996}, 'rotation': {'x': -0.0, 'y': 179.99984741210938, 'z': -0.0}, 'layer': 'Procedural2'}, {'assetId': 'Tissue_Box_1', 'id': 'TissueBox|surface|2|35', 'kinematic': false, 'position': {'x': 3.914646625518799, 'y': 1.054113507270813, 'z': 2.41792368888855}, 'rotation': {'x': -0.0, 'y': 179.99984741210938, 'z': -0.0}, 'layer': 'Procedural2'}, {'assetId': 'Vase_Medium_3', 'id': 'Vase|surface|2|37', 'kinematic': false, 'position': {'x': 3.8278543949127197, 'y': 1.1237839937210083, 'z': 2.136301040649414}, 'rotation': {'x': -0.0, 'y': 179.99984741210938, 'z': -0.0}, 'layer': 'Procedural2'}], 'id': 'Dresser|2|1', 'kinematic': true, 'position': {'x': 4.000335551261902, 'y': 0.48569753766059875, 'z': 2.2211330239772793}, 'rotation': {'x': 0, 'y': 90, 'z': 0}, 'layer': 'Procedural2', 'material': null}, {'assetId': 'Houseplant_26', 'id': 'HousePlant|2|2|0', 'kinematic': false, 'position': {'x': 5.794403162867567, 'y': 0.5485243797302246, 'z': 0.36199759896329575}, 'rotation': {'x': 0, 'y': 238.47228886040693, 'z': 0}, 'layer': 'Procedural2', 'material': null}, {'assetId': 'Box_9', 'id': 'Box|2|3', 'kinematic': false, 'openness': 1, 'position': {'x': 3.868995226383209, 'y': 0.15604570508003235, 'z': 3.391422737598419}, 'rotation': {'x': 0, 'y': 90, 'z': 0}, 'layer': 'Procedural2', 'material': null}, {'assetId': 'Television_27', 'id': 'Television|2|4', 'kinematic': true, 'position': {'x': 7.191932071328163, 'y': 1.741492340591913, 'z': 1.2399645736083271}, 'rotation': {'x': 0, 'y': 270, 'z': 0}, 'layer': 'Procedural2', 'material': null}, {'assetId': 'Wall_Decor_Painting_3', 'id': 'Painting|7|4', 'kinematic': true, 'position': {'x': 2.9943560989060796, 'y': 2.0705913664576077, 'z': 1.827674623608589}, 'rotation': {'x': 0, 'y': 0, 'z': 0}, 'layer': 'Procedural1', 'material': null}, {'assetId': 'Wall_Decor_Painting_6', 'id': 'Painting|7|5', 'kinematic': true, 'position': {'x': 0.02135, 'y': 1.8212260765974362, 'z': 4.0157892108858615}, 'rotation': {'x': 0, 'y': 90, 'z': 0}, 'layer': 'Procedural1', 'material': null}, {'assetId': 'Wall_Decor_Painting_9', 'id': 'Painting|6|3', 'kinematic': true, 'position': {'x': 0.01457472436130047, 'y': 1.4385592622369678, 'z': 6.9728543900031905}, 'rotation': {'x': 0, 'y': 90, 'z': 0}, 'layer': 'Procedural1', 'material': null}, {'assetId': 'Wall_Decor_Painting_8', 'id': 'Painting|2|5', 'kinematic': true, 'position': {'x': 3.639605435490608, 'y': 2.00685382256267, 'z': 2.849781120377277}, 'rotation': {'x': 0, 'y': 90, 'z': 0}, 'layer': 'Procedural2', 'material': null}, {'assetId': 'Wall_Decor_Photo_3', 'id': 'Painting|2|6', 'kinematic': true, 'position': {'x': 6.83001732573649, 'y': 1.6342110151791651, 'z': 3.6051295145452023}, 'rotation': {'x': 0, 'y': 180, 'z': 0}, 'layer': 'Procedural2', 'material': null}], 'proceduralParameters': {'ceilingMaterial': {'name': 'Porcelain_Sand_Mat'}, 'floorColliderThickness': 1.0, 'lights': [{'id': 'DirectionalLight', 'indirectMultiplier': 1.0, 'intensity': 1, 'position': {'x': 0.84, 'y': 0.1855, 'z': -1.09}, 'rgb': {'r': 1.0, 'g': 0.694, 'b': 0.78}, 'rotation': {'x': 6, 'y': -166, 'z': 0}, 'shadow': {'type': 'Soft', 'strength': 1, 'normalBias': 0, 'bias': 0, 'nearPlane': 0.2, 'resolution': 'FromQualitySettings'}, 'type': 'directional'}, {'id': 'light_2', 'intensity': 0.75, 'position': {'x': 5.741777757352941, 'y': 4.139345902674191, 'z': 2.115277757352941}, 'range': 15, 'rgb': {'r': 1.0, 'g': 0.855, 'b': 0.722}, 'shadow': {'type': 'Soft', 'strength': 1, 'normalBias': 0, 'bias': 0.05, 'nearPlane': 0.2, 'resolution': 'FromQualitySettings'}, 'type': 'point', 'layer': 'Procedural2', 'cullingMaskOff': ['Procedural0', 'Procedural1', 'Procedural3']}, {'id': 'light_6', 'intensity': 0.75, 'position': {'x': 4.231111070254868, 'y': 4.139345902674191, 'z': 5.741722186482513}, 'range': 15, 'rgb': {'r': 1.0, 'g': 0.855, 'b': 0.722}, 'shadow': {'type': 'Soft', 'strength': 1, 'normalBias': 0, 'bias': 0.05, 'nearPlane': 0.2, 'resolution': 'FromQualitySettings'}, 'type': 'point', 'layer': 'Procedural1', 'cullingMaskOff': ['Procedural0', 'Procedural2', 'Procedural3']}, {'id': 'light_7', 'intensity': 0.75, 'position': {'x': 1.813, 'y': 4.139345902674191, 'z': 3.6265}, 'range': 15, 'rgb': {'r': 1.0, 'g': 0.855, 'b': 0.722}, 'shadow': {'type': 'Soft', 'strength': 1, 'normalBias': 0, 'bias': 0.05, 'nearPlane': 0.2, 'resolution': 'FromQualitySettings'}, 'type': 'point', 'layer': 'Procedural1', 'cullingMaskOff': ['Procedural0', 'Procedural2', 'Procedural3']}], 'receptacleHeight': 0.7, 'reflections': [], 'skyboxId': 'SkySouthLakeUnion'}, 'rooms': [{'ceilings': [], 'children': [], 'floorMaterial': {'name': 'OrangeWood'}, 'floorPolygon': [{'x': 3.626, 'y': 0, 'z': 1.813}, {'x': 3.626, 'y': 0, 'z': 3.626}, {'x': 7.253, 'y': 0, 'z': 3.626}, {'x': 7.253, 'y': 0, 'z': 0.0}, {'x': 5.44, 'y': 0, 'z': 0.0}, {'x': 5.44, 'y': 0, 'z': 1.813}], 'id': 'room|2', 'roomType': 'Bedroom', 'layer': 'Procedural2'}, {'ceilings': [], 'children': [], 'floorMaterial': {'name': 'WoodFloorsCrossRED'}, 'floorPolygon': [{'x': 7.253, 'y': 0, 'z': 3.626}, {'x': 3.626, 'y': 0, 'z': 3.626}, {'x': 3.626, 'y': 0, 'z': 5.44}, {'x': 0.0, 'y': 0, 'z': 5.44}, {'x': 0.0, 'y': 0, 'z': 7.253}, {'x': 7.253, 'y': 0, 'z': 7.253}], 'id': 'room|6', 'roomType': 'Kitchen', 'layer': 'Procedural1'}, {'ceilings': [], 'children': [], 'floorMaterial': {'name': 'GreyFloor 3'}, 'floorPolygon': [{'x': 0.0, 'y': 0, 'z': 1.813}, {'x': 0.0, 'y': 0, 'z': 5.44}, {'x': 3.626, 'y': 0, 'z': 5.44}, {'x': 3.626, 'y': 0, 'z': 3.626}, {'x': 3.626, 'y': 0, 'z': 1.813}], 'id': 'room|7', 'roomType': 'LivingRoom', 'layer': 'Procedural1'}], 'walls': [{'id': 'wall|2|3.63|1.81|3.63|3.63', 'material': {'name': 'BrownMarbleFake'}, 'polygon': [{'x': 3.626, 'y': 0, 'z': 1.813}, {'x': 3.626, 'y': 0, 'z': 3.626}, {'x': 3.626, 'y': 4.339345902674191, 'z': 1.813}, {'x': 3.626, 'y': 4.339345902674191, 'z': 3.626}], 'roomId': 'room|2', 'layer': 'Procedural2'}, {'id': 'wall|2|3.63|3.63|7.25|3.63', 'material': {'name': 'BrownMarbleFake'}, 'polygon': [{'x': 3.626, 'y': 0, 'z': 3.626}, {'x': 7.253, 'y': 0, 'z': 3.626}, {'x': 3.626, 'y': 4.339345902674191, 'z': 3.626}, {'x': 7.253, 'y': 4.339345902674191, 'z': 3.626}], 'roomId': 'room|2', 'layer': 'Procedural2'}, {'id': 'wall|2|7.25|0.00|7.25|3.63', 'material': {'name': 'BrownMarbleFake'}, 'polygon': [{'x': 7.253, 'y': 0, 'z': 3.626}, {'x': 7.253, 'y': 0, 'z': 0.0}, {'x': 7.253, 'y': 4.339345902674191, 'z': 3.626}, {'x': 7.253, 'y': 4.339345902674191, 'z': 0.0}], 'roomId': 'room|2', 'layer': 'Procedural2'}, {'id': 'wall|2|5.44|0.00|7.25|0.00', 'material': {'name': 'BrownMarbleFake'}, 'polygon': [{'x': 7.253, 'y': 0, 'z': 0.0}, {'x': 5.44, 'y': 0, 'z': 0.0}, {'x': 7.253, 'y': 4.339345902674191, 'z': 0.0}, {'x': 5.44, 'y': 4.339345902674191, 'z': 0.0}], 'roomId': 'room|2', 'layer': 'Procedural2'}, {'id': 'wall|2|5.44|0.00|5.44|1.81', 'material': {'name': 'BrownMarbleFake'}, 'polygon': [{'x': 5.44, 'y': 0, 'z': 0.0}, {'x': 5.44, 'y': 0, 'z': 1.813}, {'x': 5.44, 'y': 4.339345902674191, 'z': 0.0}, {'x': 5.44, 'y': 4.339345902674191, 'z': 1.813}], 'roomId': 'room|2', 'layer': 'Procedural2'}, {'id': 'wall|2|3.63|1.81|5.44|1.81', 'material': {'name': 'BrownMarbleFake'}, 'polygon': [{'x': 5.44, 'y': 0, 'z': 1.813}, {'x': 3.626, 'y': 0, 'z': 1.813}, {'x': 5.44, 'y': 4.339345902674191, 'z': 1.813}, {'x': 3.626, 'y': 4.339345902674191, 'z': 1.813}], 'roomId': 'room|2', 'layer': 'Procedural2'}, {'color': {'b': 0.49019607843137253, 'g': 0.5176470588235295, 'r': 0.44313725490196076}, 'id': 'wall|6|3.63|3.63|7.25|3.63', 'material': {'name': 'PureWhite', 'color': {'b': 0.49019607843137253, 'g': 0.5176470588235295, 'r': 0.44313725490196076}}, 'polygon': [{'x': 7.253, 'y': 0, 'z': 3.626}, {'x': 3.626, 'y': 0, 'z': 3.626}, {'x': 7.253, 'y': 4.339345902674191, 'z': 3.626}, {'x': 3.626, 'y': 4.339345902674191, 'z': 3.626}], 'roomId': 'room|6', 'layer': 'Procedural1'}, {'color': {'b': 0.49019607843137253, 'g': 0.5176470588235295, 'r': 0.44313725490196076}, 'empty': true, 'id': 'wall|6|3.63|3.63|3.63|5.44', 'material': {'name': 'PureWhite', 'color': {'b': 0.49019607843137253, 'g': 0.5176470588235295, 'r': 0.44313725490196076}}, 'polygon': [{'x': 3.626, 'y': 0, 'z': 3.626}, {'x': 3.626, 'y': 0, 'z': 5.44}, {'x': 3.626, 'y': 4.339345902674191, 'z': 3.626}, {'x': 3.626, 'y': 4.339345902674191, 'z': 5.44}], 'roomId': 'room|6', 'layer': 'Procedural1'}, {'color': {'b': 0.49019607843137253, 'g': 0.5176470588235295, 'r': 0.44313725490196076}, 'empty': true, 'id': 'wall|6|0.00|5.44|3.63|5.44', 'material': {'name': 'PureWhite', 'color': {'b': 0.49019607843137253, 'g': 0.5176470588235295, 'r': 0.44313725490196076}}, 'polygon': [{'x': 3.626, 'y': 0, 'z': 5.44}, {'x': 0.0, 'y': 0, 'z': 5.44}, {'x': 3.626, 'y': 4.339345902674191, 'z': 5.44}, {'x': 0.0, 'y': 4.339345902674191, 'z': 5.44}], 'roomId': 'room|6', 'layer': 'Procedural1'}, {'color': {'b': 0.49019607843137253, 'g': 0.5176470588235295, 'r': 0.44313725490196076}, 'id': 'wall|6|0.00|5.44|0.00|7.25', 'material': {'name': 'PureWhite', 'color': {'b': 0.49019607843137253, 'g': 0.5176470588235295, 'r': 0.44313725490196076}}, 'polygon': [{'x': 0.0, 'y': 0, 'z': 5.44}, {'x': 0.0, 'y': 0, 'z': 7.253}, {'x': 0.0, 'y': 4.339345902674191, 'z': 5.44}, {'x': 0.0, 'y': 4.339345902674191, 'z': 7.253}], 'roomId': 'room|6', 'layer': 'Procedural1'}, {'color': {'b': 0.49019607843137253, 'g': 0.5176470588235295, 'r': 0.44313725490196076}, 'id': 'wall|6|0.00|7.25|7.25|7.25', 'material': {'name': 'PureWhite', 'color': {'b': 0.49019607843137253, 'g': 0.5176470588235295, 'r': 0.44313725490196076}}, 'polygon': [{'x': 0.0, 'y': 0, 'z': 7.253}, {'x': 7.253, 'y': 0, 'z': 7.253}, {'x': 0.0, 'y': 4.339345902674191, 'z': 7.253}, {'x': 7.253, 'y': 4.339345902674191, 'z': 7.253}], 'roomId': 'room|6', 'layer': 'Procedural1'}, {'color': {'b': 0.49019607843137253, 'g': 0.5176470588235295, 'r': 0.44313725490196076}, 'id': 'wall|6|7.25|3.63|7.25|7.25', 'material': {'name': 'PureWhite', 'color': {'b': 0.49019607843137253, 'g': 0.5176470588235295, 'r': 0.44313725490196076}}, 'polygon': [{'x': 7.253, 'y': 0, 'z': 7.253}, {'x': 7.253, 'y': 0, 'z': 3.626}, {'x': 7.253, 'y': 4.339345902674191, 'z': 7.253}, {'x': 7.253, 'y': 4.339345902674191, 'z': 3.626}], 'roomId': 'room|6', 'layer': 'Procedural1'}, {'color': {'b': 0.9333333333333333, 'g': 0.9490196078431372, 'r': 0.9450980392156862}, 'id': 'wall|7|0.00|1.81|0.00|5.44', 'material': {'name': 'PureWhite', 'color': {'b': 0.9333333333333333, 'g': 0.9490196078431372, 'r': 0.9450980392156862}}, 'polygon': [{'x': 0.0, 'y': 0, 'z': 1.813}, {'x': 0.0, 'y': 0, 'z': 5.44}, {'x': 0.0, 'y': 4.339345902674191, 'z': 1.813}, {'x': 0.0, 'y': 4.339345902674191, 'z': 5.44}], 'roomId': 'room|7', 'layer': 'Procedural1'}, {'color': {'b': 0.9333333333333333, 'g': 0.9490196078431372, 'r': 0.9450980392156862}, 'empty': true, 'id': 'wall|7|0.00|5.44|3.63|5.44', 'material': {'name': 'PureWhite', 'color': {'b': 0.9333333333333333, 'g': 0.9490196078431372, 'r': 0.9450980392156862}}, 'polygon': [{'x': 0.0, 'y': 0, 'z': 5.44}, {'x': 3.626, 'y': 0, 'z': 5.44}, {'x': 0.0, 'y': 4.339345902674191, 'z': 5.44}, {'x': 3.626, 'y': 4.339345902674191, 'z': 5.44}], 'roomId': 'room|7', 'layer': 'Procedural1'}, {'color': {'b': 0.9333333333333333, 'g': 0.9490196078431372, 'r': 0.9450980392156862}, 'empty': true, 'id': 'wall|7|3.63|3.63|3.63|5.44', 'material': {'name': 'PureWhite', 'color': {'b': 0.9333333333333333, 'g': 0.9490196078431372, 'r': 0.9450980392156862}}, 'polygon': [{'x': 3.626, 'y': 0, 'z': 5.44}, {'x': 3.626, 'y': 0, 'z': 3.626}, {'x': 3.626, 'y': 4.339345902674191, 'z': 5.44}, {'x': 3.626, 'y': 4.339345902674191, 'z': 3.626}], 'roomId': 'room|7', 'layer': 'Procedural1'}, {'color': {'b': 0.9333333333333333, 'g': 0.9490196078431372, 'r': 0.9450980392156862}, 'id': 'wall|7|3.63|1.81|3.63|3.63', 'material': {'name': 'PureWhite', 'color': {'b': 0.9333333333333333, 'g': 0.9490196078431372, 'r': 0.9450980392156862}}, 'polygon': [{'x': 3.626, 'y': 0, 'z': 3.626}, {'x': 3.626, 'y': 0, 'z': 1.813}, {'x': 3.626, 'y': 4.339345902674191, 'z': 3.626}, {'x': 3.626, 'y': 4.339345902674191, 'z': 1.813}], 'roomId': 'room|7', 'layer': 'Procedural1'}, {'color': {'b': 0.9333333333333333, 'g': 0.9490196078431372, 'r': 0.9450980392156862}, 'id': 'wall|7|0.00|1.81|3.63|1.81', 'material': {'name': 'PureWhite', 'color': {'b': 0.9333333333333333, 'g': 0.9490196078431372, 'r': 0.9450980392156862}}, 'polygon': [{'x': 3.626, 'y': 0, 'z': 1.813}, {'x': 0.0, 'y': 0, 'z': 1.813}, {'x': 3.626, 'y': 4.339345902674191, 'z': 1.813}, {'x': 0.0, 'y': 4.339345902674191, 'z': 1.813}], 'roomId': 'room|7', 'layer': 'Procedural1'}, {'id': 'wall|exterior|0.00|1.81|0.00|5.44', 'material': {'name': 'FireplaceTiles2'}, 'polygon': [{'x': 0.0, 'y': 4.339345902674191, 'z': 5.44}, {'x': 0.0, 'y': 4.339345902674191, 'z': 1.813}, {'x': 0.0, 'y': 0, 'z': 5.44}, {'x': 0.0, 'y': 0, 'z': 1.813}], 'roomId': 'exterior', 'layer': 'Procedural1'}, {'id': 'wall|exterior|0.00|1.81|3.63|1.81', 'material': {'name': 'FireplaceTiles2'}, 'polygon': [{'x': 0.0, 'y': 4.339345902674191, 'z': 1.813}, {'x': 3.626, 'y': 4.339345902674191, 'z': 1.813}, {'x': 0.0, 'y': 0, 'z': 1.813}, {'x': 3.626, 'y': 0, 'z': 1.813}], 'roomId': 'exterior', 'layer': 'Procedural1'}, {'id': 'wall|exterior|7.25|3.63|7.25|7.25', 'material': {'name': 'FireplaceTiles2'}, 'polygon': [{'x': 7.253, 'y': 4.339345902674191, 'z': 3.626}, {'x': 7.253, 'y': 4.339345902674191, 'z': 7.253}, {'x': 7.253, 'y': 0, 'z': 3.626}, {'x': 7.253, 'y': 0, 'z': 7.253}], 'roomId': 'exterior', 'layer': 'Procedural1'}, {'id': 'wall|exterior|0.00|5.44|0.00|7.25', 'material': {'name': 'FireplaceTiles2'}, 'polygon': [{'x': 0.0, 'y': 4.339345902674191, 'z': 7.253}, {'x': 0.0, 'y': 4.339345902674191, 'z': 5.44}, {'x': 0.0, 'y': 0, 'z': 7.253}, {'x': 0.0, 'y': 0, 'z': 5.44}], 'roomId': 'exterior', 'layer': 'Procedural1'}, {'id': 'wall|exterior|0.00|7.25|7.25|7.25', 'material': {'name': 'FireplaceTiles2'}, 'polygon': [{'x': 7.253, 'y': 4.339345902674191, 'z': 7.253}, {'x': 0.0, 'y': 4.339345902674191, 'z': 7.253}, {'x': 7.253, 'y': 0, 'z': 7.253}, {'x': 0.0, 'y': 0, 'z': 7.253}], 'roomId': 'exterior', 'layer': 'Procedural1'}, {'id': 'wall|exterior|7.25|0.00|7.25|3.63', 'material': {'name': 'FireplaceTiles2'}, 'polygon': [{'x': 7.253, 'y': 4.339345902674191, 'z': 0.0}, {'x': 7.253, 'y': 4.339345902674191, 'z': 3.626}, {'x': 7.253, 'y': 0, 'z': 0.0}, {'x': 7.253, 'y': 0, 'z': 3.626}], 'roomId': 'exterior', 'layer': 'Procedural2'}, {'id': 'wall|exterior|5.44|0.00|5.44|1.81', 'material': {'name': 'FireplaceTiles2'}, 'polygon': [{'x': 5.44, 'y': 4.339345902674191, 'z': 1.813}, {'x': 5.44, 'y': 4.339345902674191, 'z': 0.0}, {'x': 5.44, 'y': 0, 'z': 1.813}, {'x': 5.44, 'y': 0, 'z': 0.0}], 'roomId': 'exterior', 'layer': 'Procedural2'}, {'id': 'wall|exterior|5.44|0.00|7.25|0.00', 'material': {'name': 'FireplaceTiles2'}, 'polygon': [{'x': 5.44, 'y': 4.339345902674191, 'z': 0.0}, {'x': 7.253, 'y': 4.339345902674191, 'z': 0.0}, {'x': 5.44, 'y': 0, 'z': 0.0}, {'x': 7.253, 'y': 0, 'z': 0.0}], 'roomId': 'exterior', 'layer': 'Procedural2'}, {'id': 'wall|exterior|3.63|1.81|5.44|1.81', 'material': {'name': 'FireplaceTiles2'}, 'polygon': [{'x': 3.626, 'y': 4.339345902674191, 'z': 1.813}, {'x': 5.44, 'y': 4.339345902674191, 'z': 1.813}, {'x': 3.626, 'y': 0, 'z': 1.813}, {'x': 5.44, 'y': 0, 'z': 1.813}], 'roomId': 'exterior', 'layer': 'Procedural2'}], 'windows': [{'assetId': 'Window_Slider_48x48', 'id': 'window|2|0', 'room0': 'room|2', 'room1': 'room|2', 'wall0': 'wall|2|7.25|0.00|7.25|3.63', 'wall1': 'wall|exterior|7.25|0.00|7.25|3.63', 'holePolygon': [{'x': 0.0519079643737399, 'y': 0.8893231153488159, 'z': 0}, {'x': 1.2681518274795138, 'y': 2.102747529745102, 'z': 0}], 'assetPosition': {'x': 0.6610103327285373, 'y': 1.4979770481586456, 'z': 0}}]}";
       Debug.Log($"json: {jsonStr}");
 
       JObject obj = JObject.Parse(jsonStr);
 
       //    action["house"] = obj;
       //    CurrentActiveController().ProcessControlCommand(new DynamicServerAction(action));
       Debug.Log("Reached 2");
       _agentManager.PrimaryAgent.CreateHouse(obj.ToObject<ProceduralHouse>());   
   }

    // Called when you want to toggle controller mode
    public void ToggleLocomotionMode() {
        if (!_isInitialized) {
            return;
        }

        Debug.Log("[RECORDING ACTION] In Game Toggle Locomotion Button Pressed");
        _locomotionMode = ~_locomotionMode;
        bool value = Convert.ToBoolean((int)_locomotionMode);

        StopCoroutine("FadeNotificationCoroutine");

        if (value) {
            _onAgentLocomotionEvent?.Invoke();
            _notificationText.text = "Locomotion: <color=#0000FF>Agent</color>";
            _locomotionText.text = _notificationText.text;
            StartCoroutine("FadeNotificationCoroutine");
        } else {
            _onUserLocomotionEvent?.Invoke();
            _notificationText.text = "Locomotion: <color=#FF0000>User</color>";
            _locomotionText.text = _notificationText.text;
            StartCoroutine("FadeNotificationCoroutine");
        }
    }

    public void TogglePOV() {
        if (!_isInitialized) {
            return;
        }

        Debug.Log("[RECORDING ACTION] In Game Toggle POV Button Pressed");
        _isFPSMode = !_isFPSMode;

        StopCoroutine("FadeNotificationCoroutine");

        if (_isFPSMode) {
            _notificationText.text = "POV: <color=#0000FF>First</color>";
            _povText.text = _notificationText.text;

            // Set locomotion to agent
            _locomotionMode = ControllerMode.agent;
            _onAgentLocomotionEvent?.Invoke();
            _locomotionText.text = "Locomotion: <color=#0000FF>Agent</color>";

            StartCoroutine("FadeNotificationCoroutine");
        } else {
            _notificationText.text = "POV: <color=#FF0000>Third</color>";
            _povText.text = _notificationText.text;
            StartCoroutine("FadeNotificationCoroutine");
        }

        _onTogglePOVEvent?.Invoke(_isFPSMode);
    }

    public void ToggleArm() {
        if (!_isInitialized) {
            return;
        }

        Debug.Log("[RECORDING ACTION] In Game Toggle Arm Button Pressed");
        _isArmMode = !_isArmMode;

        StopCoroutine("FadeNotificationCoroutine");

        if (_isArmMode) {
            _onArmOnEvent?.Invoke(_isArmMode);
            _notificationText.text = "Arm: <color=#0000FF>On</color>";
            _armText.text = _notificationText.text;
            StartCoroutine("FadeNotificationCoroutine");

        } else {
            _onArmOffEvent?.Invoke(_isArmMode);
            _notificationText.text = "Arm: <color=#FF0000>Off</color>";
            _armText.text = _notificationText.text;
            StartCoroutine("FadeNotificationCoroutine");
        }
    }

    public void ArmReset() {
        Debug.Log("[RECORDING ACTION] In Game ResetArm Button Pressed");

        ToggleArm();
        ToggleArm();
    }

    private void ToggleCrouch() {
        if (!_isInitialized || _locomotionMode == ControllerMode.user) {
            return;
        }
        Debug.Log("[RECORDING ACTION] LeftPrimaryButton pressed");
        _isCrouching = !_isCrouching;
        Dictionary<string, object> action = new Dictionary<string, object>();
        
        if (_isCrouching) {
            action["action"] = "Crouch";
            
        } else {
            action["action"] = "Stand";
        }
        CurrentActiveController().ProcessControlCommand(action);
    }

    private void ToggleGuide() {
        _showingGuide = !_showingGuide;
        _onToggleGuideEvent?.Invoke(_showingGuide);
        Debug.Log("[RECORDING ACTION] LeftSecondButton pressed");
    }

    private void ToggleOpenClose() {
        Debug.Log("Check 1 Okayy");
        if (!_isInitialized || !_isArmMode) {
            return;
        }

        _onOpenObjectEvent?.Invoke();
    }

    private void ResetArm() {
        if (!_isArmMode || !_isInitialized) {
            return;
        }
        Debug.Log("[RECORDING ACTION] RightSecondaryButton pressed");

        StopCoroutine("FadeNotificationCoroutine");

        _notificationText.text = "Reset Arm";
        _notificationText.color = Color.white;


        if (_isFPSMode) {
            _notificationText.text = "Reset Arm";
            _onResetArmEvent?.Invoke();
        } else {
            _notificationText.text = "Default Arm";
            _onDefaultArmEvent?.Invoke();
        }
        StartCoroutine("FadeNotificationCoroutine");
    }

    private void ToggleMenu() {
        if (!_isInitialized) {
            return;
        }
        Debug.Log("[RECORDING ACTION] LeftMenuButton pressed");
        _armMenu.gameObject.SetActive(!_armMenu.gameObject.activeSelf);
    }

    private IEnumerator FadeNotificationCoroutine() {
        float timer = 0;
        while (timer < _fadeTime) {
            timer += Time.deltaTime;
            _notificationCanvasGroup.alpha =  timer / _fadeTime;
            yield return null;
        }
        _notificationCanvasGroup.alpha = 1;

        yield return new WaitForSeconds(_fadeTime);

        timer = _fadeTime;
        while (timer > 0) {
            timer -= Time.deltaTime;
            _notificationCanvasGroup.alpha = timer / _fadeTime;
            yield return null;
        }
        _notificationCanvasGroup.alpha = 0;
    }

    /*
     * Events Helper Functions
     */

    public void AddListenerToUserEvent(UnityAction action) {
        _onUserLocomotionEvent.AddListener(action);
    }
    public void RemoveListenerToUserEvent(UnityAction action) {
        _onUserLocomotionEvent.RemoveListener(action);
    }
    public void AddListenerToAgentEvent(UnityAction action) {
        _onAgentLocomotionEvent.AddListener(action);
    }
    public void RemoveListenerToAgentEvent(UnityAction action) {
        _onAgentLocomotionEvent.RemoveListener(action);
    }
    public void AddListenerToInitializeEvent(UnityAction action) {
        _onInitializedEvent.AddListener(action);
    }
    public void RemoveListenerToInitializeEvent(UnityAction action) {
        _onInitializedEvent.RemoveListener(action);
    }
    public void AddListenerToOpenObjectEvent(UnityAction action) {
        _onOpenObjectEvent.AddListener(action);
    }
    public void RemoveListenerToOpenObjectEvent(UnityAction action) {
        _onOpenObjectEvent.RemoveListener(action);
    }

}
