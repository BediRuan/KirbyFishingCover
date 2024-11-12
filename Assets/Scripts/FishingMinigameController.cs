using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FishingMinigameController : MonoBehaviour
{

    public Sprite leftArrow, rightArrow, upArrow, downArrow; // Arrow sprites
    public Image resultDisplay; // UI Image for success/failure
    public Sprite successSprite, failureSprite; // Success/failure images
    public int sequenceLength = 5; // Number of prompts in the sequence

    private List<KeyCode> promptSequence = new List<KeyCode>(); // Holds the sequence of prompts
    private bool isPlayerInputActive = false; // Checks if input phase is active


    public Transform fishingFloat; // Reference to the fishing float
    public float floatAmplitude = 0.1f; // How far the float moves up and down
    public float floatFrequency = 2.0f; // Speed of the up-and-down movement
    private Vector3 originalFloatPosition; // To remember the original position

    public Image leftArrowDisplay, rightArrowDisplay, upArrowDisplay, downArrowDisplay;

    public Image characterImage;  // Reference to the character's image component
    public Sprite poseLeft;       // Sprite for left arrow pose
    public Sprite poseRight;      // Sprite for right arrow pose
    public Sprite poseUp;         // Sprite for up arrow pose
    public Sprite poseDown;       // Sprite for down arrow pose
    public Sprite idlePose;       // Sprite for the idle/default pose

    private bool isVibrating = false; // Controls if the float is vibrating

    public GameObject bigFish;  // Reference to the big fish 
    public GameObject mediumFish; // Reference to the medium fish 
    public GameObject smallFish;  // Reference to the small fish 

    private float inputStartTime; // Tracks when input phase begins



    void Start()
    {
        originalFloatPosition = fishingFloat.position; // Save initial position
        StartGame();
    }


    // Generates a sequence of random arrow keys
    void GenerateSequence(int length)
    {
        promptSequence.Clear();
        for (int i = 0; i < length; i++)
        {
            KeyCode randomKey = GetRandomKey();
            promptSequence.Add(randomKey);
        }
        Debug.Log("Generated Sequence: " + string.Join(", ", promptSequence));
    }


    // Returns a random KeyCode for the arrow keys
    KeyCode GetRandomKey()
    {
        KeyCode[] keys = { KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow };
        return keys[Random.Range(0, keys.Length)];
    }

    // Starts the game and shows the sequence of prompts
    void StartGame()
    {
        GenerateSequence(sequenceLength);
        HideAllFish();  // Hide all fish at the beginning
        StartCoroutine(ShowSequence());
    }

    void HideAllFish()
    {
        bigFish.SetActive(false);
        mediumFish.SetActive(false);
        smallFish.SetActive(false);
    }
    // Coroutine to display each prompt one by one
    IEnumerator ShowSequence()
    {
        isPlayerInputActive = false; // Disable player input during prompt display
        foreach (var key in promptSequence)
        {
            ShowPrompt(key);
            yield return new WaitForSeconds(1.5f); // Wait before showing the next prompt
            HidePrompt(); // Make the prompt invisible
            yield return new WaitForSeconds(0.5f); // Short pause before the next prompt
        }

        // Start float vibration and input timer after all prompts are shown
        StartCoroutine(StartFloatAndInputTimer());
    }



    // Updates the prompt display based on the current key
    void ShowPrompt(KeyCode key)
    {
        HideAllPrompts();
        switch (key)
        {
            case KeyCode.LeftArrow:
                leftArrowDisplay.sprite = leftArrow;
                leftArrowDisplay.color = new Color(1, 1, 1, 1);
                break;
            case KeyCode.RightArrow:
                rightArrowDisplay.sprite = rightArrow;
                rightArrowDisplay.color = new Color(1, 1, 1, 1);
                break;
            case KeyCode.UpArrow:
                upArrowDisplay.sprite = upArrow;
                upArrowDisplay.color = new Color(1, 1, 1, 1);
                break;
            case KeyCode.DownArrow:
                downArrowDisplay.sprite = downArrow;
                downArrowDisplay.color = new Color(1, 1, 1, 1);
                break;
        }
    }

    // Call this method to hide the arrow display
    void HidePrompt()
    {
        HideAllPrompts();
    }



    // Coroutine to check player input for each prompt in sequence
    IEnumerator CheckPlayerInput()
    {
        for (int i = 0; i < promptSequence.Count; i++)
        {
            bool correctKeyPressed = false;

            Debug.Log("Current expected key: " + promptSequence[i]);

            // Wait until the correct key is pressed or time runs out (handled in StartFloatAndInputTimer)
            while (!correctKeyPressed && isPlayerInputActive)
            {
                if (Input.GetKeyDown(promptSequence[i]))
                {
                    Debug.Log("Correct key pressed: " + promptSequence[i]);
                    correctKeyPressed = true;

                    // Change character pose based on the pressed key
                    ChangeCharacterPose(promptSequence[i]);

                    // Wait a moment for the player to see the pose
                    yield return new WaitForSeconds(0.5f);

                    // Reset to idle pose after showing the selected pose
                    characterImage.sprite = idlePose;
                }
                else if (Input.anyKeyDown)
                {
                    Debug.Log("Incorrect key pressed.");
                    ShowFailure();
                    StopCoroutine("VibrateFloat"); // Stop float vibration
                    isPlayerInputActive = false; // Disable player input
                    yield break; // Exit if an incorrect key is pressed
                }

                yield return null; // Wait for the next frame to check again
            }
        }

        ShowSuccess();
        StopCoroutine("VibrateFloat"); // Stop float vibration
        isPlayerInputActive = false; // Disable player input after success
    }


    void ChangeCharacterPose(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.LeftArrow:
                characterImage.sprite = poseLeft;
                break;
            case KeyCode.RightArrow:
                characterImage.sprite = poseRight;
                break;
            case KeyCode.UpArrow:
                characterImage.sprite = poseUp;
                break;
            case KeyCode.DownArrow:
                characterImage.sprite = poseDown;
                break;
            default:
                characterImage.sprite = idlePose; // Reset to idle if no key matches
                break;
        }
    }


    void HideAllPrompts()
    {
        leftArrowDisplay.color = new Color(1, 1, 1, 0);
        rightArrowDisplay.color = new Color(1, 1, 1, 0);
        upArrowDisplay.color = new Color(1, 1, 1, 0);
        downArrowDisplay.color = new Color(1, 1, 1, 0);
    }





    
    // Shows the success image and displays the appropriate fish based on completion time
    void ShowSuccess()
    {
        resultDisplay.sprite = successSprite; // Set to success sprite
        resultDisplay.color = new Color(1, 1, 1, 1); // Fully visible
        isVibrating = false; // Stop float vibration
        isPlayerInputActive = false; // Disable further input

        float timeTaken = Time.time - inputStartTime; // Calculate the time taken

        // Show fish based on time taken to complete input
        if (timeTaken <= 4.0f)
        {
            bigFish.SetActive(true);
            Debug.Log("Big Fish displayed!");
        }
        else if (timeTaken <= 5.0f)
        {
            mediumFish.SetActive(true);
            Debug.Log("Medium Fish displayed!");
        }
        else if (timeTaken <= 6.0f)
        {
            smallFish.SetActive(true);
            Debug.Log("Small Fish displayed!");
        }
        else
        {
            ShowFailure(); // If somehow input completed after 5 seconds, show failure
        }
    }


    // Shows the failure image and makes it visible
    void ShowFailure()
    {
        resultDisplay.sprite = failureSprite; // Set to failure sprite
        resultDisplay.color = new Color(1, 1, 1, 1); // Fully visible
        isVibrating = false; // Stop float vibration
    }


    // Hides the result display by making it fully transparent
    void HideResult()
    {
        resultDisplay.color = new Color(1, 1, 1, 0); // Fully transparent
    }




    // Coroutine to handle float vibration and input timing
    // Coroutine to handle float vibration and input timing
    IEnumerator StartFloatAndInputTimer()
    {
        // Random delay before the float starts vibrating
        float randomDelay = Random.Range(1.0f, 3.0f);
        yield return new WaitForSeconds(randomDelay);

        // Start vibrating the float
        isVibrating = true;
        StartCoroutine(VibrateFloat());

        // Enable player input once the float starts vibrating
        isPlayerInputActive = true;
        StartCoroutine(CheckPlayerInput());
        inputStartTime = Time.time; // Record the time when input begins

        // Give the player 5 seconds to complete input after vibration starts
        yield return new WaitForSeconds(7.0f);

        // If the player hasn't finished the input sequence in time, trigger failure
        if (isPlayerInputActive)
        {
            Debug.Log("Time ran out!");
            ShowFailure();
            StopCoroutine("CheckPlayerInput"); // Stop input-checking coroutine if still running
            isPlayerInputActive = false; // Disable player input
            isVibrating = false; // Stop float vibration
        }
    }


    
    // Coroutine to vibrate the float
    IEnumerator VibrateFloat()
    {
        while (isVibrating)
        {
            float yOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            fishingFloat.position = originalFloatPosition + new Vector3(0, yOffset, 0);
            yield return null;
        }
        fishingFloat.position = originalFloatPosition; // Reset position when done
    }


}