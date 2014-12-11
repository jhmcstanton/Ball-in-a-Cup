using UnityEngine;
using System.Collections;


public class makeRope : MonoBehaviour {

	static int NUMBER_OF_LINKS     = 500;
	static float LENGTH_MASS_RATIO = 0.001f;
	static int BALL_TO_LINK_RATIO  = 20;
	static float BASE_Y_SCALE      = 0.001f;
	static float LINK_LENGTH       = 0.005f;


	GameObject[] rope_links        = new GameObject[NUMBER_OF_LINKS];



	// Use this for initialization
	void Start () {
		GameObject rope_prefab       = Resources.Load<GameObject>("Rope_Segment");
		GameObject rope_base         = GameObject.CreatePrimitive (PrimitiveType.Cube);
		GameObject ball              = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		ConfigurableJoint dl_joint;
		var JOINT_SEPARATION         = LINK_LENGTH;//new Vector3 (LINK_LENGTH, LINK_LENGTH, LINK_LENGTH);
	    

		rope_base.transform.localScale = new Vector3 (1, BASE_Y_SCALE, 1); //BASE_Y_SCALE,1);
		//rope_base.GetComponent<BoxCollider> ().size = new Vector3 (1, 1, 1); //BASE_Y_SCALE, 1);
		
		Rigidbody base_rbody = rope_base.AddComponent<Rigidbody>();
		
		rope_base.transform.position = new Vector3(0, -1.25f, 0); // Change this when working!
		rope_base.GetComponent<BoxCollider> ().size = new Vector3 (0, 0.001f, 0);
		//base_rbody.freezeRotation    = true;
		base_rbody.constraints       = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
		//rope_base.GetComponent<BoxCollider> ().enabled = false;                              
		//rope_base.GetComponent<MeshRenderer> ().enabled = false;		                     




		// Create the ball
		ball.transform.localScale    = new Vector3 ( BALL_TO_LINK_RATIO * LINK_LENGTH
		                                           , BALL_TO_LINK_RATIO * LINK_LENGTH
		                                           , BALL_TO_LINK_RATIO * LINK_LENGTH );
		ball.transform.parent        = rope_base.transform;
		ball.transform.localPosition = new Vector3 ( 0
		                                           , (2 * (NUMBER_OF_LINKS + 2) * LINK_LENGTH + 0.5f * BASE_Y_SCALE) / BASE_Y_SCALE
		                                           , 0
		                                           );



		var ball_rigid_body          = ball.AddComponent<Rigidbody> ();
		ball_rigid_body.mass         = LENGTH_MASS_RATIO * NUMBER_OF_LINKS * 0.5f * BASE_Y_SCALE;
		ball_rigid_body.useGravity   = true;


		// Create the final string link
		GameObject dl              = Instantiate(rope_prefab) as GameObject; 
		dl.transform.parent        = rope_base.transform;
		dl.transform.localPosition = new Vector3 (0, (2 * (NUMBER_OF_LINKS * LINK_LENGTH) + .5f * BASE_Y_SCALE) / BASE_Y_SCALE, 0);
		
		var dl_rigid_body          = dl.AddComponent<Rigidbody> ();
		dl_rigid_body.mass         = LENGTH_MASS_RATIO;


	 
		rope_links [NUMBER_OF_LINKS - 1] = dl;
		for (int i = NUMBER_OF_LINKS - 2; i >= 0; i--) {
			dl = Instantiate(rope_prefab) as GameObject;
			dl.transform.parent          = rope_base.transform;
			dl.transform.localPosition   = new Vector3(0, (2 * (LINK_LENGTH * (i+1)) + .5f * BASE_Y_SCALE) / BASE_Y_SCALE, 0); 

			if(i % 3 == 0){
				dl.GetComponent<CapsuleCollider> ().enabled = true;
			}
			dl_rigid_body                = dl.AddComponent<Rigidbody>();
			dl_rigid_body.mass           = (NUMBER_OF_LINKS - i) * LENGTH_MASS_RATIO;

			make_joint(dl, rope_links[i+1].GetComponent<Rigidbody>());

			rope_links[i] = dl;
		}
		make_joint (rope_base, rope_links [0].GetComponent<Rigidbody>());

		make_joint (rope_links [NUMBER_OF_LINKS - 1], ball.GetComponent<Rigidbody> ());

		rope_base.transform.Rotate (new Vector3 (0, 0, 180));
	}
	
	// Update is called once per frame
	void Update () {
	}

	void make_joint(GameObject base_object, Rigidbody connected){
		var joint                = base_object.AddComponent<ConfigurableJoint>();
		SoftJointLimit joint_lim = new SoftJointLimit ();
	
		joint_lim.bounciness     = 0.001f;
		joint_lim.spring         = 0.001f;
		joint_lim.damper         = 100000;
		joint.linearLimit        = joint_lim;

		joint.xMotion            = ConfigurableJointMotion.Locked;
		joint.yMotion            = ConfigurableJointMotion.Locked;
		joint.zMotion            = ConfigurableJointMotion.Locked;

		joint.connectedBody      = connected;
		//joint.projectionDistance = LINK_LENGTH;
    }
}