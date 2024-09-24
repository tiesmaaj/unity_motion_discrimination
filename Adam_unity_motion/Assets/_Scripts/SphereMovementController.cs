using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO; // For file handling

public class SphereMovementController : MonoBehaviour
{
    public List<GameObject> spheres = new List<GameObject>(); // Reference to 20 spheres already in the scene
    public Camera mainCamera; // Reference to the main camera
    public int loopCount = 5; // Number of loops
    public float speed = 2f; // Movement speed
    public float distanceFromCamera = 5f; // Distance from the camera
    public float visibleTime = 0.7f; // Visible time before the spheres disappear
    public float circleRadius = 3f; // The radius of the circular space for random positions
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip incorrectSound;
    public TextMeshProUGUI questionMarkText; // Reference to the UI Text element for the question mark
    public TextMeshProUGUI fixationText; // Declare the fixationText as public so it can be assigned in the editor

    public float[] coherences = { 1f, 0.5f, 0.4f, 0.2f, 0.1f, 0.05f }; // List of coherence values (100%, 50%, etc.)
    private int currentLoop = 0;
    private int direction; // Direction of the signal dots (-1 for left, 1 for right)
    private float selectedCoherence; // Coherence for the current trial
    private List<float[]> trialData = new List<float[]>(); // Modified to store direction, coherence, and input

    public int numSpheres;

    private List<TrialDef> trialDefs;
    public BlockGenerator blockGen;
    
    void Start()
    {
        questionMarkText.gameObject.SetActive(false);
        fixationText.gameObject.SetActive(true); // Ensure the fixation cross is visible
        trialDefs = blockGen.GenerateBlock();
        SetupSpheres();
        StartCoroutine(StartMovementLoop());
    }

    private void SetupSpheres()
    {
        GameObject RDK = new GameObject("RDK");
        for (int i = 0; i < numSpheres; i++)
        {
            GameObject newSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere); 
            
            //Change individual sphere parameters here
            
            newSphere.transform.SetParent(RDK.transform);
            spheres.Add(newSphere);
            
        }
    }

    IEnumerator StartMovementLoop()
    {
        while (currentLoop < trialDefs.Count)
        {
            
            // Randomly select a coherence value for this trial
            selectedCoherence = trialDefs[currentLoop].vis_coherence;//coherences[Random.Range(0, coherences.Length)];

            // Determine the number of signal dots based on the selected coherence
            int signalCount = Mathf.RoundToInt(spheres.Count * selectedCoherence); // E.g., 20 dots, coherence 0.5 => 10 signal dots
            int noiseCount = spheres.Count - signalCount; // The rest will be noise dots

            // Randomize the direction of movement for signal dots
            direction = trialDefs[currentLoop].vis_direction; //Random.Range(0, 2) == 0 ? -1 : 1;

            // Shuffle and assign signal/noise behavior
            List<GameObject> signalDots = new List<GameObject>();
            List<GameObject> noiseDots = new List<GameObject>();

            for (int i = 0; i < spheres.Count; i++)
            {
                if (i < signalCount)
                {
                    signalDots.Add(spheres[i]);
                }
                else
                {
                    noiseDots.Add(spheres[i]);
                }
            }

            signalDots = ShuffleList(signalDots);
            noiseDots = ShuffleList(noiseDots);

            // Randomize positions within a circular area for each trial
            foreach (var dot in spheres)
            {
                dot.transform.position = GetRandomPositionInCircle(); // Replot all dots inside a circular area
            }

            // Start the movement
            StartCoroutine(MoveSignalDots(signalDots, direction));
            StartCoroutine(MoveNoiseDots(noiseDots));

            // Wait for the movement duration to complete
            yield return new WaitForSeconds(visibleTime);

            // Hide the spheres after the trial
            foreach (var dot in spheres)
            {
                dot.SetActive(false);
            }

            currentLoop++;

            // Wait for player input (e.g., left or right arrow key)
            questionMarkText.gameObject.SetActive(true);
            yield return StartCoroutine(WaitForKeyPress());
            questionMarkText.gameObject.SetActive(false);
        }

        // Save the trial data after the experiment
        SaveTrialDataToCSV();
    }

    // Move the signal dots in a straight line (left or right)
    IEnumerator MoveSignalDots(List<GameObject> signalDots, int direction)
    {
        foreach (var dot in signalDots)
        {
            dot.SetActive(true); // Make the signal dots visible
        }

        float elapsedTime = 0f;
        Vector3 moveDirection = new Vector3(direction, 0, 0); // Move left or right

        while (elapsedTime < visibleTime)
        {
            foreach (var dot in signalDots)
            {
                dot.transform.Translate(moveDirection * speed * Time.deltaTime);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    // Move the noise dots in random directions
    IEnumerator MoveNoiseDots(List<GameObject> noiseDots)
    {
        foreach (var dot in noiseDots)
        {
            dot.SetActive(true); // Make the noise dots visible
        }

        float elapsedTime = 0f;
        float directionChangeInterval = 0.3f; // Change direction every 350 ms
        Dictionary<GameObject, Vector3> currentDirections = new Dictionary<GameObject, Vector3>();

        foreach (var dot in noiseDots)
        {
            currentDirections[dot] = GetRandomDirection(); // Assign initial random direction
        }

        while (elapsedTime < visibleTime)
        {
            foreach (var dot in noiseDots)
            {
                dot.transform.Translate(currentDirections[dot] * speed * Time.deltaTime);
            }

            // Change direction every 350 ms
            if (elapsedTime % directionChangeInterval < Time.deltaTime)
            {
                foreach (var dot in noiseDots)
                {
                    currentDirections[dot] = GetRandomDirection();
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    // Generate a random position within a circle
    Vector3 GetRandomPositionInCircle()
    {
        Vector2 randomPoint = Random.insideUnitCircle * circleRadius; // Get random point inside a circle of radius
        return mainCamera.transform.position + mainCamera.transform.forward * distanceFromCamera
            + new Vector3(randomPoint.x, randomPoint.y, 0); // Convert the 2D point to 3D space (x, y, 0)
    }

    // Shuffle a list of dots (to randomize which ones are signal and noise)
    List<GameObject> ShuffleList(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            GameObject temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
        return list;
    }

    // Get a random direction vector for the noise dots
    Vector3 GetRandomDirection()
    {
        return new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    IEnumerator WaitForKeyPress()
    {
        bool responseGiven = false;
        int playerDirection = 0;

        while (!responseGiven)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                playerDirection = -1; // Player pressed left
                responseGiven = true;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                playerDirection = 1; // Player pressed right
                responseGiven = true;
            }

            yield return null;
        }

        // Store trial data: [direction, selectedCoherence, playerDirection]
        trialData.Add(new float[] { direction, selectedCoherence, playerDirection });
        CheckResponse(playerDirection);
    }

    void CheckResponse(int playerDirection)
    {
        if (playerDirection == direction)
        {
            Debug.Log("Correct!");
            audioSource.PlayOneShot(correctSound);
        }
        else
        {
            Debug.Log("Incorrect!");
            audioSource.PlayOneShot(incorrectSound);
        }
    }

    // Save the trial data to a CSV file
    void SaveTrialDataToCSV()
    {
        string filePath = Application.dataPath + "/TrialData.csv";
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("TrialDirection,Coherence,UserInput"); // Write headers

            foreach (var trial in trialData)
            {
                // Write each trial's data to the CSV file
                writer.WriteLine($"{trial[0]},{trial[1]},{trial[2]}");
            }
        }

        Debug.Log($"Trial data saved to {filePath}");
    }
}
