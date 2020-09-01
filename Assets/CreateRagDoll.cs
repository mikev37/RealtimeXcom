using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRagDoll : MonoBehaviour
{
    public Bone origin;
    public void Start()
    {
        origin = CreateSkeleton(transform);
    }

    public Bone CreateSkeleton(Transform t)
    {
        Bone b = new Bone();
        b.bone = t.gameObject;
        b.joints = new List<Bone>();
        for (int i = 0; i < t.childCount; i++)
        {
            Transform child = t.GetChild(i);
            if(child.gameObject.layer.Equals(t.gameObject.layer))
                b.joints.Add(CreateSkeleton(child));
        }
        return b;
    }

    // Start is called before the first frame update
    public void Break()
    {
        Rigidbody rb = Break(origin,30);
        rb.AddForce((Random.insideUnitSphere+Vector3.down) * 10000,ForceMode.Impulse);
        rb.gameObject.AddComponent<DeadBody>();
    }

    private Rigidbody Break(Bone b,int mass)
    {
        Rigidbody rb = b.bone.AddComponent<Rigidbody>();
        //rb.gameObject.AddComponent<DeadBody>();
        if (rb)
        {
            rb.mass = mass;
            rb.isKinematic = false;
            rb.angularDrag = 1;
            
            //CapsuleCollider cc = b.bone.AddComponent<CapsuleCollider>();
        }
        foreach(Bone bro in b.joints)
        {
            Rigidbody r = Break(bro, mass);
            /*
            FixedJoint fj = bro.bone.AddComponent<FixedJoint>();
            fj.connectedBody = rb;*/
            //fj.
            /*
            SpringJoint sj = bro.bone.AddComponent<SpringJoint>();
            sj.connectedBody = rb;
            //sj.autoConfigureConnectedAnchor = false;
            sj.damper = 10000;
            sj.spring = 10000;
            sj.maxDistance = (bro.bone.transform.position - b.bone.transform.position).magnitude+.1f;
            sj.minDistance = (bro.bone.transform.position - b.bone.transform.position).magnitude-.1f;
            sj.axis = (bro.bone.transform.position - b.bone.transform.position).normalized * -1;
            */
            CharacterJoint cj = bro.bone.AddComponent<CharacterJoint>();
            cj.axis = (bro.bone.transform.position - b.bone.transform.position).normalized * -1;
            cj.swingAxis = bro.bone.transform.right;
            SoftJointLimitSpring sjls = new SoftJointLimitSpring();
            sjls.spring = 1000;
            sjls.damper = 10000;
            SoftJointLimit sjl90 = new SoftJointLimit();
            sjl90.bounciness = 0;
            sjl90.limit = 10;// 35;
            SoftJointLimit sjl0 = new SoftJointLimit();
            sjl0.bounciness = 00;
            sjl0.limit = 0;
            cj.swingLimitSpring = sjls;
            cj.twistLimitSpring = sjls;
            cj.swing1Limit = sjl90;
            cj.swing2Limit = sjl90;
            cj.highTwistLimit = sjl0;
            cj.lowTwistLimit = sjl0;
            cj.connectedBody = rb;
            /*
            sj.autoConfigureConnectedAnchor = false;
            sj.spring = 100;
            sj.damper = 100;*/

        }

        //Destroy(b.bone.GetComponent<Collider>());
        //b.bone.AddComponent<SphereCollider>();
        //rb.mass = 30;

        /*
        if(b.joints.Count <= 0 && b.bone.GetComponent<Collider>() == null)
        {
            b.bone.AddComponent<BoxCollider>();
            rb.mass = 30;
        }*/
        return rb;

    }
}
[System.Serializable]
public struct Bone
{
    public GameObject bone;
    public List<Bone> joints;
}