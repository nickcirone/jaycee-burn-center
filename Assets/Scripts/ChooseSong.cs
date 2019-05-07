﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using Joint = Windows.Kinect.Joint;
using UnityEngine.SceneManagement;
public class ChooseSong : MonoBehaviour 
{
    
    public BodySourceManager mBodySourceManager;
    public GameObject happybdaybutton;
    public GameObject takemebutton;
    public GameObject londonbutton;
    public GameObject spaceship;

   static public int songNum;

    private List<Kinect.JointType> _joints;
    private bool progressDisplayed;
    private ulong playerID=999;
    void Start()
    {
        happybdaybutton = GameObject.Find("Happy Birthday");
        takemebutton = GameObject.Find("Take Me Out to The Ball Game");
        londonbutton = GameObject.Find("London Bridge");
        if(PlayerPrefs.GetString("hand", "right") == "right"){
           _joints = new List<Kinect.JointType>{Kinect.JointType.HandRight,};
        } else{ 
            _joints = new List<Kinect.JointType>{Kinect.JointType.HandLeft,};
        }
    }
    
    private Dictionary<ulong, GameObject> mBodies = new Dictionary<ulong, GameObject>();

    
    void Update () 
    {
        #region Process Clicks
         //Test
            var move = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
            spaceship.transform.position += move * (float)10.0 * Time.deltaTime;
        //endTest
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("Pressed primary button.");
            Vector2 rayPos = new Vector2(spaceship.transform.position.x, spaceship.transform.position.y);
            RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);
            if (hit)
            {
                if (hit.transform.gameObject.name.Contains("Happy Birthday"))
                {
                    //songNum=PlayerPrefs.GetInt("songNum",1);
                    PlayerPrefs.SetInt("songNum",2);
                    SceneManager.LoadScene("Game");
                } else if (hit.transform.gameObject.name.Contains("Take Me Out to The Ball Game"))
                {
                   //songNum= PlayerPrefs.GetInt("songNum",3);
                   PlayerPrefs.SetInt("songNum",0);
                   SceneManager.LoadScene("Game");
                }else if (hit.transform.gameObject.name.Contains("London Bridge"))
                {
                   // songNum=PlayerPrefs.GetInt("songNum",5);
                   PlayerPrefs.SetInt("songNum",4);
                   SceneManager.LoadScene("Game");
                } else if (hit.transform.gameObject.name.Contains("Home"))
                {
                    SceneManager.LoadScene("MainMenu");
                }
            }
        }
        #endregion
        #region Get Kinect data
        Kinect.Body[] data = mBodySourceManager.GetData();

        if (data == null)
        {
            return;
        }
        
        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data)
        {
            if (body == null)
                continue;

            if (body.IsTracked)
                trackedIds.Add(body.TrackingId);
        }

        #endregion

         #region Delete Kinect bodies
        List<ulong> knownIds = new List<ulong>(mBodies.Keys);
        foreach (ulong trackingID in knownIds)
        {
            if (!trackedIds.Contains(trackingID))
            {
                //if the main player disappears
                if(trackingID == playerID){
                    //set spaceship parent to null so it doesn't get destroyed
                    spaceship.transform.parent = null;
                    playerID = 999;
                }
                //Destroy body object
                Destroy(mBodies[trackingID]);

                //Remove from list
                mBodies.Remove(trackingID);
            }
        }
        #endregion

        #region Create and update Kinect bodies
        foreach (var body in data)
        {
            //if no body, skip
            if(body == null)
                continue;

            if (body.IsTracked)
            {
                //IF body isn't tracked, create body
                if(!mBodies.ContainsKey(body.TrackingId))
                    mBodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                
                if(playerID == 999){
                    playerID = body.TrackingId;
                    AssignSpaceship(body.TrackingId, mBodies[body.TrackingId]);
                }
                // Update positions
                UpdateBodyObject(body, mBodies[body.TrackingId]);

            }
        }
        #endregion
       
    }

    private GameObject CreateBodyObject(ulong id)
    {
        //Create body parent
        GameObject body = new GameObject("Body:" + id);

         //Creates joints if a player does not exist
        if(playerID == 999){
            playerID = id;
            //Create joints
            foreach (Kinect.JointType joint in _joints)
            {
                //Create object
                spaceship.name = joint.ToString();

                //Parent to body
                spaceship.transform.parent = body.transform;

            }
        } 
        return body;
    }

    private void AssignSpaceship(ulong id, GameObject body){
        playerID = id;
        //Create joints
        foreach (Kinect.JointType joint in _joints)
            {
                //Create object
                spaceship.name = joint.ToString();

                //Parent to body
                spaceship.transform.parent = body.transform;

            }
    }

    private void UpdateBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        //Update joints
        foreach (Kinect.JointType _joint in _joints)
        {
            //Get new target position
            Joint sourceJoint = body.Joints[_joint];
            Vector3 targetPosition = GetVector3FromJoint(sourceJoint);
            targetPosition.z=-2;

            //Get joint, set new position
            Transform jointObject = bodyObject.transform.Find(_joint.ToString());
            
            jointObject.transform.position = Vector3.Lerp(jointObject.transform.position, targetPosition*2, 3*Time.deltaTime);
        }
    }
    
    private Vector3 GetVector3FromJoint (Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }

}
