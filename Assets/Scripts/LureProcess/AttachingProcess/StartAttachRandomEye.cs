using UnityEngine;

public class StartAttachRandomEye : StartAttach
{
    protected override void OnMouseDown()
    {
        int random = Random.Range(0, 5);
        AttachingType type = random switch
        {
            0 => AttachingType.Eye1,
            1 => AttachingType.Eye2,
            2 => AttachingType.Eye3,
            3 => AttachingType.Eye4,
            4 => AttachingType.Eye5,
            _ => AttachingType.Eye1,
        };

        AttachingProcess.Instance.StartAttachingObject(type);
    }
}
