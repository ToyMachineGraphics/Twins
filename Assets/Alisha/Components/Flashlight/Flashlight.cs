﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class Flashlight : NetworkBehaviour
{
    public GameObject Model;
    [SerializeField]
    private Light _innerSpotlight;
	[SyncVar]
	public float LightLength = 1;
    public SphereCollider InnerCollider;
    [SerializeField]
    private Light _outerSpotlight;
    public SphereCollider OuterCollider;

    public NetworkIdentity NetId;

    private void Update()
    {
        float innerRadius = LightLength * Mathf.Tan(_innerSpotlight.spotAngle * Mathf.Deg2Rad / 2.0f);
        InnerCollider.radius = innerRadius;
        InnerCollider.transform.position = _innerSpotlight.transform.position + _innerSpotlight.transform.forward * LightLength;

        float outerRadius = LightLength * Mathf.Tan(_outerSpotlight.spotAngle * Mathf.Deg2Rad / 2.0f);
        OuterCollider.radius = outerRadius;
        OuterCollider.transform.position = _outerSpotlight.transform.position + _outerSpotlight.transform.forward * LightLength;

		if (_triggerChanged) {
			_triggerChanged = false;

			_diffSet = _outerLightTriggered.Except (_innerLightTriggered);
			foreach (DenryuIrairaBoAgent a in _diffSet) {
			}
		}
    }

	#region Trigger
	private HashSet<DenryuIrairaBoAgent> _outerLightTriggered = new HashSet<DenryuIrairaBoAgent>();
	private HashSet<DenryuIrairaBoAgent> _innerLightTriggered = new HashSet<DenryuIrairaBoAgent>();
	private IEnumerable<DenryuIrairaBoAgent> _diffSet;
	private bool _triggerChanged;

	public void OnOuterLightTriggerBugEnter(Collider other)
	{
		if (hasAuthority) {
			_triggerChanged = true;
			DenryuIrairaBoAgent agent = other.GetComponent<DenryuIrairaBoAgent> ();
			_outerLightTriggered.Add (agent);
		}
	}

	public void OnOuterLightTriggerBugExit(Collider other)
	{
		if (hasAuthority) {
			_triggerChanged = true;
			DenryuIrairaBoAgent agent = other.GetComponent<DenryuIrairaBoAgent> ();
			_outerLightTriggered.Remove (agent);
		}
	}

	public void OnInnerLightTriggerBugEnter(Collider other)
	{
		if (hasAuthority) {
			_triggerChanged = true;
			DenryuIrairaBoAgent agent = other.GetComponent<DenryuIrairaBoAgent> ();
			_innerLightTriggered.Add (agent);
		}
	}

	public void OnInnerLightTriggerBugExit(Collider other)
	{
		if (hasAuthority) {
			_triggerChanged = true;
			DenryuIrairaBoAgent agent = other.GetComponent<DenryuIrairaBoAgent> ();
			_innerLightTriggered.Remove (agent);
		}
	}
	#endregion
}
