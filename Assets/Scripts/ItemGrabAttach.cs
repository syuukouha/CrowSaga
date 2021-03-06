﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRTK.GrabAttachMechanics;
using DG.Tweening;
public class ItemGrabAttach : VRTK_BaseGrabAttach
{
    public Vector3 AttachPosition;
    public Vector3 AttachRotation;
    public GrabItem grabItem;
    public GameObject Effect;
    public Transform SpawnEffectPos;
    private List<Vector3> vectorTemp = new List<Vector3>(10);
    private List<Vector3> directionTemp = new List<Vector3>(10);
    private float distance;
    private bool isShake;
    protected override void Initialise()
    {
        tracked = false;
        climbable = false;
        kinematic = true;
        isShake = false;
    }
    /// <summary>
    /// The StartGrab method sets up the grab attach mechanic as soon as an object is grabbed. It is also responsible for creating the joint on the grabbed object.
    /// </summary>
    /// <param name="grabbingObject">The object that is doing the grabbing.</param>
    /// <param name="givenGrabbedObject">The object that is being grabbed.</param>
    /// <param name="givenControllerAttachPoint">The point on the grabbing object that the grabbed object should be attached to after grab occurs.</param>
    /// <returns>Is true if the grab is successful, false if the grab is unsuccessful.</returns>
    public override bool StartGrab(GameObject grabbingObject, GameObject givenGrabbedObject, Rigidbody givenControllerAttachPoint)
    {
        if (base.StartGrab(grabbingObject, givenGrabbedObject, givenControllerAttachPoint))
        {
            this.transform.SetParent(grabbingObject.transform.parent);
            this.transform.localPosition = AttachPosition;
            this.transform.localRotation = Quaternion.Euler(AttachRotation);
            grabbedObjectScript.isKinematic = true;
            ClearTemp();
            return true;
        }
        return false;
    }

    /// <summary>
    /// The StopGrab method ends the grab of the current object and cleans up the state.
    /// </summary>
    /// <param name="applyGrabbingObjectVelocity">If true will apply the current velocity of the grabbing object to the grabbed object on release.</param>
    public override void StopGrab(bool applyGrabbingObjectVelocity)
    {
        ReleaseObject(applyGrabbingObjectVelocity);
        base.StopGrab(applyGrabbingObjectVelocity);
    }

    private void Update()
    {
        if (!grabItem.IsGrabbed() || EnemyManager.Instance.IsDeath || GameManager.Instance.IsDeath || !GameManager.Instance.IsCanAttack)
            return;
        for (int i = 9; i > 0; i--)
        {
            vectorTemp.Insert(i, vectorTemp[i - 1]);
            directionTemp.Insert(i, directionTemp[i - 1]);
        }
        vectorTemp.Insert(0, this.transform.position);
        directionTemp.Insert(0, this.transform.forward);
        switch (grabItem.itemType)
        {
            case ItemType.Magic:
                distance = 0;
                for (int i = 0; i < 10; i++)
                {
                    distance += Mathf.Abs(Vector3.Distance(vectorTemp[i], vectorTemp[i + 1]));
                }
                if ((int)(distance * 100) > 15)
                {
                    isShake = true;
                }
                break;
            case ItemType.Sword:
                distance = 0;
                for (int i = 0; i < 10; i++)
                {
                    distance += Mathf.Abs(Vector3.Distance(vectorTemp[i], vectorTemp[i + 1]));
                }
                if ((int)(distance * 100) > 30)
                {
                    isShake = true;
                }
                break;
            case ItemType.Shield:
                distance = 0;
                float angle = Vector3.Angle(this.transform.forward, directionTemp[9]);
                if (angle < 10)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        distance += Mathf.Abs(Vector3.Distance(vectorTemp[i], vectorTemp[i + 1]));
                    }
                    if ((int)(distance * 100) > 8)
                    {
                        isShake = true;
                    }
                }
                break;
            default:
                break;
        }
        if (isShake)
            ShakedController();

    }


    void ShakedController()
    {
        Vector3 target = Vector3.zero;
        isShake = false;
        GameManager.Instance.IsCanAttack = false;
        GameObject effect = Instantiate(Effect);
        effect.transform.position = SpawnEffectPos.position;
        switch (grabItem.itemType)
        {
            case ItemType.Magic:
                target = GameObject.FindGameObjectWithTag("Magic").transform.position;
                break;
            case ItemType.Sword:
                target = GameObject.FindGameObjectWithTag("Sword").transform.position;
                break;
            case ItemType.Shield:
                target = GameObject.FindGameObjectWithTag("Knight").transform.position;
                break;
            default:
                break;
        }
        if (target != null)
            effect.transform.DOMove(target, 1.0f);

        grabItem.Haptic();

        switch (grabItem.itemType)
        {
            case ItemType.Magic:
                Destroy(this.gameObject);
                GameManager.Instance.ItemRest();
                break;
            case ItemType.Sword:
                GameManager.Instance.ChangeMaterial(false, 1);
                break;
            case ItemType.Shield:
                GameManager.Instance.ChangeMaterial(false, 2);
                break;
            default:
                break;
        }
    }
    public void ClearTemp()
    {
        vectorTemp.Clear();
        directionTemp.Clear();
        for (int i = 0; i < 10; i++)
        {
            vectorTemp.Add(this.transform.position);
            directionTemp.Add(this.transform.forward);
        }
    }
}
