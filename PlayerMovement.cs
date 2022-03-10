using UnityEngine;
using System.Collections;
using DG.Tweening;

public class PlayerMovement : MonoBehaviour
{
	[SerializeField]
	PlayerController playerController;
	public float rotationPeriod = 0.3f; 
	Vector3 scale;                      
	public bool isRotate = false;       
	float directionX = 0;               
	float directionZ = 0;               

	float startAngleRad = 0;            
	Vector3 startPos;                   
	float rotationTime = 0;             
	float radius = 1;                   
	Quaternion fromRotation;               
	Quaternion toRotation;                  
	[SerializeField]
	Vector2 pos1,pos2;
	Vector2 lastPos1, lastPos2;
	[SerializeField]
	bool canMove;
	[SerializeField]
	float boxScaleX;
	[SerializeField]
	float boxScaleY;
	[SerializeField]
	float boxScaleZ;
	[SerializeField]
	Ease spawnEase=Ease.InQuad;
	[SerializeField]
	float spawnDur=1f,spawnHeight=10f;
	[SerializeField]
	AnimationCurve animationCurve;

	public float fallinSpeed;
	public float fallinRotSpeed;
	public Coroutine fallinCoroutine;

	void Start()
	{
		playerController = GetComponent<PlayerController>();
		scale = transform.lossyScale;
		//Debug.Log ("[x, y, z] = [" + boxScaleX + ", " + boxScaleY + ", " + boxScaleZ + "]");
	}

	public void SetPos(Vector2 newPos1, Vector2 newPos2)
    {
		pos1 = newPos1;
		pos2 = newPos2;
    }

	public Vector2 GetPos1()
    {
		return pos1;
    }

	public Vector2 GetPos2()
	{
		return pos2;
	}

	public void SetCanMove(bool value)
    {
		canMove = value;
    }

	public bool CanMove()
	{
		return canMove;
	}

	public void Reset()
    {
		isRotate = false;
		playerController.Reset();
		
		if(fallinCoroutine != null)
        {
			StopCoroutine(fallinCoroutine);
        }
	}
	
    public void Update()
	{
		float x = 0;
		float y = 0;

		
		x = Input.GetAxisRaw("Vertical");
		if (x == 0)
		{
			y = -Input.GetAxisRaw("Horizontal");
		}

		if(canMove){
			ProcessInput(x,y);
		}
	}

	public IEnumerator Spawn(int x,int z){
		Reset();
		playerController.alive = true;
		Vector3 newPos= new Vector3(x,1,z);
		transform.position= newPos;
		pos1=pos2=new Vector2(x,z);
		SetCanMove(false);
		playerController.modelHolder.gameObject.SetActive(true);
		yield return StartCoroutine(SpawnAnim());
		Block block = LevelManager.Instance.GetTileAt(pos1);
		if (block != null)
		{
			block.SetIsPlayerOn(true);
		}
	}

	IEnumerator SpawnAnim(){
	
		Tween t= transform.DOMoveY(spawnHeight,spawnDur).From().SetEase(spawnEase);
		yield return t.WaitForCompletion();
		SetCanMove(true);
	}

	public void ProcessInput(float x, float y)
    {
	    if ((x != 0 || y != 0) && !isRotate && playerController.alive && canMove) { 

			//Debug.Log("x : " + x + " y  : " + y );
			if(x > 0)
            {
				playerController.AddMove('u');
				Debug.Log("u");
			}
			else if(x < 0)
            {
				playerController.AddMove('d');
				Debug.Log("d");
			}
			else if(y > 0)
            {
				playerController.AddMove('l');
				Debug.Log("l");
			}
			else if(y < 0)
            {
				playerController.AddMove('r');
				Debug.Log("r");

			}

			directionX = y;
			directionZ = x;
			startPos = transform.position;
			fromRotation = transform.rotation;
			transform.Rotate(directionZ * 90, 0, directionX * 90, Space.World);
			toRotation = transform.rotation;
			transform.rotation = fromRotation;
			setRadius();
			rotationTime = 0;
			isRotate = true;

			// for some reasons : WARNING
			// x = -y
			// y = x

			
			SetIsPlayerOnTile(false);

			if (pos1 == pos2)
            {
				pos1 += new Vector2(-y, x).normalized;
				pos2 += new Vector2(-y, x).normalized * 2;
            }
            else
            {
				if(pos2.y == pos1.y && pos1.x != pos2.x)
                {
					if(x != 0)
                    {
						pos1 += new Vector2(-y, x).normalized;
						pos2 += new Vector2(-y, x).normalized;
                    }
                    else
                    {
						Vector2 closerPos = pos1;
						if(-y > 0)
                        {
							if(pos2.x > pos1.x)
                            {
								closerPos = pos2;
							}
						}
                        else
                        {
							if (pos2.x < pos1.x)
							{
								closerPos = pos2;
							}
						}

						pos2 = closerPos + new Vector2(-y, x).normalized;
						pos1 = closerPos + new Vector2(-y, x).normalized;
					}
                }
				else if (pos2.x == pos1.x && pos1.y != pos2.y)
				{
					if (-y != 0)
					{
						pos1 += new Vector2(-y, x).normalized;
						pos2 += new Vector2(-y, x).normalized;
					}
					else
					{
						Vector2 closerPos = pos1;
						if (x > 0)
						{
							if (pos2.y > pos1.y)
							{
								closerPos = pos2;
							}
						}
						else
						{
							if (pos2.y < pos1.y)
							{
								closerPos = pos2;
							}
						}

						pos2 = closerPos + new Vector2(-y, x).normalized;
						pos1 = closerPos + new Vector2(-y, x).normalized;
					}
				}
			}

			//Debug.Log("VALIDE MOVE : " + (!LevelManager.Instance.IsPlayerPosOutOfBound(pos1) && !LevelManager.Instance.IsPlayerPosOutOfBound(pos2)));

		}
	}

	public void SetIsPlayerOnTile(bool value)
    {
		Block block = LevelManager.Instance.GetTileAt(pos1);
		if (block != null)
		{
			block.SetIsPlayerOn(value);

		}
		if (pos1 != pos2)
		{
			block = LevelManager.Instance.GetTileAt(pos2);
			if (block != null)
			{
				block.SetIsPlayerOn(value);
			}
		}
	}

	void FixedUpdate()
	{

		if (isRotate)
		{

			rotationTime += Time.fixedDeltaTime;
			float ratio = Mathf.Lerp(0, 1, rotationTime / rotationPeriod);

			
			float thetaRad = Mathf.Lerp(0, Mathf.PI / 2f, ratio);                
			float distanceX = -directionX * radius * (Mathf.Cos(startAngleRad) - Mathf.Cos(startAngleRad + thetaRad));   
			float distanceY = radius * (Mathf.Sin(startAngleRad + thetaRad) - Mathf.Sin(startAngleRad));                 
			float distanceZ = directionZ * radius * (Mathf.Cos(startAngleRad) - Mathf.Cos(startAngleRad + thetaRad));    
			transform.position = new Vector3(startPos.x + distanceX, startPos.y + distanceY, startPos.z + distanceZ);    
			//Debug.DrawRay(transform.position, new Vector3(-directionX, 0, directionZ)*3,Color.red);
			//Debug.LogError(rotationTime);
			
			transform.rotation = Quaternion.Lerp(fromRotation, toRotation, ratio);    

			
			if (ratio == 1)
			{
				if (LevelManager.Instance.IsPlayerPosOutOfBound(pos1) && LevelManager.Instance.IsPlayerPosOutOfBound(pos2))
				{
					ComputeFallBothNotValide(pos1, pos2, new Vector3(-directionX, 0, directionZ));
					playerController.Kill();
					Debug.Log("Les deux");
				}
				else if (LevelManager.Instance.IsPlayerPosOutOfBound(pos2))
				{
					ComputeFall(pos2, pos1);
					playerController.Kill();
					Debug.Log("Pos2");
				}
				else if (LevelManager.Instance.IsPlayerPosOutOfBound(pos1))
				{
					ComputeFall(pos1, pos2);
					playerController.Kill();
					Debug.Log("Pos1");
				}
				else if (LevelManager.Instance.IsOnUnstableTile(pos1, pos2))
                {
					StartFallFromUnstableTile();
					playerController.Kill();
					Debug.Log("Unstable Tile");
				}
				else if (LevelManager.Instance.HasPlayerWin(pos1, pos2))
				{
					SetCanMove(false);
					playerController.ValidateLevel();
					LevelManager.Instance.StartRestartLevelCoroutine();
				}
				else if (LevelManager.Instance.IsPlayerOnTeleporter(pos1, pos2))
				{
					TeleporterBlock tp = (TeleporterBlock)LevelManager.Instance.GetTileAt(pos1); 
					Vector2 newPos = LevelManager.Instance.GetOtherTeleporter(tp);
					if (newPos != Vector2.one * -1)
					{
						playerController.TeleportTo(newPos);
					}
				}
				else if (LevelManager.Instance.IsPlayerOnSoftSwitch(pos1, pos2))
				{
					Block tile = LevelManager.Instance.GetTileAt(pos1);
					PresurePlateBlock button = null;
					if(tile is PresurePlateBlock)
                    {
						button = (PresurePlateBlock)tile;
                    }
                    else
                    {
						button = (PresurePlateBlock)LevelManager.Instance.GetTileAt(pos2);

					}
					button.TriggerBridge();
				}
				else if (LevelManager.Instance.IsPlayerOnHardSwitch(pos1, pos2))
				{
					Block tile = LevelManager.Instance.GetTileAt(pos1);
					PresurePlateBlock button = null;
					if (tile is PresurePlateBlock)
					{
						button = (PresurePlateBlock)tile;
					}
					button.TriggerBridge();
				}

				SetIsPlayerOnTile(true);

				isRotate = false;
				directionX = 0;
				directionZ = 0;
				rotationTime = 0;
			}
		}
	}

	public void StartFallFromUnstableTile()
    {
		if (fallinCoroutine != null)
		{
			StopCoroutine(fallinCoroutine);
		}

		fallinCoroutine = StartCoroutine(FallFromUnstableTile());
	}

	public IEnumerator FallFromUnstableTile()
	{
		Block block = LevelManager.Instance.GetTileAt(pos1);
		block.Fall();
		transform.DOPunchRotation(block.shakefallTweenForce * 2 * Vector3.one, block.shakefallTweenDuration);
		yield return new WaitForSeconds(block.shakefallTweenDuration);
		while (true)
		{
			transform.position = transform.position + Vector3.down * Time.deltaTime * fallinSpeed * 1.5f ;
			yield return null;
		}
	}

	public void ComputeFall(Vector2 pos1, Vector2 pos2)
    {
		Vector3 pos = new Vector3(pos1.x, 0, pos1.y);
		Vector3 dir = new Vector3((pos1 - pos2).x, 0, (pos1 - pos2).y).normalized;
		Vector3 rotAround = pos - (dir / 2);
		Vector3 axis = Vector3.right;

		if (dir.z < 0)
		{
			axis = -axis;
			//Debug.Log("Axis inverse");
		}
		if (Vector3.Cross(Vector3.right, dir).magnitude == 0)
		{
			axis = Vector3.forward;
			//Debug.Log("Axis CHange");
			if (dir.x > 0)
			{
				//Debug.Log("Axis inverse");
				axis = -axis;
			}
		}
		StartFallingCoroutine(rotAround, axis, 90);
		Debug.DrawRay(rotAround, axis * 3, Color.green);
		Debug.DrawRay(rotAround, dir * 5, Color.yellow);
		Debug.DrawRay(rotAround, dir * 1, Color.blue);
	}

	public void ComputeFallBothNotValide(Vector2 pos1, Vector2 pos2, Vector3 dir)
	{
		Vector3 pos =new Vector3(pos1.x - pos2.x, 0, pos1.y - pos2.y);
		pos = new Vector3(pos2.x,0,pos2.y) + pos;
		Vector3 rotAround = Vector3.zero;
		if (pos1 == pos2)
        {
			rotAround -= new Vector3(0, 1, 0);
        }
        else
        {
			rotAround -= new Vector3(0, 0.5f, 0);
		}
	    rotAround = pos - (dir / 2);
		Vector3 axis = Vector3.right;


		if (dir.z < 0)
		{
			axis = -axis;
			//Debug.Log("Axis inverse");
		}
		if (Vector3.Cross(Vector3.right, dir).magnitude == 0)
		{
			axis = Vector3.forward;
			//Debug.Log("Axis CHange");
			if (dir.x > 0)
			{
				//Debug.Log("Axis inverse");
				axis = -axis;
			}
		}
		
		StartFallingCoroutine(rotAround, axis, 45);
		Debug.DrawRay(rotAround, axis * 3, Color.green);
		Debug.DrawRay(rotAround, dir * 5, Color.yellow);
		Debug.DrawRay(rotAround, dir * 1, Color.blue);
		Debug.LogError("both");
	}

	public void StartFallingCoroutine(Vector3 rotAround, Vector3 axis, float angle)
    {
		if(fallinCoroutine != null)
        {
			StopCoroutine(fallinCoroutine);
        }

		fallinCoroutine = StartCoroutine(Fall(rotAround, axis, angle));
    }

	public IEnumerator Fall(Vector3 rotAround, Vector3 axis, float angle)
    {
		float rotationLeft = angle;                                   
		float rotAmount = 0;
		while (rotationLeft > 0)
        {
	        rotAmount = fallinRotSpeed * Time.deltaTime;
			transform.RotateAround(rotAround, axis, rotAmount);
			rotationLeft -= rotAmount;
			yield return null;
        }
		SetIsPlayerOnTile(false);
		while (true)
		{
			rotAmount = fallinRotSpeed * Time.deltaTime;
			transform.RotateAround(transform.position, axis, rotAmount);
			transform.position = transform.position + Vector3.down * Time.deltaTime * fallinSpeed;
			yield return null;
		}
	}

	void setRadius()
	{

		Vector3 dirVec = new Vector3(0, 0, 0);
		Vector3 nomVec = Vector3.up;                    // (0,1,0)
		
		if (directionX != 0)
		{                           
			dirVec = Vector3.right;                     // (1,0,0)
		}
		else if (directionZ != 0)
		{                  
			dirVec = Vector3.forward;                   // (0,0,1)
		}

		if (Mathf.Abs(Vector3.Dot(transform.right, dirVec)) > 0.99)
		{                      
			if (Mathf.Abs(Vector3.Dot(transform.up, nomVec)) > 0.99)
			{                  
				radius = Mathf.Sqrt(Mathf.Pow(boxScaleX / 2f, 2f) + Mathf.Pow(boxScaleY / 2f, 2f));
				startAngleRad = Mathf.Atan2(boxScaleY, boxScaleX);
				
			}
			else if (Mathf.Abs(Vector3.Dot(transform.forward, nomVec)) > 0.99)
			{ 
				radius = Mathf.Sqrt(Mathf.Pow(boxScaleX / 2f, 2f) + Mathf.Pow(boxScaleZ / 2f, 2f));
				startAngleRad = Mathf.Atan2(boxScaleZ, boxScaleX);
			}

		}
		else if (Mathf.Abs(Vector3.Dot(transform.up, dirVec)) > 0.99)
		{             
			if (Mathf.Abs(Vector3.Dot(transform.right, nomVec)) > 0.99)
			{              
				radius = Mathf.Sqrt(Mathf.Pow(boxScaleY / 2f, 2f) + Mathf.Pow(boxScaleX / 2f, 2f));
				startAngleRad = Mathf.Atan2(boxScaleX, boxScaleY);
			}
			else if (Mathf.Abs(Vector3.Dot(transform.forward, nomVec)) > 0.99)
			{      
				radius = Mathf.Sqrt(Mathf.Pow(boxScaleY / 2f, 2f) + Mathf.Pow(boxScaleZ / 2f, 2f));
				startAngleRad = Mathf.Atan2(boxScaleZ, boxScaleY);
			}
		}
		else if (Mathf.Abs(Vector3.Dot(transform.forward, dirVec)) > 0.99)
		{          
			if (Mathf.Abs(Vector3.Dot(transform.right, nomVec)) > 0.99)
			{                  
				radius = Mathf.Sqrt(Mathf.Pow(boxScaleZ / 2f, 2f) + Mathf.Pow(boxScaleX / 2f, 2f));
				startAngleRad = Mathf.Atan2(boxScaleX, boxScaleZ);
			}
			else if (Mathf.Abs(Vector3.Dot(transform.up, nomVec)) > 0.99)
			{            
				radius = Mathf.Sqrt(Mathf.Pow(boxScaleZ / 2f, 2f) + Mathf.Pow(boxScaleY / 2f, 2f));
				startAngleRad = Mathf.Atan2(boxScaleY, boxScaleZ);
			}
		}
		//Debug.Log (radius + ", " + startAngleRad);
	}
}