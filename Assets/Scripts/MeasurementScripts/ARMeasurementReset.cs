using UnityEngine;

public class ARMeasurementReset : MonoBehaviour
{
    public void ResetMeasurements()
    {
        if (ARMeasurementTool.Instance != null)
        {
            ARMeasurementTool.Instance.ClearAllMeasurements();
        }
    }
}