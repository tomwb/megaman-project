using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	[Header ("Configurações")]
	public float maxJumpHeight = 2;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;
	public float moveSpeed = 4;
	
	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;

	float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	Vector3 velocity;
	float velocityXSmoothing;


	
	Controller2D controller;
	Animator animator;
	
	void Start() {
		animator = GetComponent<Animator>();

		controller = GetComponent<Controller2D> ();
		controller.useColliderFunctions = true;

		// defino a gravidade baseado no tamanho do pulo que quero e no tempo de queda
		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);
	}
	
	void Update() {
		flip ();

		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		int wallDirX = (controller.collisions.left) ? -1 : 1;
		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
			
		jump ();

		// aplico a gravidade
		velocity.y += gravity * Time.deltaTime;

		// chamo a função que movimenta de verdade levando em conta as colisões
		controller.Move (velocity * Time.deltaTime, input);

		// caso colidir com o chao ou com o teto eu não ando em Y
		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}

		// variaveis de animação
		animator.SetFloat ("Yvelocity", velocity.y);
		animator.SetFloat ("Xvelocity", Mathf.Abs(velocity.x) );
		if (controller.collisions.below) {
			animator.SetBool ("grounded", true);
		} else {
			animator.SetBool ("grounded", false);
		}
	}

	void jump () {
		if (Input.GetKeyDown (KeyCode.UpArrow)) {
			// pulo normal
			if (controller.collisions.below) {
				velocity.y = maxJumpVelocity;
			}
		}
		// quando solto jogo a velocidade minima de pulo, para pular baixo se soltar rapido
		if (Input.GetKeyUp (KeyCode.UpArrow)) {
			if (velocity.y > minJumpVelocity) {
				velocity.y = minJumpVelocity;
			}
		}
	}

	// inverto o personagem
	void flip(){
		if ( (transform.localScale.x == 1 && Input.GetAxisRaw ("Horizontal") < 0 ) 
		    || (transform.localScale.x == -1 && Input.GetAxisRaw ("Horizontal") > 0 )) {
			velocity.x = 0;
		}

		if (Input.GetAxisRaw ("Horizontal") > 0) {
			transform.localScale = new Vector3( 1.0f, transform.localScale.y,1f);
		}
		if (Input.GetAxisRaw ("Horizontal") < 0) {
			transform.localScale = new Vector3( -1.0f, transform.localScale.y,1f);
		}
	}


}