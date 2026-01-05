using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
{
    private CubePos nowCube = new CubePos(0,1,0);
    public Text scoreTxt;
    private Rigidbody allCubesRb;
    private bool IsLose, firstCube;
    private int prevCountMaxHor;
    private Transform mainCam;
    private Coroutine showCubePlace;
    private float camMoveToYPosition, speedCam = 2f;
    private Color toCameraColor;
    public float cubeChangePlaceSpeed = 0.6f;
    public Transform cubeToPlace;
    public GameObject cubeToCreate, allCubes, vfx;
    public GameObject[] canvasStartPage;
    public Color[] bgColors;

    List<Vector3> allCubesPositions = new List<Vector3>
    {
    new Vector3(0,0,0),
    new Vector3(1,0,0),
    new Vector3(-1,0,0),
    new Vector3(0,1,0),
    new Vector3(0,0,1),
    new Vector3(0,0,-1),
    new Vector3(1,0,1),
    new Vector3(-1,0,-1),
    new Vector3(-1,0,1),
    new Vector3(1,0,-1),
};


    IEnumerator ShowCubePlace() // шо это
    {
        while (true)
        {
            SpawnPositions();

            yield return new WaitForSeconds(cubeChangePlaceSpeed);
        }
    }

    private void SpawnPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        if(IsPositionEmpty(new Vector3(nowCube.x + 1, nowCube.y, nowCube.z)) && nowCube.x + 1 != cubeToPlace.position.x)
            positions.Add(new Vector3(nowCube.x + 1, nowCube.y, nowCube.z)); // есть ли какой то указатель? зачем копировать одно и тоже с верхней строчки, например this
        if (IsPositionEmpty(new Vector3(nowCube.x - 1, nowCube.y, nowCube.z)) && nowCube.x - 1 != cubeToPlace.position.x)
            positions.Add(new Vector3(nowCube.x - 1, nowCube.y, nowCube.z));

        if (IsPositionEmpty(new Vector3(nowCube.x, nowCube.y + 1, nowCube.z)) && nowCube.y+1 != cubeToPlace.position.y)
            positions.Add(new Vector3(nowCube.x, nowCube.y + 1, nowCube.z));
        if (IsPositionEmpty(new Vector3(nowCube.x, nowCube.y - 1, nowCube.z)) && nowCube.y - 1 != cubeToPlace.position.y)
            positions.Add(new Vector3(nowCube.x, nowCube.y - 1, nowCube.z));

        if (IsPositionEmpty(new Vector3(nowCube.x, nowCube.y, nowCube.z +1)) && nowCube.z + 1 != cubeToPlace.position.z)
            positions.Add(new Vector3(nowCube.x, nowCube.y, nowCube.z+1));
        if (IsPositionEmpty(new Vector3(nowCube.x, nowCube.y, nowCube.z-1)) && nowCube.z - 1 != cubeToPlace.position.z)
            positions.Add(new Vector3(nowCube.x, nowCube.y, nowCube.z-1));

        if (positions.Count > 1)
        {
            cubeToPlace.position = positions[UnityEngine.Random.Range(0, positions.Count)];
        }
        else if (positions.Count == 0)
        {
            IsLose = true;
        }
        else
        {
            cubeToPlace.position = positions[0];
        }
    }

    private bool IsPositionEmpty(Vector3 target_pos)  // почему бы не сделать функцию константной, по логике она ничего не меняет
    {
        if(target_pos.y == 0) return false;

        foreach (Vector3 pos in allCubesPositions)
        {
            if (pos.x == target_pos.x && pos.y == target_pos.y && pos.z == target_pos.z)
                return false;

        }
        return true;
        
    }

    public void Update()
    {
        if (IsLose) return;
        if (Input.GetMouseButtonDown(0) && cubeToPlace != null && allCubes != null ) //&& !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        {
#if !UNITY_EDITOR
            if ((Input.GetTouch(0).phase != TouchPhase.Began) || EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                return;
#endif

            if (!firstCube)
            {
                firstCube = true;
                foreach (GameObject obj in canvasStartPage)
                    Destroy(obj);
            }

            GameObject newCube = Instantiate(cubeToCreate, cubeToPlace.position, Quaternion.identity) as GameObject;


            newCube.transform.SetParent(allCubes.transform);
            nowCube.SetVector(cubeToPlace.position);
            allCubesPositions.Add(nowCube.GetVector());

            Instantiate(vfx, cubeToPlace.position, Quaternion.identity);

            allCubesRb.isKinematic = true;
            allCubesRb.isKinematic = false;

            SpawnPositions();

            MoveCameraChangeBg();

        }
        if (!IsLose && allCubesRb.linearVelocity.magnitude > 0.1f)
        {
            Destroy(cubeToPlace.gameObject);
            IsLose = true;
            StopCoroutine(showCubePlace);
        }
        mainCam.localPosition = Vector3.MoveTowards(mainCam.localPosition,
            new Vector3(mainCam.localPosition.x, camMoveToYPosition, mainCam.localPosition.z),
            speedCam * Time.deltaTime);

        if (Camera.main.backgroundColor != toCameraColor)
        {
            Camera.main.backgroundColor = Color.Lerp(Camera.main.backgroundColor, toCameraColor, Time.deltaTime / 1.5f);
        }
    }



    private void Start()
    {
        scoreTxt.text = "Best:" + PlayerPrefs.GetInt("score") + "\n<size=25>Now: 0</size>";
        toCameraColor = Camera.main.backgroundColor;
        mainCam = Camera.main.transform;
        camMoveToYPosition = 5.9f + nowCube.y - 1f;

        allCubesRb = allCubes.GetComponent<Rigidbody>();
        showCubePlace = StartCoroutine(ShowCubePlace()); 

    }

    private void MoveCameraChangeBg()
    {
        int maxX = 0, maxY = 0, maxZ = 0, maxHor;
        foreach(Vector3 pos in allCubesPositions)
        {
            if (Mathf.Abs(Convert.ToInt32(pos.x)) > maxX)
                maxX = Convert.ToInt32(pos.x);

            if (Mathf.Abs(Convert.ToInt32(pos.y)) > maxY)
                maxY = Convert.ToInt32(pos.y);

            if (Mathf.Abs(Convert.ToInt32(pos.z)) > maxZ)
                maxZ = Convert.ToInt32(pos.z);
        }

        maxY--;
        if (PlayerPrefs.GetInt("score") < maxY)
            PlayerPrefs.SetInt("score", maxY);

        scoreTxt.text = "Best:" + PlayerPrefs.GetInt("score") + "\n<size=25>Now:</size>" + maxY;

        camMoveToYPosition = 5.9f + nowCube.y - 1f;

        maxHor = maxX > maxZ ? maxX : maxZ;
        if(maxHor % 2 == 0 && prevCountMaxHor != maxHor)
        {
            mainCam.localPosition -= new Vector3(0, 0, 3f);
            prevCountMaxHor = maxHor;
        }

        if (maxY >= 20)
        {
            toCameraColor = bgColors[0];
        }
        else if (maxY >= 15)
        {
            toCameraColor = bgColors[1];
        }
        else if (maxY >= 10)
        {
            toCameraColor = bgColors[2];
        }
        else if (maxY >= 6)
        {
            toCameraColor = bgColors[3];
        }
        else if (maxY >= 3)
        {
            toCameraColor = bgColors[4];
        }

    }

    
}



struct CubePos
{
    public int x, y, z;

    public CubePos(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3 GetVector()
    {
        return new Vector3(x, y, z);
    }

    public void SetVector(Vector3 pos)
    {
        x = Convert.ToInt32(pos.x);
        y = Convert.ToInt32(pos.y);
        z = Convert.ToInt32(pos.z);
    }
}
