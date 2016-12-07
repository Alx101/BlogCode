using UnityEngine;
using System.Collections;

public class GrabObjectScript : MonoBehaviour
{
	public LayerMask grabables;
	public float radius = 0.07f;
	SteamVR_TrackedObject trackedObj;
	GameObject grabbedObject = null;

	void Awake()
	{
		trackedObj = GetComponent<SteamVR_TrackedObject>();
	}
							   
	void Start ()
	{
		SphereCollider col = gameObject.AddComponent<SphereCollider>();
		col.isTrigger = true;
		col.radius = radius;
		col.center = new Vector3(0f, -0.03f, 0f);
	}
							  
	void FixedUpdate ()
	{
		SteamVR_Controller.Device device = SteamVR_Controller.Input((int)trackedObj.index);
		if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger) && grabbedObject != null)
		{
			Collider col = grabbedObject.GetComponent<Collider>();
			col.attachedRigidbody.isKinematic = false;
			col.gameObject.transform.SetParent(null);
			tossObject(col.attachedRigidbody, device);
			grabbedObject = null;
		} 
		
	}

	void OnTriggerStay(Collider col)
	{
		SteamVR_Controller.Device device = SteamVR_Controller.Input((int)trackedObj.index);
        if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger) && grabbedObject == null)
        {
			col.attachedRigidbody.isKinematic = true;
			col.gameObject.transform.SetParent(this.transform);
			grabbedObject = col.gameObject;
		} 
	}

	void tossObject(Rigidbody rigidbody, SteamVR_Controller.Device device)
	{
		Transform origin = trackedObj.origin ? trackedObj.origin : trackedObj.transform.parent;
		if (origin != null)
		{
			rigidbody.velocity = origin.TransformVector(device.velocity);
			rigidbody.angularVelocity = origin.TransformVector(device.angularVelocity);
		}
		else
		{
			rigidbody.velocity = device.velocity;
			rigidbody.angularVelocity = device.angularVelocity;
		}
	}
}
