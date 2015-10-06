using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Controller2D : RaycastController {
	
	float maxClimbAngle = 80;
	float maxDescendAngle = 80;
	
	public CollisionInfo collisions;
	[HideInInspector]
	public Vector2 playerInput;

	[HideInInspector]
	public bool useColliderFunctions = false;
	[HideInInspector]
	List<RaycastHit2D> ColissionList = new List<RaycastHit2D>();

	public override void Start() {
		base.Start ();
		collisions.faceDir = 1;
		
	}
	
	public void Move(Vector3 velocity, bool standingOnPlatform) {
		Move (velocity, Vector2.zero, standingOnPlatform);
	}
	
	public void Move(Vector3 velocity, Vector2 input, bool standingOnPlatform = false) {
		UpdateRaycastOrigins ();
		collisions.Reset ();
		collisions.velocityOld = velocity;
		playerInput = input;
		
		if (velocity.x != 0) {
			collisions.faceDir = (int)Mathf.Sign(velocity.x);
		}
		
		if (velocity.y < 0) {
			DescendSlope(ref velocity);
		}
		
		HorizontalCollisions (ref velocity);
		if (velocity.y != 0) {
			VerticalCollisions (ref velocity);
		}

		if ( useColliderFunctions ) {
			checkColissionFunctions ();
		}
		
		transform.Translate (velocity);
		
		if (standingOnPlatform) {
			collisions.below = true;
		}
	}

	void checkColissionFunctions(){
		List<RaycastHit2D> tempColissionList = new List<RaycastHit2D>();

		float directionX = 0;
		float directionY = 0;
		float rayLength = 0.05f;
		
		for (int i = 0; i < horizontalRayCount; i ++) {

			// direita
			directionX = Mathf.Sign (1);
			Vector2 rayOrigin = (directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D[] hitCollision = Physics2D.RaycastAll(rayOrigin, Vector2.right * directionX, rayLength);
//			Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength,Color.blue);
			for (int j = 0; j < hitCollision.Length; j ++) {
				if ( hitCollision[j] ) {

					// caso colidir e n estiver no array
					if (hitCollision[j].collider != this.collider && ! RaycastHit2DExistInList(tempColissionList,hitCollision[j])  ) {
						tempColissionList.Add(hitCollision[j]);
					}
				}
			}


			//esquerda
			directionX = Mathf.Sign (-1);
			rayOrigin = (directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			hitCollision = Physics2D.RaycastAll(rayOrigin, Vector2.right * directionX, rayLength);
//			Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength,Color.blue);
			for (int j = 0; j < hitCollision.Length; j ++) {
				if (hitCollision[j]) {
					if (hitCollision[j].collider != this.collider && ! RaycastHit2DExistInList(tempColissionList,hitCollision[j])  ) {
						tempColissionList.Add(hitCollision[j]);
					}
				}
			}
		}
		
		for (int i = 0; i < verticalRayCount; i ++) {
			//cima
			directionY = Mathf.Sign (1);
			Vector2 rayOrigin = (directionY == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i);
			RaycastHit2D[] hitCollision = Physics2D.RaycastAll(rayOrigin, Vector2.up * directionY, rayLength);
			
//			Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength,Color.blue);
			for (int j = 0; j < hitCollision.Length; j ++) {
				if (hitCollision[j]) {
					if (hitCollision[j].collider != this.collider && ! RaycastHit2DExistInList(tempColissionList,hitCollision[j])  ) {
						tempColissionList.Add(hitCollision[j]);
					}
				}
			}

			//baixo
			directionY = Mathf.Sign (-1);
			rayOrigin = (directionY == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i);
			hitCollision = Physics2D.RaycastAll(rayOrigin, Vector2.up * directionY, rayLength);
			
//			Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength,Color.blue);
			for (int j = 0; j < hitCollision.Length; j ++) {
				if (hitCollision[j]) {
					if (hitCollision[j].collider != this.collider && ! RaycastHit2DExistInList(tempColissionList,hitCollision[j])  ) {
						tempColissionList.Add(hitCollision[j]);
					}
				}
			}
		}


		// caso seja uma colisao nova que não estava no array, chamo o colisionEnter
		foreach (RaycastHit2D tempColission in tempColissionList) {
			if ( ! RaycastHit2DExistInList(ColissionList,tempColission)  ) {
				this.gameObject.SendMessage("RaycastOnCollisionEnter", tempColission,SendMessageOptions.DontRequireReceiver );
			}
		}
		//caso algo tenha saido do array chamo o colisionOut
		foreach (RaycastHit2D tempColission in ColissionList) {
			if ( ! RaycastHit2DExistInList(tempColissionList,tempColission)  ) {
				this.gameObject.SendMessage("RaycastOnCollisionOut", tempColission,SendMessageOptions.DontRequireReceiver );
			}
		}

		// caso ainda esteja colidindo chamo o collisionStay
		foreach (RaycastHit2D tempColission in tempColissionList) {
			if ( RaycastHit2DExistInList(ColissionList,tempColission)  ) {
				this.gameObject.SendMessage("RaycastOnCollisionStay", tempColission,SendMessageOptions.DontRequireReceiver );
			}
		}

		ColissionList = tempColissionList;

	}

	bool RaycastHit2DExistInList(List<RaycastHit2D> list, RaycastHit2D value)
	{
		foreach (RaycastHit2D temp in list) {
			if( temp.collider == value.collider ){
				return true;
			}
		}
		return false;
	}

	// colisão em X -  horizontal 
	void HorizontalCollisions(ref Vector3 velocity) {
		float directionX = collisions.faceDir;
		float rayLength = Mathf.Abs (velocity.x) + skinWidth;
		
		if (Mathf.Abs(velocity.x) < skinWidth) {
			rayLength = 2*skinWidth;
		}
		
		for (int i = 0; i < horizontalRayCount; i ++) {
			Vector2 rayOrigin = (directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
			
			Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength,Color.red);
			
			if (hit) {
				
				if (hit.distance == 0) {
					continue;
				}
				
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				
				if (i == 0 && slopeAngle <= maxClimbAngle) {
					if (collisions.descendingSlope) {
						collisions.descendingSlope = false;
						velocity = collisions.velocityOld;
					}
					float distanceToSlopeStart = 0;
					if (slopeAngle != collisions.slopeAngleOld) {
						distanceToSlopeStart = hit.distance-skinWidth;
						velocity.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope(ref velocity, slopeAngle);
					velocity.x += distanceToSlopeStart * directionX;
				}
				
				if (!collisions.climbingSlope || slopeAngle > maxClimbAngle) {
					velocity.x = (hit.distance - skinWidth) * directionX;
					rayLength = hit.distance;
					
					if (collisions.climbingSlope) {
						velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
					}
					
					collisions.left = directionX == -1;
					collisions.right = directionX == 1;
				}
			}
		}
	}

	// colisão em Y -  vertical 
	void VerticalCollisions(ref Vector3 velocity) {
		float directionY = Mathf.Sign (velocity.y);
		float rayLength = Mathf.Abs (velocity.y) + skinWidth;
		
		for (int i = 0; i < verticalRayCount; i ++) {
			
			Vector2 rayOrigin = (directionY == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
			
			Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength,Color.red);
			
			if (hit) {
				// caso tenha esta tag ela deixa vc passar ao pular sobre
				if (hit.collider.tag == "Through") {
					if (directionY == 1 || hit.distance == 0) {
						continue;
					}
					if (collisions.fallingThroughPlatform) {
						continue;
					}
					// caso ele aperte para baixo eu desço ele
					if (playerInput.y == -1) {
						collisions.fallingThroughPlatform = true;
						Invoke("ResetFallingThroughPlatform",.2f);
						continue;
					}
				}
				
				velocity.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;
				
				if (collisions.climbingSlope) {
					velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
				}
				
				collisions.below = directionY == -1;
				collisions.above = directionY == 1;
			}
		}
		
		if (collisions.climbingSlope) {
			float directionX = Mathf.Sign(velocity.x);
			rayLength = Mathf.Abs(velocity.x) + skinWidth;
			Vector2 rayOrigin = ((directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight) + Vector2.up * velocity.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin,Vector2.right * directionX,rayLength,collisionMask);
			
			if (hit) {
				float slopeAngle = Vector2.Angle(hit.normal,Vector2.up);
				if (slopeAngle != collisions.slopeAngle) {
					velocity.x = (hit.distance - skinWidth) * directionX;
					collisions.slopeAngle = slopeAngle;
				}
			}
		}
	}
	
	void ClimbSlope(ref Vector3 velocity, float slopeAngle) {
		float moveDistance = Mathf.Abs (velocity.x);
		float climbVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;
		
		if (velocity.y <= climbVelocityY) {
			velocity.y = climbVelocityY;
			velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
			collisions.below = true;
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
		}
	}
	
	void DescendSlope(ref Vector3 velocity) {
		float directionX = Mathf.Sign (velocity.x);
		Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
		RaycastHit2D hit = Physics2D.Raycast (rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);
		
		if (hit) {
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
			if (slopeAngle != 0 && slopeAngle <= maxDescendAngle) {
				if (Mathf.Sign(hit.normal.x) == directionX) {
					if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x)) {
						float moveDistance = Mathf.Abs(velocity.x);
						float descendVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;
						velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
						velocity.y -= descendVelocityY;
						
						collisions.slopeAngle = slopeAngle;
						collisions.descendingSlope = true;
						collisions.below = true;
					}
				}
			}
		}
	}
	
	void ResetFallingThroughPlatform() {
		collisions.fallingThroughPlatform = false;
	}

	public struct CollisionInfo {
		public bool above, below;
		public bool left, right;
		
		public bool climbingSlope;
		public bool descendingSlope;
		public float slopeAngle, slopeAngleOld;
		public Vector3 velocityOld;
		public int faceDir;
		public bool fallingThroughPlatform;
		
		public void Reset() {
			above = below = false;
			left = right = false;
			climbingSlope = false;
			descendingSlope = false;
			
			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
	
}