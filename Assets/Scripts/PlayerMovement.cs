using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement
{
    public Player player;
    private float distance;

    //Vector3 dirVector;
    //Quaternion targetRotation;

    float ZRotation;
    float maxZRotation = 10;
    float ZRotationPerFrame = 1f;

    Vector3 oldGroundVector;
    MeshCollider currentMeshCollider;
    bool waitForTriangleUpdate;
    float waitForTriangleTime;

    float oldSpeed;
    float speedDifference;

    public void GroundMovement()
    {
        oldSpeed = player.speed;
        player.BonusSpeed -= player.boostSpeedReduction;
        //player.bonusSpeed = Mathf.Lerp(player.bonusSpeed, 0, player.boostSpeedReduction * Time.fixedDeltaTime);
        if (player.moveDirection.y == 0)
        {
            //Mathf.Lerp(speed, 0, reduceSpeed * Time.fixedDeltaTime);
            //rb.drag = reduceSpeed;
            player.speed = Mathf.Lerp(player.speed, 0, player.reduceSpeedIfNoInput * Time.fixedDeltaTime);
        }
        else
        {
            if (player.moveDirection.y > 0)
            {
                if (player.rb.velocity.sqrMagnitude < 0.1f && player.speed < 1) player.speed = 1;
                player.speed = Mathf.Lerp(player.speed, player.maxspeed, Time.fixedDeltaTime * player.acceleration);
            }

            else if (player.moveDirection.y < 0)
            {
                if (player.rb.velocity.sqrMagnitude < 0.1f && player.speed > 1) player.speed = -1;
                player.speed = Mathf.Lerp(player.speed, -player.maxspeed * 0.5f, Time.fixedDeltaTime * (player.acceleration * 2));

            }
        }
        speedDifference = player.speed - oldSpeed;  //wieviel Speed pro Fixed Update gemacht wird
        float fixedUpdatesInIntervalTime = player.beyondMaxSpeedGainInterval / Time.fixedDeltaTime; //wieviel Ticks in der intervat Zeit gemacht werden
        speedDifference *= fixedUpdatesInIntervalTime; // wieviel Speed in der Interval Zeit gemacht wird = wenn der Speed weniger als 1 ist(weil ich 1 Speed pro Zeit Interval machen will) wird der beyondmaxSpeed getriggert

        //if (distance < 1.5f) player.rb.AddForce(player.transform.up * 5, ForceMode.Force);
        //else if (distance > 1.7f) player.rb.AddForce(-player.transform.up * 5, ForceMode.Force);

        player.rb.velocity = Vector3.zero;

        player.finalSpeed = player.speed + player.bonusSpeed + player.beyondMaxSpeed;

        player.rb.AddForce(player.transform.forward * player.finalSpeed, ForceMode.Impulse);
        //player.rb.AddForce(Vector3.ProjectOnPlane(player.transform.forward, player.groundVector).normalized * player.speed, ForceMode.Impulse);
        // player.rb.MovePosition(Vector3.Lerp(player.transform.position, player.groundVector, 10));

        //float rayDirVel = Vector3.Dot(player.groundVector, player.rb.velocity);
        //float x = distance - 1;
        //float springForce = (x * 100) - (rayDirVel * 0.01f);

        ////Debug.Log(springForce);

        //player.rb.AddForce(-player.transform.up * springForce);

        //Debug.Log(distance);

        float strength;
        float target = 1f;
        if (distance > target) strength = distance - target;
        else strength = (distance - target) * -1;


        strength *= 6;
        if(distance > target) 
            player.rb.AddForce(-player.transform.up * strength, ForceMode.Impulse);
        else //if (distance < 0.7f)
        {
            player.rb.AddForce(player.transform.up * strength, ForceMode.Impulse);
            //player.rb.velocity = new Vector3(player.rb.velocity.x, 0, player.rb.velocity.z) + player.groundVector;
        }

        //float rotationspeed = 0.5f * player.speed;
        //if (rotationspeed < 6) rotationspeed = 6;
        //Quaternion test = Quaternion.FromToRotation(player.transform.up, player.groundVector) * player.rb.rotation;
        //player.rb.rotation = Quaternion.Lerp(player.rb.rotation, test, rotationspeed * Time.deltaTime);
        //Quaternion test = Quaternion.FromToRotation(player.transform.up, player.groundVector) * player.transform.rotation;
        //player.rb.transform.rotation = Quaternion.Lerp(player.transform.rotation, test, 20 * Time.fixedDeltaTime);

        //player.rb.MoveRotation(Quaternion.FromToRotation(player.transform.up, player.groundVector) * player.transform.rotation);
    }
    public void BeyondMaxSpeed()
    {
        if (speedDifference < 1 && player.moveDirection.y > 0)
        {
            player.beyondMaxSpeedTime += Time.deltaTime;
            if (player.beyondMaxSpeedTime > player.beyondMaxSpeedGainInterval)
            {
                player.beyondMaxSpeedTime = 0;
                player.beyondMaxSpeed++;
            }
        }
        else
        {
            if (player.beyondMaxSpeed > 0)
            {
                player.beyondMaxSpeedTime += Time.deltaTime;
                if (player.beyondMaxSpeedTime > player.beyondMaxSpeedLoseInterval)
                {
                    player.beyondMaxSpeedTime = 0;
                    player.beyondMaxSpeed--;
                }
            }
        }
    }
    public void GroundCheck()
    {
        if (Physics.Raycast(player.transform.position, -player.transform.up, out RaycastHit ray, 4, player.groundCheckLayer))
        {
            GroundRayHit(ray);
        }
        ////if(Physics.Raycast(player.transform.position, -player.transform.up, out RaycastHit ground , 5, player.groundCheckLayer))
        //if (Physics.BoxCast(player.transform.position, player.transform.localScale * 0.1f, -player.transform.up, out RaycastHit groundHit, player.rb.rotation, 4f, player.groundCheckLayer))
        //{
        //    //distance = groundHit.distance;
        //    //player.currentGroundAngle = Vector3.Angle(player.transform.up, hit.normal);
        //    //player.groundVector = groundHit.normal;
        //    //Debug.Log(ground.normal);
        //}
        else if (Physics.BoxCast(player.transform.position, player.transform.localScale * 0.45f, -player.transform.up, out RaycastHit hit, player.rb.rotation, 4f, player.groundCheckLayer))
        {
            GroundRayHit(hit);
            Debug.Log("BoxCast");
        }
        else
        {
            player.SwitchToAirState();
        }
    }
    private void GroundRayHit(RaycastHit ray)
    {
        if (waitForTriangleUpdate)
        {
            waitForTriangleTime += Time.deltaTime;
            if(waitForTriangleTime > 0.1f)
            {
                waitForTriangleTime = 0;
                waitForTriangleUpdate = false;
            }
        }
        // Just in case, also make sure the collider also has a renderer
        // material and texture
        MeshCollider meshCollider = ray.collider as MeshCollider;

        if(meshCollider != currentMeshCollider)
        {
            oldGroundVector = player.groundVector;
            waitForTriangleUpdate = true;
        }
        currentMeshCollider = meshCollider;
        if (meshCollider != null || meshCollider.sharedMesh != null)
        {
            Mesh mesh = meshCollider.sharedMesh;
            Vector3[] normals = mesh.normals;
            int[] triangles = mesh.triangles;

            // Extract local space normals of the triangle we hit
            Vector3 n0 = normals[triangles[ray.triangleIndex * 3 + 0]];
            Vector3 n1 = normals[triangles[ray.triangleIndex * 3 + 1]];
            Vector3 n2 = normals[triangles[ray.triangleIndex * 3 + 2]];

            // interpolate using the barycentric coordinate of the hitpoint
            Vector3 baryCenter = ray.barycentricCoordinate;

            // Use barycentric coordinate to interpolate normal
            Vector3 interpolatedNormal = n0 * baryCenter.x + n1 * baryCenter.y + n2 * baryCenter.z;
            // normalize the interpolated normal
            interpolatedNormal = interpolatedNormal.normalized;

            // Transform local space normals to world space
            Transform hitTransform = ray.collider.transform;
            player.groundVector = hitTransform.TransformDirection(interpolatedNormal);

            distance = ray.distance;

            //dirVector = ray.point - player.transform.up;
            //targetRotation = Quaternion.FromToRotation(player.transform.up, ray.normal) * player.rb.rotation;
        }
    }

    public void KillLateralVelocity()
    {
        if (player.moveDirection.y > 0) player.rb.velocity = Vector3.Lerp(player.rb.velocity.normalized, player.transform.forward, player.driftFactor * Time.fixedDeltaTime) * player.rb.velocity.magnitude;
        else if (player.moveDirection.y < 0) player.rb.velocity = Vector3.Lerp(player.rb.velocity.normalized, -player.transform.forward, player.driftFactor * Time.fixedDeltaTime) * player.rb.velocity.magnitude;

        //Vector3 forwardVelocity = transform.forward * speed;
        //Vector3 lateralVelocity = transform.right * Vector3.Dot(rb.velocity, transform.right);
        //rb.velocity = forwardVelocity + lateralVelocity * driftfactor;
    }
    public void Steering()
    {
        float YRotation = 0;
        //if (speed > 1 || speed < -1)
        {

            if (player.moveDirection.x > 0)
            {
                YRotation = player.turningSpeed;
                ZRotation += -ZRotationPerFrame;
                if (ZRotation <= -maxZRotation) ZRotation = -maxZRotation;
            }
            else if (player.moveDirection.x < 0)
            { 
                YRotation = -player.turningSpeed;
                ZRotation += ZRotationPerFrame;
                if (ZRotation >= maxZRotation) ZRotation = maxZRotation;
            }
            else
            {
                if(ZRotation < 0) ZRotation += ZRotationPerFrame;
                else if (ZRotation > 0) ZRotation += -ZRotationPerFrame;
            }

            
            //Quaternion q = Quaternion.AngleAxis(YRotation, new Vector3(0, 1, 0));
            //player.rb.MoveRotation(targetRotation);

            //player.rb.rotation = Quaternion.Lerp(player.rb.rotation, Quaternion.FromToRotation(player.transform.up, player.groundVector) * player.transform.rotation * q, 6f * Time.deltaTime);
            //player.rb.MoveRotation(player.playerRotation);

            if (waitForTriangleUpdate)
            {
                Quaternion q = Quaternion.AngleAxis(YRotation * 3, new Vector3(0, 1, 0)); //*3 wegen rotation speed
                player.rb.MoveRotation(Quaternion.Lerp(player.rb.transform.rotation, Quaternion.FromToRotation(player.transform.up, player.groundVector) * player.transform.rotation * q, Time.fixedDeltaTime * player.rotationSpeed));
            }
            //player.rb.MoveRotation(Quaternion.FromToRotation(player.transform.up, oldGroundVector) * player.transform.rotation * q);
            //else player.rb.MoveRotation(Quaternion.Lerp(player.rb.transform.rotation, Quaternion.FromToRotation(player.transform.up, player.groundVector) * player.transform.rotation * q, Time.fixedDeltaTime * player.rotationSpeed));
            else
            {
                Quaternion q = Quaternion.AngleAxis(YRotation, new Vector3(0, 1, 0));
                player.rb.MoveRotation(Quaternion.FromToRotation(player.transform.up, player.groundVector) * player.transform.rotation * q); 
            }


            player.ChildTransform.rotation = player.transform.rotation * Quaternion.AngleAxis(ZRotation, new Vector3(0, 0, 1)); //Quaternion.FromToRotation(player.transform.up, oldGroundVector) *


            //player.rb.transform.Rotate(new Vector3(0, 1, 0) * Time.deltaTime * rotation, Space.Self);

            //player.playerRotation = Quaternion.Euler(player.transform.eulerAngles.x, player.transform.eulerAngles.y + rotation, player.transform.eulerAngles.z);
            //player.rb.MoveRotation(player.playerRotation);

            //float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, transform.eulerAngles.y + rotation, ref angleref, 0.1f);
            //transform.rotation = Quaternion.Euler(0, angle, 0);

            //Vector3 newRotation = new Vector3(0, rotation, 0);
            //Quaternion deltaRotation = Quaternion.Euler(newRotation * Time.fixedDeltaTime);
            //rb.MoveRotation(rb.rotation * deltaRotation);


            //transform.Rotate(Vector3.up * rotation);

            //transform.rotation = Quaternion.RotateTowards(transform.rotation, playerRotation, rotationTest * Time.deltaTime);

            //transform.rotation = Quaternion.Lerp(transform.rotation, playerRotation, rotationTest * Time.fixedDeltaTime);
        }
    }
    public void DriftSteering()
    {
        float rotation;
        if (player.driftingLeft) rotation = DriftRotation(player.maxDriftTurningSpeed, player.minDriftTurningSpeed);
        else 
        {
            rotation = DriftRotation(player.minDriftTurningSpeed, player.maxDriftTurningSpeed);
            rotation *= -1;
        }

        Quaternion q = Quaternion.AngleAxis(rotation, new Vector3(0, 1, 0));
        //Quaternion targetRotation = player.rb.rotation * q;
        //player.rb.MoveRotation(targetRotation);

        player.rb.MoveRotation(Quaternion.FromToRotation(player.transform.up, player.groundVector) * player.transform.rotation * q);

        player.ChildTransform.rotation = Quaternion.FromToRotation(player.transform.up, player.groundVector) * player.transform.rotation * Quaternion.AngleAxis(ZRotation, new Vector3(0, 0, 1));

        //player.playerRotation = Quaternion.Euler(0, player.transform.eulerAngles.y + rotation, 0);
        //player.rb.MoveRotation(player.playerRotation);
    }
    private float DriftRotation(float firstDriftValue, float secondDriftValue)
    {
        float rotation;
        if (player.moveDirection.x > 0)
        {
            rotation = firstDriftValue;
            ZRotation += -ZRotationPerFrame;
            if (ZRotation <= -maxZRotation) ZRotation = -maxZRotation;
        }
        else if (player.moveDirection.x < 0)
        {
            rotation = secondDriftValue;
            ZRotation += ZRotationPerFrame;
            if (ZRotation >= maxZRotation) ZRotation = maxZRotation;
        }
        else
        {
            rotation = player.baseDriftTurningSpeed;
            if (ZRotation < 0) ZRotation += ZRotationPerFrame;
            else if (ZRotation > 0) ZRotation += -ZRotationPerFrame;
        }

        return rotation;
    }
    public void Drift()
    {
        if (player.controls.Player.Drift.WasPerformedThisFrame() && player.moveDirection.x != 0 && player.speed > 3)
        {
            if (player.moveDirection.x > 0)
            {
                player.driftingLeft = true;
            }
            else if (player.moveDirection.x < 0)
            { 
                player.driftingLeft = false;
            }
            player.drifting = true;
            player.driftFactor = 0.1f;
        }

        if (player.controls.Player.Drift.WasReleasedThisFrame())
        {
            player.drifting = false;
            player.driftime = 0;

            if (player.getDriftBoost)
            {
                player.BonusSpeed += player.driftBoost;
                player.getDriftBoost = false;
            }

            player.StopCoroutine(nameof(player.ChangeDriftfactor));
            player.StartCoroutine(player.ChangeDriftfactor());
        }

        DriftTimerUpdate();
    }
    private void DriftTimerUpdate()
    {
        if (player.drifting)
        {
            player.driftime += Time.deltaTime;
            if (player.driftime > player.maxDrifttime)
            {
                player.getDriftBoost = true;
            }
        }
    }
    public void AirMovement()
    {
        //if (player.moveDirection.y == 0)
        //{
        //    player.speed = Mathf.Lerp(player.speed, 0, player.reduceSpeedIfNoInput * Time.fixedDeltaTime);
        //}
        //else
        //{
        //    if (player.moveDirection.y > 0)
        //    {
        //        if (player.rb.velocity.sqrMagnitude < 0.1f && player.speed < 1) player.speed = 1;
        //        player.speed = Mathf.Lerp(player.speed, player.maxspeed, Time.fixedDeltaTime * player.acceleration);
        //    }

        //    else if (player.moveDirection.y < 0)
        //    {
        //        if (player.rb.velocity.sqrMagnitude < 0.1f && player.speed > 1) player.speed = -1;
        //        player.speed = Mathf.Lerp(player.speed, -player.maxspeed * 0.5f, Time.fixedDeltaTime * (player.acceleration * 2));
        //    }
        //}

        //if (player.gravity > player.maxGravity) player.gravity = player.maxGravity;
        player.BonusSpeed -= player.boostSpeedReduction;
        //player.bonusSpeed = Mathf.Lerp(player.bonusSpeed, 0, player.boostSpeedReduction * Time.fixedDeltaTime);

        player.rb.velocity = Vector3.zero;

        player.finalSpeed = player.speed + player.bonusSpeed + player.beyondMaxSpeed;

        player.rb.AddForce(player.transform.forward * player.finalSpeed, ForceMode.Impulse);

        player.maxGravity = player.baseGravity + player.XAirRotation;
        player.rb.AddForce(Vector3.down * player.maxGravity, ForceMode.Impulse);
    }
    public void AirCheck()
    {
        if (Physics.BoxCast(player.transform.position, player.transform.localScale * 0.45f, -player.transform.up, out RaycastHit ray, player.rb.rotation, 3, player.groundCheckLayer))
        //if (Physics.BoxCast(player.transform.position, player.playerCollider.bounds.size * 0.48f, -player.transform.up, out RaycastHit hit, player.rb.rotation, 1f, player.groundCheckLayer))
        {
            MeshCollider meshCollider = ray.collider as MeshCollider;
            if (meshCollider != null || meshCollider.sharedMesh != null)
            {
                GroundRayHit(ray);

                player.SwitchAirIntoGround();
            }
        }
    }
    public void AirSteering()
    {
        //Quaternion toRotation = new Quaternion(0, player.transform.rotation.y, player.transform.rotation.z, player.transform.rotation.w);
        //player.transform.rotation = Quaternion.RotateTowards(player.transform.rotation, toRotation, player.airRotationSpeed * Time.deltaTime);

        //Quaternion toRotation2 = new Quaternion(player.transform.rotation.x, player.transform.rotation.y, 0, player.transform.rotation.w);
        //player.transform.rotation = Quaternion.RotateTowards(player.transform.rotation, toRotation2, player.airRotationSpeed * Time.deltaTime);

        float YRotation = 0;
        if (player.moveDirection.x > 0)
        {
            YRotation = player.turningSpeed;
            ZRotation += -ZRotationPerFrame;
            if (ZRotation <= -maxZRotation) ZRotation = -maxZRotation;
        }
        else if (player.moveDirection.x < 0)
        {
            YRotation = -player.turningSpeed;
            ZRotation += ZRotationPerFrame;
            if (ZRotation >= maxZRotation) ZRotation = maxZRotation;
        }
        else
        {
            if (ZRotation < 0) ZRotation += ZRotationPerFrame;
            else if (ZRotation > 0) ZRotation += -ZRotationPerFrame;
        }

        if(player.transform.eulerAngles.x <= 10 && player.moveDirection.y > 0)
        {
            player.XAirRotation++;
            if (player.XAirRotation >= 9) player.XAirRotation = 9;
        }
        else if (player.moveDirection.y < 0)
        {
            if (player.transform.eulerAngles.x > 350 || player.transform.eulerAngles.x == 0)
            {
                    player.XAirRotation--;
                    if (player.XAirRotation < -9) player.XAirRotation = -9;          
            }
        }
        else if(player.moveDirection.y == 0)
        {
            if (player.XAirRotation >= 1) player.XAirRotation--;
            else if (player.XAirRotation <= -1) player.XAirRotation++;
            else player.XAirRotation = 0;
        }

        player.playerRotation = Quaternion.Euler(PlayerAirRotation(player.transform.eulerAngles.x, 2), player.transform.eulerAngles.y + YRotation, PlayerAirRotation(player.transform.eulerAngles.z, 2));
        player.rb.MoveRotation(player.playerRotation);
        
        player.ChildTransform.rotation = player.transform.rotation * Quaternion.AngleAxis(ZRotation, new Vector3(0, 0, 1)) * Quaternion.AngleAxis(player.XAirRotation, new Vector3(1, 0, 0));
    }

    private float PlayerAirRotation(float baseRotation, float rotationSpeed)
    {
        float rotation = rotationSpeed;
        if (baseRotation < 2) rotation = 0;
        else if (baseRotation > 358) rotation = 0;
        else if (baseRotation < 180)
        {
            rotation *= -1;
            rotation += baseRotation;
        }
        else if (baseRotation > 180)
        {
            rotation *= 1;
            rotation += baseRotation;
        }
        return rotation;
    }
}
