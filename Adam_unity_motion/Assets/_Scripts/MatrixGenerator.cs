using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatrixGenerator : MonoBehaviour
{
    public float[] coherences = { 1f, 0.5f, 0.4f, 0.2f, 0.1f, 0.05f }; // List of coherence values (100%, 50%, etc.)
    
    // This method generates a matrix of trials with equal left (-1) and right (1) directions and coherences.
    public List<float[]> GenerateTrialMatrix()
    {
        List<float[]> trials = new List<float[]>();

        // Generate trials: for each coherence, add one left (-1) and one right (1) trial.
        foreach (float coherence in coherences)
        {
            trials.Add(new float[] { coherence, -1 }); // Left trial
            trials.Add(new float[] { coherence, 1 });  // Right trial
        }

        // Shuffle the trial list to randomize the order of the trials
        trials = trials.OrderBy(x => Random.value).ToList();

        return trials;
    }
}
