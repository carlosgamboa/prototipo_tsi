﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;		// This allows for the use of lists, like <GameObject>
using pseudoSinCos;


public class verts
{
	public float angle {get;set;}
	public int location {get;set;} // 1= left end point    0= middle     -1=right endpoint
	public Vector3 pos {get;set;}
	public bool endpoint { get; set;}

}


public class DynamicLight : MonoBehaviour {



	// Public variables

	public string version = "1.0.3"; //release date 31/01/2016

	public Material lightMaterial;

	[HideInInspector]public PolygonCollider2D[] allMeshes;									// Array for all of the meshes in our scene


	[HideInInspector] public List<verts> allVertices = new List<verts>();								// Array for all of the vertices in our meshes

	[SerializeField]
	public float lightRadius = 20f;

	[Range(4,20)]
	public int lightSegments = 8;

    public List<GameObject> exitPoints;
    public List<GameObject> agents;
	// Private variables
	Mesh lightMesh;													// Mesh for our light mesh
	LayerMask layer;

    private bool renderLights = false;
    public List<GameObject> excludeMeshes = new List<GameObject>();

    public bool RenderLights
    {
        get
        {
            return renderLights;
        }

        set
        {
            renderLights = value;
        }
    }


    // Called at beginning of script execution
    void Start () {

		PseudoSinCos.initPseudoSinCos();


		
		//-- Step 1: obtain all active meshes in the scene --//
		//---------------------------------------------------------------------//

		MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent(typeof(MeshFilter));				// Add a Mesh Filter component to the light game object so it can take on a form
		MeshRenderer renderer = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;		// Add a Mesh Renderer component to the light game object so the form can become visible
		//gameObject.name = "2DLight";
		//renderer.material.shader = Shader.Find ("Transparent/Diffuse");							// Find the specified type of material shader
		renderer.sharedMaterial = lightMaterial;														// Add this texture
		lightMesh = new Mesh();																	// create a new mesh for our light mesh
		meshFilter.mesh = lightMesh;															// Set this newly created mesh to the mesh filter
		lightMesh.name = "Light Mesh";															// Give it a name
		lightMesh.MarkDynamic ();


	}
	

	void Update(){

        if (exitPoints == null)
        {
            exitPoints = new List<GameObject>();
        }

        if (exitPoints.Count > 0)
        {
            exitPoints.Clear();
        }
        
		getAllMeshes();
		setLight ();

        if (RenderLights)
		    renderLightMesh ();
		resetBounds ();

	}


	void getAllMeshes(){
		//allMeshes = FindObjectsOfType(typeof(PolygonCollider2D)) as PolygonCollider2D[];
		Collider2D [] allColl2D = Physics2D.OverlapCircleAll(transform.position, lightRadius, layer);
		allMeshes = new PolygonCollider2D[allColl2D.Length];

		for (int i=0; i<allColl2D.Length; i++) {
            allMeshes[i] = (PolygonCollider2D)allColl2D[i];		   
        }
	}

	void resetBounds(){
		Bounds b = lightMesh.bounds;
		b.center = Vector3.zero;
		lightMesh.bounds = b;
	}

	void setLight () {

		bool sortAngles = false;

		allVertices.Clear();// Since these lists are populated every frame, clear them first to prevent overpopulation

	
		layer = 1 << 8;


		//--Step 2: Obtain vertices for each mesh --//
		//---------------------------------------------------------------------//
	
		// las siguientes variables usadas para arregla bug de ordenamiento cuando
		// los angulos calcuados se encuentran en cuadrantes mixtos (1 y 4)
		bool lows = false; // check si hay menores a -0.5
		bool his = false; // check si hay mayores a 2.0
		float magRange = 0.15f;

		List <verts> tempVerts = new List<verts>();

		for (int m = 0; m < allMeshes.Length; m++) {
		//for (int m = 0; m < 1; m++) {
			tempVerts.Clear();
			PolygonCollider2D mf = allMeshes[m];

                // las siguientes variables usadas para arregla bug de ordenamiento cuando
                // los angulos calcuados se encuentran en cuadrantes mixtos (1 y 4)
            lows = false; // check si hay menores a -0.5
			his = false; // check si hay mayores a 2.0

			if(((1 << mf.transform.gameObject.layer) & layer) != 0){
				for (int i = 0; i < mf.GetTotalPointCount(); i++) {								   // ...and for ever vertex we have of each mesh filter...
					
					verts v = new verts();
					// Convert to world space
					Vector3 worldPoint = mf.transform.TransformPoint(mf.points[i]);

                    Vector3 dir = worldPoint - transform.position;
                    Vector3 offsetToCast = Vector3.zero;
                    if (allMeshes[m].gameObject.name == "PointCollider")
                    {
                        offsetToCast = dir.normalized * 1.5f;
                    }

                        // Reforma fecha 24/09/2014 (ultimo argumento lighradius X worldPoint.magnitude (expensivo pero preciso))
                    
                    RaycastHit2D ray = Physics2D.Raycast(transform.position + offsetToCast, dir, (worldPoint - transform.position).magnitude, layer);
					
					
					if(ray){
						v.pos = ray.point;
						if( worldPoint.sqrMagnitude >= (ray.point.sqrMagnitude - magRange) && worldPoint.sqrMagnitude <= (ray.point.sqrMagnitude + magRange) )
							v.endpoint = true;
						
					}else{
						v.pos =  worldPoint;
						v.endpoint = true;
					}
					
					Debug.DrawLine(transform.position, v.pos, Color.white);	
					
					//--Convert To local space for build mesh (mesh craft only in local vertex)
					v.pos = transform.InverseTransformPoint(v.pos); 
					//--Calculate angle
					v.angle = getVectorAngle(true,v.pos.x, v.pos.y);
					
					
					
					// -- bookmark if an angle is lower than 0 or higher than 2f --//
					//-- helper method for fix bug on shape located in 2 or more quadrants
					if(v.angle < 0f )
						lows = true;
					
					if(v.angle > 2f)
						his = true;
					
					
					//--Add verts to the main array
					if((v.pos).sqrMagnitude <= lightRadius*lightRadius){
						tempVerts.Add(v);
						
					}
					
					if(sortAngles == false)
						sortAngles = true;
					
					
				}

			}





			// Indentify the endpoints (left and right)
			if(tempVerts.Count > 0){

				sortList(tempVerts); // sort first

				int posLowAngle = 0; // save the indice of left ray
				int posHighAngle = 0; // same last in right side

				//Debug.Log(lows + " " + his);

				if(his == true && lows == true){  //-- FIX BUG OF SORTING CUANDRANT 1-4 --//
					float lowestAngle = -1f;//tempVerts[0].angle; // init with first data
					float highestAngle = tempVerts[0].angle;


					for(int d=0; d<tempVerts.Count; d++){



						if(tempVerts[d].angle < 1f && tempVerts[d].angle > lowestAngle){
							lowestAngle = tempVerts[d].angle;
							posLowAngle = d;
						}

						if(tempVerts[d].angle > 2f && tempVerts[d].angle < highestAngle){
							highestAngle = tempVerts[d].angle;
							posHighAngle = d;
						}
					}


				}else{
					//-- convencional position of ray points
					// save the indice of left ray
					posLowAngle = 0; 
					posHighAngle = tempVerts.Count-1;

				}


				tempVerts[posLowAngle].location = 1; // right
				tempVerts[posHighAngle].location = -1; // left



				//--Add vertices to the main meshes vertexes--//
				allVertices.AddRange(tempVerts); 
				//allVertices.Add(tempVerts[0]);
				//allVertices.Add(tempVerts[tempVerts.Count - 1]);



				// -- r ==0 --> right ray
				// -- r ==1 --> left ray
				for(int r = 0; r<2; r++){

					//-- Cast a ray in same direction continuos mode, start a last point of last ray --//
					Vector3 fromCast = new Vector3();
					bool isEndpoint = false;

					if(r==0){
						fromCast = transform.TransformPoint(tempVerts[posLowAngle].pos);
						isEndpoint = tempVerts[posLowAngle].endpoint;

					}else if(r==1){
						fromCast = transform.TransformPoint(tempVerts[posHighAngle].pos);
						isEndpoint = tempVerts[posHighAngle].endpoint;
					}





					if(isEndpoint == true){
						Vector2 from = (Vector2)fromCast;
						Vector2 dir = (from - (Vector2)transform.position);

						/*
						
						float mag = (lightRadius);// - fromCast.magnitude;
						const float checkPointLastRayOffset= 0.005f; 
						
						from += (dir * checkPointLastRayOffset);
											

						RaycastHit2D rayCont = Physics2D.Raycast(from, dir, mag, layer);
						Vector3 hitp;
						if(rayCont){
							hitp = rayCont.point;
						}else{
							Vector2 newDir = transform.InverseTransformDirection(dir);	//local p
							hitp = (Vector2)transform.TransformPoint( newDir.normalized * mag); //world p
						}

						if(((Vector2)hitp - (Vector2)transform.position ).sqrMagnitude > (lightRadius * lightRadius)){
							dir = (Vector2)transform.InverseTransformDirection(dir);	//local p
							hitp = (Vector2)transform.TransformPoint( dir.normalized * mag);
						}

						Debug.DrawLine(fromCast, hitp, Color.green);	

						verts vL = new verts();
						vL.pos = transform.InverseTransformPoint(hitp);

						vL.angle = getVectorAngle(true,vL.pos.x, vL.pos.y);
						allVertices.Add(vL);
                        */


                        if (allMeshes[m].gameObject.name == "PointCollider")
                        {
                            exitPoints.Add(allMeshes[m].transform.parent.gameObject);
                        }
                        if (allMeshes[m].gameObject.name == "AgentCollider")
                        {
                            agents.Add(allMeshes[m].transform.parent.gameObject);
                        }
                    }
                    else
                    {
                        if (allMeshes[m].gameObject.name == "PointCollider")
                            Debug.Log("point excluded, no light");
                    }


				}


			}

			
		}

        return;

		//--Step 3: Generate vectors for light cast--//
		//---------------------------------------------------------------------//

		int theta = 0;
		//float amount = (Mathf.PI * 2) / lightSegments;
		int amount = 360 / lightSegments;



		for (int i = 0; i < lightSegments; i++)  {

			theta =amount * (i);
			if(theta == 360) theta = 0;

			verts v = new verts();
			//v.pos = new Vector3((Mathf.Sin(theta)), (Mathf.Cos(theta)), 0); // in radians low performance
			v.pos = new Vector3((PseudoSinCos.SinArray[theta]), (PseudoSinCos.CosArray[theta]), 0); // in dregrees (previous calculate)

			v.angle = getVectorAngle(true,v.pos.x, v.pos.y);
			v.pos *= lightRadius;
			v.pos += transform.position;



			RaycastHit2D ray = Physics2D.Raycast(transform.position,v.pos - transform.position,lightRadius, layer);
			//Debug.DrawRay(transform.position, v.pos - transform.position, Color.white);

			if (!ray){

				//Debug.DrawLine(transform.position, v.pos, Color.white);

				v.pos = transform.InverseTransformPoint(v.pos);
				allVertices.Add(v);

			}
		 
		}




		//-- Step 4: Sort each vertice by angle (along sweep ray 0 - 2PI)--//
		//---------------------------------------------------------------------//
		if (sortAngles == true) {
			sortList(allVertices);
		}
		//-----------------------------------------------------------------------------


		//--auxiliar step (change order vertices close to light first in position when has same direction) --//
		float rangeAngleComparision = 0.00001f;
		for(int i = 0; i< allVertices.Count-1; i+=1){
			
			verts uno = allVertices[i];
			verts dos = allVertices[i +1];

			// -- Comparo el angulo local de cada vertex y decido si tengo que hacer un exchange-- //
			if(uno.angle >= dos.angle-rangeAngleComparision && uno.angle <= dos.angle + rangeAngleComparision){
				
				if(dos.location == -1){ // Right Ray
					
					if(uno.pos.sqrMagnitude > dos.pos.sqrMagnitude){
						allVertices[i] = dos;
						allVertices[i+1] = uno;
						//Debug.Log("changing left");
					}
				}
				

				// ALREADY DONE!!
				if(uno.location == 1){ // Left Ray
					if(uno.pos.sqrMagnitude < dos.pos.sqrMagnitude){
						
						allVertices[i] = dos;
						allVertices[i+1] = uno;
						//Debug.Log("changing");
					}
				}
				
				
				
			}


		}



	}

	void renderLightMesh(){
		//-- Step 5: fill the mesh with vertices--//
		//---------------------------------------------------------------------//
		
		//interface_touch.vertexCount = allVertices.Count; // notify to UI
		
		Vector3 []initVerticesMeshLight = new Vector3[allVertices.Count+1];
		
		initVerticesMeshLight [0] = Vector3.zero;
		
		
		for (int i = 0; i < allVertices.Count; i++) { 
			//Debug.Log(allVertices[i].angle);
			initVerticesMeshLight [i+1] = allVertices[i].pos;
			
			//if(allVertices[i].endpoint == true)
			//Debug.Log(allVertices[i].angle);
			
		}
		
		lightMesh.Clear ();
		lightMesh.vertices = initVerticesMeshLight;
		
		Vector2 [] uvs = new Vector2[initVerticesMeshLight.Length];
		for (int i = 0; i < initVerticesMeshLight.Length; i++) {
			uvs[i] = new Vector2(initVerticesMeshLight[i].x, initVerticesMeshLight[i].y);		
		}
		lightMesh.uv = uvs;
		
		// triangles
		int idx = 0;
		int [] triangles = new int[(allVertices.Count * 3)];
		for (int i = 0; i < (allVertices.Count*3); i+= 3) {
			
			triangles[i] = 0;
			triangles[i+1] = idx+1;
			
			
			if(i == (allVertices.Count*3)-3){
				//-- if is the last vertex (one loop)
				triangles[i+2] = 1;	
			}else{
				triangles[i+2] = idx+2; //next next vertex	
			}
			
			idx++;
		}
		
		
		lightMesh.triangles = triangles;												
		//lightMesh.RecalculateNormals();
		GetComponent<Renderer>().sharedMaterial = lightMaterial;
	}

	void sortList(List<verts> lista){
			lista.Sort((item1, item2) => (item2.angle.CompareTo(item1.angle)));
	}

	void drawLinePerVertex(){
		for (int i = 0; i < allVertices.Count; i++)
		{
			if (i < (allVertices.Count -1))
			{
				Debug.DrawLine(allVertices [i].pos , allVertices [i+1].pos, new Color(i*0.02f, i*0.02f, i*0.02f));
			}
			else
			{
				Debug.DrawLine(allVertices [i].pos , allVertices [0].pos, new Color(i*0.02f, i*0.02f, i*0.02f));
			}
		}
	}

	float getVectorAngle(bool pseudo, float x, float y){
		float ang = 0;
		if(pseudo == true){
			ang = pseudoAngle(x, y);
		}else{
			ang = Mathf.Atan2(y, x);
		}
		return ang;
	}
	
	float pseudoAngle(float dx, float dy){
		// Hight performance for calculate angle on a vector (only for sort)
		// APROXIMATE VALUES -- NOT EXACT!! //
		float ax = Mathf.Abs (dx);
		float ay = Mathf.Abs (dy);
		float p = dy / (ax + ay);
		if (dx < 0){
			p = 2 - p;

		}
		return p;
	}

}