namespace CsharpToColouredHTML.Core;

internal struct DetectionStatus
{
    public bool Detected { get; set; }

    public bool SkipPostProcessing { get; set; }

    public static DetectionStatus NotDetected()
    {
        return new DetectionStatus
        {
            Detected = false
        };
    }

    public static DetectionStatus DetectedAndSkipPostProcessing()
    {
        return new DetectionStatus
        {
            Detected = true,
            SkipPostProcessing = true
        };
    }

    public static DetectionStatus DetectedAndDontSkipPostProcessing()
    {
        return new DetectionStatus
        {
            Detected = true,
            SkipPostProcessing = false
        };
    }
}

