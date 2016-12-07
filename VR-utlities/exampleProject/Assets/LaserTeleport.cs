using UnityEngine;
using System.Collections;
using System;

public struct LaserEventArgs
{
	public uint controllerIndex;
	public uint flags;
	public float distance;
	public Transform target;
}

public delegate void LaserEventHandler(object sender, LaserEventArgs e);

public class LaserTeleport : MonoBehaviour {


	public LayerMask walkables;
	public float maxTelDst = 2f;
	public bool active = true;
	public Color color;
	public float thickness = 0.002f;
	GameObject holder;
	GameObject pointer;
	public Transform head;
	bool isActive = false;
	Transform reference;
	public event LaserEventHandler PointerIn;
	public event LaserEventHandler PointerOut;

	Vector3 lastPoint;

	Transform previousContact = null;

	// Use this for initialization
	void Start()
	{
		var trackedController = GetComponent<SteamVR_TrackedController>();
		if (trackedController == null)
		{
			trackedController = gameObject.AddComponent<SteamVR_TrackedController>();
		}

		trackedController.PadClicked += new ClickedEventHandler(DoClick);

		holder = new GameObject();
		holder.transform.parent = this.transform;
		holder.transform.localPosition = Vector3.zero;
		holder.transform.rotation = this.transform.rotation;


		pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
		Destroy((UnityEngine.Object)pointer.GetComponent<Collider>());
		pointer.transform.parent = holder.transform;
		pointer.transform.localScale = new Vector3(thickness, thickness, 100f);
		pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
		Material newMaterial = new Material(Shader.Find("Unlit/Color"));
		newMaterial.SetColor("_Color", color);
		pointer.GetComponent<MeshRenderer>().material = newMaterial;
	}

	private void DoClick(object sender, ClickedEventArgs e)
	{
		//Teleport to laser point
		Ray ray = new Ray(holder.transform.position, holder.transform.forward);	
		RaycastHit hit = new RaycastHit();
		if (Physics.Raycast(ray, out hit, maxTelDst, walkables))
		{
			lastPoint = hit.point;
			Vector3 offset = new Vector3(transform.parent.position.x - head.position.x, 0, transform.parent.position.z - head.position.z);
			transform.parent.position = hit.point + offset;
		}
	}

	public virtual void OnPointerIn(LaserEventArgs e)
	{
		if (PointerIn != null)
			PointerIn(this, e);
	}

	public virtual void OnPointerOut(LaserEventArgs e)
	{
		if (PointerOut != null)
			PointerOut(this, e);
	}


	// Update is called once per frame
	void Update()
	{
		
		if (!isActive)
		{
			isActive = true;
			this.transform.GetChild(0).gameObject.SetActive(true);
		}

		float dist = 100f;

		SteamVR_TrackedController controller = GetComponent<SteamVR_TrackedController>();

		Ray raycast = new Ray(transform.position, transform.forward);
		RaycastHit hit;
		bool bHit = Physics.Raycast(raycast, out hit);

		if (previousContact && previousContact != hit.transform)
		{
			LaserEventArgs args = new LaserEventArgs();
			if (controller != null)
			{
				args.controllerIndex = controller.controllerIndex;
			}
			args.distance = 0f;
			args.flags = 0;
			args.target = previousContact;
			OnPointerOut(args);
			previousContact = null;
		}
		if (bHit && previousContact != hit.transform)
		{
			LaserEventArgs argsIn = new LaserEventArgs();
			if (controller != null)
			{
				argsIn.controllerIndex = controller.controllerIndex;
			}
			argsIn.distance = hit.distance;
			argsIn.flags = 0;
			argsIn.target = hit.transform;
			OnPointerIn(argsIn);
			previousContact = hit.transform;
		}
		if (!bHit)
		{
			previousContact = null;
		}
		if (bHit && hit.distance < 100f)
		{
			dist = hit.distance;
		}

		if (controller != null && controller.triggerPressed)
		{
			pointer.transform.localScale = new Vector3(thickness * 5f, thickness * 5f, dist);
		}
		else
		{
			pointer.transform.localScale = new Vector3(thickness, thickness, dist);
		}
		pointer.transform.localPosition = new Vector3(0f, 0f, dist / 2f);
	}
}