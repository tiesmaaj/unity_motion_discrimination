using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialDef : MonoBehaviour
{
    public float vis_coherence, aud_coherence;
    public int vis_direction, aud_direction;

    public TrialDef(float vis_co, int vis_dir)
    {
        vis_coherence = vis_co;
        vis_direction = vis_dir;
    }

}
