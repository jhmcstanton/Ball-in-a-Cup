using UnityEngine;
using System.Collections;

public class makeRope : MonoBehaviour {

	static int NUMBER_OF_LINKS     = 600;
	static float LENGTH_MASS_RATIO = 0.001f;
	static int BALL_TO_LINK_RATIO  = 20;
	static float BASE_Y_SCALE      = 0.001f;
	float LINK_LENGTH              = 0.005f;

	GameObject[] rope_links        = new GameObject[NUMBER_OF_LINKS];




	// Use this for initialization
	void Start () {
		GameObject   rope_prefab       = Resources.Load<GameObject>("Rope_Segment");
		// Create base objects
		GameObject rope_base           = GameObject.CreatePrimitive (PrimitiveType.Cube);
		
		GameObject ball                = GameObject.CreatePrimitive(PrimitiveType.Sphere);

		rope_base.transform.localScale = new Vector3 (1, BASE_Y_SCALE, 1); //BASE_Y_SCALE,1);
		//rope_base.GetComponent<BoxCollider> ().size = new Vector3 (1, 1, 1); //BASE_Y_SCALE, 1);

		Rigidbody base_rbody = rope_base.AddComponent<Rigidbody>();
		
		rope_base.transform.position = new Vector3(0,1,-9); // Change this when working!
		//base_rbody.freezeRotation    = true;
		base_rbody.constraints       = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
		//rope_base.GetComponent<BoxCollider> ().enabled = false;                              
				                     




		// Create the ball
		ball.transform.localScale    = new Vector3 ( BALL_TO_LINK_RATIO * LINK_LENGTH
		                                           , BALL_TO_LINK_RATIO * LINK_LENGTH
		                                           , BALL_TO_LINK_RATIO * LINK_LENGTH );
		ball.transform.parent        = rope_base.transform;
		ball.transform.localPosition = new Vector3 (0
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

	    // add joint to the ball here
		rope_links [NUMBER_OF_LINKS - 1] = dl;
		for (int i = NUMBER_OF_LINKS - 2; i >= 0; i--) {
			dl = Instantiate(rope_prefab) as GameObject;
			dl.transform.parent          = rope_base.transform;
			dl.transform.localPosition   = new Vector3(0, (2 * (LINK_LENGTH * (i+1)) + .5f * BASE_Y_SCALE) / BASE_Y_SCALE, 0); 
			dl_rigid_body                = dl.AddComponent<Rigidbody>();
			dl_rigid_body.mass           = LENGTH_MASS_RATIO * (NUMBER_OF_LINKS - i) ;

			var dl_joint                 = dl.AddComponent<HingeJoint>();
			dl_joint.connectedBody       = rope_links[i+1].GetComponent<Rigidbody>();

			rope_links[i] = dl;
		}
		// Link the base to the first link and the ball to the last link
		rope_base.AddComponent <HingeJoint> ().connectedBody = rope_links[0].GetComponent<Rigidbody>();
		rope_links [NUMBER_OF_LINKS - 1].AddComponent<HingeJoint> ().connectedBody = ball_rigid_body;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
