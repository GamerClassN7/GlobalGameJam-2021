﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public float speed = 8;
    public float runSpeed = 20;
    public float mouseSensitive = 100;
    public float jump = 5;
    public float health = 100;
    public float actualHealth;
    private float[] actualPowerTimes = new float[] { 0f, 0f, 0f };
    public bool onGround = true;
    private bool run = false;
    private Animator playerAnimator;
    private Rigidbody rigidBody;
    private PowerCubeManager powerCubeManager;
    private bool interact = false;

    public List<int> activeAbility = new List<int>(); //without ability=0 or null, dubleJump = 1, push/pull = 2, dash = 3
    public List<GameObject> PowerPrefabs = new List<GameObject>(); //dubleJump = 0, push/pull = 1, dash = 2
    private bool dubleJump = true;
    private GameObject pushPullObject;
    public float dashPower = 40f;
    public float dashTime = 0.2f;
    private float actualDashTime;
    private int dashButton;
    private bool dash = false;

    private bool startEating = false;

    private float saveSpeed;
    private float saveRunSpeed;
    private float saveJump;
    private Vector3 saveSize;
    private Vector3 savePosition;
    private Vector3 saveCameraPosition;

    // Start is called before the first frame update
    void Start()
    {
        actualHealth = health;
        playerAnimator = GetComponentInChildren<Animator>();
        rigidBody = GetComponent<Rigidbody>();

        saveSpeed = speed;
        saveRunSpeed = runSpeed;
        saveJump = jump;
        saveSize = playerAnimator.transform.localScale;
        savePosition = playerAnimator.transform.localPosition;
        saveCameraPosition = Camera.main.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit(0);
        }
        if (actualHealth <= 0)
        {
            playerAnimator.Play("Die");
        }
        if (interact)
        {
            if (powerCubeManager != null && (
                powerCubeManager.powerType == PowerCubeManager.PowerType.Artefact ||
                powerCubeManager.powerType == PowerCubeManager.PowerType.DubleJump ||
                powerCubeManager.powerType == PowerCubeManager.PowerType.PushPull ||
                powerCubeManager.powerType == PowerCubeManager.PowerType.Dash
            )) {
                if (Input.GetKeyUp(KeyCode.E))
                {
                    playerAnimator.SetTrigger("Eat");
                    interact = false;
                }
            }
        }
        AbilityAction();
        DeactivePowerCube();
        Move();
        RunSwitch();
        Animation();

        if (startEating)
        {
            float stepSmaller = 0.8f * Time.deltaTime;
            powerCubeManager.transform.localScale = Vector3.Slerp(powerCubeManager.transform.localScale, new Vector3(0.01f, 0.01f, 0.01f), stepSmaller);
        }
    }

    private void FixedUpdate()
    {
        Jump();
    }

    private void AbilityAction()
    {
        if (activeAbility.Count > 0 && activeAbility[0] == 2)
        {
            if (pushPullObject != null)
            {
                pushPullObject.GetComponent<Rigidbody>().MovePosition(
                    pushPullObject.transform.position +
                    (pushPullObject.transform.right * (run ? runSpeed : speed) * Input.GetAxis("Horizontal") * Time.deltaTime)
                );
            }
        }
        else if (activeAbility.Count > 0 && activeAbility[0] == 3)
        {
            if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.A) && !dash) {
                if (actualDashTime < Time.time)
                {
                    if (Input.GetKeyUp(KeyCode.D))
                    {
                        dashButton = 1;
                    }
                    else if (Input.GetKeyUp(KeyCode.A))
                    {
                        dashButton = 2;
                    }
                    actualDashTime = Time.time + dashTime;
                }
                else
                {
                    if (dashButton == 1 && Input.GetKeyUp(KeyCode.D)) {
                        rigidBody.AddForce(
                            (transform.right * dashPower * 10 * 5 * 1 * Time.deltaTime) +
                            (transform.up * 1 * 10 * Time.deltaTime),
                            ForceMode.VelocityChange
                        );
                        dash = true;
                        dashButton = 0;
                        actualDashTime = Time.time - 1f;
                    } 
                    else if (dashButton == 2 && Input.GetKeyUp(KeyCode.A))
                    {
                        rigidBody.AddForce(
                            (transform.right * dashPower * 10 * 5 * -1 * Time.deltaTime) +
                            (transform.up * 1 * 10 * Time.deltaTime),
                            ForceMode.VelocityChange
                        );
                        dash = true;
                        dashButton = 0;
                        actualDashTime = Time.time - 1f;
                    }
                }
            }
        }
    }

    public GameObject GetPushPullObject()
    {
        return pushPullObject;
    }

    public void SetPushPullObject(GameObject objectPP)
    {
        if (activeAbility.Count > 0 && activeAbility[0] == 2) {
            pushPullObject = objectPP;
        }
    }

    public void RemovePushPullObject()
    {
        pushPullObject = null;
    }

    void RunSwitch()
    {
        if (Input.GetAxisRaw("Run") > 0)
        {
            run = true;
        }
        else
        {
            run = false;
        }
    }

    public void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Animation()
    {
        float localSpeed = 5f;
        if (Input.GetAxis("Horizontal") > 0)
        {
            playerAnimator.SetBool("Walk", true);
            if (pushPullObject == null) {
                playerAnimator.transform.rotation = Quaternion.Lerp(
                    playerAnimator.transform.rotation,
                    Quaternion.Euler(playerAnimator.transform.eulerAngles.x, -90f, playerAnimator.transform.eulerAngles.z),
                    localSpeed * Time.deltaTime
                );
            }
        }
        else if (Input.GetAxis("Horizontal") < 0)
        {
            playerAnimator.SetBool("Walk", true);
            if (pushPullObject == null)
            {
                playerAnimator.transform.rotation = Quaternion.Lerp(
                    playerAnimator.transform.rotation,
                    Quaternion.Euler(playerAnimator.transform.eulerAngles.x, 90f, playerAnimator.transform.eulerAngles.z),
                    localSpeed * Time.deltaTime
                );
            }
        }
        else
        {
            playerAnimator.SetBool("Walk", false);
        }
    }

    void Move()
    {
        if (onGround) {
            rigidBody.MovePosition(
                transform.position +
                (transform.right * (run ? runSpeed : speed) * Input.GetAxis("Horizontal") * Time.deltaTime)
            );
        }
    }

    void Jump()
    {
        if (Input.GetAxisRaw("Jump") > 0)
        {
            if (rigidBody.velocity.y <= 1 && (onGround || (dubleJump && activeAbility.Count > 0 && activeAbility[0] == 1)))
            {
                if (!onGround)
                {
                    dubleJump = false;
                }
                pushPullObject = null;
                rigidBody.AddForce(
                    (transform.right * (run ? runSpeed : speed) * 5 * Input.GetAxis("Horizontal") * Time.deltaTime) + 
                    (transform.up * jump * 10 * Time.deltaTime), 
                    ForceMode.VelocityChange
                );
            }
        }
    }

    private void DropPower()
    {
        if (activeAbility.Count > 0 && activeAbility[0] != 0)
        {
            Instantiate(PowerPrefabs[activeAbility[0] - 1], new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z + 0.5f), PowerPrefabs[activeAbility[0] - 1].transform.rotation);
        }
    }

    public void ActivePowerCube(float power, float powerTime, PowerCubeManager.PowerType powerType, string nextSceneName = "")
    {
        if ((powerType.GetHashCode() - 1) == 3)
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else if ((powerType.GetHashCode() - 1) == 4)
        {
            DropPower();
            //doubleJump
            activeAbility[0] = 1;
        }
        else if ((powerType.GetHashCode() - 1) == 5)
        {
            DropPower();
            //pushpull
            activeAbility[0] = 2;
        }
        else if ((powerType.GetHashCode() - 1) == 6)
        {
            DropPower();
            //dash
            activeAbility[0] = 3;
        }
        else if (actualPowerTimes.Length <= (powerType.GetHashCode()) || actualPowerTimes[powerType.GetHashCode() - 1] < Time.time) 
        {
            actualPowerTimes[powerType.GetHashCode() - 1] = Time.time + powerTime;
            if (powerType == PowerCubeManager.PowerType.Bigger)
            {
                saveSize = playerAnimator.transform.localScale;
                savePosition = playerAnimator.transform.localPosition;
                saveCameraPosition = Camera.main.transform.localPosition;

                playerAnimator.transform.localScale = new Vector3(
                    playerAnimator.transform.localScale.x + power,
                    playerAnimator.transform.localScale.y + power,
                    playerAnimator.transform.localScale.z + power
                );
                playerAnimator.transform.localPosition = new Vector3(
                    playerAnimator.transform.localPosition.x,
                    playerAnimator.transform.localPosition.y + power * 2,
                    playerAnimator.transform.localPosition.z
                );
                Camera.main.transform.localPosition = new Vector3(
                    Camera.main.transform.localPosition.x,
                    Camera.main.transform.localPosition.y,
                    Camera.main.transform.localPosition.z - power * 2
                );

            }
            else if (powerType == PowerCubeManager.PowerType.Faster)
            {
                saveSpeed = speed;
                saveRunSpeed = runSpeed;

                speed += power;
                runSpeed += power;
            }
            else if (powerType == PowerCubeManager.PowerType.Jumper)
            {
                saveJump = jump;

                jump += power;
            }
        }
    }

    private void DeactivePowerCube()
    {
        if (actualPowerTimes[0] != 0f && actualPowerTimes[0] < Time.time)
        {
            playerAnimator.transform.localScale = saveSize;
            playerAnimator.transform.localPosition = savePosition;
            Camera.main.transform.localPosition = saveCameraPosition;
            actualPowerTimes[0] = 0f;
        }
        if (actualPowerTimes[1] != 0f && actualPowerTimes[1] < Time.time)
        {
            speed = saveSpeed;
            runSpeed = saveRunSpeed;
            actualPowerTimes[1] = 0f;
        }
        if (actualPowerTimes[2] != 0f && actualPowerTimes[2] < Time.time)
        {
            jump = saveJump;
            actualPowerTimes[2] = 0f;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ground" || other.tag == "Objects")
        {
            rigidBody.MovePosition(transform.position);
            onGround = true;
            dubleJump = true;
            dash = false;
        }
        if (other.gameObject.GetComponent<PowerCubeManager>() != null)
        {
            powerCubeManager = other.gameObject.GetComponent<PowerCubeManager>();
            interact = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Ground" || other.tag == "Objects")
        {
            rigidBody.AddForce(
                (transform.right * (run ? runSpeed : speed) * 5 * Input.GetAxis("Horizontal") * Time.deltaTime) +
                (transform.up * 10 * Time.deltaTime),
                ForceMode.VelocityChange
            );
            onGround = false;
            dash = false;
        }
        if (other.gameObject.GetComponent<PowerCubeManager>()  != null)
        {
            interact = false;
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.tag == "Ground" || other.tag == "Objects")
        {
            if (rigidBody.velocity.y <= 1 && !onGround) {
                rigidBody.MovePosition(transform.position);
                onGround = true;
                dubleJump = true;
            }
            if (onGround) {
                dash = false;
            }
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PowerCubeManager>() != null)
        {
            Vector3 hit = collision.contacts[0].normal;
            if (hit.x != 0 && hit.y == 0)
            {
                powerCubeManager = collision.gameObject.GetComponent<PowerCubeManager>();
                playerAnimator.SetTrigger("Eat");
            }
        }
    }

    public void damage(float damage, bool instaKill)
    {
        if (instaKill)
        {
            actualHealth = 0;
        } 
        else
        {
            actualHealth -= damage;
        }
    }

    public void StartEatPowerCube()
    {
        startEating = true;
    }

    public void EndEatPowerCube()
    {
        startEating = false;
        ActivePowerCube(powerCubeManager.powerUnit, powerCubeManager.powerTime, powerCubeManager.powerType, powerCubeManager.nextSceneName);
        Destroy(powerCubeManager.gameObject);
    }
}
