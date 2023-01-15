using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using InputDevice = UnityEngine.XR.InputDevice;
using Qualcomm.Snapdragon.Spaces.Samples;
using Qualcomm.Snapdragon.Spaces;




public class Player : SampleController
{
  private bool alternate;
  public float swipeDistance = 0.5f;
  public float positionScale = 0.1f;
  public float minDistanceForNewPerson = 0.1f;

public Person pizzaPersonData;

public ImageTrackingSampleController imgController;
  public GameObject prefab_hoverPanel;
  public TextHoverPanel hoverPanel;
  public List<TextHoverPanel> activePanels;

  public Transform parent_panels;
  public Transform parent_persons;
  public Transform camera;



  public List<GameObject> prefab_persons;

  public Transform targetPerson;

  public Text debugText;




  public UnityEvent swipeRight;
  public UnityEvent swipeLeft;
  public UnityEvent swipeUp;
  public UnityEvent swipeDown;




  public Text LeftHandGestureName;
  public Text LeftHandGestureRatio;
  public Text LeftHandFlipRatio;
  public Text RightHandGestureName;
  public Text RightHandGestureRatio;
  public Text RightHandFlipRatio;

  public GameObject Mirror;
  public GameObject MirroredPlayer;
  public GameObject MirroredPlayerHead;
  public GameObject MirroredJointObject;


  private GameObject[] _leftMirroredHandJoints;
  private GameObject[] _rightMirroredHandJoints;
  private Transform _mainCameraTransform;
  private SpacesHandManager _spacesHandManager;


  public bool lh_isVisible;
  public bool rh_isVisible;

  public Vector3 lh_StartPos;
  public Vector3 rh_StartPos;

private bool trackImageAnchor = false;




  public override void Start() {
      base.Start();
      _mainCameraTransform = Camera.main.transform;
      _spacesHandManager = FindObjectOfType<SpacesHandManager>();
      _spacesHandManager.handsChanged += UpdateGesturesUI;

  }


  public override void Update() {
      base.Update();


      // if(imgController)
      // {
      //   if(trackImageAnchor && activePanels.Count > 0)
      //   {
      //     activePanels[activePanels.Count - 1].anchorOffset =  imgController.getAnchor();
      //
      //   }
      //
      // }

      if(Keyboard.current.aKey.wasPressedThisFrame )
      {Swipe_Left();  }
      if(Keyboard.current.sKey.wasPressedThisFrame )
      {Swipe_Right();  }
      if(Keyboard.current.dKey.wasPressedThisFrame )
      {Swipe_Up();  }
      if(Keyboard.current.fKey.wasPressedThisFrame )
      {Swipe_Down();  }
      if(Keyboard.current.gKey.wasPressedThisFrame )
      {ScanNewImage("PizzA",camera.position);  }

  }


public void ScanNewImage(string _name,Vector3 _pos)
{
  if(FindClosestPerson(_pos) == null || Vector3.Distance(_pos,FindClosestPerson(_pos).transform.position) > minDistanceForNewPerson)
  {
        if(parent_persons == null){parent_panels = new GameObject().transform;}
        if(parent_persons.childCount < prefab_persons.Count )
        {
          GameObject clone = Instantiate(prefab_persons[parent_persons.childCount],_pos,transform.rotation);
          clone.transform.parent = parent_persons;

          int count = 0;
          while(count <= 2)
          {
            TextHoverPanel newPanel = GetPanel();
            newPanel.gameObject.SetActive(true);
            newPanel.hover = true;
            newPanel.observer = camera;
            newPanel.anchor = clone.transform;

            newPanel.anchorOffset  = new Vector3(0,0,0);
            newPanel.NextStyle(activePanels.Count);
           // newPanel.SetRowInformation(0,_name);
            newPanel.anchorOffset  = new Vector3(count * positionScale,0 ,0);
                    clone.GetComponent<Person>().SetPanel(newPanel);

            count++;
              alternate = !alternate;
          }



        }


  }

}

public Person FindClosestPerson(Vector3 _pos)
{
  Person closest = null;
  float dist = 9999;

  if(parent_persons.childCount > 0 )
  {
      foreach(Transform el in parent_persons)
      {
        if(closest == null)
        {
            closest = el.GetComponent<Person>();
            dist = Vector3.Distance(_pos,el.position);

        }else
        {
            if(Vector3.Distance(_pos,el.position) < dist)
            {
              dist = Vector3.Distance(_pos,el.position);
              closest = el.GetComponent<Person>();
            }

        }

      }

  }


    return closest;
}


public TextHoverPanel GetPanel()
{
  TextHoverPanel newHover = null;
  if(parent_panels == null){parent_panels = new GameObject().transform;}

  if(parent_panels.childCount == 0 || parent_panels.GetChild(0).gameObject.activeSelf || parent_panels.GetChild(0).GetComponent<TextHoverPanel>() == null )
  {
      newHover = Instantiate(prefab_hoverPanel).GetComponent<TextHoverPanel>();

  }else{newHover = parent_panels.GetChild(0).GetComponent<TextHoverPanel>();}

    newHover.transform.parent = null;
    newHover.transform.parent = parent_panels;
    activePanels.Add(newHover);

    // newHover.anchor = targetPerson;
    newHover.gameObject.SetActive(true);
  return newHover;
}

public void UpdateDebugText(string _text)
{
  if(debugText)
  {debugText.text = _text + debugText.text ;}

}

public void UpdateHandVisible(SpacesHand hand)
{

    if(hand.IsLeft)
    {
      if(lh_isVisible == false)
      {
        UpdateDebugText("left hand visible \n");

        lh_StartPos = hand.transform.position;
        lh_isVisible = true;
        if(hoverPanel)
        {
          hoverPanel.SetColor(0);

        }
      }
    }
    else
    {
      if(rh_isVisible == false)
      {
        UpdateDebugText("right hand visible \n");
          rh_StartPos = hand.transform.position;
          rh_isVisible = true;
      }
    }

}

public void HandExit(SpacesHand hand)
{

    if(hand.IsLeft)
    {
      if(lh_isVisible == true)
      {
          DetectSwipe(hand,lh_StartPos);

          if(hoverPanel)
          {
          //  hoverPanel.SetColor(1);

          }
        if (Vector3.Distance(hand.transform.position , lh_StartPos) > swipeDistance)
        {
          //ShowPanel();

        }
        lh_isVisible = false;
      }
    }
    else
    {
      if(rh_isVisible == true)
      {
        DetectSwipe(hand,rh_StartPos);

          rh_isVisible = false;
          if (Vector3.Distance(hand.transform.position , rh_StartPos) > swipeDistance)
          {
          //  HidePanel();

          }

      }
    }

}

public void DetectSwipe(SpacesHand _hand,Vector3 _startPos)
{
  float dist = Vector3.Distance(_hand.transform.position , _startPos);
  float hort = _hand.transform.position.x - _startPos.x;
  float vert = _hand.transform.position.y - _startPos.y;
  float depth = _hand.transform.position.z - _startPos.z;

  if (dist < swipeDistance)
  {return;}

  if(Mathf.Abs(vert) >  Mathf.Abs(hort))
  {
    if(vert <= 0)
    {
     Swipe_Down();
    }else
    {

      //PullPanel( _hand);
      Swipe_Up();

    }

  }
  else
  {

    if(hort <= 0)
    {
      Swipe_Left();


    }else
    {
      Swipe_Right();

    }
  }


}


public void PullPanel(SpacesHand _hand)
{
  TextHoverPanel closest = FindClosestPanel(_hand.transform.position);
  TextHoverPanel furthest =FindFurthestPanel( _hand.transform.position);

  if(_hand.IsLeft)
  {
    closest.anchorOffset = _hand.transform.position - transform.position;

  }
  else
  {
    closest.anchorOffset = transform.forward;
    closest.anchor = furthest.transform;

  }

}



public TextHoverPanel FindClosestPanel(Vector3 _pos)
{
  TextHoverPanel closest = null;
  float dist = 9999;

  if(parent_panels.childCount > 0 )
  {
      foreach(Transform el in parent_panels)
      {
        if(closest == null)
        {
            closest = el.GetComponent<TextHoverPanel>();
            dist = Vector3.Distance(_pos,el.position);

        }else
        {
            if(Vector3.Distance(_pos,el.position) < dist)
            {
              dist = Vector3.Distance(_pos,el.position);
              closest = el.GetComponent<TextHoverPanel>();
            }

        }

      }

  }


    return closest;
}

public TextHoverPanel FindFurthestPanel(Vector3 _pos)
{
  TextHoverPanel furthest = null;
  float dist = 0;

  if(parent_panels.childCount > 0 )
  {
      foreach(Transform el in parent_panels)
      {
        if(furthest == null)
        {
            furthest = el.GetComponent<TextHoverPanel>();
            dist = Vector3.Distance(_pos,el.position);

        }else
        {
            if(Vector3.Distance(_pos,el.position) > dist)
            {
              dist = Vector3.Distance(_pos,el.position);
              furthest = el.GetComponent<TextHoverPanel>();
            }

        }

      }

  }


    return furthest;
}




public void Swipe_Up()
{
  swipeUp.Invoke();
  UpdateDebugText("swipe up" + '\n');



    Person closestPerson = FindClosestPerson(transform.position);
    if(closestPerson != null)
    {
      closestPerson.ShowPanels(true);

    }

  //
  // if(activePanels != null && activePanels.Count > 1)
  // {
  //   TextHoverPanel panel = activePanels[activePanels.Count - 1];
  //   activePanels.RemoveAt(activePanels.Count - 1);
  //   activePanels.Insert(0,panel);
  //   panel.anchorOffset = new Vector3(panel.anchorOffset.x,Mathf.Abs(panel.anchorOffset.y),panel.anchorOffset.z);
  //
  // }


}
public void Swipe_Down()
{
  swipeDown.Invoke();
  UpdateDebugText("swipe down" + '\n');

  Person closestPerson = FindClosestPerson(transform.position);
  if(closestPerson != null)
  {
    closestPerson.ShowPanels(false);

  }

  // if(activePanels != null && activePanels.Count > 1)
  // {
  //   TextHoverPanel panel = activePanels[0];
  //   activePanels.RemoveAt(0);
  //   panel.anchorOffset = Vector3.zero;
  //   panel.anchorOffset = new Vector3(panel.anchorOffset.x,Mathf.Abs(panel.anchorOffset.y) * -1,panel.anchorOffset.z);
  //   activePanels.Add(panel);
  //
  // }
}

public void Swipe_Left()
{
  swipeLeft.Invoke();
  UpdateDebugText("swipe left" + '\n');

    trackImageAnchor = false;
  //if(activePanels != null && activePanels.Count > 0)
  //{
  //  activePanels[activePanels.Count - 1].hover = false;
  //  activePanels[activePanels.Count - 1].anchorOffset = Vector3.zero;
  //  activePanels[activePanels.Count - 1].gameObject.SetActive(false);
  //  activePanels.RemoveAt(activePanels.Count - 1);
  //}

}
public void Swipe_Right()
{
  swipeRight.Invoke();

  UpdateDebugText("swipe right" + '\n');

//    trackImageAnchor = true;
//  TextHoverPanel newPanel = GetPanel();
//  newPanel.gameObject.SetActive(true);
//  newPanel.hover = true;
//  newPanel.observer = camera;
//  //newPanel.anchor = targetPerson;

//  newPanel.anchorOffset  = new Vector3(0,0,0);
//newPanel.NextStyle(activePanels.Count);

  if(alternate)
  {
   // newPanel.SetRowInformation(0,activePanels.Count.ToString());

   // newPanel.anchorOffset  += new Vector3(0,-(activePanels.Count + 1)* positionScale ,-(activePanels.Count + 1)* positionScale);
  }
  else
  {
     // newPanel.SetRowInformation(0,"not " + activePanels.Count.ToString());
    //  newPanel.anchorOffset  += new Vector3(0,(activePanels.Count + 1) *  positionScale ,-(activePanels.Count + 1)* positionScale);

  }
  alternate = !alternate;
}


public void ShowPanel()
{


}

public void HidePanel()
{


}



public void GestureProcess(SpacesHand hand)
{


}





  private void UpdateGesturesUI(SpacesHandsChangedEventArgs args) {
      foreach (var hand in args.updated) {

          var gestureNameTextField = hand.IsLeft ? LeftHandGestureName : RightHandGestureName;
          var gestureRatioTextField = hand.IsLeft ? LeftHandGestureRatio : RightHandGestureRatio;
          var flipRatioTextField = hand.IsLeft ? LeftHandFlipRatio : RightHandFlipRatio;



          // gestureNameTextField.text = Enum.GetName(typeof(SpacesHand.GestureType), hand.CurrentGesture.Type);
          gestureRatioTextField.text = (int) (hand.CurrentGesture.GestureRatio * 100f) + " %";
          // flipRatioTextField.text = hand.CurrentGesture.FlipRatio.ToString("0.00");
          flipRatioTextField.text = hand.transform.position.ToString();

          MirroredPlayer.transform.position = GetMirroredPosition(_mainCameraTransform.transform.position);

          var reflectedForward = Vector3.Reflect(hand.transform.rotation * Vector3.forward, Mirror.transform.forward);
          var reflectedUp = Vector3.Reflect(hand.transform.rotation * Vector3.up, Mirror.transform.forward);
          MirroredPlayerHead.transform.rotation = Quaternion.LookRotation(reflectedForward, reflectedUp);

          UpdateHandVisible(hand);
      }

      foreach (var hand in args.removed) {
          var gestureNameTextField = hand.IsLeft ? LeftHandGestureName : RightHandGestureName;
          var gestureRatioTextField = hand.IsLeft ? LeftHandGestureRatio : RightHandGestureRatio;
          var flipRatioTextField = hand.IsLeft ? LeftHandFlipRatio : RightHandFlipRatio;



          gestureNameTextField.text = "-";
          gestureRatioTextField.text = "-";
          flipRatioTextField.text = "-";

          HandExit(hand);
      }
  }

  private void UpdateMirroredPlayer() {
      MirroredPlayer.transform.position = GetMirroredPosition(_mainCameraTransform.transform.position);

      var reflectedForward = Vector3.Reflect(_mainCameraTransform.transform.rotation * Vector3.forward, Mirror.transform.forward);
      var reflectedUp = Vector3.Reflect(_mainCameraTransform.transform.rotation * Vector3.up, Mirror.transform.forward);
      MirroredPlayerHead.transform.rotation = Quaternion.LookRotation(reflectedForward, reflectedUp);

      // UpdateMirroredHand(true);
      // UpdateMirroredHand(false);
  }

  // private void UpdateMirroredHand(bool leftHand) {
  //     var joints = leftHand ? _leftMirroredHandJoints : _rightMirroredHandJoints;
  //     for (var i = 0; i < _leftMirroredHandJoints.Length; i++) {
  //         var hand = leftHand ? _spacesHandManager.LeftHand : _spacesHandManager.RightHand;
  //         if (hand == null) {
  //             joints[i].SetActive(false);
  //             continue;
  //         }
  //         joints[i].SetActive(true);
  //         joints[i].transform.position = GetMirroredPosition(hand.Joints[i].Pose.position);
  //     }
  // }

  private Vector3 GetMirroredPosition(Vector3 positionToMirror) {
      /* Maths for reflection across a line can be found here: https://en.wikipedia.org/wiki/Reflection_(mathematics) */
      var mirrorNormal = Mirror.transform.forward;
      var mirrorPosition = Mirror.transform.position;
      /* Position to be reflected in a hyperplane through the origin. Therefore offset, the position by the mirror position. */
      var adjustedPosition = positionToMirror - mirrorPosition;
      var reflectedPosition = adjustedPosition - 2  * Vector3.Dot(adjustedPosition, mirrorNormal) / Vector3.Dot(mirrorNormal, mirrorNormal) * mirrorNormal;

      /* Offset the origin of the reflection again by the mirror position. */
      return mirrorPosition + reflectedPosition;
  }
}
