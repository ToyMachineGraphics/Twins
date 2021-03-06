﻿using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Aisha : NetworkBehaviour
{
    public VRController Controller;
    public VRMenuUI UI;
    private NetworkIdentity _networkId;

    [SerializeField]
    private Flashlight _flashlightPrefab;

    [SerializeField]
    private Flashlight _flashlight;

    public DenryuIrairaBoAgent Agent;

    public Transform FlashlightParent;
    public Transform FlashlightParent2;

    private Light _spotlight;
    public static Aisha Instance = null;

    private Ray _ray;
    private RaycastHit[] _raycastHitBuffer = new RaycastHit[1];
    private int _denryuIrairaBoMask;
    private float _flashlightUpdateInterval = 0.125f;
    private float _flashlightUpdateTimer;

    public Camera Camera;

    public SyncFieldCommand SyncCmd;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        _networkId = GetComponent<NetworkIdentity>();
        _denryuIrairaBoMask = LayerMask.GetMask("DenryuIrairaBo");
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        Controller = VRController.Instance;
        CmdSpawnFlashlight();
        _flashlightUpdateTimer = 0;
        StartCoroutine(SpawnBugs());
        Debug.Log("Aisha OnStartLocalPlayer");
    }

    private void Start()
    {
        if (true || isLocalPlayer)
        {
            //SyncFieldCommand.Instance.OnStageClear -= GameClear;
            SyncCmd.OnStageClear += GameClear;
            WorldText.Instance.text.text = "SyncFieldCommand.Instance.OnStageClear += GameClear";
        }
    }

    private IEnumerator SpawnBugs()
    {
        while (true)
        {
            Debug.Log("DenryuIrairaBoAgent.DenryuIrairaBo != null, AgentCount " + DenryuIrairaBoAgent.AgentCount);
            if (!DenryuIrairaBoAgent.DenryuIrairaBo || DenryuIrairaBoAgent.AgentCount >= 50)
            {
                yield return null;
            }
            else if (DenryuIrairaBoAgent.AgentCount < 50)
            {
                yield return new WaitForSeconds(Random.Range(0.25f, 1.25f));
                DenryuIrairaBoAgent.DenryuIrairaBo.SpawnAgent();
            }
        }
    }

    private void Update()
    {
        if (localPlayerAuthority && Controller)
        {
            if (_flashlight)
            {
                if (_flashlight.gameObject.activeInHierarchy)
                {
                    _flashlight.transform.position = Controller.ControllerModel.transform.position;
                    _flashlight.transform.rotation = Controller.ControllerModel.transform.rotation;
                    _flashlightUpdateTimer += Time.deltaTime;
                    if (_flashlightUpdateTimer > _flashlightUpdateInterval)
                    {
                        _flashlightUpdateTimer = 0;
                        _ray.origin = _flashlight.transform.position;
                        _ray.direction = _flashlight.transform.forward;
                        if (_flashlight.hasAuthority && Physics.RaycastNonAlloc(_ray, _raycastHitBuffer, 64, _denryuIrairaBoMask) > 0)
                        {
                            RaycastHit hit = _raycastHitBuffer[0];
                            CmdSetFlashlightParam(hit.point, Controller.MainCamera.transform.position);
                        }
                    }
                }

                if (UI)
                {
                    if (UI.OnVRMenuUIEnable)
                    {
                        UI.OnVRMenuUIEnable = false;
                        Debug.Log("Aisha update, OnVRMenuUIEnable");
                        _flashlight.CmdUnuseFlashlight();
                    }
                    if (UI.OnOpenFlag != VRMenuUI.OnOpen.None)
                    {
                        if (UI.OnOpenFlag == VRMenuUI.OnOpen.Hierachy1Flashlight)
                        {
                            Debug.Log("Aisha update, OnFlashlightSelected");
                            _flashlight.CmdUseFlashlight();
                            UI.OnOpenFlag = VRMenuUI.OnOpen.None;
                        }
                    }
                    else
                    {
                        //Ray ray = new Ray(Controller.ControllerModel.position, Controller.ControllerModel.forward);
                        //Debug.DrawRay(ray.origin, ray.direction, Color.blue);
                        //int count = Physics.RaycastNonAlloc(ray, _raycastHitBuffer, 100);
                        //if (count > 0)
                        //{
                        //    Vector3 dir = (_raycastHitBuffer[0].point - ray.origin);
                        //    //Debug.DrawLine(ray.origin, ray.origin + dir, Color.blue);
                        //    //Controller.Reticle.position = ray.origin + ray.direction;
                        //    for (int i = 0; i < count; i++)
                        //    {
                        //        RaycastHit hit = _raycastHitBuffer[i];
                        //        CirclePuzzleBehavior c = hit.transform.GetComponent<CirclePuzzleBehavior>();
                        //        if (c)
                        //        {
                        //            c.TriggerRotate();
                        //            break;
                        //        }
                        //    }
                        //}
                    }
                }
                else
                {
                }
            }

            if (Camera == null)
            {
                Camera = Controller.MainCamera;
            }
            Vector3 forward = Controller.MainCamera.transform.forward;
            Vector3 forwardProjected = forward - Vector3.Dot(forward, Vector3.up) * Vector3.up;
            transform.position = Controller.MainCamera.transform.position - forwardProjected * 0.5f + Vector3.down * Controller.PlayerOffset.y;
            transform.LookAt(transform.position + forwardProjected, Vector3.up);
        }
    }

    [Command]
    private void CmdSpawnFlashlight()
    {
        _flashlight = Instantiate(_flashlightPrefab);
        NetworkServer.SpawnWithClientAuthority(_flashlight.gameObject, gameObject);
        RpcSpawnFlashlight(_flashlight.gameObject);
    }

    [ClientRpc]
    private void RpcSpawnFlashlight(GameObject flashlight)
    {
        _flashlight = flashlight.GetComponent<Flashlight>();
        Controller.Flashlight = _flashlight;
        UI = Controller.VRMenuUI.GetComponent<VRMenuUI>();
        _flashlight.gameObject.SetActive(false);
    }

    [Command]
    private void CmdSetFlashlightParam(Vector3 point, Vector3 cameraPosition)
    {
        _flashlight.RayPoint = point;
        _flashlight.LightLength = Vector3.Distance(point, cameraPosition);
    }

    public void GameClear()
    {
        WorldText.Instance.text.text = "GameClear";
        Debug.Log("IN");
        foreach (var item in GameObject.FindGameObjectsWithTag("FinishGroup"))
        {
            Debug.Log(item);
            for (int i = 0; i < item.transform.childCount; i++)
            {
                item.transform.GetChild(i).gameObject.SetActive(true);
            }
            item.gameObject.SetActive(true);
        }
        foreach (var item in GameObject.FindGameObjectsWithTag("FinishTween"))
        {
            Debug.Log(item);
            item.GetComponent<DOTweenAnimation>().DOPlay();
        }
    }
}