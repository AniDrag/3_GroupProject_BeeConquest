using UnityEngine;
using System.Collections.Generic;

public class LightningLineController : MonoBehaviour
{
    [SerializeField] List<LineRenderer> lineRenderers = new List<LineRenderer> ();

    public void SetPositions(Vector3 startPos, Vector3 endPos)
    {
        if (lineRenderers.Count > 0)
        {
            for (int i = 0; i < lineRenderers.Count; i++)
            {

                lineRenderers[i].positionCount = 2;
                if (lineRenderers[i].positionCount >= 2)
                {
                    lineRenderers[i].SetPosition(0, startPos);
                    lineRenderers[i].SetPosition(1, endPos);
                }
                else
                    Debug.Log("Line renderer needs at least 2 pos");
            }
        }
        else
            Debug.Log("No line renderers are assigned");
    }

    // Clears the lines (hides them)
    public void ClearLines()
    {
        for (int i = 0; i < lineRenderers.Count; i++)
        {
            if (lineRenderers[i] == null) continue;
            // Setting positionCount to 0 hides the line
            lineRenderers[i].positionCount = 0;
        }
    }

}
