using UnityEngine;

public interface Status_interface
{
    Hitstatus masterHitstatus { get; set; }
    Hitstatus currentHitstatus { get; set; }

    float HP { get; set; }
    float HPMax { get; set; }
}
