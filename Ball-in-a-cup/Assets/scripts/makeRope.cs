using UnityEngine;
using System.Collections;


public class makeRope : MonoBehaviour {

	static int NUMBER_OF_LINKS     = 500;
	static float LENGTH_MASS_RATIO = 0.001f;
	static int BALL_TO_LINK_RATIO  = 100;
	static float BASE_Y_SCALE      = 0.001f;
	static float LINK_LENGTH       = 0.005f;
	static float INPUT_FORCE       = 5.8f;


	GameObject[] rope_links        = new GameObject[NUMBER_OF_LINKS];
	private GameObject ball;
	private GameObject rope_base;
	private bool       game_running;
	private int        score;


	// Use this for initialization
	void Start () {
		build_rope ();
		score = 0;
	}

	// builds the rope and base, used in initialization of game and during a reset
	void build_rope() {
		GameObject rope_prefab       = Resources.Load<GameObject>("Rope_Segment");
		rope_base                    = GameObject.CreatePrimitive (PrimitiveType.Cube);
		ball             			 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		ball.name					 = "Ball";
		game_running 				 = true;
		var JOINT_SEPARATION         = LINK_LENGTH;
		
		rope_base.transform.localScale = new Vector3 (1, BASE_Y_SCALE, 1); 
		
		
		Rigidbody base_rbody = rope_base.AddComponent<Rigidbody>();
		
		rope_base.transform.position = new Vector3(0, -1.25f, 0); 
		rope_base.GetComponent<BoxCollider> ().size = new Vector3 (0, 0.001f, 0);
		
		base_rbody.constraints       = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
		
		
		
		
		
		// Create the ball
		ball.transform.localScale    = new Vector3 ( BALL_TO_LINK_RATIO * LINK_LENGTH
		                                            , BALL_TO_LINK_RATIO * LINK_LENGTH
		                                            , BALL_TO_LINK_RATIO * LINK_LENGTH );
		ball.transform.parent        = rope_base.transform;
		ball.transform.localPosition = new Vector3 ( 0
		                                            , ((2 * (NUMBER_OF_LINKS + 1) + (BALL_TO_LINK_RATIO / 2)) * LINK_LENGTH + 0.5f * BASE_Y_SCALE) / BASE_Y_SCALE
		                                            , 0
		                                            );
		
		
		
		var ball_rigid_body          = ball.AddComponent<Rigidbody> ();
		ball_rigid_body.mass         = LENGTH_MASS_RATIO * NUMBER_OF_LINKS * 2; // * 0.5f * BASE_Y_SCALE;
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

		float xLoc = ball.transform.position.x;
		float yLoc = ball.transform.position.y;
		float zLoc = ball.transform.position.z;
				
		if ((xLoc < 0.5f && xLoc > -0.5f) && (yLoc > 0.4f && yLoc < 1f) && (zLoc < 0.5f && zLoc > -0.5f) && game_running){
			score++;
			game_running = false;

			print("In the cup!");
			ball.rigidbody.constraints   = RigidbodyConstraints.FreezeAll;
			foreach(var dl in rope_links){
				dl.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
			}
		}
		else if (game_running){
			if (Input.GetKey (KeyCode.RightArrow)) {
				Physics.gravity = new Vector3 (INPUT_FORCE, 0, 0);
			} else if (Input.GetKey (KeyCode.LeftArrow)) {
				Physics.gravity = new Vector3 (-INPUT_FORCE, 0, 0);
			} else if (Input.GetKey (KeyCode.UpArrow)) {
				Physics.gravity = new Vector3 (0, 0, INPUT_FORCE);
			} else if (Input.GetKey (KeyCode.DownArrow)) {
				Physics.gravity = new Vector3 (0, 0, -INPUT_FORCE);
			} else if (Input.GetKey (KeyCode.W)) {
				Physics.gravity = new Vector3 (0, INPUT_FORCE, 0);
			} else if (Input.GetKey (KeyCode.S)) {
				Physics.gravity = new Vector3 (0, -INPUT_FORCE, 0);
			} else {
				Physics.gravity = new Vector3(0, -9.8f, 0);		
			}
		}
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

	// used to reset the game
	void destroy_rope(){
		Destroy (ball);
		Destroy (rope_base);
		foreach (var dl in rope_links) {
			Destroy (dl);
		}
	}

	// used every frame to check for gui events
	void OnGUI(){


		GUI.TextArea (new Rect (Screen.width * 0.8f , 10, Screen.width / 10, Screen.height / 10), "\nScore:  " + score);
		if (GUI.Button (new Rect (10, 10, Screen.width / 10, Screen.height / 10), "Reset Ball")) {
			print("Reset hit!");
			destroy_rope();
			build_rope();
		}
	}
}